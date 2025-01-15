using UnityEngine;

namespace ScratchFramework
{
    public abstract class BaseWindowsView : ScratchUIBehaviour
    {
        public WindowDescriptor WindowType { get;  set; }
        protected override void OnDestroy()
        {
            WindowManager.Instance.RemoveWindow(this);
            base.OnDestroy();
        }
    }
}