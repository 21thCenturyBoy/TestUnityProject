using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class ScratchBlockMenu : ScratchUIBehaviour
    {
        public TMP_Text TitleText;
        public Button DuplicateBtn;
        public Button DeleteBtn;

        public Block Current { get; set; }


        protected override void OnInitialize()
        {
            base.OnInitialize();

            DuplicateBtn.onClick.RemoveAllListeners();
            DuplicateBtn.onClick.AddListener(DuplicateBtnOnClick);
            
            DeleteBtn.onClick.RemoveAllListeners();
            DeleteBtn.onClick.AddListener(DeleteBtnOnClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Current = null;
        }

        private void DuplicateBtnOnClick()
        {
            if (Current != null)
            {
                ScratchUtils.CloneBlock(Current);
                ScratchMenuManager.Instance.Close(this);
            }
        }
        private void DeleteBtnOnClick()
        {
            if (Current != null)
            {
                ScratchUtils.DestroyBlock(Current);
                ScratchMenuManager.Instance.Close(this);
            }
        }
    }
}