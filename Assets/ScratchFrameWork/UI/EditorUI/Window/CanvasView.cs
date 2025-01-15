using System;
using UnityEngine;

namespace ScratchFramework
{
    public class CanvasView : BaseWindowsView
    {
        public override bool Initialize()
        {
            return base.Initialize();
        }

        private void Start()
        {
            ScratchProgrammingManager.Instance.Initialize();
        }
    }
}