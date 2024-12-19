using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace ScratchFramework
{
    using UnityObject = UnityEngine.Object;

    public static partial class ScratchUtils
    {
        public const string Block_PefixName = "Body";
        public const string Section_PefixName = "Section";
        public const string SectionHeader_PefixName = "Header";
        public const string SectionBody_PefixName = "Body";

        public static T GetOrAddComponent<T>(this UnityObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public const int InvalidGuid = 0;

        public static int CreateGuid()
        {
            return CreateGuid(out var guid);
        }

        public static int CreateGuid(out Guid guid)
        {
            guid = Guid.NewGuid();
            int hashCode = guid.GetHashCode();
            var repeatGuid = ScratchEngine.Instance.ContainGuids(hashCode);
            while (hashCode == InvalidGuid || repeatGuid)
            {
                guid = Guid.NewGuid();
                hashCode = guid.GetHashCode();
            }

            return hashCode;
        }

        public static string VariableKoalaBlockToString(IEngineBlockVariableBase blockBase)
        {
            if (ScratchEngine.Instance.Core.VariableValue2String(blockBase, out var strRes))
            {
                return strRes;
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool SetNextGuid(this IEngineBlockBaseData blockBase, int nextGuid)
        {
            if (blockBase is IBlockPlug plug)
            {
                plug.NextGuid = nextGuid;
                return true;
            }

            return false;
        }

        public static int GetNextGuid(this IEngineBlockBaseData blockBase)
        {
            if (blockBase is IBlockPlug plug)
            {
                return plug.NextGuid;
            }

            return InvalidGuid;
        }

        public static int GetBranchCount(this IEngineBlockBranch branch)
        {
            return branch.BranchBlockBGuids.Length;
        }

        public static bool String2VariableKoalaBlock(string str, IEngineBlockVariableBase blockBase)
        {
            return ScratchEngine.Instance.Core.String2VariableValueTo(blockBase, str);
        }

        public static void CreateVariableName(IEngineBlockVariableBase blockdata)
        {
            //创建变量名
            if (string.IsNullOrEmpty(blockdata.VariableName))
            {
                string variableRef = blockdata.Guid.ToString();
                switch (blockdata.Type)
                {
                    case ScratchBlockType.IntegerValue:
                        blockdata.VariableName = $"[int]{variableRef}";
                        break;
                    case ScratchBlockType.VectorValue:
                        blockdata.VariableName = $"[Vector3]{variableRef}";
                        break;
                    case ScratchBlockType.EntityValue:
                        blockdata.VariableName = $"[Entity]{variableRef}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static void RefreshVariableName(IEngineBlockVariableBase blockdata, IHeaderParamVariable variableData)
        {
            variableData.VariableRef = blockdata.Guid.ToString();
        }

        /// <summary>
        /// 反序列化UI模版
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="parentTrans"></param>
        /// <returns></returns>
        public static Block DeserializeBlock(byte[] datas, RectTransform parentTrans = null)
        {
            MemoryStream memoryStream = CreateMemoryStream(datas);

            BlockData newBlockData = new BlockData();
            newBlockData.BlockData_Deserialize(memoryStream, ScratchConfig.Instance.Version, true);

            Block newblock = null;

            newblock = BlockCreator.CreateBlock(newBlockData, parentTrans);

            return newblock;
        }

        /// <summary>
        /// 刷新Guid
        /// </summary>
        /// <param name="blockDatas"></param>
        public static void RefreshDataGuids(List<IEngineBlockBaseData> blockDatas, Dictionary<int, int> guidMap = null)
        {
            bool autoRefresh = guidMap == null;
            if (autoRefresh)
            {
                guidMap = new Dictionary<int, int>();

                foreach (IEngineBlockBaseData baseData in blockDatas)
                {
                    if (guidMap.ContainsKey(baseData.Guid) || baseData.Guid == ScratchUtils.InvalidGuid)
                    {
                        Debug.LogError("failed to get guid Fixed：" + baseData.Guid);
                        baseData.Guid = ScratchUtils.CreateGuid();
                    }

                    guidMap[baseData.Guid] = baseData.Guid;
                }

                int[] orginkeys = guidMap.Keys.ToArray();
                foreach (int key in orginkeys)
                {
                    guidMap[key] = ScratchUtils.CreateGuid();
                }
            }

            foreach (IEngineBlockBaseData baseData in blockDatas)
            {
                baseData.RefreshGuids(guidMap);
            }
        }

        /// <summary>
        /// 绘制根节点
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="parentTrans"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<Block> DrawNodeRoot(IEngineBlockBaseData rootNode, Transform parentTrans, int index = -1)
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

        private static Block DrawNode(IEngineBlockBaseData node, Transform parentTrans, int index = -1)
        {
            if (node == null) return null;

            var ResourceItem = ScratchResourcesManager.Instance.GetResourcesItemData(node.Type);
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
                        
                        EngineVariableRef variableRef = new EngineVariableRef(_node);
                        variableRef.UIGuid = blockUI.BlockId;
                        ScratchEngine.Instance.Current.VariableRefs.Add(variableRef);
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

        public static IEngineBlockBaseData GetBlocksDataRef(int guid)
        {
            return ScratchEngine.Instance.Current[guid];
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

        public static IEngineBlockBaseData CreateBlockData(ScratchBlockType scratchType)
        {
            // IEngineBlockBaseData block = null;
            IEngineBlockBaseData block = scratchType.CreateBlockData();
            block.Guid = CreateGuid();

            return block;
        }

        public static bool IsReturnVariable(this IEngineBlockBaseData blockBase)
        {
            if (blockBase is IEngineBlockVariableBase variableBase)
            {
                return variableBase.ReturnParentGuid != InvalidGuid;
            }

            return false;
        }

        /// <summary>
        /// 克隆Block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block CloneBlock(Block block)
        {
            GetBlockDataTree(block.GetEngineBlockData().Guid, out var tree);

            Dictionary<int, int> dataMapGuids = new Dictionary<int, int>();
            HashSet<int> newblockGuids = new HashSet<int>();
            List<IEngineBlockBaseData> newblockDatas = new List<IEngineBlockBaseData>();

            tree.TraverseTree((deep, bNode) =>
            {
                if (bNode.Value == InvalidGuid) return true;

                IEngineBlockBaseData baseData = ScratchEngine.Instance.Current[bNode.Value];

                if (baseData == null)
                {
                    Debug.LogError("baseData is null");
                    return true;
                }

                if (newblockGuids.Contains(baseData.Guid))
                {
                    Debug.LogError("baseData Contains");
                    return true;
                }

                if (bNode.Parent != null && baseData is IEngineBlockVariableBase variableBase)
                {
                    if (variableBase.ReturnParentGuid != bNode.Parent.Value) return true; //Ref
                }

                IEngineBlockBaseData newData = ScratchUtils.CreateBlockData(baseData.Type);

                dataMapGuids[baseData.Guid] = newData.Guid;

                baseData.CopyData(ref newData);

                newData.Guid = dataMapGuids[baseData.Guid];

                newblockGuids.Add(newData.Guid);
                newblockDatas.Add(newData);

                return true;
            });

            for (int i = 0; i < newblockDatas.Count; i++)
            {
                if (!ScratchEngine.Instance.AddBlocksData(newblockDatas[i]))
                {
                    Debug.LogError("Engine Add Block Error:" + newblockDatas[i].Guid);
                }
            }

            RefreshDataGuids(newblockDatas, dataMapGuids);

            BlockCanvasManager.Instance.RefreshCanvas();
            return null;
        }


        public static int GetDataId(this ScratchVMData data)
        {
            if (data == null) return ScratchVMData.UnallocatedId;
            return data.IdPtr;
        }

        public static int GetDataId(this IBlockHeadData data)
        {
            if (data == null) return ScratchVMData.UnallocatedId;
            return GetDataId(data as ScratchVMData);
        }

        public static void DestroyBlock(Block block, bool refreshCanvas = true, bool recursion = true)
        {
            var m_blocks = ScratchEngine.Instance.Current.GetKeys();
            var blockBaseData = block.GetEngineBlockData();
            if (blockBaseData != null)
            {
                if (m_blocks.Contains(blockBaseData.Guid))
                {
                    if (recursion)
                    {
                        HashSet<int> hashSet = new HashSet<int>();
                        GetBlockDataTree(blockBaseData.Guid, out var tree);
                        tree.TraverseTree((deep, bNode) =>
                        {
                            hashSet.Add(bNode.Value);
                            return true;
                        });

                        var needSave = new List<int>();
                        foreach (int guid in hashSet)
                        {
                            if (m_blocks.Contains(guid) && ScratchEngine.Instance.Current[guid] is IEngineBlockVariableBase variableBase)
                            {
                                if (variableBase.IsReturnVariable())
                                {
                                    if (!hashSet.Contains(variableBase.ReturnParentGuid))
                                    {
                                        needSave.Add(variableBase.Guid);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < needSave.Count; i++)
                        {
                            hashSet.Remove(needSave[i]);
                        }


                        foreach (int removeGuid in hashSet)
                        {
                            ScratchEngine.Instance.Current.TryGetDataRef(removeGuid, out var removeData);
                            if (removeData != null)
                            {
                                ScratchEngine.Instance.RemoveBlocksData(removeData);
                            }
                        }

                        for (int j = 0; j < m_blocks.Length; j++)
                        {
                            var logicBlock = ScratchEngine.Instance.Current[m_blocks[j]];
                            if (logicBlock != null)
                            {
                                var logicBlockGuids = logicBlock.GetGuids();
                                Dictionary<int, int> map = new Dictionary<int, int>();
                                for (int i = 0; i < logicBlockGuids.Length; i++)
                                {
                                    if (hashSet.Contains(logicBlockGuids[i]))
                                    {
                                        map[logicBlockGuids[i]] = InvalidGuid;
                                    }
                                }

                                logicBlock.RefreshGuids(map);
                            }
                        }
                    }
                    else
                    {
                        ScratchEngine.Instance.RemoveBlocksData(blockBaseData);
                    }

                    if (refreshCanvas)
                    {
                        BlockCanvasManager.Instance.RefreshCanvas();
                    }
                }
            }
        }

        public static Vector3 ScreenPos2WorldPos(this ScratchUIBehaviour transform, Vector2 screenPos)
        {
            Vector3 worldPos = Vector3.zero;
            var camera = ScratchManager.Instance.Canvas.worldCamera;
            switch (ScratchManager.Instance.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    worldPos = screenPos;
                    break;
                case RenderMode.ScreenSpaceCamera:
                    worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.Position.z));
                    break;
                case RenderMode.WorldSpace:
                    worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.Position.z));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return worldPos;
        }

        public static Vector3 WorldPos2ScreenPos(Vector3 worldPos)
        {
            Vector3 screenPos = Vector3.zero;
            var camera = ScratchManager.Instance.Canvas.worldCamera;
            switch (ScratchManager.Instance.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    screenPos = worldPos;
                    break;
                case RenderMode.ScreenSpaceCamera:
                    screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    break;
                case RenderMode.WorldSpace:
                    screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return screenPos;
        }

        public static void SetParent(this ScratchBehaviour scratch, Transform parent)
        {
            scratch.transform.SetParent(parent);
        }

        public static void SetParent(this ScratchBehaviour scratch, Transform parent, int index)
        {
            scratch.transform.SetParent(parent);
            scratch.transform.SetSiblingIndex(index);
        }

        public static void SetParent(this ScratchBehaviour scratch, ScratchBehaviour parent)
        {
            scratch.SetParent(parent.transform);
        }

        private static Material s_material;

        internal static void DrawScreenRect(Rect rect, Color color)
        {
            if (s_material == null)
            {
                s_material = new Material(Shader.Find("Unlit/Color"));
            }

            s_material.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            s_material.SetColor("_Color", color);

            GL.Vertex3(rect.xMin / Screen.width, rect.yMin / Screen.height, 0);
            GL.Vertex3(rect.xMin / Screen.width, rect.yMax / Screen.height, 0);

            GL.Vertex3(rect.xMin / Screen.width, rect.yMax / Screen.height, 0);
            GL.Vertex3(rect.xMax / Screen.width, rect.yMax / Screen.height, 0);

            GL.Vertex3(rect.xMax / Screen.width, rect.yMax / Screen.height, 0);
            GL.Vertex3(rect.xMax / Screen.width, rect.yMin / Screen.height, 0);

            GL.Vertex3(rect.xMax / Screen.width, rect.yMin / Screen.height, 0);
            GL.Vertex3(rect.xMin / Screen.width, rect.yMin / Screen.height, 0);

            GL.End();
        }

        internal static void DrawScreenEllipse(Vector2 center, float xRadius, float yRadius, Color color, Material material = null, int smooth = 50)
        {
            GL.PushMatrix();
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color"));
            }

            material.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            material.SetColor("_Color", color);

            for (int i = 0; i < smooth; ++i)
            {
                int nextStep = (i + 1) % smooth;
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * i)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * i)) / Screen.height, 0);
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * nextStep)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * nextStep)) / Screen.height, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        public static Dictionary<string, byte[]> ConvertSimpleBlock(GameObject[] objs)
        {
            Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            for (int i = 0; i < objs.Length; i++)
            {
                Block block = objs[i].GetComponent<Block>();
                if (block != null)
                {
                    var data = block.GetDataRef();
                    BlockData.OrginData.Clear();
                    byte[] datas = data.Serialize();
                    string fileName = objs[i].name.Replace(".", "_").Trim();

                    dictionary[fileName] = datas;
                }
            }

            return dictionary;
        }
    }
}