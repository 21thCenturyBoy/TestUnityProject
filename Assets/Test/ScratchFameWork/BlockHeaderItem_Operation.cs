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


        public override DataType DataType => DataType.Operation;
        
        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            var bytes = ScratchUtils.ScratchSerializeInt(ParentInput.RefIdPtr);
            stream.Write(bytes);
            
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

        public byte[] BlockDatas { get; protected set; }
        private BlockData m_DeserializeBlockData = null;

        public override IBlockData GetBlockData()
        {
            return m_DeserializeBlockData;
        }

        private IBlockScratch_Block m_OperationBlock;

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
        
        // public override IBlockHeadData CopyData()
        // {
        //     return DataRef();
        // }
        protected override void OnCreateContextData()
        {
        }

        protected override void OnContextDataChanged(BlockHeaderParam_Data_Operation orginData, BlockHeaderParam_Data_Operation newData)
        {
            base.OnContextDataChanged(orginData, newData);
            if (orginData != null) orginData.OperationBlock = null;
            if (newData != null) newData.OperationBlock = GetComponent<Block>();
        }

        public override void OnUpdateLayout()
        {
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