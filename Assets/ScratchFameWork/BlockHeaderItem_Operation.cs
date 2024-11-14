using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BlockHeaderParam_Data_Operation : BlockHeaderParam_Data<BlockHeaderParam_Data_Operation>, IScratchRefreshRef
    {
        private ScratchVMDataRef<BlockHeaderParam_Data_Input> _parentInput = ScratchVMDataRef<BlockHeaderParam_Data_Input>.NULLRef;

        [Newtonsoft.Json.JsonIgnore]
        public ScratchVMDataRef<BlockHeaderParam_Data_Input> ParentInput
        {
            get => _parentInput;
            set
            {
                if (value == null || !value.InVaildPtr)
                {
                    value ??= ScratchVMDataRef<BlockHeaderParam_Data_Input>.NULLRef;
                    if (Equals(value, _parentInput)) return;

                    _parentInput = value;

                    OnPropertyChanged();
                }
                else
                {
                    value ??= ScratchVMDataRef<BlockHeaderParam_Data_Input>.NULLRef;
                    _parentInput = value;
                }
            }
        }
        private int _dataValueType = -1;
        public int DataValueType
        {
            get => _dataValueType;
            set
            {
                if (value == _dataValueType) return;
                _dataValueType = value;
                OnPropertyChanged();
            }
        }
        
        public override DataType DataType => DataType.Operation;
        public ScratchValueType ValueType => (ScratchValueType)DataValueType;
        
        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            var bytes = ScratchUtils.ScratchSerializeInt(ParentInput.RefIdPtr);
            stream.Write(bytes);
            
            stream.Write(ScratchUtils.ScratchSerializeInt(DataValueType));
            
            BlockData blockData = OperationBlock.GetDataRef() as BlockData;

            byte[] datas = blockData.Serialize();
            stream.WriteBytes(ScratchUtils.ScratchSerializeInt(datas.Length));
            stream.WriteBytes(datas);
            
            return stream.ToArray();
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            int DataPtr = memoryStream.ScratchDeserializeInt();
            if (DataPtr == UnallocatedId)
            {
                ParentInput = null;
            }
            else
            {
                ParentInput = ScratchVMDataRef<BlockHeaderParam_Data_Input>.CreateInVaildPtr<BlockHeaderParam_Data_Input>(DataPtr);
            }
            
            if (version >= 1)
            {
                DataValueType = memoryStream.ScratchDeserializeInt();
            }
            else
            {
                DataValueType = (int)ScratchValueType.Undefined;
            }
            
            int size = memoryStream.ScratchDeserializeInt();

            if (size != -1)
            {
                var stream = ScratchUtils.CreateMemoryStream(memoryStream, size);

                memoryStream.Position += size;
                
                m_DeserializeBlockData = new BlockData();
                m_DeserializeBlockData.BlockData_Deserialize(stream, version);

                BlockDatas = stream.ToArray();
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public byte[] BlockDatas { get; protected set; }
        private BlockData m_DeserializeBlockData = null;

        public override IBlockData GetBlockData()
        {
            return m_DeserializeBlockData;
        }

        private IBlockScratch_Block m_OperationBlock;
        [Newtonsoft.Json.JsonIgnore]
        public IBlockScratch_Block OperationBlock
        {
            get
            {
                return m_OperationBlock;
            }
            set
            {
                m_OperationBlock = value;
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(ParentInput)}: {ParentInput}";
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
            if (ParentInput != null)
            {
                ParentInput.RefreshRef(refreshDic);
            }
            
            OnPropertyChanged(nameof(ParentInput));
        }
    }

    public class BlockHeaderItem_Operation : BlockHeaderItem<BlockHeaderParam_Data_Operation>
    {
        private Block m_block;

        public Block OperationBlock
        {
            get
            {
                if (m_block == null)
                {
                    m_block = GetComponent<Block>();
                }
                return m_block;
            }
        }
        
        public ScratchValueType ValueType;
        
        protected override void OnCreateContextData()
        {
            ContextData.DataValueType = (int)ValueType;
        }

        protected override void OnContextDataChanged(BlockHeaderParam_Data_Operation orginData, BlockHeaderParam_Data_Operation newData)
        {
            base.OnContextDataChanged(orginData, newData);
            if (orginData != null) orginData.OperationBlock = null;
            if (newData != null) newData.OperationBlock = OperationBlock;
        }

        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_Operation.ParentInput):

                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
        }
    }
}