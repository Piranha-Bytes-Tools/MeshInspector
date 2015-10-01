using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Autodesk.Max;
using Autodesk.Max.MaxSDK.Util;
using MahApps.Metro.Controls.Dialogs;
using MeshInspector.Properties;
using MeshInspector.UI;
using MeshInspector.Utils;

namespace MeshInspector.NodeView
{
    /// <summary>
    /// wrapper around iinode
    /// properties will be presented inside the datagrid
    /// </summary>
    public class NodeView : BindableObject, IEquatable <INoncopyable>, IDisposable
    {
        #region Fields

        private IINode m_node;
        private IMesh m_triMesh;
        private IObject m_baseObject;

        private int m_iNumVerts;
        private int m_iNumTris;

        private float m_inspectionProgress;

        private bool m_containsErrors;

        private bool m_containsDegTriErrors;
        private bool m_containsBadSMGErrors;
        private bool m_containsDegUVErrors;
        private bool m_containsMiscErrors;


        private int m_iDegTriIndex;
        private int m_iBadSMGTriIndex;
        private int m_iDegUVTriIndex;

        private int m_iNumIsoVerts;
        private int m_iNumIsoUVVerts;
        private int m_iNumWrongMatIds;

        private bool m_bInspectionRunning;

        private HashSet <int> m_hashDegTris;
        private HashSet <int> m_hashBadSMGTris;
        private HashSet <int> m_hashDegUVTris;

        private static Dispatcher ms_dispatcher;

        #endregion

        #region properties 

        [Browsable(false)]
        public IINode Node
        {
            get { return this.m_node; }
        }

        [Browsable(false)]
        public IMesh Mesh
        {
            get { return this.m_triMesh; }
        }

        [Browsable(false)]
        public IObject BaseObject
        {
            get { return this.m_baseObject; }
        }

        [Browsable(false)]
        public bool IsValid
        {
            get { return this.m_node != null; }
        }

        [Browsable(false)]
        public bool IsValidWorkingNode
        {
            get
            {
                return this.m_node != null
                       && this.m_baseObject != null
                       && this.m_triMesh != null;
            }
        }

        [Browsable(false)]
        public bool InspectionRunning
        {
            get { return this.m_bInspectionRunning; }
            set
            {
                if (this.m_bInspectionRunning == value)
                    return;

                this.m_bInspectionRunning = value;
                this.OnPropertyChanged(() => this.InspectionRunning);
            }
        }

        public bool ContainsErrors
        {
            get { return this.m_containsErrors; }
            set
            {
                if (this.m_containsErrors == value)
                    return;

                this.m_containsErrors = value;
                this.OnPropertyChanged(() => this.ContainsErrors);
            }
        }

        [Browsable(false)]
        public bool ContainsDegTriErrors
        {
            get { return this.m_containsDegTriErrors; }
            set
            {
                if (this.m_containsDegTriErrors == value)
                    return;

                this.m_containsDegTriErrors = value;
                this.OnPropertyChanged(() => this.ContainsDegTriErrors);

                if (this.m_containsDegTriErrors)
                    this.ContainsErrors = true;
            }
        }

        [Browsable(false)]
        public bool ContainsBadSMGErrors
        {
            get { return this.m_containsBadSMGErrors; }
            set
            {
                if (this.m_containsBadSMGErrors == value)
                    return;

                this.m_containsBadSMGErrors = value;
                this.OnPropertyChanged(() => this.ContainsBadSMGErrors);

                if (this.m_containsBadSMGErrors)
                    this.ContainsErrors = true;
            }
        }

        [Browsable(false)]
        public bool ContainsDegUvErrors
        {
            get { return this.m_containsDegUVErrors; }
            set
            {
                if (this.m_containsDegTriErrors == value)
                    return;

                this.m_containsDegUVErrors = value;
                this.OnPropertyChanged(() => this.ContainsDegUvErrors);

                if (this.m_containsDegUVErrors)
                    this.ContainsErrors = true;
            }
        }

        [Browsable(false)]
        public bool ContainsMiscErrors
        {
            get { return this.m_containsMiscErrors; }
            set
            {
                if (this.m_containsMiscErrors == value)
                    return;

                this.m_containsMiscErrors = value;
                this.OnPropertyChanged(() => this.ContainsMiscErrors);

                if (this.m_containsMiscErrors)
                    this.ContainsErrors = true;
            }
        }

        public HashSet <int> HashDegTris
        {
            get { return this.m_hashDegTris; }
        }

        public HashSet <int> HashBadSMGTris
        {
            get { return this.m_hashBadSMGTris; }
        }

        public HashSet <int> HashDegUVTris
        {
            get { return this.m_hashDegUVTris; }
        }

        public string Name
        {
            get { return this.m_node.NodeName; }
        }

