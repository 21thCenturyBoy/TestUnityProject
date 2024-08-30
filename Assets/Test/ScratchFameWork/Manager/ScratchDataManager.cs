using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{

    public class ScratchDataManager : Singleton_Class<ScratchDataManager>, IScratchManager
    {
        public bool Initialize()
        {
            return true;
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