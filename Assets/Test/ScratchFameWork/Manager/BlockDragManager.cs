using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ScratchFramework
{
    public class BlockDragManager : ScratchSingleton<BlockDragManager>, IScratchManager
    {
        [Header("配置")] public float DetectionDistance = 40f;

        private List<BlockSpot> m_spotsList = new List<BlockSpot>();

        public List<BlockSpot> SpotsList => m_spotsList;

        private LinkedList<BlockSpot> m_AddListenSpotQueue = new LinkedList<BlockSpot>();

        private Block m_CurrentDragBlock;
        private BlockSpot m_TargetSpot;

        public BlockSpot TargetSpot => m_TargetSpot;
        public Block CurrentDragBlock => m_CurrentDragBlock;

        public override bool Initialize()
        {
            return base.Initialize();
        }

        public void OnBlockBeginDrag(BlockDrag blockDragTrigger)
        {
            blockDragTrigger.SetParent(transform);

            m_CurrentDragBlock = blockDragTrigger.Block;

            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockBeginDrag, blockDragTrigger.Block);
        }

        public void OnBlockDrag(BlockDrag blockDragTrigger)
        {
            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockDrag, blockDragTrigger.Block);
        }

        public void OnBlockEndDrag(BlockDrag blockDragTrigger)
        {
            blockDragTrigger.SetParent(BlockCanvasManager.Instance);

            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockEndDrag, blockDragTrigger.Block);

            m_CurrentDragBlock = null;
        }

        public void SetTargetSpot(BlockSpot spot)
        {
            m_TargetSpot = spot;
        }

        public void AddSpot(BlockSpot spot)
        {
            if (!Inited)
            {
                m_AddListenSpotQueue.AddLast(new LinkedListNode<BlockSpot>(spot));
                return;
            }

            if (!SpotsList.Contains(spot) && spot != null)
            {
                SpotsList.Add(spot);
            }
        }

        public void RemoveSpot(BlockSpot spot)
        {
            if (m_AddListenSpotQueue.Contains(spot))
            {
                m_AddListenSpotQueue.Remove(spot);
            }

            if (SpotsList.Contains(spot))
            {
                SpotsList.Remove(spot);
            }
        }

        public void OnUpdate()
        {
            if (m_AddListenSpotQueue.Count != 0)
            {
                SpotsList.AddRange(m_AddListenSpotQueue);
                m_AddListenSpotQueue.Clear();
            }
        }

        private void OnRenderObject()
        {
            if (BlockDrag.BlockDragDebug)
            {
                if (CurrentDragBlock != null)
                {
                    ScratchUtils.DrawScreenEllipse(CurrentDragBlock.GetComponent<BlockDrag>().RectTrans.position, BlockDragManager.Instance.DetectionDistance, BlockDragManager.Instance.DetectionDistance, Color.green);
                }
                for (int i = 0; i < SpotsList.Count; i++)
                {
                    if (SpotsList[i] != null)
                    {
                        if (CurrentDragBlock == null)
                        {
                            ScratchUtils.DrawScreenEllipse(SpotsList[i].DropPosition, DetectionDistance, DetectionDistance, Color.yellow);
                      
                        }
                        else
                        {
                            if (SpotsList[i].GetComponentInParent<Block>() != CurrentDragBlock)
                            {
                                ScratchUtils.DrawScreenEllipse(SpotsList[i].DropPosition, DetectionDistance, DetectionDistance, Color.yellow);
                            }
                        }
                 
                    }
                }

       
            }
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