using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScratchFramework
{
    public class BlockDragManager : ScratchSingleton<BlockDragManager>, IScratchManager
    {
        [Header("配置")] public float DetectionDistance = 40f;

        private List<BlockSectionBody_Spot> m_spotsList = new List<BlockSectionBody_Spot>();

        public List<BlockSectionBody_Spot> SpotsList => m_spotsList;

        private LinkedList<BlockSectionBody_Spot> m_AddListenSpotQueue = new LinkedList<BlockSectionBody_Spot>();

        private Block m_CurrentDrag;

        public Block CurrentDrag => m_CurrentDrag;

        public override bool Initialize()
        {
            return base.Initialize();
        }

        public void OnBlockBeginDrag(BlockDrag blockDragTrigger)
        {
            blockDragTrigger.transform.SetParent(transform);

            m_CurrentDrag = blockDragTrigger.Block;
            
            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockBeginDrag, blockDragTrigger.Block);
        }

        public void OnBlockDrag(BlockDrag blockDragTrigger)
        {
            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockDrag, blockDragTrigger.Block);
        }

        public void OnBlockEndDrag(BlockDrag blockDragTrigger)
        {
            blockDragTrigger.transform.SetParent(BlockCanvasManager.Instance.transform);
            
            ScratchEventManager.Instance.SendEvent<Block>(ScratchEventDefine.OnBlockEndDrag, blockDragTrigger.Block);
            
            m_CurrentDrag = null;
        }

        public void AddSpot(BlockSectionBody_Spot spot)
        {
            if (!Inited)
            {
                m_AddListenSpotQueue.AddLast(new LinkedListNode<BlockSectionBody_Spot>(spot));
                return;
            }

            if (!SpotsList.Contains(spot) && spot != null)
            {
                SpotsList.Add(spot);
            }
        }

        public void RemoveSpot(BlockSectionBody_Spot spot)
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

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}