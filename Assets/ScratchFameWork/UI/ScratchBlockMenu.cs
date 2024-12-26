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
        public RectTransform ReplaceScrollView;
        public RectTransform ReplaceScrollView_Content;

        [SerializeField] private GameObject m_ButtonPrefab;
        public Block Current { get; set; }

        private List<Button> contentBtns = new List<Button>();


        protected override void OnInitialize()
        {
            base.OnInitialize();

            DuplicateBtn.onClick.RemoveAllListeners();
            DuplicateBtn.onClick.AddListener(DuplicateBtnOnClick);

            DeleteBtn.onClick.RemoveAllListeners();
            DeleteBtn.onClick.AddListener(DeleteBtnOnClick);
        }

        private void ReplaceBtnOnClick()
        {
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
                MenuUIManager.Instance.Close(this);
            }
        }

        private void DeleteBtnOnClick()
        {
            if (Current != null)
            {
                ScratchUtils.DestroyBlock(Current);
                MenuUIManager.Instance.Close(this);
            }
        }

        public void ShowReplaceScrollView()
        {
            if (Current.GetEngineBlockData() is not IBlockPlug)
            {
                ReplaceScrollView.gameObject.SetActive(false);
                return;
            }
            else
            {
                ReplaceScrollView.gameObject.SetActive(true);
            }

            for (int i = 0; i < contentBtns.Count; i++)
            {
                GameObject.DestroyImmediate(contentBtns[i].gameObject);
            }

            contentBtns.Clear();

            if (Current.BlockFucType == FucType.Variable)
            {
                return;
            }

            if (Current != null)
            {
                var teledatas = BlockResourcesManager.Instance.GetTemplateData(Current.BlockFucType);
                for (int i = 0; i < teledatas.Length; i++)
                {
                    Button btn = GameObject.Instantiate(m_ButtonPrefab).GetComponent<Button>();
                    btn.GetComponentInChildren<TMP_Text>().text = "Replace " + teledatas[i].Name;
                    btn.transform.SetParent(ReplaceScrollView_Content);
                    btn.transform.localScale = Vector3.one;

                    ResourcesItemData itemData = teledatas[i];
                    var type = teledatas[i].ScratchType;
                    btn.onClick.AddListener(() =>
                    {
                        Block newBlock = itemData.CreateBlock(null);

                        newBlock.GetEngineBlockData().AsCanvasData().IsRoot = Current.GetEngineBlockData().AsCanvasData().IsRoot;
                        newBlock.GetEngineBlockData().AsCanvasData().CanvasPos = Current.GetEngineBlockData().AsCanvasData().CanvasPos;

                        ScratchUtils.ReplaceBlock(Current, newBlock);

                        MenuUIManager.Instance.Close(this);
                    });
                    btn.gameObject.SetActive(true);

                    contentBtns.Add(btn);
                }
            }
        }
    }
}