        public int NumVerts
        {
            get { return this.m_iNumVerts; }
        }

        public int NumTris
        {
            get { return this.m_iNumTris; }
        }

        public int ChildCount
        {
            get { return this.m_node.NumberOfChildren; }
        }

        [Browsable(false)]
        public int DegTriCount
        {
            get { return this.m_hashDegTris.Count; }
        }

        [Browsable(false)]
        public int BadSMGTriCount
        {
            get { return this.m_hashBadSMGTris.Count; }
        }

        [Browsable(false)]
        public int DegUVTriCount
        {
            get { return this.m_hashDegUVTris.Count; }
        }

        [Browsable(false)]
        public int IsoVertCount
        {
            get { return this.m_iNumIsoVerts; }
            set
            {
                if (this.m_iNumIsoVerts == value)
                    return;

                this.m_iNumIsoVerts = value;
                this.OnPropertyChanged(() => this.IsoVertCount);

                if (this.IsoVertCount > 0)
                    this.ContainsMiscErrors = true;
            }
        }

        [Browsable(false)]
        public int IsoUVVertCount
        {
            get { return this.m_iNumIsoUVVerts; }
            set
            {
                if (this.m_iNumIsoUVVerts == value)
                    return;

                this.m_iNumIsoUVVerts = value;
                this.OnPropertyChanged(() => this.IsoUVVertCount);

                if (this.IsoUVVertCount > 0)
                    this.ContainsMiscErrors = true;
            }
        }

        [Browsable(false)]
        public int WrongMatIDCount
        {
            get { return this.m_iNumWrongMatIds; }
            set
            {
                if (this.m_iNumWrongMatIds == value)
                    return;

                this.m_iNumWrongMatIds = value;
                this.OnPropertyChanged(() => this.WrongMatIDCount);

                if (this.WrongMatIDCount > 0)
                    this.ContainsMiscErrors = true;
            }
        }



        public float InspectionProgress
        {
            get { return this.m_inspectionProgress; }
            set
            {
                if (Math.Abs(this.m_inspectionProgress - value) < float.Epsilon)
                    return;

                this.m_inspectionProgress = value;
                this.OnPropertyChanged(() => this.InspectionProgress);
            }
        }

        #endregion

        #region Life&Death

        public NodeView(IINode node)
        {
            this.m_node = node;

            this.m_hashDegTris = new HashSet <int>();
            this.m_hashBadSMGTris = new HashSet <int>();
            this.m_hashDegUVTris = new HashSet <int>();

            this.CollectTriMeshData();

            this.InspectionProgress = 0;
            this.InspectionRunning = false;

            if (NodeView.ms_dispatcher == null)
                NodeView.ms_dispatcher = MainWindow.Instance.Dispatcher;
        }

        #region IDisposable

        private bool m_bDisposed;

        public bool IsDisposed
        {
            get { return this.m_bDisposed; }
        }

