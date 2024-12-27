using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    /// <summary>
    /// UI BlockCanvas管理类
    /// </summary>
    public class BlockCanvasUIManager : ScratchUISingleton<BlockCanvasUIManager>, IScratchManager
    {
        private Dictionary<Guid, Block> m_BlockDict = new Dictionary<Guid, Block>();
        public Dictionary<Guid, Block> BlockDict => m_BlockDict;

        public Action OnBlockDictChanged;

        protected override void OnInitialize()
        {
            ClearCanvas();
            OnBlockDictChanged = null;

            base.OnInitialize();
        }

        public void ClearCanvas()
        {
            var childs = GetChildTempBlock();
            for (int i = 0; i < childs.Length; i++)
            {
                DestroyImmediate(childs[i].gameObject);
            }

            m_BlockDict.Clear();
        }

        public void AddBlock(Block block)
        {
            if (block.Type == BlockType.none) return;

            if (block.BlockId == Guid.Empty || !m_BlockDict.ContainsKey(block.BlockId))
            {
                block.BlockId = Guid.NewGuid();
                m_BlockDict[block.BlockId] = block;

                BlockResourcesManager.Instance.OnCanvasAddBlock(block);

                OnBlockDictChanged?.Invoke();
            }
            else
            {
                Debug.LogError("AddBlock Failed!");
            }
        }

        public void RemoveBlock(Block block)
        {
            if (block.Type == BlockType.none) return;

            if (m_BlockDict.ContainsKey(block.BlockId))
            {
                BlockResourcesManager.Instance.OnCanvasRemoveBlock(block);

                m_BlockDict.Remove(block.BlockId);

                OnBlockDictChanged?.Invoke();
                return;
            }

            Debug.LogError($"RemoveBlock Failed!{block.gameObject.name}");
        }

        public void RefreshCanvas()
        {
            var childs = GetChildTempBlock();
            for (int i = 0; i < childs.Length; i++)
            {
                DestroyImmediate(childs[i].gameObject);
            }

            HashSet<Block> res = new HashSet<Block>(BlockDict.Count);
            var keys = ScratchEngine.Instance.Current.GetBlockKeys();
            for (int j = 0; j < keys.Length; j++)
            {
                var block = ScratchEngine.Instance.Current[keys[j]].AsCanvasData();
                if (block is { IsRoot: true })
                {
                    var blocks = ScratchUtils.DrawNodeRoot(block, BlockCanvasUIManager.Instance.RectTrans);
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        res.Add(blocks[i]);
                    }
                }
            }

            var refBlocks = ScratchEngine.Instance.Current.FragmentDataRefs;
            foreach (var refBlock in refBlocks)
            {
                if (refBlock.Value is { IsRoot: true })
                {
                    var blocks = ScratchUtils.DrawNodeRoot(refBlock.Value, RectTrans);
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        res.Add(blocks[i]);
                    }
                }
            }

            ScratchUtils.FixedBindOperation(res);

            BlockResourcesManager.Instance.RefreshResources();
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

        public Block[] GetChildTempBlock()
        {
            int childCount = transform.childCount;
            List<Block> res = new List<Block>();
            for (int i = 0; i < childCount; i++)
            {
                Block block = transform.GetChild(i).GetComponent<Block>();

                if (block != null && block is not Block_GhostBlock && !block.IsDestroying)
                {
                    res.Add(block);
                }
            }

            return res.ToArray();
        }

        public T GetScratchUIAtPointer<T>() where T : ScratchUIBehaviour
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null) return null;

            PointerEventData _pointerEventData = new PointerEventData(eventSystem);

            _pointerEventData.position = BlockDragUIManager.Instance.PointerPos;
            List<RaycastResult> globalResults = new List<RaycastResult>();
            eventSystem.RaycastAll(_pointerEventData, globalResults);

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                RaycastResult result = globalResults[i];

                var com = result.gameObject.GetComponent<T>();
                if (com != null) return com;
            }

            return null;
        }

        public T[] GetScratchUIsAtPointer<T>() where T : ScratchUIBehaviour
        {
            EventSystem eventSystem = EventSystem.current;

            List<T> results = new List<T>();
            if (eventSystem == null) return results.ToArray();

            PointerEventData _pointerEventData = new PointerEventData(eventSystem);

            _pointerEventData.position = BlockDragUIManager.Instance.PointerPos;
            List<RaycastResult> globalResults = new List<RaycastResult>();
            eventSystem.RaycastAll(_pointerEventData, globalResults);

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                RaycastResult result = globalResults[i];
                results.AddRange(result.gameObject.GetComponents<T>());
            }

            return results.ToArray();
        }

        public T FindClosestSpotOfType<T>(BlockDrag drag, float maxDistance, bool usePointer = false) where T : BlockSpot
        {
            float minDistance = Mathf.Infinity;
            T found = null;
            var spots = BlockDragUIManager.Instance.SpotsList;
            Vector2 dragPos = ScratchUtils.WorldPos2ScreenPos(drag.Position);
            for (int i = 0; i < spots.Count; i++)
            {
                BlockSpot spot = spots[i];

                if ((spot is T targetT && spot.Active && spot.Visible))
                {
                    BlockDrag d = spot.GetComponentInParent<BlockDrag>();

                    if (d.transform.IsChildOf(transform))
                    {
                        if (d != drag && Active && Visible)
                        {
                            Vector2 spotPos = ScratchUtils.WorldPos2ScreenPos(spot.DropPosition);
                            float distance = Vector2.Distance(usePointer ? BlockDragUIManager.Instance.PointerPos : dragPos, spotPos);

                            if (distance < minDistance && distance <= maxDistance)
                            {
                                found = targetT;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            return found;
        }

        public BlockSpot FindClosestSpotForBlock(BlockDrag drag, float maxDistance, bool usePointer = false)
        {
            float minDistance = Mathf.Infinity;
            BlockSpot found = null;
            var spots = BlockDragUIManager.Instance.SpotsList;
            Vector2 dragPos = ScratchUtils.WorldPos2ScreenPos(drag.Position);
            for (int i = 0; i < spots.Count; i++)
            {
                BlockSpot spot = spots[i];

                if ((spot is BlockSpot_SectionBody || (spot is BlockSpot_OuterArea && spot.Block.ParentSection != null)) && spot.Active && spot.Visible)
                {
                    BlockDrag d = spot.GetComponentInParent<BlockDrag>();

                    if (d.transform.IsChildOf(transform))
                    {
                        if (d != drag && Active && Visible)
                        {
                            Vector2 spotPos = ScratchUtils.WorldPos2ScreenPos(spot.DropPosition);
                            float distance = Vector2.Distance(usePointer ? BlockDragUIManager.Instance.PointerPos : dragPos, spotPos);
                            if (distance < minDistance && distance <= maxDistance)
                            {
                                found = spot;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            return found;
        }
    }
}