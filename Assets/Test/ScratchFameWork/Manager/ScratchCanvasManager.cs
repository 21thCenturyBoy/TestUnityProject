using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchCanvasManager : Singleton<ScratchCanvasManager>, IScratchManager
    {
        private void Start()
        {
            Initialize();
        }

        public bool Initialize()
        {
            ScratchResources.Instance.LoadAllResource(LoadResourceFinish);

            return true;
        }

        private void LoadResourceFinish()
        {
            //ScratchDataManager
            ScratchDataManager.Instance.Initialize();

            //BlockCanvasManager
            BlockCanvasManager.Instance.transform.SetParent(transform);
            BlockCanvasManager.Instance.transform.localPosition = Vector3.zero;
            BlockCanvasManager.Instance.Initialize();

            //BlockDragManager
            BlockDragManager.Instance.transform.SetParent(transform);
            BlockDragManager.Instance.transform.localPosition = Vector3.zero;
            BlockDragManager.Instance.Initialize();
        }

        public bool Active { get; set; }

        public void OnUpdate()
        {
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