        /// <summary>
        /// dispose the node
        /// </summary>
        /// <param name="bDisposing"></param>
        private void Dispose(bool bDisposing)
        {
            if (this.m_bDisposed)
                return;

            this.m_node = null;
            this.m_triMesh = null;
            this.m_baseObject = null;

            if (this.m_hashDegUVTris != null)
            {
                this.m_hashDegUVTris.Clear();
                this.m_hashDegUVTris = null;
            }

            if (this.m_hashBadSMGTris != null)
            {
                this.m_hashBadSMGTris.Clear();
                this.m_hashBadSMGTris = null;
            }

            if (this.m_hashDegTris != null)
            {
                this.m_hashDegTris.Clear();
                this.m_hashDegTris = null;
            }

            this.m_containsErrors = false;
            this.m_containsBadSMGErrors = false;
            this.m_containsDegTriErrors = false;
            this.m_containsDegUVErrors = false;
            this.m_containsMiscErrors = false;

            this.m_bInspectionRunning = false;

            this.m_iDegUVTriIndex = -1;
            this.m_iBadSMGTriIndex = -1;
            this.m_iDegTriIndex = -1;

            this.m_iNumTris = -1;
            this.m_iNumVerts = -1;

            this.m_iNumIsoVerts = -1;
            this.m_iNumIsoUVVerts = -1;
            this.m_iNumWrongMatIds = -1;

            this.m_inspectionProgress = 0;

            if (bDisposing)
            {

            }

            this.m_bDisposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        /// <summary>
        /// collect and fill the property base on the trimesh
        /// </summary>
        public void CollectTriMeshData()
        {
            if (!this.IsValid)
                return;

            // get the current node world state
            IObjectState state = this.m_node.EvalWorldState(AssemblyFunctions.Core.Time, false);

            // get the object out of the state and check if it's possible to convert to editable mesh
            IObject obj = state.Obj;
            if (obj.CanConvertToType(AssemblyFunctions.GlobalInterface.TriObjectClassID) == 0)
                return;

            // convert the obj to triobject
            // if the node type was editable mesh, tit will return just the current node mesh
            // elsewhere it will return a new triobj instance which is not bound to anything
            ITriObject tri = obj.ConvertToType(AssemblyFunctions.Core.Time, AssemblyFunctions.GlobalInterface.TriObjectClassID) as ITriObject;

            if (tri == null)
                return;

            this.m_iNumVerts = tri.Mesh.NumVerts;
            this.m_iNumTris = tri.Mesh.NumFaces;

            // check if triobj is identical with the object from the node
            // if its not the same we got a new mesh so delete it
            if (!tri.Equals((INoncopyable) obj))
                tri.DeleteMe();
        }

        /// <summary>
        /// convert the actual node to editable mesh
        /// </summary>
        public void ConvertToMesh()
        {
            if (!this.IsValid)
                return;

            // get the current node world state
            IObjectState state = this.m_node.EvalWorldState(AssemblyFunctions.Core.Time, false);

            // get the object out of the state and check if it's possible to convert to editable mesh
            IObject obj = state.Obj;
            if (obj.CanConvertToType(AssemblyFunctions.GlobalInterface.TriObjectClassID) == 0)
                return;

            // convert the obj to triobject
            // if the node type was editable mesh, tit will return just the current node mesh
            // elsewhere it will return a new triobj instance which is not bound to anything
            ITriObject tri = obj.ConvertToType(AssemblyFunctions.Core.Time, AssemblyFunctions.GlobalInterface.TriObjectClassID) as ITriObject;

            // check if we got something
            if (tri == null)
                return;

            // remeber the obj and mesh for later use
            this.m_triMesh = tri.Mesh;
            this.m_baseObject = obj;

            // check if the tri is identical with the current object from the node state
            // if the node was already editable mesh we don't need to do anything
            if (this.m_triMesh == null || tri.Equals((INoncopyable)obj))
                return;

            // notfiy all dependents
            // this notfiy was from a max sample. not sure why call first before change and call change afterwards
            this.m_node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);
            
            // the the object reference of the node to the new triobj
            // important: if you exchange the object ref ith the same objref
            // like this.m_node.ObjectRef = this.m_node.ObjectRef you will produce a nice crash sometimes
            // the property setter inside max.net will delete the old reference and since the old is the new
            // the reference will be set to null which is illegal
            this.m_node.ObjectRef = tri;

            // notfiy all dependents
            this.m_node.NotifyDependents(Globals.FOREVER, 0, RefMessage.SubanimStructureChanged);
            this.m_node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);
        }

        /// <summary>
        /// reset the states
        /// </summary>
        public void Reset()
        {
            this.InspectionProgress = 0.0f;

            this.m_hashDegTris.Clear();
            this.m_hashBadSMGTris.Clear();
            this.m_hashDegUVTris.Clear();

            this.ContainsErrors = false;
            this.ContainsBadSMGErrors = false;
            this.ContainsDegTriErrors = false;
            this.ContainsDegUvErrors = false;

            this.m_iBadSMGTriIndex = 0;
            this.m_iDegTriIndex = 0;
            this.m_iDegUVTriIndex = 0;

            this.ResetMiscErrors();
        }

        /// <summary>
        /// reset misc errors ( isolated and matid
        /// </summary>
        public void ResetMiscErrors()
        {
            this.ContainsMiscErrors = false;

            this.IsoVertCount = 0;
            this.IsoUVVertCount = 0;
            this.WrongMatIDCount = 0;
        }

