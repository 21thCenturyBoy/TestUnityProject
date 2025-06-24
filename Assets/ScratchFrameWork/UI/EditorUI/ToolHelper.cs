using UnityEngine;

namespace ScratchFramework
{
    public static class ToolHelper
    {
        public static void OnOpen()
        {
            ScratchProgrammingManager.Instance.Active = true;
        }

        public static void OnGame()
        {
            ScratchProgrammingManager.Instance.Active = false;
        }

        public static void OnPlay()
        {
            var wm = ScratchEngine.Instance.Core.GetVirtualMachine();
            if (wm != null)
            {
                wm.SetState(IVirtualMachine.Switch.Play);
            }
            
            if (WindowManager.Instance.SingletonIsOpen<ToolsView>(out var toolsView))
            {
                toolsView.PasueBtn.Active = true;
                toolsView.PlayBtn.Active = false;
            }
        }

        public static void OnPause()
        {
            var wm = ScratchEngine.Instance.Core.GetVirtualMachine();
            if (wm != null)
            {
                wm.SetState(IVirtualMachine.Switch.Pause);
            }
            
            if (WindowManager.Instance.SingletonIsOpen<ToolsView>(out var toolsView))
            {
                toolsView.PasueBtn.Active = false;
                toolsView.PlayBtn.Active = true;
            }
        }

        public static void OnStop()
        {
            var wm = ScratchEngine.Instance.Core.GetVirtualMachine();
            if (wm != null)
            {
                wm.SetState(IVirtualMachine.Switch.Stop);
            }
            
            if (WindowManager.Instance.SingletonIsOpen<ToolsView>(out var toolsView))
            {
                toolsView.PasueBtn.Active = false;
                toolsView.PlayBtn.Active = true;
            }
        }

        public static void ExitScratchEditor()
        {
            WindowManager.Instance.HideAllRegion();
        }

        public static void OpenObjectView(bool isShow = true)
        {
         
        }

        public static void LoadBlockFile(bool isShow = true)
        {
            //从引擎可视化数据中加载
            ScratchEngine.Instance.Core.LoadBlockFile((fileData) =>
            {
                ScratchEngine.Instance.SetFileData(fileData);
            });
        }

        public static void OpenSceneView(bool isShow = true)
        {
            WindowManager.Instance.HideAllRegion();
        }

        public static void OpenCodeView(bool isShow = true)
        {
            WindowManager.Instance.ShowAllRegion();
        }

        public static void OpenMaterialView(bool isShow = true)
        {
        }
    }
}