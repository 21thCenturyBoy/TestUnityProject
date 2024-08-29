using System;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchMenuManager : ScratchSingleton<ScratchMenuManager>
    {
        public MenuMask ScratchMenuMask { get; set; }

        public ScratchBlockMenu BlockMenu { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ScratchMenuMask = TempCanvasManager.Instance.MenuMask;
            BlockMenu = TempCanvasManager.Instance.BlockMenu;

            //TODO Menu
            BlockMenu.SetParent(transform);

            //Mask Index = 0
            ScratchMenuMask.SetParent(transform,0);

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
            BlockMenu.RectTrans.position = block.RectTrans.position + new Vector3(block.GetSize().x, -block.GetSize().y) / 2;
            BlockMenu.TitleText.text = block.name;

            ScratchMenuMask.Active = true;
            BlockMenu.Active = true;
        }
    }
}