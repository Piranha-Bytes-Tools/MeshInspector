using System.Windows;
using MeshInspector.NodeView;

namespace MeshInspector.UI
{
    /// <summary>
    /// Interaction logic for ErrorGroup.xaml
    /// </summary>
    public partial class ErrorGroup
    {
        private NodeView.NodeView m_selectedNode;

        public NodeView.NodeView SelectedNode
        {
            get { return this.m_selectedNode; }
            set
            {
                this.DataContext = null;

                this.m_selectedNode = value;

                if (this.m_selectedNode != null)
                    this.DataContext = this.m_selectedNode;

                this.m_gridExpanderContent.IsEnabled = this.m_selectedNode != null;
            }
        }

        public ErrorGroup()
        {
            this.InitializeComponent();
            this.SelectedNode = null;
        }

        private void OnSelectFirstDegTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegTriCount > 0)
                this.SelectedNode.SelectFirstDegeneratedTri();
        }

        private void OnSelectNextDegTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegTriCount > 0)
                this.SelectedNode.SelectNextDegeneratedTri();
        }

        private void OnSelectAllDegTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegTriCount > 0)
                this.SelectedNode.SelectAllDegeneratedTris();
        }

        private void OnSelectFirstBadSMGTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.BadSMGTriCount > 0)
                this.SelectedNode.SelectFirstBadSMGTris();
        }

        private void OnSelectNextBadSMGTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.BadSMGTriCount > 0)
                this.SelectedNode.SelectNextBadSMGTris();
        }

        private void OnSelectAllBadSMGTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.BadSMGTriCount > 0)
                this.SelectedNode.SelectAllBadSMGTris();
        }

        private void OnSelectFirstDegUVTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegUVTriCount > 0)
                this.SelectedNode.SelectFirstDegeneratedUVTri();
        }

        private void OnSelectNextDegUVTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegUVTriCount > 0)
                this.SelectedNode.SelectNextDegeneratedUVTri();
        }

        private void OnSelectAllDegUVTri(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.DegUVTriCount > 0)
                this.SelectedNode.SelectAllDegeneratedUVTris();
        }

        private void OnFixMisc(object sender, RoutedEventArgs e)
        {
            if (this.SelectedNode == null)
                return;

            if (this.SelectedNode.ContainsMiscErrors)
                NodeAnalyzer.CleanMesh(this.SelectedNode);
        }
    }
}
