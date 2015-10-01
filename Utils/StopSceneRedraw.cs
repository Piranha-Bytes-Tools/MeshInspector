using System;

namespace MeshInspector.Utils
{
    /// <summary>
    /// this will automatic stop the max scene drawing and will enable it again on dispose
    /// use it with using statement to auto disable and enable scene redraw on everything inside the using scope
    /// </summary>
    internal class StopSceneRedraw : IDisposable
    {
        private bool m_bExpertMode;


        /// <summary>
        /// create and disable scene redraw. enable expert mode if wanted
        /// </summary>
        /// <param name="expertMode"></param>
        public StopSceneRedraw(bool expertMode = true)
        {
            this.m_bExpertMode = expertMode;

            AssemblyFunctions.Core.DisableSceneRedraw();

            if (this.m_bExpertMode)
                AssemblyFunctions.Interface7.ExpertMode = 1;
        }

        /// <summary>
        /// enable scene redraw and turn off expert mode if set
        /// </summary>
        public void Dispose()
        {
            if (this.m_bExpertMode)
                AssemblyFunctions.Interface7.ExpertMode = 0;

            AssemblyFunctions.Core.EnableSceneRedraw();
            AssemblyFunctions.Core.ForceCompleteRedraw(true);
        }
    }
}
