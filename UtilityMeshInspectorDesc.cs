using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace MeshInspector
{
    /// <summary>
    /// Descriptor class, used internaly inside max for register the class
    /// </summary>
    public class UtilityMeshInspectorDesc : ClassDesc2
    {
        internal static IClass_ID ms_classID;

        #region Properties

        public override bool IsPublic
        {
            get { return true; }
        }

        /// <summary>
        /// the name how the plugin is visualized in the utility list
        /// </summary>
        public override string ClassName
        {
            get { return "Mesh Inspector"; }
        }

        /// <summary>
        /// the super class id
        /// </summary>
        public override SClass_ID SuperClassID
        {
            get { return SClass_ID.Utility; }
        }

        /// <summary>
        /// the class id of the plugin
        /// </summary>
        public override IClass_ID ClassID
        {
            get { return UtilityMeshInspectorDesc.ms_classID; }
        }

        /// <summary>
        /// the categrory where the plugin is ordered by inside the utility list
        /// </summary>
        public override string Category
        {
            get { return "Piranha-Bytes Utilities"; }
        }

        #endregion

        #region Life&Death
        public UtilityMeshInspectorDesc(IGlobal global)
        {
            // create a class id
            // this id must be unique, use the gencid.exe inside maxsdk\help for creating one
            UtilityMeshInspectorDesc.ms_classID = global.Class_ID.Create(0x62067494, 0x259d7195);
        }

        /// <summary>
        /// create a new instance of the plugin
        /// </summary>
        /// <param name="loading"></param>
        /// <returns></returns>
        public override object Create(bool loading)
        {
            return new UtilityMeshInspector();
        }
        #endregion
    }
}
