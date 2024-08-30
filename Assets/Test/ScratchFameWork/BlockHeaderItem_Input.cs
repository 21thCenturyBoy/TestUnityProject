using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public class BlockHeaderParam_Data_Input : BlockHeaderParam_Data<BlockHeaderParam_Data_Input>
    {
        private string _dataProperty;
        private BlockHeaderParam_Data_Operation _bindOperation;

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


        public BlockHeaderParam_Data_Operation BindOperation
        {
            get => _bindOperation;
            set
            {
                if (Equals(value, _bindOperation)) return;
                _bindOperation = value;
                OnPropertyChanged();
            }
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
        public override bool Initialize(BlockHeaderParam_Data_Input context = null)
        {
            if (base.Initialize(context))
            {
                if (context == null)
                {
                    ContextData.DataProperty = InputField.text;
                }
            }

            return Inited;
        }
        
        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_Input.DataProperty):
                    InputField.text = ContextData.DataProperty;
                    break;
                case nameof(BlockHeaderParam_Data_Input.BindOperation):
                    // InputField.text = ContextData.DataProperty;
                    break;
                default:
                    break;
            }
        }
    }

}
