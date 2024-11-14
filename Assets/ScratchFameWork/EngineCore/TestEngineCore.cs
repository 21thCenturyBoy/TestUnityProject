using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// 单元测试引擎核心
    /// </summary>
    public class TestEngineCore : IEngineCoreInterface
    {
        Dictionary<int, IEngineBlockBaseData> m_blocks = new Dictionary<int, IEngineBlockBaseData>();

        public string GetEngineVersion()
        {
            return string.Empty;
        }

        public Dictionary<int, IEngineBlockBaseData> GetAllBlocks()
        {
            return m_blocks;
        }

        public IEngineBlockBaseData GetBlocksDataRef(int guid)
        {
            if (m_blocks.ContainsKey(guid)) return m_blocks[guid];
            return null;
        }

        public bool ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans)
        {
            return true;
        }

        public bool TryFixedBlockBaseDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            return true;
        }

        public bool UpdateDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            return true;
        }

        public void DeleteBlock(IEngineBlockBaseData block, bool recursion = true)
        {
            // TexturePacker_JsonArray.Frame f = KoalaQuantumRunner.GetCurrentFrame(KoalaQuantumRunner.FrameType.Verified);
            // KoalaScratchUtil.Instance.DeleteBlock(f, block.blockData);
        }

        public IEngineBlockBaseData CreateBlock(ScratchBlockType scratchType, bool isAdd = true)
        {
            // Log.Info("CreateBlock:" + scratchType);
            IEngineBlockBaseData block = null;
            block = scratchType.CreateBlockData();

            // if (AllBlocks == null) AllBlocks = new Dictionary<int, KoalaBlockBase>();
            // if (isAdd)
            // {
            //     block.Guid = Guid.NewGuid().GetHashCode();
            //     while (AllBlocks.ContainsKey(block.Guid))
            //     {
            //         block.Guid = Guid.NewGuid().GetHashCode();
            //     }
            //
            //     AllBlocks.Add(block.Guid, block);
            // }
            //
            return block;
        }

        public IEngineBlockBaseData FindPreBlock(int rootGuid, int CurGuid)
        {
            var tempblock = GetBlocksDataRef(rootGuid);

            while (tempblock != null)
            {
                if (tempblock.NextBlockGuid == CurGuid)
                {
                    return tempblock;
                }

                tempblock = GetBlocksDataRef(tempblock.NextBlockGuid);
            }

            return null;
        }

        #region 生成预支存储数据

        public List<Block> GenerateBlocks(string fileName = null)
        {
            List<Block> blocks = new List<Block>();
            List<IEngineBlockBaseData> blockDatas = null;

            TextAsset textAsset = null;
            if (string.IsNullOrEmpty(fileName))
            {
                textAsset = Resources.Load<TextAsset>("TempCanvas/TestCanvas");
            }
            else
            {
                textAsset = Resources.Load<TextAsset>(fileName);
            }
            try
            {
                blockDatas = JsonConvert.DeserializeObject<List<IEngineBlockBaseData>>(textAsset.text);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            if (blockDatas == null)
            {
                blockDatas = new List<IEngineBlockBaseData>();
            }

            for (int i = 0; i < blockDatas.Count; i++)
            {
                var rootGuid = blockDatas[i].Guid;
                var blockUI = PaintBlockInfo(rootGuid, blockDatas[i].CanvasPos);

                if (blockUI != null) blocks.Add(blockUI);
            }

            //Fixed UI数据 Operation绑定关系
            for (int i = 0; i < blocks.Count; i++)
            {
                var childBlocks = blocks[i].GetComponentsInChildren<Block>();
                for (int j = 0; j < childBlocks.Length; j++)
                {
                    var childBlock = childBlocks[j];
                    if (childBlock.Type == BlockType.Operation)
                    {
                        //检查一下 Input赋值
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

            return blocks;
        }

        public void SaveBlocks(string filepath = null, Action<bool> callback = null)
        {

        }

#if UNITY_EDITOR

        public bool BlockCreateCSFile(string blockCreateName)
        {
            return true;
        }

#endif

        HashSet<int> check = new HashSet<int>();

        private Block PaintBlockInfo(int guid, Vector3 pos)
        {
            var block = GetBlocksDataRef(guid);
            if (block == null) return null;
            check.Clear();

            var blockview = DrawNode(block, BlockCanvasManager.Instance.RectTrans, -1);
            blockview.transform.position = pos;

            return blockview.GetComponent<Block>();
        }

        private Block DrawNode(IEngineBlockBaseData node, Transform parentTrans, int index = -1)
        {
            if (node == null) return null;
            if (check.Contains(node.Guid) && node.ClassName != ScratchClassName.Values) return null;
            if (!check.Contains(node.Guid)) check.Add(node.Guid);

            var ResourceItem = ScratchResourcesManager.Instance.TemplateResourcesDict[node.Type];
            Block blockUI = ResourceItem.CreateBlock(node);

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


            switch (node.ClassName)
            {
                case ScratchClassName.Trigger: //Event
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockTriggerBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerOperations = section0.Header.GetHeaderOperation();
                        int variableLen = _node.GetReturnValuesLength();
                        if (variableLen != headerOperations.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            int guid = _node.GetReturnValueGuid(i);
                            if (headerOperations[i].OperationBlock.GetEngineBlockData() == null)
                            {
                                var operationBlock = GetBlocksDataRef(guid);
                                headerOperations[i].OperationBlock.SetKoalaBlock(operationBlock);
                            }
                        }
                    }

                    //0 Body 
                    if (section0.Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), section0.Body.RectTrans);
                    }
                }
                    break;
                case ScratchClassName.Condition: //if else
                {
                    var sections = blockUI.GetChildSection();

                    var _node = node as IEngineBlockConditionBase;
                    //0 Header 
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

                    //0 Body 
                    if (sections[0].Body != null)
                    {
                        var childTrueBlock = DrawNode(GetBlocksDataRef(_node.TrueBlockGuid), sections[0].Body.RectTrans);
                    }

                    //1 Body 
                    if (sections[1].Body != null)
                    {
                        var childFalseBlock = DrawNode(GetBlocksDataRef(_node.FalseBlockGuid), sections[1].Body.RectTrans);
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Loop: //Reapeat Timer
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockLoopBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = _node.GetVarGuidsLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    //0 Body 
                    if (section0.Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(_node.ChildRootGuid), section0.Body.RectTrans);
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Simple: //Controls KoalaStartCountdownBlock？？？？、Actions KoalaApplyForceBlock
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockSimpleBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = _node.GetVarGuidsLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Operation:
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockOperationBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();

                        if (2 != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        var variable1Block = DrawNode(GetBlocksDataRef(_node.Variable1Guid), section0.Header.RectTrans, headerInputs[0].RectTrans.GetSiblingIndex());
                        var variable2Block = DrawNode(GetBlocksDataRef(_node.Variable2Guid), section0.Header.RectTrans, headerInputs[1].RectTrans.GetSiblingIndex());
                    }
                }
                    break;
                case ScratchClassName.Values:
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockVariableBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = _node.GetVarGuidsLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }
                }
                    break;
            }

            return blockUI;
        }

        #endregion
    }
}