        /// <summary>
        /// start the mesh inspection
        /// </summary>
        /// <returns></returns>
        public async Task InspectMesh()
        {
            // select the current node inside the datagrid 
            MainWindow.Instance.SetRowSelection(this);

            this.InspectionRunning = true;

            this.Reset();

            string strErrorString = string.Empty;
            bool bFaulted = false;

            try
            {
                this.ConvertToMesh();

                await NodeAnalyzer.Analyze(this);
            }
            catch (Exception e)
            {
                bFaulted = true;
                strErrorString = string.Format("Current Node caused an excpetion and will be skipped. \n {0}", e.InnerException.Message);
            }

            if (bFaulted)
                await MainWindow.Instance.ShowMessageAsync("ERROR", strErrorString);

            // update the ui with the new possible changes
            await NodeView.ms_dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.ContainsErrors)
                {
                    this.OnPropertyChanged(() => this.DegTriCount);
                    this.OnPropertyChanged(() => this.BadSMGTriCount);
                    this.OnPropertyChanged(() => this.DegUVTriCount);
                }
            }), DispatcherPriority.Loaded);

            this.InspectionProgress = 100;

            this.InspectionRunning = false;
        }

        #region IEquatable<INoncopyable> Interface

        public bool Equals(INoncopyable other)
        {
            return this.IsValid && this.m_node.Equals(other);
        }

        #endregion

        #region Error Selection

        /// <summary>
        /// set a face selection on the current node
        /// </summary>
        /// <param name="selection"></param>
        private void SetFaceSelection(IBitArray selection)
        {
            // disbale scene redraw but dont turn on expert mode
            using (new StopSceneRedraw(false))
            {
                if (!this.IsValidWorkingNode)
                    return;

                // switch the main max mode to modify
                if (AssemblyFunctions.Core.CommandPanelTaskMode != Globals.TASK_MODE_MODIFY)
                    AssemblyFunctions.Core.CommandPanelTaskMode = Globals.TASK_MODE_MODIFY;

                // select the node
                this.SetNodeSelection();

                // clear the current active selection
                this.ClearSelection();

                // set the current subobjlevel to face
                AssemblyFunctions.Core.SetSubObjectLevel(Globals.MESH_SUBLEVEL_FACE, true);
                this.m_triMesh.FaceSel = selection;

                // notify all node dependents that there was a change
                this.m_node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);

                // zoom to selection
                if (Settings.Default.ErrorZoomExtends)
                    AssemblyFunctions.Core.ViewportZoomExtents(true, false);
            }
        }

        /// <summary>
        /// select a node inside the max scene
        /// if the node is already selected nothing will happen
        /// </summary>
        public void SetNodeSelection()
        {
            if (!NodeManager.IsSingleNodeSelected(this))
            {
                // if the last parameter is false, you can add a node to the current slected node
                AssemblyFunctions.Core.SelectNode(this.m_node, true);
            }
        }

        /// <summary>
        /// clear the current face selection
        /// </summary>
        public void ClearSelection()
        {
            // create a new bitarray size of the tri faces
            // since they all are initialized as notset its an empty selection
            using (IBitArray faceSelection = AssemblyFunctions.GlobalInterface.BitArray.Create(this.NumTris))
            {
                // set subobjlevel to object
                // if you keep the current level, the new selection will not be visible
                // maybe a missing notifiy command in max.net or my side
                AssemblyFunctions.Core.SetSubObjectLevel(Globals.MESH_SUBLEVEL_OBJECT, true);

                // set the face selection to the empty bitarray
                this.m_triMesh.FaceSel = faceSelection;

                // notify all node dependents that there was a change
                this.m_node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);
            }
        }

        public void SelectAllDegeneratedTris()
        {
            using (IBitArray selectionSet = this.m_hashDegTris.SelectionAllFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        public void SelectFirstDegeneratedTri()
        {
            using (IBitArray selectionSet = this.m_hashDegTris.SelectionFirstFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);

            this.m_iDegTriIndex = 0;
        }

        public void SelectNextDegeneratedTri()
        {
            this.m_iDegTriIndex++;

            if (this.m_iDegTriIndex >= this.m_hashDegTris.Count)
                this.m_iDegTriIndex = 0;

            using (IBitArray selectionSet = this.m_hashDegTris.SelectionNextFromHashSet(this.m_iDegTriIndex, this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        public void SelectAllBadSMGTris()
        {
            using (IBitArray selectionSet = this.m_hashBadSMGTris.SelectionAllFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        public void SelectFirstBadSMGTris()
        {
            using (IBitArray selectionSet = this.m_hashBadSMGTris.SelectionFirstFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);

            this.m_iBadSMGTriIndex = 0;
        }

        public void SelectNextBadSMGTris()
        {
            this.m_iBadSMGTriIndex++;

            if (this.m_iBadSMGTriIndex >= this.m_hashBadSMGTris.Count)
                this.m_iBadSMGTriIndex = 0;

            using (IBitArray selectionSet = this.m_hashBadSMGTris.SelectionNextFromHashSet(this.m_iBadSMGTriIndex, this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        public void SelectAllDegeneratedUVTris()
        {
            using (IBitArray selectionSet = this.m_hashDegUVTris.SelectionAllFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        public void SelectFirstDegeneratedUVTri()
        {
            using (IBitArray selectionSet = this.m_hashDegUVTris.SelectionFirstFromHashSet(this.NumTris))
                this.SetFaceSelection(selectionSet);

            this.m_iDegUVTriIndex = 0;
        }

        public void SelectNextDegeneratedUVTri()
        {
            this.m_iDegUVTriIndex++;

            if (this.m_iDegUVTriIndex >= this.m_hashDegUVTris.Count)
                this.m_iDegUVTriIndex = 0;

            using (IBitArray selectionSet = this.m_hashDegUVTris.SelectionNextFromHashSet(this.m_iDegUVTriIndex, this.NumTris))
                this.SetFaceSelection(selectionSet);
        }

        #endregion
    }
}
