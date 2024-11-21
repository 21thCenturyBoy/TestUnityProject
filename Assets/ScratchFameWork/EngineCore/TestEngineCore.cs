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
                                if (GetOperationBlock(block, headerInputs[i], out var variableoperation))
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
                block.Guid = ScratchUtils.CreateGuid();
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
                if (tempblock.GetNextGuid() == CurGuid)
                {
                    return tempblock;
                }

                tempblock = GetBlocksDataRef(tempblock.GetNextGuid());
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

        private bool TryDeserializeBlockDatas(Stream stream)
        {
            Dictionary<int, IEngineBlockBaseData> allBlockDatas = new Dictionary<int, IEngineBlockBaseData>();

            List<Block> blocks = new List<Block>();
            List<IEngineBlockBaseData> blockDatas = null;
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.All;
                    settings.Converters = new List<JsonConverter> { new GuidListConverter() };
                    blockDatas = JsonConvert.DeserializeObject<List<IEngineBlockBaseData>>(json, settings);

                    if (blockDatas != null)
                    {
                        for (int i = 0; i < blockDatas.Count; i++)
                        {
                            allBlockDatas[blockDatas[i].Guid] = blockDatas[i];
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    return false;
                }

                if (blockDatas == null)
                {
                    blockDatas = new List<IEngineBlockBaseData>();
                }

                m_blocks = allBlockDatas;

                for (int i = 0; i < blockDatas.Count; i++)
                {
                    var block = blockDatas[i];
                    if (block.IsRoot)
                    {
                        var blockview = DrawNode(block, BlockCanvasManager.Instance.RectTrans, -1);
                        blockview.transform.position = blockDatas[i].CanvasPos;
                        if (blockview != null)
                        {
                            blocks.Add(blockview);
                        }
                    }
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

                return true;
            }
        }

        public void GenerateBlocks(string filepath = null, Action<List<Block>> callback = null)
        {
            List<Block> blocks = new List<Block>();
            List<IEngineBlockBaseData> blockDatas = null;

            if (string.IsNullOrEmpty(filepath))
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(tempJsonfileUrl, (stream) => { TryDeserializeBlockDatas(stream); }));
            }
            else
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(filepath, (stream) => { TryDeserializeBlockDatas(stream); }));
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
                settings.Converters = new List<JsonConverter> { new GuidListConverter() };
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


        private Block DrawNode(IEngineBlockBaseData node, Transform parentTrans, int index = -1)
        {
            if (node == null) return null;

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
                            if (headerOperations[i].OperationBlock.GetEngineBlockData() == null)
                            {
                                var operationBlock = GetBlocksDataRef(guid);
                                headerOperations[i].OperationBlock.SetKoalaBlock(operationBlock);
                            }
                        }
                    }

                    if (sections[0].Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(node.GetNextGuid()), sections[0].Body.RectTrans);
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

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextGuid()), parentTrans);
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
                        var childBlock = DrawNode(GetBlocksDataRef(_node.ChildRootGuid), sections[0].Body.RectTrans);
                    }

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextGuid()), parentTrans);
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
                            var branchBlock = DrawNode(GetBlocksDataRef(_node.BranchBlockBGuids[i]), section.Body.RectTrans);
                        }
                    }

                    var nextBlock = DrawNode(GetBlocksDataRef(node.GetNextGuid()), parentTrans);
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
                        var _node = node as IEngineBlockVariableBase;
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

        #endregion
    }
}