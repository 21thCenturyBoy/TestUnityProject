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
    public abstract class BlockHeaderParam_Data<T> : ScratchVMData, IBlockHeadData where T : BlockHeaderParam_Data<T>
    {
        public abstract DataType DataType { get; }
        public virtual IBlockData GetBlockData() => null;

        public byte[] Serialize()
        {
            byte[] datas = OnSerialize();
            var stream = ScratchUtils.CreateMemoryStream();

            stream.WriteByte((byte)DataType);
            stream.WriteBytes(datas);

            return stream.ToArray();
        }

        public bool Deserialize(MemoryStream stream, int version = -1)
        {
            DataType dataType = (DataType)ScratchUtils.ReadByte(stream);
            if (DataType != dataType)
            {
                Debug.LogError($"{dataType}  Not Equals:{DataType}");
            }

            OnDeserialize(stream, version);

            return true;
        }

        public override string ToString()
        {
            return base.ToString() + $"({DataType})";
        }

        protected abstract byte[] OnSerialize();
        protected abstract void OnDeserialize(MemoryStream memoryStream, int version = -1);
    }

    public abstract class BlockHeaderItem<T> : ScratchUIBehaviour<T>, IBlockScratch_Head where T : BlockHeaderParam_Data<T>, new()
    {
        public virtual IBlockHeadData CopyData()
        {
            byte[] bytesDatas = ContextData.Serialize();

            T newData = new T();

            //Current Editor Version
            int currentVersion = ScratchUtils.CurrentSerializeVersion;

            var memory = ScratchUtils.CreateMemoryStream(bytesDatas);
            newData.Deserialize(memory, currentVersion);

            return newData;
        }

        public IBlockHeadData DataRef()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (ContextData == null)
                    ContextData = new T();
                OnCreateContextData();
            }
#endif
            return ContextData;
        }

        public void SetData(IBlockHeadData data)
        {
            if (data is T tdata)
            {
                Initialize(tdata);
            }
        }

        public abstract void RefreshUI();
    }
}