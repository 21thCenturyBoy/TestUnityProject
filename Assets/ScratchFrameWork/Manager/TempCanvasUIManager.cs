using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class TempCanvasCenter : ScratchUIBehaviour
    {
        public override bool Initialize()
        {
            TryAddComponent<RectTransform>();
            
            return base.Initialize();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            RectTrans.anchorMin = new Vector2(0.5f, 0.5f);
            RectTrans.anchorMax = new Vector2(0.5f,0.5f);
            RectTrans.anchoredPosition = Vector2.zero;
            RectTrans.sizeDelta = Vector2.zero;
            RectTrans.pivot = new Vector2(0.5f, 0.5f);
            RectTrans.localScale = Vector3.one;
            RectTrans.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// TempCanvas 临时UI预制
    /// </summary>
    public class TempCanvasUIManager : ScratchUIBehaviour, IScratchManager
    {
        private Block_GhostBlock m_GhostBlock;
        public Block_GhostBlock GhostBlock => m_GhostBlock;

        private MenuMask m_MenuMask;
        public MenuMask MenuMask => m_MenuMask;

        private ScratchBlockMenu m_BlockMenu;
        public ScratchBlockMenu BlockMenu => m_BlockMenu;

        private static TempCanvasUIManager _instance;

        public static TempCanvasUIManager Instance => _instance;


        private TempCanvasCenter m_CanvasCenter;
        public TempCanvasCenter CanvasCenter => m_CanvasCenter;


        public TopCanvasGroup TopCanvasGroup { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _instance = null;
        }

        public override bool Initialize()
        {
            _instance = this;

            if (m_GhostBlock == null)
            {
                ScratchConfig.ResourceLoad<GameObject>(ScratchConfig.RES_GhostBlock, obj =>
                {
                    if (obj != null)
                    {
                        m_GhostBlock = Instantiate(obj, transform).GetComponent<Block_GhostBlock>();
                        m_GhostBlock.Active = false;
                    }
                });
            }

            m_MenuMask = transform.GetComponentInChildren<MenuMask>(true);
            m_MenuMask.Active = false;

            m_BlockMenu = transform.GetComponentInChildren<ScratchBlockMenu>(true);
            m_BlockMenu.Active = false;

            m_CanvasCenter = new GameObject(nameof(CanvasCenter)).AddComponent<TempCanvasCenter>();
            m_CanvasCenter.SetParent(this);
            m_CanvasCenter.Initialize();
            
            TopCanvasGroup = transform.Find("TopCanvasGroup").GetComponent<TopCanvasGroup>();

            return base.Initialize();
        }

        public Block[] GetChildTempBlock()
        {
            int childCount = transform.childCount;
            List<Block> res = new List<Block>();
            for (int i = 0; i < childCount; i++)
            {
                Block block = transform.GetChild(i).GetComponent<Block>();
                if (block != null && block is not Block_GhostBlock)
                {
                    res.Add(block);
                }
            }

            return res.ToArray();
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