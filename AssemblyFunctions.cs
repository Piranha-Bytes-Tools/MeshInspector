using System;
using System.Windows;
using Autodesk.Max;
using Autodesk.Max.Plugins;
using MahApps.Metro.Controls.Dialogs;
using MeshInspector.UI;

namespace MeshInspector
{
    /// <summary>
    /// static method for managing criticial and must have methods
    /// </summary>
    public static class AssemblyFunctions
    {
        #region Fields

        /// <summary>
        /// global interface holds base max sdk methods like create interfaces
        /// </summary>
        public static IGlobal GlobalInterface;

        /// <summary>
        /// base interface to interact with max sdk
        /// </summary>
        public static IInterface Core;

        /// <summary>
        /// interface for interact with the views
        /// </summary>
        public static IInterface7 Interface7;
        public static ClassDesc2 Descriptor;

        #endregion

        #region Max SDK assembly loader must have

        /// <summary>
        /// will be called when the plugin will initialized from max
        /// </summary>
        public static void AssemblyMain()
        {
            AssemblyFunctions.GlobalInterface = Autodesk.Max.GlobalInterface.Instance;
            AssemblyFunctions.Core = AssemblyFunctions.GlobalInterface.COREInterface;
            AssemblyFunctions.Interface7 = AssemblyFunctions.GlobalInterface.COREInterface7;
            
            //create the descritor and add the class to max via core interface
            AssemblyFunctions.Descriptor = new UtilityMeshInspectorDesc(AssemblyFunctions.GlobalInterface);
            AssemblyFunctions.Core.AddClass(AssemblyFunctions.Descriptor);
        }

        /// <summary>
        /// will be called on shutdown
        /// </summary>
        public static void AssemblyShutdown()
        {
            if (MainWindow.Instance != null)
                MainWindow.Instance.Close();
        }

        #endregion

        internal static async void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (MainWindow.Instance == null)
                return;

            await MainWindow.Instance.ShowMessageAsync("Fault", string.Format("Mesh Inspector noticed an exception error. Closing...\n{0} ", e.Exception.Message));
            MainWindow.Instance.Close();
        }

        internal static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.DispatcherUnhandledException -= AssemblyFunctions.Current_DispatcherUnhandledException;

            if (MainWindow.Instance == null)
                return;

            await MainWindow.Instance.ShowMessageAsync("Fault", string.Format("Mesh Inspector noticed an exception error. Closing...\n{0} ", e));
            MainWindow.Instance.Close();
        }
    }
}
