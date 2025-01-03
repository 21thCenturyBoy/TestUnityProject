using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// Scratch 菜单管理类
    /// </summary>
    public class MenuUIManager : ScratchUISingleton<MenuUIManager>,IScratchManager
    {
        public MenuMask ScratchMenuMask { get; set; }

        public ScratchBlockMenu BlockMenu { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ScratchMenuMask = TempCanvasUIManager.Instance.MenuMask;
            BlockMenu = TempCanvasUIManager.Instance.BlockMenu;

            //TODO Menu
            BlockMenu.SetParent(transform);

            //Mask Index = 0
            ScratchMenuMask.SetParent(transform, 0);

            ScratchMenuMask.Initialize();
            BlockMenu.Initialize();

            CloseAll();
        }

        public void CloseAll()
        {
            ScratchMenuMask.Active = false;
            BlockMenu.Active = false;
        }

        public void Close(ScratchUIBehaviour menu)
        {
            ScratchMenuMask.Active = false;

            menu.Active = false;
        }

        public void RightPointerMenu_Block(Block block)
        {
            BlockMenu.Current = block;

            BlockMenu.Position = block.Position;
            BlockMenu.LocalPosition += new Vector3(block.GetSize().x, -block.GetSize().y) / 2;

            BlockMenu.TitleText.text = block.name;
            BlockMenu.ShowReplaceScrollView();

            ScratchMenuMask.Active = true;
            BlockMenu.Active = true;
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