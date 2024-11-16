using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            var engineBlockData = block.GetEngineBlockData();

            if (engineBlockData == null) return false;

            //Clear orginParentTrans
            if (orginParentTrans != null)
            {
                var oldTag = orginParentTrans.GetComponent<IScratchSectionChild>();
                if (oldTag != null)
                {
                    BlockSection oldParentSection = oldTag.GetSection() as BlockSection;
                    Block oldParentBlock = oldParentSection.Block;
                    IEngineBlockBaseData oldParentBlockBase = oldParentSection.Block.GetEngineBlockData();

                    switch (oldParentBlockBase.ClassName) //Old Parent Block
                    {
                        case FucType.Event:
                        {
                            //Header 理论上不存在清理数据，因为全是返回值

                            //Body
                            if (oldParentBlockBase.NextBlock.Guid == engineBlockData.Guid)
                            {
                                var preBlock = FindPreBlock(oldParentBlockBase.Guid, engineBlockData.Guid);
                                if (preBlock != null)
                                {
                                    preBlock.NextBlockGuid = engineBlockData.NextBlockGuid;
                                }
                            }
                        }
                            break;
                        case FucType.Condition:
                        {
                            var parentCondition = oldParentBlockBase as IEngineBlockConditionBase;

                            //0 Header
                            if (FindVarIndex(block, parentCondition, out var index))
                            {
                                //Header上的变量
                                parentCondition.SetVarsGuid(index, -1);
                                break;
                            }

                            //True 分支
                            if (parentCondition.TrueBlockGuid == engineBlockData.Guid)
                            {
                                parentCondition.TrueBlockGuid = engineBlockData.NextBlockGuid;
                            }

                            var preBlock = FindPreBlock(parentCondition.TrueBlockGuid, engineBlockData.Guid);
                            if (preBlock != null)
                            {
                                preBlock.NextBlockGuid = engineBlockData.NextBlockGuid;
                                break;
                            }

                            //False 分支
                            if (parentCondition.FalseBlockGuid == engineBlockData.Guid)
                            {
                                parentCondition.FalseBlockGuid = engineBlockData.NextBlockGuid;
                            }

                            preBlock = FindPreBlock(parentCondition.FalseBlockGuid, engineBlockData.Guid);
                            if (preBlock != null)
                            {
                                preBlock.NextBlockGuid = engineBlockData.NextBlockGuid;
                                break;
                            }
                        }
                            break;
                        case FucType.Control:
                        {
                            var parentLoop = oldParentBlockBase as IEngineBlockLoopBase;

                            //0 Header
                            if (FindVarIndex(block, parentLoop, out var index))
                            {
                                //Header上的变量
                                parentLoop.SetVarsGuid(index, -1);
                                break;
                            }

                            //0 Body
                            if (parentLoop.ChildRootGuid == engineBlockData.Guid)
                            {
                                parentLoop.ChildRootGuid = engineBlockData.NextBlockGuid;
                                break;
                            }

                            var preBlock = FindPreBlock(parentLoop.ChildRootGuid, engineBlockData.Guid);
                            if (preBlock != null)
                            {
                                preBlock.NextBlockGuid = engineBlockData.NextBlockGuid;
                                break;
                            }
                        }
                            break;
                        case FucType.Action:
                        {
                            var parentSimple = oldParentBlockBase as IEngineBlockSimpleBase;

                            //0 Header
                            if (FindVarIndex(block, parentSimple, out var index))
                            {
                                //Header上的变量
                                parentSimple.SetVarsGuid(index, -1);
                                break;
                            }
                        }
                            break;
                        case FucType.GetValue:
                        {
                            var parentOperation = oldParentBlockBase as IEngineBlockOperationBase;

                            //Var 1
                            if (parentOperation.Variable1Guid == engineBlockData.Guid)
                            {
                                parentOperation.Variable1Guid = -1;
                            }
                            //Var 2
                            else
                            {
                                parentOperation.Variable2Guid = -1;
                            }
                        }
                            break;
                        case FucType.Variable:
                        {
                            var parentValues = oldParentBlockBase as IEngineBlockVariableBase;

                            //0 Header
                            if (FindVarIndex(block, parentValues, out var index))
                            {
                                //Header上的变量
                                parentValues.SetVarsGuid(index, -1);
                                break;
                            }
                        }
                            break;
                        default:
                            break;
                    }
                }
            }

            //Set newParentTrans
            var tag = block.GetComponentInParent<IScratchSectionChild>();
            if (tag != null)
            {
                var parentSection = tag.GetSection() as BlockSection;
                var parentBlock = parentSection.Block;
                var ParentBlockBase = parentBlock.GetEngineBlockData();

                Debug.Log(block.transform.name + " Parent changed to: " + (block.transform.parent != null ? block.transform.parent.name : "null") + " parentType:" + parentBlock.Type + " SiblingIndex: " + (block.transform.GetSiblingIndex()));

                switch (ParentBlockBase.ClassName)
                {
                    case FucType.Event:
                    {
                        var parentTrigger = ParentBlockBase as IEngineBlockTriggerBase;

                        if (IsOnBody(block, out var body, out var bodyindex))
                        {
                            var siblingIndex = bodyindex;
                            if (siblingIndex == 0)
                            {
                                engineBlockData.NextBlockGuid = ParentBlockBase.NextBlockGuid;
                                ParentBlockBase.NextBlockGuid = engineBlockData.Guid;
                            }
                            else
                            {
                                if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                {
                                    engineBlockData.NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
                                    preblock.GetEngineBlockData().NextBlockGuid = engineBlockData.Guid;
                                }
                                else
                                {
                                    engineBlockData.NextBlockGuid = ParentBlockBase.NextBlockGuid;
                                    ParentBlockBase.NextBlockGuid = engineBlockData.Guid;
                                }
                            }
                        }

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
                                        }
                                    }
                                }
                            }
                        }
                    }
                        break;
                    case FucType.Condition:
                    {
                        var parentCondition = ParentBlockBase as IEngineBlockConditionBase;

                        if (IsOnHeader(block, out var header, out var headindex))
                        {
                            var headerInputs = header.GetHeaderInput();
                            int variableLen = parentCondition.GetVarGuidsLength();
                            if (variableLen != headerInputs.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + parentCondition.Type);
                                return false;
                            }

                            for (int i = 0; i < variableLen; i++)
                            {
                                if (GetOperationBlock(block, headerInputs[i], out var operation))
                                {
                                    if (operation.OperationBlock == block)
                                    {
                                        parentCondition.SetVarsGuid(i, engineBlockData.Guid);
                                        //koalaBlock.NextBlockGuid = parentCondition.Guid; //operation执行后返回condition
                                    }
                                }
                            }
                        }

                        if (IsOnBody(block, out var body, out var bodyindex))
                        {
                            //Body Index
                            int bodyBlockSectionIndex = parentSection.RectTrans.GetSiblingIndex();

                            //0 Body 
                            if (bodyBlockSectionIndex == 0)
                            {
                                var siblingIndex = block.RectTrans.GetSiblingIndex();
                                if (siblingIndex == 0)
                                {
                                    engineBlockData.NextBlockGuid = parentCondition.TrueBlockGuid;
                                    parentCondition.TrueBlockGuid = engineBlockData.Guid;
                                }
                                else
                                {
                                    if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                    {
                                        engineBlockData.NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
                                        preblock.GetEngineBlockData().NextBlockGuid = engineBlockData.Guid;
                                    }
                                    else
                                    {
                                        engineBlockData.NextBlockGuid = parentCondition.TrueBlockGuid;
                                        parentCondition.TrueBlockGuid = engineBlockData.Guid;
                                    }
                                }
                            }

                            //1 Body 
                            if (bodyBlockSectionIndex == 1)
                            {
                                var siblingIndex = block.RectTrans.GetSiblingIndex();
                                if (siblingIndex == 0)
                                {
                                    engineBlockData.NextBlockGuid = parentCondition.FalseBlockGuid;
                                    parentCondition.FalseBlockGuid = engineBlockData.Guid;
                                }
                                else
                                {
                                    if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                    {
                                        engineBlockData.NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
                                        preblock.GetEngineBlockData().NextBlockGuid = engineBlockData.Guid;
                                    }
                                    else
                                    {
                                        engineBlockData.NextBlockGuid = parentCondition.FalseBlockGuid;
                                        parentCondition.FalseBlockGuid = engineBlockData.Guid;
                                    }
                                }
                            }
                        }
                    }
                        break;
                    case FucType.Control:
                    {
                        var parentLoop = ParentBlockBase as IEngineBlockLoopBase;

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
                                if (GetOperationBlock(block, headerInputs[i], out var operation))
                                {
                                    if (operation.OperationBlock == block)
                                    {
                                        parentLoop.SetVarsGuid(i, engineBlockData.Guid);
                                        //koalaBlock.NextBlockGuid = parentLoop.Guid; //operation执行后返回condition
                                    }
                                }
                            }
                        }

                        if (IsOnBody(block, out var body, out var bodyindex))
                        {
                            //Body
                            var siblingIndex = block.RectTrans.GetSiblingIndex();
                            if (siblingIndex == 0)
                            {
                                engineBlockData.NextBlockGuid = parentLoop.ChildRootGuid;
                                parentLoop.ChildRootGuid = engineBlockData.Guid;
                            }
                            else
                            {
                                if (GetPreBlock(block, out var preblock, out var preblockIndex))
                                {
                                    engineBlockData.NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
                                    preblock.GetEngineBlockData().NextBlockGuid = engineBlockData.Guid;
                                }
                                else
                                {
                                    engineBlockData.NextBlockGuid = parentLoop.ChildRootGuid;
                                    parentLoop.ChildRootGuid = engineBlockData.Guid;
                                }
                            }
                        }
                    }
                        break;
                    case FucType.Action:
                    {
                        var parentSimple = ParentBlockBase as IEngineBlockSimpleBase;

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
                                if (GetOperationBlock(block, headerInputs[i], out var operation))
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
                    case FucType.GetValue:
                    {
                        var parentOperation = ParentBlockBase as IEngineBlockOperationBase;
                        if (IsOnHeader(block, out var header, out var headindex))
                        {
                            var headerInputs = header.GetHeaderInput();
                            if (2 != headerInputs.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + parentOperation.Type);
                                return false;
                            }

                            //variable1
                            if (GetOperationBlock(block, headerInputs[0], out var variable1operation))
                            {
                                if (variable1operation.OperationBlock == block)
                                {
                                    parentOperation.SetVarsGuid(0, engineBlockData.Guid);
                                    //koalaBlock.NextBlockGuid = parentOperation.Guid; //operation执行后返回condition
                                }
                            }

                            //variable2
                            if (GetOperationBlock(block, headerInputs[1], out var variable2operation))
                            {
                                if (variable2operation.OperationBlock == block)
                                {
                                    parentOperation.SetVarsGuid(1, engineBlockData.Guid);
                                    //koalaBlock.NextBlockGuid = parentOperation.Guid; //operation执行后返回condition
                                }
                            }
                        }
                    }
                        break;
                    case FucType.Variable:
                    {
                        var parentVariable = ParentBlockBase as IEngineBlockVariableBase;
                        if (IsOnHeader(block, out var header, out var headindex))
                        {
                            var headerInputs = header.GetHeaderInput();
                            int variableLen = parentVariable.GetVarGuidsLength();
                            if (variableLen != headerInputs.Length)
                            {
                                Debug.LogError("UI结构和数据结构对不上！:" + parentVariable.Type);
                                return false;
                            }

                            for (int i = 0; i < variableLen; i++)
                            {
                                if (GetOperationBlock(block, headerInputs[i], out var operation))
                                {
                                    if (operation.OperationBlock == block)
                                    {
                                        parentVariable.SetVarsGuid(i, engineBlockData.Guid);
                                        //koalaBlock.NextBlockGuid = parentVariable.Guid; //operation执行后返回condition
                                    }
                                }
                            }
                        }
                    }

                        break;

                    default: throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        public bool TryFixedBlockBaseDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            if (m_blocks.ContainsKey(blockBaseData.Guid))
            {
                m_blocks[blockBaseData.Guid].CanvasPos = pos;
            }

            return true;
        }

        public bool UpdateDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            if (m_blocks.ContainsKey(blockBaseData.Guid))
            {
                m_blocks[blockBaseData.Guid].CanvasPos = pos;
            }

            return true;
        }

        /// <summary>
        /// 删除方块或者方块组
        /// </summary>
        /// <param name="block"></param>
        /// <param name="recursion"></param>
        public void DeleteBlock(IEngineBlockBaseData block, bool recursion = true)
        {
            if (block != null)
            {
                if (m_blocks.ContainsKey(block.Guid))
                {
                    if (recursion)
                    {
                        //TODO 删除关联关系
                    }

                    m_blocks.Remove(block.Guid);
                }
            }
        }

        public IEngineBlockBaseData CreateBlock(ScratchBlockType scratchType, bool isAdd = true)
        {
            IEngineBlockBaseData block = null;
            block = scratchType.CreateBlockData();

            if (isAdd)
            {
                // Debug.LogError(scratchType);
                block.Guid = Guid.NewGuid().GetHashCode();
                while (m_blocks.ContainsKey(block.Guid))
                {
                    block.Guid = Guid.NewGuid().GetHashCode();
                }

                m_blocks.Add(block.Guid, block);
            }

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

        /// <summary> 在Header上 </summary>
        private bool IsOnHeader(Block block, out BlockSectionHeader header, out int index)
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

        /// <summary> 在Body上 </summary>
        private bool IsOnBody(Block block, out BlockSectionBody body, out int bodyindex)
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

        /// <summary> 查找新哥哥 </summary>
        private bool GetPreBlock(Block block, out Block preblock, out int preblockIndex)
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

        /// <summary> 查找Input绑定Operation </summary>
        private bool GetOperationBlock(Block block, BlockHeaderItem_Input input, out BlockHeaderItem_Operation operation)
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

        /// <summary> 查找变量索引 </summary>
        private bool FindVarIndex(Block block, IEngineBlockBaseData parentBase, out int index)
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

        private readonly string tempJsonfileUrl = "file://" + tempJsonfilePath;


        IEnumerator GetJsonFile(string pathUrl, Action<Stream> callback = null)
        {
            WWW www = new WWW(pathUrl);
            yield return www;

            if (www.isDone)
            {
                if (www.bytes != null)
                {
                    var stream = new MemoryStream(www.bytes);
                    callback?.Invoke(stream);
                }
            }

            yield break;
        }

        public void GenerateBlocks(string filepath = null, Action<List<Block>> callback = null)
        {
            List<Block> blocks = new List<Block>();
            List<IEngineBlockBaseData> blockDatas = null;

            if (string.IsNullOrEmpty(filepath))
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(tempJsonfileUrl, (stream) =>
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        try
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings();
                            settings.TypeNameHandling = TypeNameHandling.All;
                            blockDatas = JsonConvert.DeserializeObject<List<IEngineBlockBaseData>>(json, settings);
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
                            m_blocks[blockDatas[i].Guid] = blockDatas[i];
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

                        callback?.Invoke(blocks);
                    }
                }));
            }
            else
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(filepath, (stream) =>
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        try
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings();
                            settings.TypeNameHandling = TypeNameHandling.All;
                            blockDatas = JsonConvert.DeserializeObject<List<IEngineBlockBaseData>>(json, settings);
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

                        callback?.Invoke(blocks);
                    }
                }));
            }
        }

        #region 生成预支存储数据

        private static readonly string tempJsonfilePath = Application.streamingAssetsPath + "/TempCanvas/TestCanvas.json";

        public void SaveBlocks(string filepath = null, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = tempJsonfilePath;
            }

            if (File.Exists(filepath)) File.Delete(filepath);
            FileStream stream = new FileStream(filepath, FileMode.CreateNew);
            using (stream)
            {
                List<IEngineBlockBaseData> blockDatas = new List<IEngineBlockBaseData>();
                foreach (var block in m_blocks)
                {
                    blockDatas.Add(block.Value);
                }

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                settings.Formatting = Formatting.Indented;

                var json = JsonConvert.SerializeObject(blockDatas, settings);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public bool VariableValue2String(IEngineBlockVariableBase blockBase, out string value)
        {
            value = String.Empty;

            switch (blockBase.ValueType)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    if (blockBase.VariableValue == null)
                    {
                        value = bool.FalseString;
                        return false;
                    }

                    break;
                case ScratchValueType.Byte:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return false;
                    }

                    break;
                case ScratchValueType.Integer:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return false;
                    }

                    break;
                case ScratchValueType.Float:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0.00";
                        return false;
                    }

                    break;
                case ScratchValueType.Vector2:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector2.zero.ToString();
                        return false;
                    }

                    break;
                case ScratchValueType.Vector3:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector3.zero.ToString();
                        return false;
                    }

                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        private bool TryParseVector3(string value, out Vector3 result)
        {
            result = Vector3.zero;
            value = value.Trim(new char[] { '(', ')' });
            string[] sArray = value.Split(',');
            if (sArray.Length != 3) return false;
            else
            {
                result = new Vector3(
                    float.Parse(sArray[0]),
                    float.Parse(sArray[1]),
                    float.Parse(sArray[2]));
                return true;
            }
        }

        private bool TryParseVector2(string value, out Vector2 result)
        {
            result = Vector2.zero;
            value = value.Trim(new char[] { '(', ')' });
            string[] sArray = value.Split(',');
            if (sArray.Length != 2) return false;
            else
            {
                result = new Vector2(
                    float.Parse(sArray[0]),
                    float.Parse(sArray[1]));
                return true;
            }
        }

        public bool String2VariableValueTo(IEngineBlockVariableBase blockBase, string value)
        {
            switch (blockBase.ValueType)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    if (bool.TryParse(value, out bool boolresult))
                    {
                        blockBase.VariableValue = boolresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Byte:
                    if (byte.TryParse(value, out byte byteresult))
                    {
                        blockBase.VariableValue = byteresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Integer:
                    if (int.TryParse(value, out int intresult))
                    {
                        blockBase.VariableValue = intresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Float:
                    if (float.TryParse(value, out float floatresult))
                    {
                        blockBase.VariableValue = floatresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Vector2:
                    if (TryParseVector2(value, out Vector2 vector2result))
                    {
                        blockBase.VariableValue = vector2result;
                        return true;
                    }

                    break;
                case ScratchValueType.Vector3:
                    if (TryParseVector3(value, out Vector3 vector3result))
                    {
                        blockBase.VariableValue = vector3result;
                        return true;
                    }

                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
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
            if (check.Contains(node.Guid) && node.ClassName != FucType.Variable) return null;
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
                case FucType.Event: //Event
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
                case FucType.Condition: //if else
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
                case FucType.Control: //Reapeat Timer
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
                case FucType.Action: //Controls KoalaStartCountdownBlock？？？？、Actions KoalaApplyForceBlock
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
                case FucType.GetValue:
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
                case FucType.Variable:
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