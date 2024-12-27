namespace ScratchFramework
{
    /// <summary>
    /// ScratchDebug 管理类
    /// </summary>
    public class ScratchDebugManager : ScratchSingleton<ScratchDebugManager>, IScratchManager
    {
        public bool DragDebug = false;

        private CameraDragDebug m_CameraDragDebug;
        private bool m_pre_DragDebug = false;

        public void OnUpdate()
        {
            if (m_pre_DragDebug != DragDebug)
            {
                if (DragDebug)
                {
                    if (m_CameraDragDebug == null)
                    {
                        m_CameraDragDebug = ScratchManager.Instance.CanvasCamera.gameObject.AddComponent<CameraDragDebug>();
                    }
                }
                else
                {
                    if (m_CameraDragDebug != null)
                    {
                        Destroy(m_CameraDragDebug);
                        m_CameraDragDebug = null;
                    }
                }
            }

            m_pre_DragDebug = DragDebug;
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}