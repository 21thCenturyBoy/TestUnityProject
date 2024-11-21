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
        [Newtonsoft.Json.JsonIgnore]
        public Vector3 LocalPosition { get; set; }
        public FucType BlockFucType { get; set; }
        public BlockType Type { get; set; }
        public ScratchBlockType ScratchType { get; set; }
        public int Version { get; set; }
        public IBlockSectionData[] SectionTreeList { get; set; } = Array.Empty<IBlockSectionData>();

        #region 引用刷新
        [Newtonsoft.Json.JsonIgnore]
        public static List<IScratchData> OrginData { get; set; } = new List<IScratchData>();
        [Newtonsoft.Json.JsonIgnore]
        public static List<IScratchData> NewData { get; set; } = new List<IScratchData>();

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

    public static partial class ScratchUtils
    {
        public static SerializeMode SerializeMode = SerializeMode.Bit;

        public static int CurrentSerializeVersion = 0;

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
            BGuid guid = new BGuid();
            guid.SetGuid((int)reader.Value);
            return guid;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BGuid);
        }
    }
}