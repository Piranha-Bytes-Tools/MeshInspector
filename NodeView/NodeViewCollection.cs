using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.Max;

namespace MeshInspector.NodeView
{
    /// <summary>
    /// the node view collection which is presented inside the datagrid
    /// </summary>
    public class NodeViewCollection : ObservableCollection <NodeView>
    {
        // map to find already created nodeview 
        private readonly Dictionary<IINode, NodeView> m_MapNodes = new Dictionary<IINode, NodeView>();
        
        /// <summary>
        /// create a new nodeview with iinodes 
        /// </summary>
        /// <param name="nodes"></param>
        public void AddINodes(params IINode[] nodes)
        {
            NodeView newNode;
            for (int i = 0; i < nodes.Length; i++)
            {
                newNode = new NodeView(nodes[i]);
                this.m_MapNodes[nodes[i]] = newNode;
                this.Add(newNode);
            }
        }

        /// <summary>
        /// clear all nodes from the collection and call dispose on all items
        /// that way all events and datas are handled correctly
        /// </summary>
        public new void Clear()
        {
            this.m_MapNodes.Clear();

            for(int i=0;i<this.Count;i++)
                this[i].Dispose();

            base.Clear();
        }


        /// <summary>
        /// delete a nodeview and dispose it
        /// </summary>
        /// <param name="maxNode"></param>
        public void TryDeleteNode(IINode maxNode)
        {
            NodeView nodeToDelete;
            if (!this.m_MapNodes.TryGetValue(maxNode, out nodeToDelete))
                return;

            this.m_MapNodes.Remove(maxNode);
            this.Remove(nodeToDelete);
            nodeToDelete.Dispose();
        }
    }
}
