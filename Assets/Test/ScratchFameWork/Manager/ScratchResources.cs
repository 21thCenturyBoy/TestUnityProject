using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchResources : Singleton_Class<ScratchResources>
    {
        public void LoadAllResource(Action successCallback = null, Action failedCallback = null)
        {
            //TODO 资源管理
            successCallback?.Invoke();
        }
        
    }
}