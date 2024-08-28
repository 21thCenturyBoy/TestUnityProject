using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class TempCanvasManager : ScratchUIBehaviour, IScratchManager
    {
        private Block_GhostBlock m_GhostBlock;

        public Block_GhostBlock GhostBlock => m_GhostBlock;

        private static TempCanvasManager _instance;

        public static TempCanvasManager Instance => _instance;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _instance = null;
        }

        public override bool Initialize()
        {
            _instance = this;
            m_GhostBlock = transform.GetComponentInChildren<Block_GhostBlock>(true);
            m_GhostBlock.Active = false;

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