using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public class BlockHeaderParam_Data_Operation : BlockHeaderParam_Data<BlockHeaderParam_Data_Operation>
    {
        private BlockHeaderParam_Data_Input _parentInput;

        public BlockHeaderParam_Data_Input ParentInput
        {
            get => _parentInput;
            set
            {
                if (Equals(value, _parentInput)) return;
                _parentInput = value;
                OnPropertyChanged();
            }
        }
    }

    public class BlockHeaderItem_Operation : BlockHeaderItem<BlockHeaderParam_Data_Operation>
    {
        public override bool Initialize(BlockHeaderParam_Data_Operation context = null)
        {
            if (base.Initialize(context))
            {
                if (context == null)
                {
                    //TODO Initialize Data
                    // ContextData.DataProperty = LabelText.text;
                }
            }

            return Inited;
        }

        public override void OnUpdateLayout()
        {
        }

        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_Operation.ParentInput):
                    // LabelText.text = ContextData.DataProperty;
                    break;
                default:
                    break;
            }
        }
    }
}