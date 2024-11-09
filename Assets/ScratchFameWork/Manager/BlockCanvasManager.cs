using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockTree
    {
        public Guid BlockGuid;

        public List<BlockTreeNode> BlockTreeNode = new List<BlockTreeNode>();

        public int Index { get; set; }
        public int Depth { get; set; }

        public string DisplayName
        {
            get
            {
                string rex = IsRoot ? "" : Header ? "[H]" : "[B]";
                return $"{rex}{BlockCanvasManager.Instance.BlockDict[BlockGuid].name}";
            }
        }

        public bool IsRoot;
        public bool Header;
    }

    public class BlockTreeNode
    {
        public string DisplayName => nameof(BlockTreeNode) + SectionIndex.ToString();
        public int SectionIndex;

        public List<BlockTree> HeadBlocks = new List<BlockTree>();
        public List<BlockTree> BodyBlocks = new List<BlockTree>();
    }


    public class BlockCanvasManager : ScratchUISingleton<BlockCanvasManager>, IScratchManager
    {
        private Dictionary<Guid, Block> m_BlockDict = new Dictionary<Guid, Block>();
        public Dictionary<Guid, Block> BlockDict => m_BlockDict;

        public Action OnBlockDictChanged;

        protected override void OnInitialize()
        {
            ClearCanvas();

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
            OnBlockDictChanged = null;
        }

        public void AddBlock(Block block)
        {
            if (block.Type == BlockType.none) return;

            if (block.BlockId == Guid.Empty || !m_BlockDict.ContainsKey(block.BlockId))
            {
                block.BlockId = Guid.NewGuid();
                m_BlockDict[block.BlockId] = block;

                ScratchResourcesManager.Instance.OnCanvasAddBlock(block);
                
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
                ScratchResourcesManager.Instance.OnCanvasRemoveBlock(block);
                
                m_BlockDict.Remove(block.BlockId);

                OnBlockDictChanged?.Invoke();
                return;
            }

            Debug.LogError("RemoveBlock Failed!");
        }


        public List<BlockTree> GetBlockTree()
        {
            List<BlockTree> trees = new List<BlockTree>();
            trees.Clear();
            int childCount = transform.childCount;
            int index = 0;
            int deep = 0;
            for (int i = 0; i < childCount; i++)
            {
                Block block = transform.GetChild(i).GetComponent<Block>();

                if (block != null && block.Active && block.Visible)
                {
                    BlockTree tree = new BlockTree();
                    tree.BlockGuid = block.BlockId;
                    tree.Depth = deep;
                    tree.Index = index;
                    tree.Header = false;
                    tree.IsRoot = true;

                    GetBlockDeep(tree, deep + 1);
                    trees.Add(tree);
                    index++;
                }
            }

            return trees;
        }

        private void GetBlockDeep(BlockTree block, int deep)
        {
            var sections = m_BlockDict[block.BlockGuid].Layout.SectionsArray;
            for (int j = 0; j < sections.Length; j++)
            {
                BlockTreeNode treeNode = new BlockTreeNode();
                treeNode.SectionIndex = j;
                block.BlockTreeNode.Add(treeNode);
                if (sections[j].Header != null)
                {
                    int headChildCount = sections[j].Header.transform.childCount;
                    int index_head = 0;
                    for (int k = 0; k < headChildCount; k++)
                    {
                        Block childBlock = sections[j].Header.transform.GetChild(k).GetComponent<Block>();
                        if (childBlock != null)
                        {
                            BlockTree blockTree_headBlock = new BlockTree();
                            blockTree_headBlock.BlockGuid = childBlock.BlockId;
                            blockTree_headBlock.Depth = deep;
                            blockTree_headBlock.Index = index_head;
                            blockTree_headBlock.Header = true;

                            treeNode.HeadBlocks.Add(blockTree_headBlock);

                            GetBlockDeep(blockTree_headBlock, deep + 1);

                            index_head++;
                        }
                    }
                }

                if (sections[j].Body != null)
                {
                    int bodyChildCount = sections[j].Body.transform.childCount;
                    int index_body = 0;

                    for (int k = 0; k < bodyChildCount; k++)
                    {
                        Block childBlock = sections[j].Header.transform.GetChild(k).GetComponent<Block>();
                        if (childBlock != null)
                        {
                            index_body++;
                            BlockTree blockTree_bodyBlock = new BlockTree();
                            blockTree_bodyBlock.BlockGuid = childBlock.BlockId;
                            blockTree_bodyBlock.Depth = deep;
                            blockTree_bodyBlock.Index = index_body;
                            blockTree_bodyBlock.Header = false;

                            treeNode.BodyBlocks.Add(blockTree_bodyBlock);

                            GetBlockDeep(blockTree_bodyBlock, deep + 1);
                        }
                    }
                }
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

        public T GetScratchUIAtPointer<T>() where T : ScratchUIBehaviour
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null) return null;

            PointerEventData _pointerEventData = new PointerEventData(eventSystem);

            _pointerEventData.position = BlockDragManager.Instance.PointerPos;
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

            _pointerEventData.position = BlockDragManager.Instance.PointerPos;
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
            var spots = BlockDragManager.Instance.SpotsList;
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
                            float distance = Vector2.Distance(usePointer ? BlockDragManager.Instance.PointerPos : dragPos, spotPos);

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
            var spots = BlockDragManager.Instance.SpotsList;
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
                            float distance = Vector2.Distance(usePointer ? BlockDragManager.Instance.PointerPos : dragPos, spotPos);
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