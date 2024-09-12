using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BlockHeaderParam_Data_Input : BlockHeaderParam_Data<BlockHeaderParam_Data_Input>, IScratchRefreshRef
    {
        private string _dataProperty = string.Empty;

        private ScratchVMDataRef<BlockHeaderParam_Data_Operation> _childOperation = ScratchVMDataRef<BlockHeaderParam_Data_Operation>.NULLRef;

        public string DataProperty
        {
            get => _dataProperty;
            set
            {
                if (value == _dataProperty) return;
                _dataProperty = value;
                OnPropertyChanged();
            }
        }

        public ScratchVMDataRef<BlockHeaderParam_Data_Operation> ChildOperation
        {
            get => _childOperation;
            set
            {
                if (value == null || !value.InVaildPtr)
                {
                    if (_childOperation != null)
                    {
                        //push null
                        _childOperation.GetData().ParentInput = null;
                    }

                    value ??= ScratchVMDataRef<BlockHeaderParam_Data_Operation>.NULLRef;
                    if (Equals(value, _childOperation)) return;

                    _childOperation = value;

                    if (_childOperation != null)
                    {
                        _childOperation.GetData().ParentInput = this.CreateRef<BlockHeaderParam_Data_Input>();
                    }

                    OnPropertyChanged();
                }
                else
                {
                    value ??= ScratchVMDataRef<BlockHeaderParam_Data_Operation>.NULLRef;
                    _childOperation = value;
                }
            }
        }

        public override DataType DataType => DataType.Input;

        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            var bytes = ScratchUtils.ScratchSerializeString(DataProperty);
            stream.Write(bytes);
            bytes = ScratchUtils.ScratchSerializeInt(ChildOperation.RefIdPtr);
            stream.Write(bytes);

            return stream.ToArray();
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            DataProperty = memoryStream.ScratchDeserializeString();

            int DataPtr = memoryStream.ScratchDeserializeInt();
            if (DataPtr == UnallocatedId)
            {
                ChildOperation = null;
            }
            else
            {
                ChildOperation = ScratchVMDataRef<BlockHeaderParam_Data_Operation>.CreateInVaildPtr<BlockHeaderParam_Data_Operation>(DataPtr);
            }
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
            if (ChildOperation != null)
            {
                ChildOperation.RefreshRef(refreshDic);

                var data = ChildOperation.GetData();
                if (data != null)
                {
                    data.ParentInput = this.CreateRef<BlockHeaderParam_Data_Input>();
                }
            }

            OnPropertyChanged(nameof(ChildOperation));
        }

        public BlockHeaderItem_Input InputBlock { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DataProperty)}: {DataProperty}, {nameof(ChildOperation)}: {ChildOperation}";
        }
    }

    public class BlockHeaderItem_Input : BlockHeaderItem<BlockHeaderParam_Data_Input>
    {
        private TMP_InputField m_InputField;

        public TMP_InputField InputField
        {
            get
            {
                if (m_InputField == null)
                {
                    m_InputField = GetComponent<TMP_InputField>();
                }

                return m_InputField;
            }
        }

        protected override void OnCreateContextData()
        {
            ContextData.DataProperty = InputField.text;
            ContextData.InputBlock = this;
        }

        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_Input.DataProperty):
                    InputField.SetTextWithoutNotify(ContextData.DataProperty);
                    break;
                case nameof(BlockHeaderParam_Data_Input.ChildOperation):
                    Active = ContextData.ChildOperation == ScratchVMDataRef<BlockHeaderParam_Data_Operation>.NULLRef;
                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
            InputField.SetTextWithoutNotify(ContextData.DataProperty);
            Active = ContextData.ChildOperation == ScratchVMDataRef<BlockHeaderParam_Data_Operation>.NULLRef;
        }
    }
}