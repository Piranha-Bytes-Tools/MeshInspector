using Autodesk.Max;

namespace MeshInspector.Utils
{
    /// <summary>
    /// definitions of important defines from the max sdk
    /// </summary>
    internal static class Globals
    {
        public static readonly int TOPO_CHAN_NUM = 0;
        public static readonly int GEOM_CHAN_NUM = 1;
        public static readonly int TEXMAP_CHAN_NUM = 2;
        public static readonly int MTL_CHAN_NUM = 3;
        public static readonly int SELECT_CHAN_NUM = 4;
        public static readonly int SUBSEL_TYPE_CHAN_NUM = 5;
        public static readonly int DISP_ATTRIB_CHAN_NUM = 6;
        public static readonly int VERT_COLOR_CHAN_NUM = 7;
        public static readonly int GFX_DATA_CHAN_NUM = 8;
        public static readonly int DISP_APPROX_CHAN_NUM = 9;
        public static readonly int EXTENSION_CHAN_NUM = 10;

        public static readonly int TOPO_CHANNEL = (1 << 0);
        public static readonly int GEOM_CHANNEL = (1 << 1);
        public static readonly int TEXMAP_CHANNEL = (1 << 2);
        public static readonly int MTL_CHANNEL = (1 << 3);
        public static readonly int SELECT_CHANNEL = (1 << 4);
        public static readonly int SUBSEL_TYPE_CHANNEL = (1 << 5);
        public static readonly int DISP_ATTRIB_CHANNEL = (1 << 6);
        public static readonly int VERTCOLOR_CHANNEL = (1 << 7);
        public static readonly int GFX_DATA_CHANNEL = (1 << 8);
        public static readonly int DISP_APPROX_CHANNEL = (1 << 9);
        public static readonly int EXTENSION_CHANNEL = (1 << 13);
        public static readonly int TM_CHANNEL = (1 << 10);
        public static readonly int EDGEVISIBLITY_CHANNEL = (1 << 11);
        public static readonly int DONT_RECREATE_TRISTRIP_CHANNEL = (1 << 12);
        public static readonly int GLOBMTL_CHANNEL = (int) (1 << 31);

        public static readonly int OBJ_CHANNELS =
            (Globals.TOPO_CHANNEL
             | Globals.GEOM_CHANNEL
             | Globals.SELECT_CHANNEL
             | Globals.TEXMAP_CHANNEL
             | Globals.MTL_CHANNEL
             | Globals.SUBSEL_TYPE_CHANNEL
             | Globals.DISP_ATTRIB_CHANNEL
             | Globals.VERTCOLOR_CHANNEL
             | Globals.GFX_DATA_CHANNEL
             | Globals.DISP_APPROX_CHANNEL
             | Globals.EXTENSION_CHANNEL
                );

        public static readonly int ALL_CHANNELS = (Globals.OBJ_CHANNELS | Globals.TM_CHANNEL | Globals.GLOBMTL_CHANNEL);

        public static readonly int PART_TOPO = Globals.TOPO_CHANNEL;
        public static readonly int PART_GEOM = Globals.GEOM_CHANNEL;
        public static readonly int PART_TEXMAP = Globals.TEXMAP_CHANNEL;
        public static readonly int PART_MTL = Globals.MTL_CHANNEL;
        public static readonly int PART_SELECT = Globals.SELECT_CHANNEL;
        public static readonly int PART_SUBSEL_TYPE = Globals.SUBSEL_TYPE_CHANNEL;
        public static readonly int PART_DISPLAY = Globals.DISP_ATTRIB_CHANNEL;
        public static readonly int PART_VERTCOLOR = Globals.VERTCOLOR_CHANNEL;
        public static readonly int PART_GFX_DATA = Globals.GFX_DATA_CHANNEL;
        public static readonly int PART_DISP_APPROX = Globals.DISP_APPROX_CHANNEL;
        public static readonly int PART_EXTENSION = Globals.EXTENSION_CHANNEL;
        public static readonly int PART_TM_CHAN = Globals.TM_CHANNEL;
        public static readonly int PART_MTL_CHAN = Globals.GLOBMTL_CHANNEL;
        public static readonly int PART_OBJECT_TYPE = (1 << 11);
        public static readonly int PART_TM = (1 << 12);
        public static readonly int PART_OBJ = (Globals.PART_TOPO | Globals.PART_GEOM);
        public static readonly int PART_ALL = (Globals.ALL_CHANNELS | Globals.PART_TM);

        // this was missing inside managed max sdk but is important for notify dependent
        public static readonly SClass_ID NOTIFY_ALL = (SClass_ID) 0xfffffff0;

        public static readonly uint TIME_NegInfinity = 0x80000000;
        public static readonly uint TIME_PosInfinity = 0x7fffffff;

        // fixed interval settings
        public static readonly IInterval FOREVER = AssemblyFunctions.GlobalInterface.Interval.Create((int) Globals.TIME_NegInfinity, (int) Globals.TIME_PosInfinity);
        public static readonly IInterval NEVER = AssemblyFunctions.GlobalInterface.Interval.Create((int) Globals.TIME_NegInfinity, (int) Globals.TIME_NegInfinity);

        // used for changing the main mode
        public const int TASK_MODE_CREATE = 1;
        public const int TASK_MODE_MODIFY = 2;
        public const int TASK_MODE_HIERARCHY = 3;
        public const int TASK_MODE_MOTION = 4;
        public const int TASK_MODE_DISPLAY = 5;
        public const int TASK_MODE_UTILITY = 6;


        // used for switching the sub selection level
        public const int MESH_SUBLEVEL_OBJECT = 0;
        public const int MESH_SUBLEVEL_VERTEX = 1;
        public const int MESH_SUBLEVEL_EDGE = 2;
        public const int MESH_SUBLEVEL_FACE = 3;
        public const int MESH_SUBLEVEL_POLYGON = 4;
        public const int MESH_SUBLEVEL_ELEMENT = 5;

        // test not working yet
        public static readonly IClass_ID NODEMONITOR_CLASS_ID = AssemblyFunctions.GlobalInterface.Class_ID.Create(0x18f81903, 0x19033fd2);
    }
}
