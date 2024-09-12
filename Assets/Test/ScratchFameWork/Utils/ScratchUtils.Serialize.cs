using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
        public IBlockData[] BlockTreeList { get; set; } = Array.Empty<IBlockData>();


        public byte[] Serialize()
        {
            MemoryStream stream = ScratchUtils.CreateMemoryStream();

            int blockHeadLen = BlockHeadTreeList.Length;
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(blockHeadLen));
            for (int i = 0; i < blockHeadLen; i++)
            {
                stream.WriteByte((byte)BlockHeadTreeList[i].DataType);
                byte[] bytes = BlockHeadTreeList[i].Serialize();
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(bytes.Length));
                stream.WriteBytes(bytes);
            }

            int blockLen = BlockTreeList.Length;
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(blockLen));
            for (int i = 0; i < blockLen; i++)
            {
                byte[] bytes = BlockTreeList[i].Serialize();
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(bytes.Length));
                stream.WriteBytes(bytes);
            }

            return stream.ToArray();
        }

        public bool Deserialize(MemoryStream stream, int version = -1)
        {
            int headBytesLen = ScratchUtils.ScratchDeserializeInt(stream);

            BlockHeadTreeList = new IBlockHeadData[headBytesLen];
            for (int i = 0; i < headBytesLen; i++)
            {
                DataType headData = (DataType)ScratchUtils.ReadByte(stream);
                BlockHeadTreeList[i] = BlockHeadDataFactorty.CreateBlockHeadData(headData);

                int dataSize = ScratchUtils.ScratchDeserializeInt(stream);
                MemoryStream dataStream = ScratchUtils.CreateMemoryStream(stream, dataSize);
                stream.Position += dataSize;

                BlockHeadTreeList[i].Deserialize(dataStream, version);
            }

            int blockBytesLen = ScratchUtils.ScratchDeserializeInt(stream);
            BlockTreeList = new IBlockData[blockBytesLen];
            for (int i = 0; i < blockBytesLen; i++)
            {
                BlockData blockData = new BlockData();
                BlockTreeList[i] = blockData;

                int dataSize = ScratchUtils.ScratchDeserializeInt(stream);

                MemoryStream dataStream = ScratchUtils.CreateMemoryStream(stream, dataSize);
                stream.Position += dataSize;

                BlockTreeList[i].Deserialize(dataStream, version);
            }

            return true;
        }
    }

    public partial class BlockData : IBlockData
    {
        public Vector3 Position { get; set; }
        public FucType BlockFucType { get; set; }
        public BlockType Type { get; set; }
        public int Version { get; set; }
        public IBlockSectionData[] SectionTreeList { get; set; } = Array.Empty<IBlockSectionData>();

        public readonly Dictionary<int, int> DataRefIdDict = new Dictionary<int, int>();
        public readonly HashSet<IScratchRefreshRef> RefreshRefDict = new HashSet<IScratchRefreshRef>();

        public string Name
        {
            get { return $"{BlockFucType} {Type} v.{Version}"; }
        }

        public Block CreateBlock()
        {
            // BlockCreator.CreateBlock(this);
            return null;
        }

        public void CopyData(Block block)
        {
            Position = block.Position;
            BlockFucType = block.BlockFucType;
            Type = block.Type;
            Version = block.Version;
            //TODO Other Serialize Data

            var sections = block.GetChildSection();
            SectionTreeList = new IBlockSectionData[sections.Count];
            for (int i = 0; i < sections.Count; i++)
            {
                BlockSectionData sectionData = sections[i].CopyData() as BlockSectionData;
                SectionTreeList[i] = sectionData;
            }

            List<BlockData> blockDatas = new List<BlockData>() { this };
            GetAllBlock(this, ref blockDatas);

            DataRefIdDict.Clear();

            for (int i = 0; i < blockDatas.Count; i++)
            {
                for (int j = 0; j < blockDatas[i].SectionTreeList.Length; j++)
                {
                    BlockSectionData sectionData = blockDatas[i].SectionTreeList[j] as BlockSectionData;

                    foreach (KeyValuePair<int, int> valuePair in sectionData.OperationRefDict)
                    {
                        DataRefIdDict[valuePair.Key] = valuePair.Value;
                    }

                    for (int k = 0; k < blockDatas[i].SectionTreeList[j].BlockHeadTreeList.Length; k++)
                    {
                        if (blockDatas[i].SectionTreeList[j].BlockHeadTreeList[k] is IScratchRefreshRef refreshRef)
                        {
                            RefreshRefDict.Add(refreshRef);
                        }
                    }
                }
            }
        }

        public void GetAllBlock(BlockData blockData, ref List<BlockData> blockDatas)
        {
            if (blockDatas == null)
            {
                blockDatas = new List<BlockData>();
            }

            for (int i = 0; i < blockData.SectionTreeList.Length; i++)
            {
                for (int j = 0; j < blockData.SectionTreeList[i].BlockHeadTreeList.Length; j++)
                {
                    if (blockData.SectionTreeList[i].BlockHeadTreeList[j].GetBlockData() is BlockData tempOpera)
                    {
                        if (tempOpera != null)
                        {
                            blockDatas.Add(tempOpera);
                            GetAllBlock(tempOpera, ref blockDatas);
                        }
                    }
                }

                for (int j = 0; j < blockData.SectionTreeList[i].BlockTreeList.Length; j++)
                {
                    BlockData temp = blockData.SectionTreeList[i].BlockTreeList[j] as BlockData;
                    blockDatas.Add(temp);
                    GetAllBlock(temp, ref blockDatas);
                }
            }
        }


        public byte[] Serialize()
        {
            var sectionBytesList = new List<byte[]>();
            int dataSize = 0;
            int sectionLen = SectionTreeList.Length;
            for (int i = 0; i < sectionLen; i++)
            {
                byte[] bytes = SectionTreeList[i].Serialize();
                sectionBytesList.Add(bytes);
            }

            MemoryStream stream = ScratchUtils.CreateMemoryStream();

            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(Version));
            stream.WriteByte((byte)Type);
            stream.WriteByte((byte)BlockFucType);
            stream.WriteBytes(ScratchUtils.ScratchSerializeVector3(Position));

            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(sectionBytesList.Count));
            for (int i = 0; i < sectionBytesList.Count; i++)
            {
                stream.WriteBytes(ScratchUtils.ScratchSerializeInt(sectionBytesList[i].Length));
                stream.WriteBytes(sectionBytesList[i]);
            }

            return stream.ToArray();
        }

        public bool Deserialize(MemoryStream stream, int version = -1)
        {
            Version = ScratchUtils.ScratchDeserializeInt(stream);
            Type = (BlockType)ScratchUtils.ReadByte(stream);
            BlockFucType = (FucType)ScratchUtils.ReadByte(stream);
            Position = ScratchUtils.ScratchDeserializeVector3(stream);

            int sectionBytesLen = ScratchUtils.ScratchDeserializeInt(stream);
            SectionTreeList = new IBlockSectionData[sectionBytesLen];
            for (int i = 0; i < sectionBytesLen; i++)
            {
                BlockSectionData sectionData = new BlockSectionData();
                int dataSectionSize = ScratchUtils.ScratchDeserializeInt(stream);

                MemoryStream dataSectionStream = ScratchUtils.CreateMemoryStream(stream, dataSectionSize);

                stream.Position += dataSectionSize;

                if (sectionData.Deserialize(dataSectionStream, Version))
                {
                    SectionTreeList[i] = sectionData;
                }
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

                    data.x = BitConverter.ToSingle(span.ToArray(), 0);
                    data.y = BitConverter.ToSingle(span.ToArray(), size - 1);
                    data.z = BitConverter.ToSingle(span.ToArray(), (size << 1) - 2);

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
}