
using Opc.Ua.Export;
using Opc.Ua.Server;
using System.Collections.Generic;
using System.IO;

namespace Opc.Ua.Sample
{
    public class SimpleNodeManager : CustomNodeManager2
    {
        public SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration)
        {
            SystemContext.NodeIdFactory = this;

            List<string> namespaceUris = new List<string>();

            // TODO: Read this from the nodeset file instead of hard-coding
            namespaceUris.Add("http://opcfoundation.org/UA/Station/");

            NamespaceUris = namespaceUris;
            Server.NamespaceUris.GetIndexOrAppend(namespaceUris[0]);

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

                // TODO: Read the filename from user input
                ImportNodeset2Xml(externalReferences, "Station.NodeSet2.xml");

                AddReverseReferences(externalReferences);
            }
        }

        private void ImportNodeset2Xml(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcepath)
        {
            NodeStateCollection predefinedNodes = new NodeStateCollection();

            Stream stream = new FileStream(resourcepath, FileMode.Open);
            UANodeSet nodeSet = UANodeSet.Read(stream);

            foreach (string namespaceUri in nodeSet.NamespaceUris)
            {
                SystemContext.NamespaceUris.GetIndexOrAppend(namespaceUri);
            }
            nodeSet.Import(SystemContext, predefinedNodes);

            for (int i = 0; i < predefinedNodes.Count; i++)
            {
                AddPredefinedNode(SystemContext, predefinedNodes[i]);
            }
        }
    }
}
