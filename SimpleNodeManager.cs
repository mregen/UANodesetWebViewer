
using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Export;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UANodesetWebViewer.Controllers;
using UANodesetWebViewer.Models;

namespace UANodesetWebViewer
{
    public class SimpleNodeManager : CustomNodeManager2
    {
        public SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration)
        {
            SystemContext.NodeIdFactory = this;

            List<string> namespaces = new List<string>();
            foreach (string nodesetFile in BrowserController._nodeSetFilename)
            {
                using (Stream stream = new FileStream(nodesetFile, FileMode.Open))
                {
                    UANodeSet nodeSet = UANodeSet.Read(stream);
                    foreach (string ns in nodeSet.NamespaceUris)
                    {
                        if (!namespaces.Contains(ns))
                        {
                            namespaces.Add(ns);
                        }
                    }
                }
            }

            NamespaceUris = namespaces.ToArray();
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                IList<IReference> references = null;
                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                }

                DTDL.GeneratedDTDL = string.Empty;

                foreach (string nodesetFile in BrowserController._nodeSetFilename)
                {
                    ImportNodeset2Xml(externalReferences, nodesetFile);
                }

                AddReverseReferences(externalReferences);
            }
        }

        private void ImportNodeset2Xml(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcepath)
        {
            using (Stream stream = new FileStream(resourcepath, FileMode.Open))
            {
                UANodeSet nodeSet = UANodeSet.Read(stream);

                NodeStateCollection predefinedNodes = new NodeStateCollection();
                nodeSet.Import(SystemContext, predefinedNodes);

                Debug.WriteLine(" Server Namespaces:");
                for(int i = 0; i < Server.NamespaceUris.Count; i++)
                {
                    Debug.WriteLine(i + ": " + Server.NamespaceUris.GetString((uint)i));
                }
                Debug.WriteLine("\r\nNodeset Namespaces:");
                for (int i = 0; i < nodeSet.NamespaceUris.Length; i++)
                {
                    Debug.WriteLine(i + ": " + nodeSet.NamespaceUris[i]);
                }

                // Create a DTDL Interface form our nodeset file (pick the first namespace as the name)
                string interfaceName = nodeSet.NamespaceUris[0].Replace("http://","").Replace("/",":").TrimEnd(':');
                DtdlInterface dtdlInterface = new DtdlInterface
                {
                    Id = "dtmi:" + interfaceName + ";1",
                    Type = "Interface",
                    DisplayName = interfaceName,
                    Contents = new List<DtdlContents>(),
                };

                for (int i = 0; i < predefinedNodes.Count; i++)
                {
                    DTDL.AddNodeToDTDLInterface(predefinedNodes[i], dtdlInterface.Contents);

                    // debug output
                    BaseInstanceState instance = predefinedNodes[i] as BaseInstanceState;
                    if (instance != null)
                    {
                        Debug.WriteLine("Instance: ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier + " " + "TypeDefinition: ns=" + ((BaseInstanceState)predefinedNodes[i]).TypeDefinitionId.NamespaceIndex + ";i=" + ((BaseInstanceState)predefinedNodes[i]).TypeDefinitionId.Identifier);
                    }
                    else
                    {
                        BaseObjectTypeState objectType = predefinedNodes[i] as BaseObjectTypeState;
                        if (objectType != null)
                        {
                            Debug.WriteLine("Object Type: ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier + " " + "Supertype: ns=" + ((BaseObjectTypeState)predefinedNodes[i]).SuperTypeId.NamespaceIndex + ";i=" + ((BaseObjectTypeState)predefinedNodes[i]).SuperTypeId.Identifier);
                        }
                        else
                        {
                            BaseObjectState objectState = predefinedNodes[i] as BaseObjectState;
                            if (objectState != null)
                            {
                                Debug.WriteLine("Object: ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier + " " + "TypeDefinition: ns=" + ((BaseObjectState)predefinedNodes[i]).TypeDefinitionId.NamespaceIndex + ";i=" + ((BaseObjectState)predefinedNodes[i]).TypeDefinitionId.Identifier);
                            }
                            else
                            {
                                ReferenceTypeState reference = predefinedNodes[i] as ReferenceTypeState;
                                if (objectState != null)
                                {
                                    Debug.WriteLine("Reference Type: ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier + " " + "Supertype: ns=" + ((ReferenceTypeState)predefinedNodes[i]).SuperTypeId.NamespaceIndex + ";i=" + ((ReferenceTypeState)predefinedNodes[i]).SuperTypeId.Identifier);
                                }
                                else
                                {
                                    Debug.WriteLine("Unknown: ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier);
                                }
                            }
                        }
                    }

                    try
                    {
                        AddPredefinedNode(SystemContext, predefinedNodes[i]);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Importing node ns=" + predefinedNodes[i].NodeId.NamespaceIndex + ";i=" + predefinedNodes[i].NodeId.Identifier + " (" + predefinedNodes[i].DisplayName + ") failed with error: " + ex.Message);
                    }
                }

                DTDL.GeneratedDTDL += JsonConvert.SerializeObject(dtdlInterface, Formatting.Indented);
            }
        }
    }
}
