using System;
using System.ComponentModel;
using Autodesk.Max;
using Autodesk.Max.Plugins;
using ManagedServices;
using MeshInspector.UI;

namespace MeshInspector
{
    /// <summary>
    /// The utility class
    /// implementing IPlugin interface is a must have. Dependant assemblies won't load correctly otherwise
    /// </summary>
    public class UtilityMeshInspector : UtilityObj, IPlugin
    {
        #region Abstract UtilityObj overrides

        /// <summary>
        /// This will be called when the utility plugin is activated
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="iu"></param>
        public override void BeginEditParams(IInterface ip, IIUtil iu)
        {
            //the window will stay open regardless if you close the utility plugin
            //if a running instance of the mianwindow is found it will just do nothing
            //else it will create a window and show it
            if (MainWindow.Instance != null)
                return;

            // attach the window to the window handling of max
            // that way the window itself wont loose its order
            // but now it wont go to system tray if you minimize it
            MainWindow window = new MainWindow();
            AppSDK.ConfigureWindowForMax(window);

            window.Show();
        }

        /// <summary>
        /// This will be called when the utility plugin is closed.
        /// will also be called if you switch the main mod pannel (create, edit etc.)
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="iu"></param>
        public override void EndEditParams(IInterface ip, IIUtil iu)
        {

        }

        #endregion

        #region IPlugin Interface
        // Those methods can be empty.
        // You just need the plugin interface to load dependant assemblies correctly
        public void Initialize(IGlobal global, ISynchronizeInvoke sync)
        {
            AppDomain.CurrentDomain.UnhandledException += AssemblyFunctions.CurrentDomain_UnhandledException;
        }

        public void Cleanup()
        {
            AppDomain.CurrentDomain.UnhandledException -= AssemblyFunctions.CurrentDomain_UnhandledException;
        }

        #endregion
    }
}
