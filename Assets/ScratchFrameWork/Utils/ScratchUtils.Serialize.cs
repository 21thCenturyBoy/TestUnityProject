using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    public enum SerializeMode
    {
        Bit,
        MessagePack,
    }

    public static class BlockHeadDataFactorty
    {
        public static IBlockHeadData CreateBlockHeadData(DataType datatype)
        {
            switch (datatype)
            {
                case DataType.Undefined:
                    return null;
                case DataType.Label:
                    return new BlockHeaderParam_Data_Label();
                case DataType.Input:
                    return new BlockHeaderParam_Data_Input();
                case DataType.Operation:
                    return new BlockHeaderParam_Data_Operation();
                case DataType.VariableLabel:
                    return new BlockHeaderParam_Data_VariableLabel();
                case DataType.RenturnVariableLabel:
                    return new BlockHeaderParam_Data_RenturnVariableLabel();
                case DataType.Icon:
                    return new BlockHeaderParam_Data_Icon();
                default:
                    throw new ArgumentOutOfRangeException(nameof(datatype), datatype, null);
            }
        }
    }

    public partial class BlockSectionData : IBlockSectionData
    {
        public Dictionary<int, int> OperationRefDict { get; set; } = new Dictionary<int, int>();

        public IBlockHeadData[] BlockHeadTreeList { get; set; } = Array.Empty<IBlockHeadData>();
        public IBlockData[] BlockTreeList { get; set; }
        public IBlockScratch_Block[] BlockTreeRefList { get; set; } = Array.Empty<IBlockScratch_Block>();


        public byte[] Serialize()
        {
            BlockData.OrginData.Add(this);
            MemoryStream stream = ScratchUtils.CreateMemoryStream();

            int blockHeadLen = BlockHeadTreeList.Length;
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(blockHeadLen));

            for (int i = 0; i < blockHeadLen; i++)
            {
                stream.WriteByte((byte)BlockHeadTreeList[i].DataType);
                BlockData.OrginData.Add(BlockHeadTreeList[i]);
                byte[] bytes = BlockHeadTreeList[i].Serialize();
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(bytes.Length));
                stream.WriteBytes(bytes);
            }

            int blockLen = BlockTreeRefList.Length;
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(blockLen));
            for (int i = 0; i < blockLen; i++)
            {
                byte[] bytes = BlockTreeRefList[i].GetDataRef().Serialize();
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(bytes.Length));
                stream.WriteBytes(bytes);
            }

            return stream.ToArray();
        }
    }

    [Serializable]
    public partial class BlockData : IBlockData
    {
        [Newtonsoft.Json.JsonIgnore] public Vector3 LocalPosition { get; set; }
        public FucType BlockFucType { get; set; }
        public BlockType Type { get; set; }
        public ScratchBlockType ScratchType { get; set; }
        public int Version { get; set; }
        public IBlockSectionData[] SectionTreeList { get; set; } = Array.Empty<IBlockSectionData>();

        #region 引用刷新

        [Newtonsoft.Json.JsonIgnore] public static List<IScratchData> OrginData { get; set; } = new List<IScratchData>();
        [Newtonsoft.Json.JsonIgnore] public static List<IScratchData> NewData { get; set; } = new List<IScratchData>();

        #endregion

        public bool IsRoot { get; protected set; } = false;

        public string Name
        {
            get { return $"{ScratchType} v.{Version}"; }
        }

        #region 特殊Data

        public ScratchValueType OperationValueType { get; set; } = ScratchValueType.Undefined;

        #endregion

        private BlockHeaderItem_Operation OperationData;

        public void GetData(Block block)
        {
            LocalPosition = block.LocalPosition;
            BlockFucType = block.BlockFucType;
            Type = block.Type;
            ScratchType = block.scratchType;
            Version = block.Version;

            if (Type == BlockType.Operation)
            {
                OperationData = block.GetComponent<BlockHeaderItem_Operation>();
                OperationValueType = OperationData.ValueType;
            }

            //TODO Other Serialize Data
            var sections = block.GetChildSection();
            SectionTreeList = new IBlockSectionData[sections.Count];

            for (int i = 0; i < sections.Count; i++)
            {
                BlockSectionData sectionData = sections[i].GetData() as BlockSectionData;
                SectionTreeList[i] = sectionData;
            }
        }

        public byte[] Serialize()
        {
            OrginData.Add(this);

            var sectionBytesList = new List<byte[]>();
            int dataSize = 0;
            int sectionLen = SectionTreeList.Length;
            for (int i = 0; i < sectionLen; i++)
            {
                byte[] bytes = SectionTreeList[i].Serialize();
                sectionBytesList.Add(bytes);
            }

            MemoryStream stream = ScratchUtils.CreateMemoryStream();

            Serialize_Base(stream);

            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(sectionBytesList.Count));
            for (int i = 0; i < sectionBytesList.Count; i++)
            {
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(sectionBytesList[i].Length));
                stream.WriteBytes(sectionBytesList[i]);
            }

            return stream.ToArray();
        }

        public bool Serialize_Base(MemoryStream stream)
        {
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(Version));
            stream.WriteByte((byte)Type);
            stream.WriteByte((byte)BlockFucType);
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt((int)ScratchType));
            stream.WriteBytes(ScratchUtils.ScratchSerializeVector3(LocalPosition));

            if (Type == BlockType.Operation)
            {
                stream.WriteByte((byte)OperationValueType);
            }

            return true;
        }

        public bool Deserialize_Base(MemoryStream stream)
        {
            Version = stream.ScratchDeserializeInt();
            Type = (BlockType)ScratchUtils.ReadByte(stream);
            BlockFucType = (FucType)ScratchUtils.ReadByte(stream);
            ScratchType = (ScratchBlockType)stream.ScratchDeserializeInt();
            LocalPosition = stream.ScratchDeserializeVector3();

            if (Type == BlockType.Operation)
            {
                OperationValueType = (ScratchValueType)ScratchUtils.ReadByte(stream);
            }

            return true;
        }

        public bool BlockData_Deserialize(MemoryStream stream, int version = -1, bool isRoot = false)
        {
            NewData.Add(this);
            IsRoot = isRoot;

            Deserialize_Base(stream);

            int sectionBytesLen = ScratchUtils.ScratchDeserializeInt(stream);
            SectionTreeList = new IBlockSectionData[sectionBytesLen];
            for (int i = 0; i < sectionBytesLen; i++)
            {
                BlockSectionData sectionData = new BlockSectionData();
                int dataSectionSize = ScratchUtils.ScratchDeserializeInt(stream);

                MemoryStream dataSectionStream = ScratchUtils.CreateMemoryStream(stream, dataSectionSize);

                stream.Position += dataSectionSize;

                if (SectionData_Deserialize(sectionData, dataSectionStream, version))
                {
                    SectionTreeList[i] = sectionData;
                }
            }

            return true;
        }

        public bool SectionData_Deserialize(IBlockSectionData sectionData, MemoryStream stream, int version = -1)
        {
            NewData.Add(sectionData);
            int headBytesLen = ScratchUtils.ScratchDeserializeInt(stream);

            sectionData.BlockHeadTreeList = new IBlockHeadData[headBytesLen];

            for (int i = 0; i < headBytesLen; i++)
            {
                DataType headData = (DataType)ScratchUtils.ReadByte(stream);
                sectionData.BlockHeadTreeList[i] = BlockHeadDataFactorty.CreateBlockHeadData(headData);

                NewData.Add(sectionData.BlockHeadTreeList[i]);
                int dataSize = ScratchUtils.ScratchDeserializeInt(stream);
                MemoryStream dataStream = ScratchUtils.CreateMemoryStream(stream, dataSize);
                stream.Position += dataSize;

                sectionData.BlockHeadTreeList[i].Deserialize(dataStream, version);

                if (sectionData.BlockHeadTreeList[i] is BlockHeaderParam_Data_Operation operationBlock)
                {
                    var blocks = operationBlock.GetBlockData();
                }
            }

            int blockBytesLen = ScratchUtils.ScratchDeserializeInt(stream);
            sectionData.BlockTreeList = new IBlockData[blockBytesLen];
            for (int i = 0; i < blockBytesLen; i++)
            {
                BlockData blockData = new BlockData();

                int dataSize = ScratchUtils.ScratchDeserializeInt(stream);

                MemoryStream dataStream = ScratchUtils.CreateMemoryStream(stream, dataSize);
                stream.Position += dataSize;

                blockData.BlockData_Deserialize(dataStream, version);
                sectionData.BlockTreeList[i] = blockData;
            }

            return true;
        }
    }

    [Flags]
    public enum BlocksDataDirtyType : byte
    {
        None = 0,
        Add = 1 << 0,
        Remove = 1 << 1,
        Update = 1 << 2,
        Change = 1 << 3,
        Refresh = 1 << 4,
    }

    public static partial class ScratchUtils
    {
        public static SerializeMode SerializeMode = SerializeMode.Bit;

        public static int CurrentSerializeVersion = 0;

        private static BlocksDataDirtyType m_DirtyTreeType = BlocksDataDirtyType.None;

        public static bool Contains(this BlocksDataDirtyType equipment, BlocksDataDirtyType checkState)
        {
            if (checkState == 0)
            {
                throw new ArgumentOutOfRangeException("checkState", "不能为NONE");
            }

            return (equipment & checkState) == checkState;
        }

        public static BlocksDataDirtyType Remove(this BlocksDataDirtyType equipment, BlocksDataDirtyType removeState)
        {
            return equipment ^ removeState;
        }

        public static BlocksDataDirtyType Append(this BlocksDataDirtyType equipment, BlocksDataDirtyType addState)
        {
            return equipment | addState;
        }

        public static MemoryStream CreateMemoryStream(int capacity = 4096)
        {
            MemoryStream stream = new MemoryStream(capacity);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream CreateMemoryStream(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length, true, true);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream CreateMemoryStream(MemoryStream readStream, int size)
        {
            MemoryStream stream = new MemoryStream(size);

            var spanArray = new Span<byte>(readStream.GetBuffer(), (int)readStream.Position, size);
            stream.Write(spanArray);
            stream.Position = 0;
            return stream;
        }

        public static ref T GetArrayRef<T>(T[] items, int index) => ref items[index];

        public static void SetDirtyType(BlocksDataDirtyType type)
        {
            if (type == BlocksDataDirtyType.None)
            {
                m_DirtyTreeType = BlocksDataDirtyType.None;
                return;
            }
            else
            {
                m_DirtyTreeType = m_DirtyTreeType.Append(type);
            }
        }

        public static void ClearDirtyType()
        {
            m_DirtyTreeType = BlocksDataDirtyType.None;
        }

        public static BTreeNode<IEngineBlockBaseData> GetBlockDatas(Block blockNode, Action<IEngineBlockBaseData> callback = null)
        {
            IEngineBlockBaseData blockBaseData = blockNode.GetEngineBlockData();
            var node = BTreeNode<IEngineBlockBaseData>.CreateNode(blockBaseData);

            var sections = blockNode.GetChildSection();
            for (int i = 0; i < sections.Count; i++)
            {
                int childCount = sections[i].transform.childCount;
                BlockSectionHeader sectionheader = sections[i].transform.GetChild(0).GetComponent<BlockSectionHeader>();
                int childCountHead = sectionheader.transform.childCount;
                for (int j = 0; j < childCountHead; j++)
                {
                    Block blockHeadr = sectionheader.transform.GetChild(j).GetComponent<Block>();
                    if (blockHeadr == null) continue;

                    node.AddChild(GetBlockDatas(blockHeadr, callback));
                }

                if (childCount == 2)
                {
                    BlockSectionBody sectionBody = sections[i].transform.GetChild(1).GetComponent<BlockSectionBody>();
                    int childCountBody = sectionBody.transform.childCount;
                    for (int j = 0; j < childCountBody; j++)
                    {
                        Block blockBody = sectionBody.transform.GetChild(j).GetComponent<Block>();
                        if (blockBody == null) continue;

                        node.AddChild(GetBlockDatas(blockBody, callback));
                    }
                }
            }

            callback?.Invoke(blockBaseData);
            return node;
        }
        
        public static BTreeNode<IEngineBlockBaseData> GetBlockDatas(IEngineBlockBaseData blockBaseData, Action<IEngineBlockBaseData> callback = null)
        {
            Block blockNode = BlockCanvasUIManager.Instance.FindBlock(block =>
            {
                return block.GetEngineBlockData() == blockBaseData;
            });

            if (blockNode == null)
            {
                return null;
            }
            else
            {
                return GetBlockDatas(blockNode, callback);
            }
        }

        private static BTreeNode<int> m_cache = null;

        public static void GetBlockDataTree(int guid, out BTreeNode<int> root, Action<IEngineBlockBaseData> callback = null, bool useCache = true)
        {
            if (m_DirtyTreeType == BlocksDataDirtyType.None)
            {
                if (useCache)
                {
                    if (m_cache != null && m_cache.Value == guid)
                    {
                        root = m_cache;
                        return;
                    }
                }
            }

            m_cache?.ReleaseTree();

            root = BTreeNode<int>.CreateNode(guid);
            IEngineBlockBaseData plugData = ScratchEngine.Instance.Current[guid];

            NextBlockPlug(plugData, root, callback);

            if (plugData is IBlockReturnVarGuid returnVarGuid)
            {
                int returnNum = returnVarGuid.GetReturnValuesLength();
                for (int i = 0; i < returnNum; i++)
                {
                    int returnGuid = returnVarGuid.GetReturnValueGuid(i);
                    var returnNode = BTreeNode<int>.CreateNode(returnGuid);
                    root.AddChild(returnNode);
                }
            }

            if (plugData is IBlockPlug plug)
            {
                int nextGuid = plug.NextGuid;
                while (nextGuid != InvalidGuid)
                {
                    IEngineBlockBaseData nextData = ScratchEngine.Instance.Current[nextGuid];
                    callback?.Invoke(nextData);

                    var nextNode = BTreeNode<int>.CreateNode(nextGuid);
                    root.AddChild(nextNode);

                    NextBlockPlug(nextData, nextNode, callback);
                    nextGuid = nextData is IBlockPlug nextPlug ? nextPlug.NextGuid : InvalidGuid;
                }
            }

            m_cache = root;
            ClearDirtyType();
        }

        private static void NextBlockPlug(IEngineBlockBaseData dataNode, BTreeNode<int> node, Action<IEngineBlockBaseData> callback = null)
        {
            if (dataNode == null) return;

            callback?.Invoke(dataNode);

            if (dataNode is IEngineBlockBranch branch)
            {
                int branchNum = branch.GetBranchCount();
                for (int i = 0; i < branchNum; i++)
                {
                    IEngineBlockBaseData branchData = ScratchEngine.Instance.Current[branch.BranchBlockBGuids[i]];

                    if (i != branchNum - 1)
                    {
                        int operaGuid = branch.BranchOperationBGuids[i];
                        var operationNode = BTreeNode<int>.CreateNode(operaGuid);
                        node.AddChild(operationNode);

                        if (operaGuid != InvalidGuid)
                        {
                            IEngineBlockBaseData operationData = ScratchEngine.Instance.Current[branch.BranchOperationBGuids[i]];
                            NextBlockPlug(branchData, operationNode, callback);
                        }
                    }

                    int branchGuid = branch.BranchBlockBGuids[i];
                    var branchNode = BTreeNode<int>.CreateNode(branchGuid);

                    node.AddChild(branchNode);

                    if (branchGuid != InvalidGuid)
                    {
                        IEngineBlockBaseData branchChildData = ScratchEngine.Instance.Current[branch.BranchBlockBGuids[i]];
                        NextBlockPlug(branchChildData, branchNode, callback);
                    }
                }
            }

            if (dataNode is IBlockVarGuid varGuid)
            {
                int varGuidLen = varGuid.GetVarGuidsLength();
                for (int j = 0; j < varGuidLen; j++)
                {
                    int varGuid_0 = varGuid.GetVarGuid(j);
                    var varGuidNode = BTreeNode<int>.CreateNode(varGuid_0);
                    node.AddChild(varGuidNode);

                    if (varGuid_0 != InvalidGuid)
                    {
                        var varGuidData = ScratchEngine.Instance.Current[varGuid_0];
                        NextBlockPlug(varGuidData, varGuidNode, callback);
                    }
                }
            }
        }

        public static byte[] ScratchSerializeInt(int data)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    return BitConverter.GetBytes(data);
                    break;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public static byte[] ScratchSerializeString(string data)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    if (string.IsNullOrEmpty(data)) data = string.Empty;
                    var datas = Encoding.UTF8.GetBytes(data);
                    var lens = ScratchSerializeInt(datas.Length);
                    byte[] databytes = new byte[lens.Length + datas.Length];
                    Buffer.BlockCopy(lens, 0, databytes, 0, lens.Length);
                    Buffer.BlockCopy(datas, 0, databytes, lens.Length, datas.Length);
                    return databytes;

                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public static byte[] ScratchSerializeVector3(Vector3 data)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    int size = Marshal.SizeOf<float>();
                    byte[] databytes = new byte[size * 3];
                    byte[] x_datas = BitConverter.GetBytes(data.x);
                    byte[] y_datas = BitConverter.GetBytes(data.y);
                    byte[] z_datas = BitConverter.GetBytes(data.z);
                    Buffer.BlockCopy(x_datas, 0, databytes, 0, size);
                    Buffer.BlockCopy(y_datas, 0, databytes, size, size);
                    Buffer.BlockCopy(z_datas, 0, databytes, (size << 1), size);
                    return databytes;
                    break;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public static int ScratchDeserializeInt(this MemoryStream memoryStream)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    int size = Marshal.SizeOf<int>();
                    int val = BitConverter.ToInt32(ReadBytes(memoryStream, size));
                    memoryStream.Position += size;
                    return val;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return default;
        }

        public static string ScratchDeserializeString(this MemoryStream memoryStream)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    var len = ScratchDeserializeInt(memoryStream);
                    string info = Encoding.UTF8.GetString(ReadBytes(memoryStream, len));
                    memoryStream.Position += len;
                    return info;

                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public static Vector3 ScratchDeserializeVector3(this MemoryStream memoryStream)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Bit:
                    //TODO IsLittleEndian 
                    int size = Marshal.SizeOf<float>();
                    var span = ReadBytes(memoryStream, size * 3);

                    byte[] databytes = new byte[size * 3];
                    Vector3 data = Vector3.zero;

                    var SPAN_X = span.Slice(0, size);
                    var SPAN_Y = span.Slice(size, size);
                    var SPAN_Z = span.Slice(size << 1, size);
                    data.x = BitConverter.ToSingle(SPAN_X);
                    data.y = BitConverter.ToSingle(SPAN_Y);
                    data.z = BitConverter.ToSingle(SPAN_Z);

                    memoryStream.Position += size * 3;
                    return data;
                    break;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return default;
        }

        public static MemoryStream WriteByte(this MemoryStream memoryStream, byte bytes, bool resize = false)
        {
            if (memoryStream == null) memoryStream = CreateMemoryStream();
            if (resize) memoryStream.Position = 0;

            memoryStream.WriteByte(bytes);

            return memoryStream;
        }

        public static MemoryStream WriteBytes(this MemoryStream memoryStream, byte[] bytes, bool resize = false)
        {
            if (memoryStream == null) memoryStream = CreateMemoryStream();
            if (resize) memoryStream.Position = 0;
            memoryStream.Write(bytes);
            return memoryStream;
        }

        public static byte ReadByte(this MemoryStream memoryStream)
        {
            if (memoryStream == null) return byte.MinValue;
            return (byte)memoryStream.ReadByte();
        }

        public static Span<byte> ReadBytes(this MemoryStream memoryStream, int size)
        {
            if (memoryStream == null) return default;

            var spanArray = new Span<byte>(memoryStream.GetBuffer(), (int)memoryStream.Position, size);

            return spanArray;
        }
    }

    public class GuidListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BGuidList list = (BGuidList)value;
            string jsonString = JsonConvert.SerializeObject(list.ToList());
            writer.WriteValue(jsonString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            BGuidList list = new BGuidList(JsonConvert.DeserializeObject<List<int>>((string)reader.Value));
            return list;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BGuidList);
        }
    }

    public class GuidConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BGuid guid = (BGuid)value;
            writer.WriteValue(guid.GetGuid());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            BGuid guid = BGuid.CreateGuid();
            guid.SetGuid((int)reader.Value, out guid);
            return guid;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BGuid);
        }
    }
}