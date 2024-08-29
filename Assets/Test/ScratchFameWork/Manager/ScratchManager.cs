using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchManager : ScratchSingleton<ScratchManager>, IScratchManager
    {
        private TempCanvasManager m_TempCanvas;
        private void Start()
        {
            Initialize();
        }

        public override bool Initialize()
        {
            base.Initialize();
            m_isInitialized = false;

            m_TempCanvas = GetComponentInChildren<TempCanvasManager>();
            m_TempCanvas.Initialize();
                
            ScratchResources.Instance.LoadAllResource(LoadResourceFinish);

     
            return true;
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

            m_isInitialized = true;
        }

        public bool Active { get; set; }


        private void Update()
        {
            if (!Inited)return;

            OnUpdate();
        }

        public void OnUpdate()
        {
            ScratchEventManager.Instance.OnUpdate();
            BlockDragManager.Instance.OnUpdate();
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