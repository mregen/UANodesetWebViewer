
using Opc.Ua.Export;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UANodesetWebViewer.Controllers;

namespace Opc.Ua.Sample
{
    public class SimpleNodeManager : CustomNodeManager2
    {
        public SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration)
        {
            SystemContext.NodeIdFactory = this;

            using (Stream stream = new FileStream(BrowserController._nodeSetFilename, FileMode.Open))
            {
                UANodeSet nodeSet = UANodeSet.Read(stream);
                NamespaceUris = nodeSet.NamespaceUris;
            }
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

                ImportNodeset2Xml(externalReferences, BrowserController._nodeSetFilename);

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

                for (int i = 0; i < predefinedNodes.Count; i++)
                {
                    try
                    {
                        AddPredefinedNode(SystemContext, predefinedNodes[i]);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }
        }
    }
}
