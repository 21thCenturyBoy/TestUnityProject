using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ScratchFramework
{
    public class CameraDragDebug : MonoBehaviour
    {
#if UNITY_EDITOR

        private Material m_MousePositionDraw;
        private Material m_CurrentDragBlockDraw;
        private Material m_SpotsListDraw;

        private void OnGUI()
        {
            if (BlockDragManager.Instance.DragDebug)
            {
                GUILayout.BeginVertical();
                GUILayout.Label(nameof(BlockDragManager.PointerPos) + BlockDragManager.Instance.PointerPos.ToString());
                if (BlockDragManager.Instance.CurrentDragBlock)
                {
                    GUILayout.Label(nameof(BlockDragManager.CurrentDragBlock) + BlockDragManager.Instance.CurrentDragBlock.Position.ToString());
                    GUILayout.Label(nameof(BlockDrag.OffsetPointerDown) + BlockDragManager.Instance.CurrentDragBlock.GetComponent<BlockDrag>().OffsetPointerDown.ToString());
                }

                GUILayout.EndVertical();
            }
        }

        private void OnPostRender()
        {
            if (BlockDragManager.Instance.DragDebug)
            {
                if (m_MousePositionDraw == null) m_MousePositionDraw = new Material(Shader.Find("Unlit/Color"));
                if (m_CurrentDragBlockDraw == null) m_CurrentDragBlockDraw = new Material(Shader.Find("Unlit/Color"));
                if (m_SpotsListDraw == null) m_SpotsListDraw = new Material(Shader.Find("Unlit/Color"));

                var CurrentDragBlock = BlockDragManager.Instance.CurrentDragBlock;
                var DetectionDistance = BlockDragManager.Instance.DetectionDistance;

                ScratchUtils.DrawScreenEllipse(Input.mousePosition, DetectionDistance, DetectionDistance, Color.gray, m_MousePositionDraw);
                if (CurrentDragBlock != null)
                {
                    ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(CurrentDragBlock.Position), DetectionDistance, DetectionDistance, Color.green, m_CurrentDragBlockDraw);
                }

                var SpotsList = BlockDragManager.Instance.SpotsList;
                for (int i = 0; i < SpotsList.Count; i++)
                {
                    if (SpotsList[i] != null)
                    {
                        if (CurrentDragBlock == null)
                        {
                            ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(SpotsList[i].DropPosition), DetectionDistance, DetectionDistance, Color.yellow, m_SpotsListDraw);
                        }
                        else
                        {
                            if (SpotsList[i].GetComponentInParent<Block>() != CurrentDragBlock)
                            {
                                ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(SpotsList[i].DropPosition), DetectionDistance, DetectionDistance, Color.yellow, m_SpotsListDraw);
                            }
                        }
                    }
                }
            }
        }

#endif
    }

    public class BlockDragManager : ScratchUISingleton<BlockDragManager>, IScratchManager
    {
        [Header("配置")] public float DetectionDistance = 40f;

#if UNITY_EDITOR
        [Header("Editor Debug")] public bool DragDebug = false;
#endif
        private List<BlockSpot> m_spotsList = new List<BlockSpot>();

        public List<BlockSpot> SpotsList => m_spotsList;

        private LinkedList<BlockSpot> m_AddListenSpotQueue = new LinkedList<BlockSpot>();

        private Block m_CurrentDragBlock;
        private BlockSpot m_TargetSpot;

        public BlockSpot TargetSpot => m_TargetSpot;
        public Block CurrentDragBlock => m_CurrentDragBlock;
        public Vector3 PointerPos => Input.mousePosition;

        public override bool Initialize()
        {
            Camera.main.gameObject.AddComponent<CameraDragDebug>();
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


        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}