using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScratchFramework
{
    public static partial class ScratchUtils
    {
        public const string Block_PefixName = "Body";
        public const string Section_PefixName = "Section";
        public const string SectionHeader_PefixName = "Header";
        public const string SectionBody_PefixName = "Body";

        /// <summary>
        /// CloneBlock
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block CloneBlock(Block block)
        {
            Dictionary<int, IEngineBlockBaseData> blockDataGuids = new Dictionary<int, IEngineBlockBaseData>();

            var tree = GetBlockDatas(block.GetEngineBlockData(), blockData =>
            {
                if (blockData.Guid == InvalidGuid) return;

                if (blockDataGuids.ContainsKey(blockData.Guid)) return;
                if (blockData.IsDataRef())
                {
                    BlockFragmentDataRef dataRef = blockData as BlockFragmentDataRef;
                    if (!dataRef.DataRef.IsReturnVariable()) return;
                    blockDataGuids[dataRef.Guid] = dataRef.DataRef;
                }
                else
                {
                    blockDataGuids[blockData.Guid] = blockData;
                }
            });

            Dictionary<int, int> dataMapGuids = new Dictionary<int, int>();
            Dictionary<int, IEngineBlockBaseData> newblockDataGuids = new Dictionary<int, IEngineBlockBaseData>();
            foreach (var orginBlockData in blockDataGuids.Values)
            {
                IEngineBlockBaseData newData = ScratchUtils.CreateBlockData(orginBlockData.Type);

                //Store Guid
                dataMapGuids[orginBlockData.Guid] = newData.Guid;

                orginBlockData.CopyData(ref newData);

                newData.Guid = dataMapGuids[orginBlockData.Guid];

                newblockDataGuids[newData.Guid] = newData;

                if (!ScratchEngine.Instance.AddBlocksData(newData))
                {
                    Debug.LogError("Engine Add Block Error:" + newData.Guid);
                }
            }

            RefreshDataGuids(newblockDataGuids.Values.ToList(), dataMapGuids);

            BlockCanvasUIManager.Instance.RefreshCanvas();
            return null;
        }

        public static void DestroyBlock(Block block, bool refreshCanvas = true, bool recursion = true)
        {
            var blockBaseData = block.GetEngineBlockData();
            var blockTree = GetBlockDatas(block);

            if (blockBaseData.IsDataRef())
            {
                BlockFragmentDataRef dataRef = blockBaseData as BlockFragmentDataRef;
                if (dataRef.IsRoot)
                {
                    ScratchEngine.Instance.RemoveFragmentDataRef(dataRef);
                }
            }

            if (recursion)
            {
                HashSet<int> hashSet = new HashSet<int>();
                blockTree.TraverseTree((deep, bNode) =>
                {
                    if (!blockBaseData.IsDataRef()) return true;
                    hashSet.Add(bNode.Value.Guid);
                    return true;
                });
            }
            else
            {
                ScratchEngine.Instance.RemoveBlocksData(blockBaseData);
            }

            if (refreshCanvas)
            {
                BlockCanvasUIManager.Instance.RefreshCanvas();
            }
        }

        public static void ReplaceBlock(this Block block, Block newBlock)
        {
            if (newBlock == null) return;

            if (block.BlockFucType != newBlock.BlockFucType) return;

            IEngineBlockBaseData blockData = newBlock.GetEngineBlockData();
            var parentBlock = block.ParentTrans.GetComponentInParent<Block>();
            var baseData = block.GetEngineBlockData();
            if (parentBlock != null)
            {
                var orginParent = parentBlock.GetEngineBlockData();
                if (orginParent is IEngineBlockBranch orginParnetBranch)
                {
                    int branchCount = orginParnetBranch.GetBranchCount();
                    var branchs = orginParnetBranch.BranchBlockBGuids;
                    for (int i = 0; i < branchs.Length; i++)
                    {
                        if (branchs[i] == baseData.Guid)
                        {
                            branchs[i] = blockData.Guid;
                        }
                    }
                }
                else if (orginParent is IBlockPlug orginParnetPlug)
                {
                    orginParnetPlug.NextGuid = blockData.Guid;
                }
            }

            int orginnext = InvalidGuid;
            if (baseData is IBlockPlug orginPlug)
            {
                orginnext = orginPlug.NextGuid;
            }

            if (blockData is IBlockPlug newPlug)
            {
                newPlug.NextGuid = orginnext;
            }

            DestroyBlock(block);


            // BlockCanvasManager.Instance.RefreshCanvas();
        }

        /// <summary>
        /// 绘制根节点
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="parentTrans"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<Block> DrawNodeRoot(IEngineBlockCanvasData rootNode, Transform parentTrans, int index = -1)
        {
            List<Block> res = new List<Block>();
            var blockview = DrawNode(rootNode, parentTrans, index);
            blockview.transform.position = rootNode.CanvasPos;
            if (blockview != null)
            {
                res.Add(blockview);
            }

            return res;
        }

        public static void FixedBindOperation(HashSet<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                var childBlocks = block.GetComponentsInChildren<Block>();
                for (int j = 0; j < childBlocks.Length; j++)
                {
                    var childBlock = childBlocks[j];
                    if (childBlock.Type == BlockType.Operation)
                    {
                        if (childBlock.TryGetOperationInput(out var input))
                        {
                            var itemOperation = childBlock.GetScratchComponent<BlockHeaderItem_Operation>();
                            if (!itemOperation.Inited) itemOperation.Initialize();
                            if (!input.Inited) input.Initialize();
                            input.ContextData.ChildOperation = itemOperation.ContextData.CreateRef<BlockHeaderParam_Data_Operation>();
                        }
                    }
                }
            }
        }

        private static Block DrawNode(IEngineBlockBaseData node, Transform parentTrans, int index = -1, Block blockUI = null)
        {
            if (node == null) return null;

            var ResourceItem = BlockResourcesManager.Instance.GetResourcesItemData(node.Type);
            if (blockUI == null)
            {
                blockUI = ResourceItem.CreateBlock(node);
            }
            else
            {
                ResourceItem.SetKoalaBlockData(blockUI, node);
                node = blockUI.GetEngineBlockData();
            }

            if (index == -1)
            {
                int lastPosIndex = parentTrans.childCount;
                blockUI.SetParent(parentTrans, lastPosIndex);
            }
            else
            {
                if (index < parentTrans.childCount)
                {
                    blockUI.SetParent(parentTrans, index);
                }
                else
                {
                    int lastPosIndex = parentTrans.childCount;
                    blockUI.SetParent(parentTrans, lastPosIndex);
                }
            }

            var sections = blockUI.GetChildSection();

            switch (node.BlockType)
            {
                case BlockType.none:
                    break;
                case BlockType.Trigger:
                {
                    var _node = node as IEngineBlockTriggerBase;
                    if (sections[0].Header != null)
                    {
                        var headerOperations = sections[0].Header.GetHeaderOperation();
                        int variableLen = _node.GetReturnValuesLength();
                        if (variableLen != headerOperations.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            int guid = _node.GetReturnValueGuid(i);

                            //引用
                            if (headerOperations[i].OperationBlock.GetEngineBlockData() == null)
                            {
                                int indexTrans = headerOperations[i].RectTrans.GetSiblingIndex();
                                var operationBlock = GetBlocksDataRef(guid);

                                DrawNode(operationBlock, sections[0].Header.RectTrans, indexTrans, headerOperations[i].OperationBlock);
                            }
                            else
                            {
                                Debug.LogError("Not Null");
                            }
                        }
                    }

                    if (sections[0].Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(node.GetNextPlug()), sections[0].Body.RectTrans, 0);
                    }

                    break;
                }
                case BlockType.Simple:
                {
                    var _node = node as IEngineBlockSimpleBase;

                    if (sections[0].Header != null)
                    {
                        var headerInputs = sections[0].Header.GetHeaderInput();
                        int variableLen = _node.GetVarGuidsLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), sections[0].Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextPlug()), parentTrans, 0);
                    break;
                }
                case BlockType.Loop:
                {
                    var _node = node as IEngineBlockLoopBase;

                    if (sections[0].Header != null)
                    {
                        var headerInputs = sections[0].Header.GetHeaderInput();
                        int variableLen = _node.GetVarGuidsLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), sections[0].Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    if (sections[0].Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(_node.ChildRootGuid), sections[0].Body.RectTrans, 0);
                    }

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextPlug()), parentTrans, 0);
                    break;
                }
                case BlockType.Condition:
                {
                    var _node = node as IEngineBlockConditionBase;

                    int branchcount = _node.BranchBlockBGuids.Length;

                    for (int i = 0; i < branchcount; i++)
                    {
                        var section = sections[i];
                        if (section.Header != null)
                        {
                            if (i == branchcount - 1) continue; //End 

                            var headerInputs = section.Header.GetHeaderInput();
                            if (headerInputs.Length != 1)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                                return null;
                            }

                            var operationBlock = DrawNode(GetBlocksDataRef(_node.BranchOperationBGuids[i]), section.Header.RectTrans, headerInputs[0].RectTrans.GetSiblingIndex());
                        }
                    }

                    for (int i = 0; i < branchcount; i++)
                    {
                        var section = sections[i];
                        if (section.Body != null)
                        {
                            var branchBlock = DrawNode(GetBlocksDataRef(_node.BranchBlockBGuids[i]), section.Body.RectTrans, i);
                        }
                    }

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextPlug()), parentTrans, 0);
                    break;
                }
                case BlockType.Operation:
                {
                    if (node.FucType == FucType.GetValue || node.FucType == FucType.Condition)
                    {
                        var _node = node as IEngineBlockOperationBase;

                        if (sections[0].Header != null)
                        {
                            var headerInputs = sections[0].Header.GetHeaderInput();

                            int variableLen = _node.GetVarGuidsLength();
                            if (variableLen != headerInputs.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                                return null;
                            }

                            for (int i = 0; i < variableLen; i++)
                            {
                                var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), sections[0].Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                            }
                        }
                    }
                    else if (node.FucType == FucType.Variable)
                    {
                        if (node is IEngineBlockVariableBase variableBase)
                        {
                            var variableLabel = blockUI.VariableLabel;
                            var variableData = variableLabel.GetVariableData();

                            ScratchUtils.CreateVariableName(variableBase);
                            //绑定数据
                            variableData.VariableRef = variableBase.Guid.ToString();
                        }
                    }

                    break;
                }

                case BlockType.Define:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return blockUI;
        }
    }
}