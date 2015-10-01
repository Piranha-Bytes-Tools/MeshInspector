using System;
using System.Threading.Tasks;
using Autodesk.Max;
using MeshInspector.UI;
using MeshInspector.Utils;

namespace MeshInspector.NodeView
{
    /// <summary>
    /// small static manager for handling the nodeview
    /// </summary>
    internal static class NodeManager
    {
        private static NodeViewCollection ms_collection;

        public static NodeViewCollection CurrentNodes
        {
            get { return NodeManager.ms_collection; }
        }

        public static bool KeepSelectedNodes { get; set; }


        public static void Initialize()
        {
            NodeManager.ms_collection = new NodeViewCollection();
        }

        public static void Deinitialize()
        {
            if (NodeManager.ms_collection != null)
            {
                NodeManager.ms_collection.Clear();
                NodeManager.ms_collection = null;
            }
        }

        public static void Reset()
        {
            if (NodeManager.ms_collection != null)
                NodeManager.ms_collection.Clear();
        }

        /// <summary>
        /// get the current max selection
        /// </summary>
        public static void GetMaxNodeSelection()
        {
            // check if the checkbox in the UI for tracking the scene sleection is checked
            if (NodeManager.KeepSelectedNodes)
                return;

            // create array for all selected nodes
            IINode[] arrNodes = new IINode[AssemblyFunctions.Core.SelNodeCount];

            // retreive the INode from the max sdk Core interface
            for (int i = 0; i < arrNodes.Length; i++)
                arrNodes[i] = AssemblyFunctions.Core.GetSelNode(i);

            // create a nodeview for the nodes
            NodeManager.SetIINodes(arrNodes);

            MainWindow.Instance.CanInspect = NodeManager.CurrentNodes.Count > 0;
        }

        public static bool IsSingleNodeSelected(NodeView nodeView)
        {
            return AssemblyFunctions.Core.SelNodeCount == 1 && nodeView.Equals(AssemblyFunctions.Core.GetSelNode(0));
        }

        public static void SetIINodes(params IINode[] nodes)
        {
            if (NodeManager.KeepSelectedNodes)
                return;

            NodeManager.ms_collection.Clear();
            NodeManager.ms_collection.AddINodes(nodes);
        }

        /// <summary>
        /// inspect all nodeviews
        /// </summary>
        /// <param name="nodeView"></param>
        /// <returns></returns>
        public static async Task InpsectMeshes(params NodeView[] nodeView)
        {
            // disable the max scene redraw and set to expert mode
            using (new StopSceneRedraw())
            {
                for (int i = 0; i < nodeView.Length; i++)
                {
                    nodeView[i].ContainsErrors = false;
                    nodeView[i].InspectionProgress = 0;
                }

                for (int i = 0; i < nodeView.Length; i++)
                    await nodeView[i].InspectMesh();
            }
        }

        /// <summary>
        /// handle max scene nodes delete
        /// </summary>
        /// <param name="nodes"></param>
        public static void SceneNodesDeleted(ITab <IINode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
                NodeManager.ms_collection.TryDeleteNode(nodes[new IntPtr(i)]);

            MainWindow.Instance.CanInspect = NodeManager.CurrentNodes.Count > 0;
        }

        /// <summary>
        /// handle max scene node delete
        /// </summary>
        /// <param name="node"></param>
        public static void SceneNodeDeleted(IINode node)
        {
            NodeManager.ms_collection.TryDeleteNode(node);

            MainWindow.Instance.CanInspect = NodeManager.CurrentNodes.Count > 0;
        }
    }
}
