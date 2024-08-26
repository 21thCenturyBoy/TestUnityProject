using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class BlockDragManager : ScratchSingleton<BlockDragManager>, IScratchManager
    {
        // Update is called once per frame
        public bool Initialize()
        {
            return true;
        }


        public void OnDrag(BlockDragTrigger blockDragTrigger)
        {
            if (blockDragTrigger == null)
            {
                int childCount = transform.childCount;

                List<Block> m_ChildBlocksArray = new List<Block>();
                for (int i = 0; i < childCount; i++)
                {
                    Block childBlock = transform.GetChild(i).GetComponent<Block>();
                    if (childBlock != null)
                    {
                        m_ChildBlocksArray.Add(childBlock);
                    }
                }

                for (int i = 0; i < m_ChildBlocksArray.Count; i++)
                {
                    m_ChildBlocksArray[i].transform.SetParent(BlockCanvasManager.Instance.transform);
                }

            }
            else
            {
                blockDragTrigger.transform.SetParent(transform);
            }
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