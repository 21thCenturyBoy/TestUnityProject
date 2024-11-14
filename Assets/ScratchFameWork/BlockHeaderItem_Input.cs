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
        
        private int _languageId;

        public int LanguageId
        {
            get => _languageId;
            set
            {
                if (value == _languageId) return;
                _languageId = value;
                OnPropertyChanged();
            }
        }

        [Newtonsoft.Json.JsonIgnore]
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
            
            stream.Write( ScratchUtils.ScratchSerializeString(DataProperty));
            stream.Write(ScratchUtils.ScratchSerializeInt(ChildOperation.RefIdPtr));
            stream.Write(ScratchUtils.ScratchSerializeInt(LanguageId));
            
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
            LanguageId = memoryStream.ScratchDeserializeInt();
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
            if (ChildOperation != null)
            {
                ChildOperation.RefreshRef(refreshDic);

                // var data = ChildOperation.GetData();
                // if (data != null)
                // {
                //     data.ParentInput = this.CreateRef<BlockHeaderParam_Data_Input>();
                // }
            }

            OnPropertyChanged(nameof(ChildOperation));
        }

        public BlockHeaderItem_Input InputBlock { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DataProperty)}: {DataProperty}, {nameof(ChildOperation)}: {ChildOperation}";
        }

        public void SetLanguageId(int languageId)
        {
        }
        
    }

    public class BlockHeaderItem_Input : BlockHeaderItem<BlockHeaderParam_Data_Input>,IBlockLanguage
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
        private BlockHeaderLanguage m_BlockHeaderLanguage;

        public BlockHeaderLanguage BlockHeaderLanguage
        {
            get
            {
                if (m_BlockHeaderLanguage == null)
                {
                    m_BlockHeaderLanguage = GetComponent<BlockHeaderLanguage>();
                }

                return m_BlockHeaderLanguage;
            }
        }
        
        protected override void OnCreateContextData()
        {
            ContextData.DataProperty = InputField.text;
            ContextData.InputBlock = this;
            
            if (BlockHeaderLanguage != null)
            {
                ContextData.LanguageId = BlockHeaderLanguage.LanguageNameId;
            }
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
                case nameof(BlockHeaderParam_Data_Label.LanguageId):
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

        public void SetLanguageId(int id)
        {
            if (ContextData != null)
            {
                ContextData.LanguageId = id;
            }
        }
    }
    
}