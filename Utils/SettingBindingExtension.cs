using System.Windows.Data;

namespace MeshInspector.Utils
{
    /// <summary>
    /// helper class for better binding xaml with the app settings
    /// </summary>
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            this.Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
