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
    public sealed class BlockHeaderParam_Data_VariableLabel : BlockHeaderParam_Data<BlockHeaderParam_Data_VariableLabel>
    {
        private string _dataProperty;

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

        public override DataType DataType => DataType.VariableLabel;

        protected override byte[] OnSerialize()
        {
            var bytes = ScratchUtils.ScratchSerializeString(_dataProperty);
            return bytes;
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            DataProperty = memoryStream.ScratchDeserializeString();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DataProperty)}: {DataProperty}";
        }
    }

    public class BlockHeaderItem_VariableLabel : BlockHeaderItem<BlockHeaderParam_Data_VariableLabel>
    {
        private TMP_Text m_LabelText;

        public TMP_Text LabelText
        {
            get
            {
                if (m_LabelText == null)
                {
                    m_LabelText = GetComponent<TMP_Text>();
                }

                return m_LabelText;
            }
        }

        protected override void OnCreateContextData()
        {
            ContextData.DataProperty = LabelText.text;
        }
        

        public override void OnUpdateLayout()
        {
            var sizeFitter = TryAddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            LabelText.raycastTarget = false;

            LayoutRebuilder.MarkLayoutForRebuild(RectTrans);
        }

        public override void ContextDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BlockHeaderParam_Data_VariableLabel.DataProperty):
                    LabelText.text = ContextData.DataProperty;
                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
            LabelText.text = ContextData.DataProperty;
        }
    }
}