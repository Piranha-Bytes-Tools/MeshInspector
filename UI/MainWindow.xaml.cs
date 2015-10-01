using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Autodesk.Max;
using MahApps.Metro;
using MeshInspector.NodeView;
using MeshInspector.Properties;
using MeshInspector.Utils;
using Xceed.Wpf.DataGrid;

namespace MeshInspector.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Singelton

        private static MainWindow ms_instance;

        public static MainWindow Instance
        {
            get { return MainWindow.ms_instance; }
        }

        #endregion

        /// <summary>
        /// delegates for scene events
        /// </summary>
        private readonly GlobalDelegates.Delegate5 m_delSelectionSetChanged;

        private readonly GlobalDelegates.Delegate5 m_delSelectedNodeRenamed;
        private readonly GlobalDelegates.Delegate5 m_delNodeDeleted;
        private readonly GlobalDelegates.Delegate5 m_delSceneReset;

        private readonly Settings m_settings = new Settings();

        /// <summary>
        /// if yes the tool wont track the current scene selection anymore
        /// </summary>
        public bool KeepSelectedNodes
        {
            get { return NodeManager.KeepSelectedNodes; }
            set
            {
                NodeManager.KeepSelectedNodes = value;
                this.OnPropertyChanged(() => this.KeepSelectedNodes);

                if (NodeManager.KeepSelectedNodes)
                    return;

                this.SortByName();
                NodeManager.GetMaxNodeSelection();
            }
        }

        private bool m_bCanInspect;

        public bool CanInspect
        {
            get { return this.m_bCanInspect; }
            set
            {
                this.m_bCanInspect = value;
                this.OnPropertyChanged(() => this.CanInspect);
            }
        }

        #region Life&Death

        public MainWindow()
        {
            this.InitializeComponent();

            if (Application.Current != null)
                Application.Current.DispatcherUnhandledException -= AssemblyFunctions.Current_DispatcherUnhandledException;

            ThemeManager.AddAccent("Dark", new Uri("pack://application:,,,/MeshInspector;component/Resources/Globals.xaml"));

            Accent accent = ThemeManager.GetAccent("Dark");
            AppTheme appTheme = ThemeManager.GetAppTheme("BaseDark");

            ThemeManager.ChangeAppStyle(this, accent, appTheme);

            MainWindow.ms_instance = this;
            NodeManager.Initialize();

            this.m_dataGrid.ItemsSource = NodeManager.CurrentNodes;

            // attach the delegates to methods
            this.m_delSelectionSetChanged = this.OnSelectionChanged;
            this.m_delSelectedNodeRenamed = this.OnNodeRenamed;
            this.m_delNodeDeleted = this.OnNodeDeleted;
            this.m_delSceneReset = this.OnSceneReset;

            this.RegisterNotifications(true);
            this.HandleEvents(true);

            this.m_groupSceneUtils.DataContext = this;
            this.KeepSelectedNodes = false;

            this.SortByName();

            this.CanInspect = false;
        }

        #region Events&Notifications

        private void HandleEvents(bool attach)
        {
            if (attach)
            {

            }
            else
            {

            }
        }

        /// <summary>
        /// register and unregister max notifications
        /// </summary>
        /// <param name="attach"></param>
        public void RegisterNotifications(bool attach)
        {
            if (attach)
            {
                AssemblyFunctions.GlobalInterface.RegisterNotification(this.m_delSelectionSetChanged, null, SystemNotificationCode.SelectionsetChanged);
                AssemblyFunctions.GlobalInterface.RegisterNotification(this.m_delSelectedNodeRenamed, null, SystemNotificationCode.NodeRenamed);
                AssemblyFunctions.GlobalInterface.RegisterNotification(this.m_delNodeDeleted, null, SystemNotificationCode.ScenePreDeletedNode);
                AssemblyFunctions.GlobalInterface.RegisterNotification(this.m_delSceneReset, null, SystemNotificationCode.PostSceneReset);
            }
            else
            {
                AssemblyFunctions.GlobalInterface.UnRegisterNotification(this.m_delSelectionSetChanged, null, SystemNotificationCode.SelectionsetChanged);
                AssemblyFunctions.GlobalInterface.UnRegisterNotification(this.m_delSelectedNodeRenamed, null, SystemNotificationCode.NodeRenamed);
                AssemblyFunctions.GlobalInterface.UnRegisterNotification(this.m_delNodeDeleted, null, SystemNotificationCode.ScenePreDeletedNode);
                AssemblyFunctions.GlobalInterface.UnRegisterNotification(this.m_delSceneReset, null, SystemNotificationCode.PostSceneReset);
            }
        }

        #endregion

        /// <summary>
        /// helper for retreiving resources from the resource stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindResource <T>(string name) where T : class
        {
            if (MainWindow.Instance == null)
                return default(T);

            return MainWindow.Instance.FindResource(name) as T;
        }

        /// <summary>
        /// will be called when the widnow is initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainWindowInitialized(object sender, EventArgs e)
        {
            Settings currentSettings = Settings.Default;

            if (!(currentSettings.Width >= 0) || !(currentSettings.Height >= 0) || !(currentSettings.Top >= 0) || !(currentSettings.Left >= 0))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                currentSettings.Width = 1280;
                currentSettings.Height = 720;

                return;
            }

            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        /// <summary>
        /// will be called when the window is loaded and will be presented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.Maximized)
                this.WindowState = WindowState.Maximized;

            this.GetCurrentSelection();
        }

        /// <summary>
        /// will be called when the state change
        /// minimize, maximize, normal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainWindowStateChanged(object sender, EventArgs e)
        {
            this.m_settings.Width = Settings.Default.Width;
            this.m_settings.Height = Settings.Default.Height;
            this.m_settings.Left = Settings.Default.Left;
            this.m_settings.Top = Settings.Default.Top;
        }

        /// <summary>
        /// shutdown of the window event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            // tell the nodemanager to remove all node views
            NodeManager.Deinitialize();

            // remove all events and unregister max notifications
            this.HandleEvents(false);
            this.RegisterNotifications(false);

            // save the current settings
            Settings.Default.Maximized = this.WindowState == WindowState.Maximized;
            if (this.WindowState != WindowState.Normal)
            {
                Settings.Default.Width = this.m_settings.Width;
                Settings.Default.Height = this.m_settings.Height;
                Settings.Default.Left = this.m_settings.Left;
                Settings.Default.Top = this.m_settings.Top;
            }

            Settings.Default.Save();

            MainWindow.ms_instance = null;
        }

        #endregion

        private void SortByName()
        {
            this.m_dataGrid.Items.SortDescriptions.Clear();
            this.m_dataGrid.Items.SortDescriptions.Add(new SortDescription(this.m_dataGrid.Columns[0].FieldName, ListSortDirection.Ascending));
        }

        private void SortByErrors()
        {
            this.m_dataGrid.Items.SortDescriptions.Clear();
            this.m_dataGrid.Items.SortDescriptions.Add(new SortDescription(this.m_dataGrid.Columns[1].FieldName, ListSortDirection.Descending));
            this.m_dataGrid.Items.SortDescriptions.Add(new SortDescription(this.m_dataGrid.Columns[0].FieldName, ListSortDirection.Ascending));
        }

        #region Max Scene Notifications

        public void OnSelectionChanged(IntPtr Obj, IntPtr Info)
        {
            this.GetCurrentSelection();
        }

        public void GetCurrentSelection()
        {
            NodeManager.GetMaxNodeSelection();
        }

        public void OnNodeRenamed(IntPtr Obj, IntPtr Info)
        {

        }

        private void OnNodeDeleted(IntPtr Obj, IntPtr Info)
        {
            // marshal the info ptr to notify info
            INotifyInfo notifyInfo = AssemblyFunctions.GlobalInterface.NotifyInfo.Marshal(Info);
            if (notifyInfo == null)
                return;

            // check if the params are a list of nodes
            // ITab is the list handling inside max sdk
            ITab <IINode> nodes = notifyInfo.CallParam as ITab <IINode>;
            if (nodes != null)
            {
                NodeManager.SceneNodesDeleted(nodes);
                return;
            }

            // check if the params are one iinode
            IINode node = notifyInfo.CallParam as IINode;
            if (node != null)
                NodeManager.SceneNodeDeleted(node);
        }

        private void OnSceneReset(IntPtr param0, IntPtr param1)
        {
            // tell the nodemanager to reset all data
            NodeManager.Reset();
            this.KeepSelectedNodes = false;
            this.SortByName();
        }

        #endregion

        /// <summary>
        /// inspect the current nodeview items inside the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnInspectMeshes(object sender, RoutedEventArgs e)
        {
            if (NodeManager.CurrentNodes.Count <= 0)
                return;

            this.CanInspect = false;

            await NodeManager.InpsectMeshes(this.m_dataGrid.Items.OfType <NodeView.NodeView>().ToArray());

            this.KeepSelectedNodes = true;

            // sort by error and name at the end
            // that way nodes with errors are top
            this.SortByErrors();
            this.SetRowSelection(this.m_dataGrid.Items.OfType <NodeView.NodeView>().FirstOrDefault());

            this.CanInspect = true;
        }

        private void OnNodeSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            this.m_errorGroup.SelectedNode = this.m_dataGrid.SelectedItem as NodeView.NodeView;
        }

        /// <summary>
        /// select a max scene node when a node is selected inside the datagrid
        /// will only select if track max selection is disabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRowSelected(object sender, MouseButtonEventArgs e)
        {
            using (new StopSceneRedraw(false))
            {
                if (!this.KeepSelectedNodes)
                    return;

                DataRow row = sender as DataRow;
                if (row == null)
                    return;

                NodeView.NodeView viewNode = row.DataContext as NodeView.NodeView;

                if (viewNode != null)
                    viewNode.SetNodeSelection();

                // zoom to the current selection
                if (Settings.Default.ErrorZoomExtends)
                    AssemblyFunctions.Core.ViewportZoomExtents(true, false);
            }
        }

        private NodeView.NodeView m_nvCurrent;

        /// <summary>
        /// select one nodeview inside the datagrid
        /// </summary>
        /// <param name="node"></param>
        public void SetRowSelection(NodeView.NodeView node)
        {
            if (node == null)
                return;

            this.m_nvCurrent = node;

            // this is the same as the winforms on application idle event
            // the row selection is also done inside the mesh inspection task
            // via this pattern it will not spam the selection with plenty of selection invokes
            ComponentDispatcher.ThreadIdle -= this.SetRowOnThreadIdle;
            ComponentDispatcher.ThreadIdle += this.SetRowOnThreadIdle;
        }


        /// <summary>
        /// do the actual selection inside the datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetRowOnThreadIdle(object sender, EventArgs e)
        {
            NodeView.NodeView nodeToSelect = this.m_nvCurrent;

            if (nodeToSelect != null)
            {
                this.Dispatcher.BeginInvoke((Action) (() => { this.m_dataGrid.CurrentItem = nodeToSelect; }), DispatcherPriority.Loaded, null);
                this.Dispatcher.BeginInvoke((Action) (() => { this.m_dataGrid.SelectedItem = nodeToSelect; }), DispatcherPriority.Loaded, null);
            }

            ComponentDispatcher.ThreadIdle -= this.SetRowOnThreadIdle;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;

        protected void OnPropertyChanged <TPropertyType>(Expression <Func <TPropertyType>> propertyExpr)
        {
            string propertyName = Extensions.GetPropertySymbol(propertyExpr);

            if (this.currentDispatcher.CheckAccess())
                this.OnPropertyChanged(propertyName);
            else
                this.currentDispatcher.BeginInvoke(new Action <string>(this.OnPropertyChanged), propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
