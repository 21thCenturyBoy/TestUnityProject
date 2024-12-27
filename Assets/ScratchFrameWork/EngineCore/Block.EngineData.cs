using System;
using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// Koala数据层
    /// </summary>
    public partial class Block
    {
        #region ScratchInfo

        /// <summary>
        /// scratch type
        /// </summary>
        public ScratchBlockType scratchType;

        /// <summary>
        /// block data
        /// </summary>
        private IEngineBlockBaseData blockData;

        /// <summary>
        /// 记录兄弟索引变化
        /// </summary>
        private int lastSiblingIndex;

        /// <summary>
        /// 记录父级变化
        /// </summary>
        private Transform lastParent;

        #endregion

        public void SetKoalaBlock(IEngineBlockBaseData koalaBlockBase)
        {
            if (blockData != null)
            {
                Debug.LogError("数据没灌进去？" + koalaBlockBase.Guid);
                return;
            }

            // Debug.LogWarning("数据进去:" + koalaBlockBase.Guid + koalaBlockBase.Type);
            blockData = koalaBlockBase;
        }

        public IEngineBlockBaseData GetEngineBlockData()
        {
            return blockData;
        }

        public void InitKoalaData()
        {
            if (Type == BlockType.none) return;

            if (GetEngineBlockData() == null)
            {
                IEngineBlockBaseData block = ScratchUtils.CreateBlockData(scratchType);

                if (ScratchEngine.Instance.AddBlocksData(block))
                {
                    Debug.LogError("Engine Add Block Error:" + block.Guid);
                    return;
                }

                SetKoalaBlock(block);
            }
        }


        public void UpdateKoalaData()
        {
            if (Type == BlockType.none) return;
            // 检查父级变化
            if (ParentTrans != lastParent)
            {
                TransformParentChanged();
            }
            // 检查兄弟索引变化
            else if (transform.GetSiblingIndex() != lastSiblingIndex)
            {
                OnSiblingIndexChanged();
            }
        }

        public void DestoryEngineData()
        {
            if (Type == BlockType.none) return;

            //Is IsReturnVariable? Remove it
            if (blockData is IEngineBlockVariableBase variableBase)
            {
                if (variableBase.IsReturnVariable())
                {
                    if (ScratchEngine.Instance.Current[variableBase.ReturnParentGuid] == null)
                    {
                        ScratchEngine.Instance.Current.RemoveBlocksData(blockData);
                    }
                }
            }
        }

        public void TransformParentChanged()
        {
            if (!transform.IsChildOf(BlockCanvasUIManager.Instance.RectTrans)) return;

            ChangeBlockData(this, lastParent, transform.parent);

            lastParent = ParentTrans;
        }

        public void FixedUIPosData()
        {
            if (!transform.IsChildOf(BlockCanvasUIManager.Instance.RectTrans)) return;
            if (blockData.AsCanvasData() == null) return;

            var canvasData = blockData.AsCanvasData();
            canvasData.IsRoot = GetComponentInParent<IScratchSectionChild>() == null;

            if (canvasData.IsRoot)
            {
                canvasData.CanvasPos = transform.position;
                ScratchEngine.Instance.Current.RootBlock[blockData.Guid] = blockData;
            }
            else
            {
                canvasData.CanvasPos = Vector3.zero;
                ScratchEngine.Instance.Current.RootBlock.Remove(blockData.Guid);
            }
        }

        public void OnSiblingIndexChanged()
        {
            if (!transform.IsChildOf(BlockCanvasUIManager.Instance.RectTrans)) return;

            Debug.Log("Sibling index changed to: " + transform.GetSiblingIndex());
            lastSiblingIndex = transform.GetSiblingIndex();
        }


        #region ChangeBlockData

        public static void ClearOrginData(Block block, Transform orginParentTrans)
        {
            var engineBlockData = block.GetEngineBlockData();
            if (orginParentTrans != null)
            {
                var oldTag = orginParentTrans.GetComponent<IScratchSectionChild>();
                if (oldTag != null)
                {
                    BlockSection oldParentSection = oldTag.GetSection() as BlockSection;
                    Block oldParentBlock = oldParentSection.Block;
                    IEngineBlockBaseData oldParentBlockBase = oldParentSection.Block.GetEngineBlockData();

                    switch (oldParentBlockBase.BlockType)
                    {
                        case BlockType.none:
                            break;
                        case BlockType.Trigger:
                        {
                            if (oldParentBlock.GetEngineBlockData().GetNextGuid() == engineBlockData.Guid)
                            {
                                var preBlock = FindPreBlock(oldParentBlockBase.Guid, engineBlockData.Guid);
                                if (preBlock != null)
                                {
                                    preBlock.SetNextGuid(engineBlockData.GetNextGuid());
                                }
                            }

                            break;
                        }
                        case BlockType.Simple:
                        {
                            var parentSimple = oldParentBlockBase as IEngineBlockSimpleBase;

                            if (FindVarIndex(block, parentSimple, out var index))
                            {
                                parentSimple.SetVarsGuid(index, ScratchUtils.InvalidGuid);
                                break;
                            }

                            break;
                        }

                        case BlockType.Condition:
                        {
                            var parentCondition = oldParentBlockBase as IEngineBlockConditionBase;

                            if (engineBlockData.BlockType == BlockType.Operation)
                            {
                                int operation_Index = parentCondition.BranchOperationBGuids.FindIndex(engineBlockData.Guid);
                                if (operation_Index != -1)
                                {
                                    parentCondition.BranchOperationBGuids[operation_Index] = ScratchUtils.InvalidGuid;
                                }
                                else
                                {
                                    Debug.LogError($"奇怪的操作：为甚么不在Branch_OperationGuids里面？{engineBlockData.Guid}");
                                }
                            }
                            else
                            {
                                int branch_Index = parentCondition.BranchBlockBGuids.FindIndex(engineBlockData.Guid);
                                if (branch_Index != -1)
                                {
                                    parentCondition.BranchBlockBGuids[branch_Index] = engineBlockData.GetNextGuid();
                                }
                                else
                                {
                                    for (int i = 0; i < parentCondition.BranchBlockBGuids.Length; i++)
                                    {
                                        int branch_root = parentCondition.BranchBlockBGuids[i];
                                        var preBlock = FindPreBlock(branch_root, engineBlockData.Guid);
                                        if (preBlock != null)
                                        {
                                            preBlock.SetNextGuid(engineBlockData.GetNextGuid());
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                        }
                        case BlockType.Loop:
                        {
                            var parentLoop = oldParentBlockBase as IEngineBlockLoopBase;

                            if (FindVarIndex(block, parentLoop, out var index))
                            {
                                parentLoop.SetVarsGuid(index, ScratchUtils.InvalidGuid);
                                break;
                            }

                            if (parentLoop.ChildRootGuid == engineBlockData.Guid)
                            {
                                parentLoop.ChildRootGuid = engineBlockData.GetNextGuid();
                                break;
                            }

                            var preBlock = FindPreBlock(parentLoop.ChildRootGuid, engineBlockData.Guid);
                            if (preBlock != null)
                            {
                                preBlock.SetNextGuid(engineBlockData.GetNextGuid());
                                break;
                            }

                            break;
                        }
                        case BlockType.Operation:
                        {
                            var parentOperation = oldParentBlockBase as IEngineBlockOperationBase;

                            int length = parentOperation.GetVarGuidsLength();
                            bool result = false;
                            for (int i = 0; i < length; i++)
                            {
                                if (parentOperation.GetVarGuid(i) == engineBlockData.Guid)
                                {
                                    parentOperation.SetVarsGuid(i, ScratchUtils.InvalidGuid);
                                    result = true;
                                    break;
                                }
                            }

                            if (!result)
                            {
                                Debug.LogError("奇怪的操作：为甚么不在GetVarGuids里面？");
                                break;
                            }

                            break;
                        }
                        case BlockType.Define:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public static bool SetNewParentTrans(Block block)
        {
            var engineBlockData = block.GetEngineBlockData();
            //Set newParentTrans
            var tag = block.GetComponentInParent<IScratchSectionChild>();
            if (tag != null)
            {
                var parentSection = tag.GetSection() as BlockSection;
                var parentBlock = parentSection.Block;
                var ParentBlockBase = parentBlock.GetEngineBlockData();

                //Debug.Log(block.transform.name + " Parent changed to: " + (block.transform.parent != null ? block.transform.parent.name : "null") + " parentType:" + parentBlock.Type + " SiblingIndex: " + (block.transform.GetSiblingIndex()));

                switch (ParentBlockBase.BlockType)
                {
                    case BlockType.none:
                        break;
                    case BlockType.Trigger:
                    {
                        var parentTrigger = ParentBlockBase as IEngineBlockTriggerBase;
                        if (engineBlockData.BlockType == BlockType.Operation)
                        {
                            if (IsOnHeader(block, out var header, out var headindex))
                            {
                                if (block.VariableLabel != null)
                                {
                                    if (block.IsReturnValue)
                                    {
                                        var sections = parentBlock.GetChildSection();
                                        var headerOperations = sections[0].Header.GetHeaderOperation();
                                        int variableLen = parentTrigger.GetReturnValuesLength();
                                        if (variableLen != headerOperations.Length)
                                        {
                                            Debug.LogError("UI结构和数据结构对不上！:" + ParentBlockBase.Type);
                                            return false;
                                        }

                                        for (int i = 0; i < headerOperations.Length; i++)
                                        {
                                            if (headerOperations[i].OperationBlock == block)
                                            {
                                                parentTrigger.SetReturnValueGuid(i, engineBlockData.Guid);
                                                if (engineBlockData is IEngineBlockVariableBase variableBase)
                                                {
                                                    variableBase.ReturnParentGuid = parentTrigger.Guid;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (IsOnBody(block, out var body, out var bodyindex))
                            {
                                var siblingIndex = bodyindex;
                                if (siblingIndex == 0)
                                {
                                    engineBlockData.SetNextGuid(ParentBlockBase.GetNextGuid());
                                    ParentBlockBase.SetNextGuid(engineBlockData.Guid);
                                }
                                else
                                {
                                    if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                    {
                                        engineBlockData.SetNextGuid(preblock.GetEngineBlockData().GetNextGuid());
                                        preblock.GetEngineBlockData().SetNextGuid(engineBlockData.Guid);
                                    }
                                    else
                                    {
                                        engineBlockData.SetNextGuid(ParentBlockBase.GetNextGuid());
                                        ParentBlockBase.SetNextGuid(engineBlockData.Guid);
                                    }
                                }
                            }
                        }

                        break;
                    }
                    case BlockType.Simple:
                    {
                        var parentSimple = ParentBlockBase as IEngineBlockSimpleBase;
                        if (engineBlockData.BlockType == BlockType.Operation)
                        {
                            if (IsOnHeader(block, out var header, out var headindex))
                            {
                                var headerInputs = header.GetHeaderInput();
                                int variableLen = parentSimple.GetVarGuidsLength();
                                if (variableLen != headerInputs.Length)
                                {
                                    Debug.LogError("UI结构和数据结构对不上！:" + parentSimple.Type);
                                    return false;
                                }

                                for (int i = 0; i < variableLen; i++)
                                {
                                    if (CanSetInputVar(block, headerInputs[i], out var operation))
                                    {
                                        if (operation.OperationBlock == block)
                                        {
                                            parentSimple.SetVarsGuid(i, engineBlockData.Guid);
                                            //koalaBlock.NextBlockGuid = parentSimple.Guid; //operation执行后返回condition
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }

                    case BlockType.Condition:
                    {
                        var parentCondition = ParentBlockBase as IEngineBlockConditionBase;
                        int sectionIndex = parentSection.RectTrans.GetSiblingIndex();
                        if (engineBlockData.BlockType == BlockType.Operation)
                        {
                            var operations = parentCondition.BranchOperationBGuids;
                            if (sectionIndex >= operations.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + parentCondition.Type);
                                return false;
                            }

                            operations[sectionIndex] = engineBlockData.Guid;
                        }
                        else
                        {
                            if (IsOnBody(block, out var body, out var bodyindex))
                            {
                                //Body Index
                                var branchs = parentCondition.BranchBlockBGuids;
                                if (sectionIndex >= branchs.Length)
                                {
                                    Debug.LogError("UI结构和数据结构对不上！:" + parentCondition.Type);
                                    return false;
                                }

                                var siblingIndex = block.RectTrans.GetSiblingIndex();

                                if (siblingIndex == 0)
                                {
                                    engineBlockData.SetNextGuid(branchs[sectionIndex]);
                                    branchs[sectionIndex] = engineBlockData.Guid;
                                }
                                else
                                {
                                    if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                    {
                                        engineBlockData.SetNextGuid(preblock.GetEngineBlockData().GetNextGuid());
                                        preblock.GetEngineBlockData().SetNextGuid(engineBlockData.Guid);
                                    }
                                    else
                                    {
                                        engineBlockData.SetNextGuid(branchs[sectionIndex]);
                                        branchs[sectionIndex] = engineBlockData.Guid;
                                    }
                                }
                            }
                        }

                        break;
                    }

                    case BlockType.Loop:
                    {
                        var parentLoop = ParentBlockBase as IEngineBlockLoopBase;
                        if (engineBlockData.BlockType == BlockType.Operation)
                        {
                            if (IsOnHeader(block, out var header, out var headindex))
                            {
                                var headerInputs = header.GetHeaderInput();
                                int variableLen = parentLoop.GetVarGuidsLength();
                                if (variableLen != headerInputs.Length)
                                {
                                    Debug.LogError("UI结构和数据结构对不上！:" + parentLoop.Type);
                                    return false;
                                }

                                for (int i = 0; i < variableLen; i++)
                                {
                                    if (CanSetInputVar(block, headerInputs[i], out var operation))
                                    {
                                        if (operation.OperationBlock == block)
                                        {
                                            parentLoop.SetVarsGuid(i, engineBlockData.Guid);
                                            //koalaBlock.NextBlockGuid = parentLoop.Guid; //operation执行后返回condition
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (IsOnBody(block, out var body, out var bodyindex))
                            {
                                //Body
                                var siblingIndex = block.RectTrans.GetSiblingIndex();
                                if (siblingIndex == 0)
                                {
                                    engineBlockData.SetNextGuid(parentLoop.ChildRootGuid);
                                    parentLoop.ChildRootGuid = engineBlockData.Guid;
                                }
                                else
                                {
                                    if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                    {
                                        engineBlockData.SetNextGuid(preblock.GetEngineBlockData().GetNextGuid());
                                        preblock.GetEngineBlockData().SetNextGuid(engineBlockData.Guid);
                                    }
                                    else
                                    {
                                        engineBlockData.SetNextGuid(parentLoop.ChildRootGuid);
                                        parentLoop.ChildRootGuid = engineBlockData.Guid;
                                    }
                                }
                            }
                        }

                        break;
                    }

                    case BlockType.Operation:
                    {
                        var parentOperation = ParentBlockBase as IEngineBlockOperationBase;
                        if (IsOnHeader(block, out var header, out var headindex))
                        {
                            var headerInputs = header.GetHeaderInput();

                            int length = parentOperation.GetVarGuidsLength();
                            if (length != headerInputs.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + parentOperation.Type);
                                return false;
                            }

                            for (int i = 0; i < length; i++)
                            {
                                if (CanSetInputVar(block, headerInputs[i], out var variableoperation))
                                {
                                    if (variableoperation.OperationBlock == block)
                                    {
                                        parentOperation.SetVarsGuid(i, engineBlockData.Guid);
                                    }
                                }
                            }
                        }

                        break;
                    }

                    case BlockType.Define:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (block.GetEngineBlockData() is BlockFragmentDataRef dataRef)
            {
                var parentBlock = block.ParentTrans.GetComponentInParent<Block>();
                if (parentBlock != null)
                {
                    ScratchEngine.Instance.RemoveFragmentDataRef(dataRef);
                }
                else
                {
                    ScratchEngine.Instance.AddFragmentDataRef(dataRef);
                }
            }

            return true;
        }

        public static bool ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans)
        {
            var engineBlockData = block.GetEngineBlockData();

            if (engineBlockData == null) return false;

            //Clear orginParentTrans
            ClearOrginData(block, orginParentTrans);

            return SetNewParentTrans(block);
        }

        public void GetParentBlock(out Block parentBlock)
        {
            parentBlock = null;
            if (ParentTrans == null) return;

            var tag = ParentTrans.GetComponentInParent<Block>();
        }

        public static IEngineBlockBaseData FindPreBlock(int rootGuid, int CurGuid)
        {
            ScratchEngine.Instance.Current.TryGetDataRef(rootGuid, out var tempblock);

            while (tempblock != null)
            {
                int next = tempblock.GetNextGuid();
                if (next == CurGuid)
                {
                    return tempblock;
                }

                if (!ScratchEngine.Instance.Current.TryGetDataRef(next, out tempblock)) return null;
            }

            return null;
        }


        public static bool IsOnHeader(Block block, out BlockSectionHeader header, out int index)
        {
            header = null;
            index = -1;
            if (block.ParentTrans == null) return false;

            header = block.ParentTrans.GetComponent<BlockSectionHeader>();
            if (header != null)
            {
                index = block.RectTrans.GetSiblingIndex();
            }

            return header != null;
        }


        public static bool IsOnBody(Block block, out BlockSectionBody body, out int bodyindex)
        {
            body = null;
            bodyindex = -1;
            if (block.ParentTrans == null) return false;

            body = block.ParentTrans.GetComponent<BlockSectionBody>();
            if (body != null)
            {
                bodyindex = block.RectTrans.GetSiblingIndex();
            }

            return body != null;
        }


        public static bool GetPreBlock(Block block, out Block preblock, out int preblockIndex)
        {
            preblockIndex = -1;
            preblock = null;
            if (block.ParentTrans == null) return false;

            int childCount = block.ParentTrans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Block childBlock = block.ParentTrans.GetChild(i).GetComponent<Block>();
                if (childBlock != null)
                {
                    if (preblock == null && childBlock == block) return false; //没哥

                    if (childBlock == block)
                    {
                        preblockIndex = i;
                        return true;
                    }

                    preblock = childBlock; //查找下一个
                }
            }

            preblock = null;
            preblockIndex = -1;
            return false;
        }


        public static bool CanSetInputVar(Block block, BlockHeaderItem_Input input, out BlockHeaderItem_Operation operation)
        {
            operation = null;
            if (block.ParentTrans == null) return false;

            if (input == null) return false;
            int inputIndex = input.RectTrans.GetSiblingIndex();
            inputIndex--;
            if (inputIndex < 0) return false;

            operation = input.ParentTrans.GetChild(inputIndex).GetComponent<BlockHeaderItem_Operation>();
            return operation != null;
        }


        public static bool FindVarIndex(Block block, IEngineBlockBaseData parentBase, out int index)
        {
            index = -1;
            if (parentBase == null) return false;
            if (parentBase is IBlockVarGuid parentVarBase)
            {
                if (block.GetEngineBlockData() is IEngineBlockVariableBase)
                {
                    int len = parentVarBase.GetVarGuidsLength();
                    for (int i = 0; i < len; i++)
                    {
                        if (parentVarBase.GetVarGuid(i) == block.GetEngineBlockData().Guid)
                        {
                            index = i;
                            return true;
                        }
                    }
                }
            }


            return false;
        }

        #endregion
    }
}