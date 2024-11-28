using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchManager : ScratchSingleton<ScratchManager>, IScratchManager
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

        private TempCanvasManager m_TempCanvas;
        private ScratchResourcesManager m_ResourcesManager;

        private void Awake()
        {
            Initialize();
        }

        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_TempCanvas = GetComponentInChildren<TempCanvasManager>();
                m_TempCanvas.Initialize();

                m_ResourcesManager = GetComponentInChildren<ScratchResourcesManager>();
            }

            m_ResourcesManager.Initialize();
            m_ResourcesManager.LoadAllResource(LoadResourceFinish);

            return Inited;
        }

        private void LoadResourceFinish()
        {
            //ScratchDataManager
            ScratchDataManager.Instance.Initialize();

            //ScratchEventManager
            ScratchEventManager.Instance.Initialize();

            //BlockCanvasManager
            BlockCanvasManager.Instance.SetParent(transform);
            BlockCanvasManager.Instance.transform.localPosition = Vector3.zero;
            BlockCanvasManager.Instance.Initialize();

            //BlockDragManager
            BlockDragManager.Instance.SetParent(transform);
            BlockDragManager.Instance.transform.localPosition = Vector3.zero;
            BlockDragManager.Instance.Initialize();

            //ScratchMenuManager
            ScratchMenuManager.Instance.SetParent(transform);
            ScratchMenuManager.Instance.transform.localPosition = Vector3.zero;
            ScratchMenuManager.Instance.Initialize();


            //ScratchDebug
            ScratchDebugManager.Instance.Initialize();


            //从引擎可视化数据中加载
            ScratchEngine.Instance.Core.LoadCanvasGroup((group) =>
            {
                ScratchEngine.Instance.CurrentGroup = group;
                ScratchEngine.Instance.Current = group.GlobalCanvas;

                ScratchEngine.Instance.CurrentGroup.RefreshDataGuids();
                ScratchEngine.Instance.Current.DrawCanvas();
                
                TempCanvasManager.Instance.TopCanvasGroup.Initialize();
            });

            m_isInitialized = true;
        }

        public bool Active { get; set; }


        private void Update()
        {
            if (!Inited) return;

            OnUpdate();
        }

        public void OnUpdate()
        {
            ScratchEventManager.Instance.OnUpdate();
            BlockDragManager.Instance.OnUpdate();
            ScratchDebugManager.Instance.OnUpdate();
        }

        public void OnLateUpdate()
        {
        }


        public bool Clear()
        {
            //TempCanvasManager
            TempCanvasManager.Instance.Clear();

            //ScratchDataManager
            ScratchDataManager.Instance.Clear();

            //ScratchEventManager
            ScratchEventManager.Instance.Clear();

            //BlockCanvasManager
            BlockCanvasManager.Instance.Clear();

            //BlockDragManager
            BlockDragManager.Instance.Clear();

            //ScratchMenuManager
            ScratchMenuManager.Instance.Clear();
            return true;
        }

 
    }
}