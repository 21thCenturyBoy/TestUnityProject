using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    [Serializable]
    public abstract class BlockHeaderParam_Data<T> : ScratchVMData, IScratchEditorData where T : BlockHeaderParam_Data<T>
    {
        protected DataType m_dataType = DataType.Undefined;
        protected int m_version;

        public virtual DataType DataType
        {
            get => m_dataType;
            protected set => m_dataType = value;
        }

        public virtual int Version
        {
            get => m_version;
            protected set => m_version = value;
        }

        public virtual byte[] Serialize()
        {
            //TODO Other Serialize Data
            int dataSize = 0;
            dataSize += Marshal.SizeOf<int>();
            dataSize += Marshal.SizeOf<byte>();

            byte[] datas = this.SerializeData(this);

            dataSize += datas.Length;

            MemoryStream memoryStream = new MemoryStream();

            memoryStream.Write(BitConverter.GetBytes(dataSize));
            memoryStream.Write(BitConverter.GetBytes((byte)m_dataType));
            memoryStream.Write(datas);

            return memoryStream.ToArray();
        }

        public bool Deserialize(MemoryStream stream, int version = -1)
        {
            //TODO Other Deserialize Data 优化缓存

            byte[] byteArray = new byte[Marshal.SizeOf<int>()];
            int offset = 0;
            stream.Read(byteArray, offset, byteArray.Length);
            offset += byteArray.Length;
            int dataSize = BitConverter.ToInt32(byteArray);

            byteArray = new byte[Marshal.SizeOf<byte>()];
            stream.Read(byteArray, offset, byteArray.Length);
            offset += byteArray.Length;
            m_dataType = (DataType)byteArray[0];

            byteArray = new byte[dataSize];
            stream.Read(byteArray, offset, byteArray.Length);
            offset += byteArray.Length;

            T data = this.DeserializeData<T>(byteArray);
            Copy(data);

            return true;
        }

        public virtual void Copy(T data)
        {
        }

        public virtual IScratchData AssetData { get; set; }
    }
    public abstract class BlockHeaderItem<T> : ScratchUIBehaviour<T>, IScratchDataBlock where T : ScratchVMData, new()
    {
        private IScratchData _mData;

        IScratchData IScratchDataBlock.Data
        {
            get => _mData;
            set => _mData = value;
        }
    }
}