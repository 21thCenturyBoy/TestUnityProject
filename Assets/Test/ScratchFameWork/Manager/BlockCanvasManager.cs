using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class BlockCanvasManager : ScratchSingleton<BlockCanvasManager>, IScratchManager
    {
        public bool Initialize()
        {
            return true;
        }


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