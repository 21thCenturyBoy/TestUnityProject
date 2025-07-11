using UnityEngine;

namespace ScratchFramework
{
    public class ScratchProgrammingManager : ScratchSingleton<ScratchProgrammingManager>, IScratchManager
    {
        private Canvas m_Canvas;

        public Canvas Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    //Search Canvas
                    m_Canvas = GetComponentInChildren<Canvas>();
                    if (m_Canvas == null)
                    {
                        m_Canvas = GetComponentInParent<Canvas>();
                    }
                }

                return m_Canvas;
            }
        }


        public Camera CanvasCamera
        {
            get
            {
                if (Canvas == null) return null;

                if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay) return Camera.main;

                return Camera.main;
            }
        }

        [SerializeField]
        private TempCanvasUIManager m_TempCanvasUI;
        private BlockResourcesManager m_ResourcesManager;

        public override bool Initialize()
        {
            base.Initialize();
 
            m_ResourcesManager = BlockResourcesManager.Instance;
            m_ResourcesManager.Initialize();
            m_ResourcesManager.LoadAllResource(LoadResourceFinish);

            return Inited;
        }

        private void LoadResourceFinish()
        {
            //TempCanvasManager
            m_TempCanvasUI.Initialize();

            //ScratchDataManager
            BlockDataUIManager.Instance.Initialize();

            //ScratchEventManager
            ScratchEventManager.Instance.Initialize();

            //BlockCanvasManager
            BlockCanvasUIManager.Instance.SetParent(transform);
            BlockCanvasUIManager.Instance.transform.localPosition = Vector3.zero;
            BlockCanvasUIManager.Instance.Initialize();

            //BlockDragManager
            BlockDragUIManager.Instance.SetParent(transform);
            BlockDragUIManager.Instance.transform.localPosition = Vector3.zero;
            BlockDragUIManager.Instance.Initialize();

            //ScratchMenuManager
            MenuUIManager.Instance.SetParent(transform);
            MenuUIManager.Instance.transform.localPosition = Vector3.zero;
            MenuUIManager.Instance.Initialize();

            //ScratchDebug
            ScratchDebugManager.Instance.SetParent(transform);
            ScratchDebugManager.Instance.Initialize();

            m_isInitialized = true;
        }

        private void Update()
        {
            if (!Inited) return;

            OnUpdate();
        }

        public void OnUpdate()
        {
            ScratchEventManager.Instance.OnUpdate();
            BlockDragUIManager.Instance.OnUpdate();
            ScratchDebugManager.Instance.OnUpdate();
        }

        public void OnLateUpdate()
        {
        }


        public bool Clear()
        {
            //TempCanvasManager
            TempCanvasUIManager.Instance.Clear();

            //ScratchDataManager
            BlockDataUIManager.Instance.Clear();

            //ScratchEventManager
            ScratchEventManager.Instance.Clear();

            //BlockCanvasManager
            BlockCanvasUIManager.Instance.Clear();

            //BlockDragManager
            BlockDragUIManager.Instance.Clear();

            //ScratchMenuManager
            MenuUIManager.Instance.Clear();
            return true;
        }

 
    }
}