using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ScratchFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BlockHeaderParam_Data_Label : BlockHeaderParam_Data<BlockHeaderParam_Data_Label>
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

        public override DataType DataType => DataType.Label;

        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            stream.Write(ScratchUtils.ScratchSerializeString(DataProperty));
            stream.Write(ScratchUtils.ScratchSerializeInt(LanguageId));

            return stream.ToArray();
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            DataProperty = memoryStream.ScratchDeserializeString();
            LanguageId = memoryStream.ScratchDeserializeInt();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(DataProperty)}: {DataProperty}";
        }
        
    }

    public class BlockHeaderItem_Label : BlockHeaderItem<BlockHeaderParam_Data_Label>, IBlockLanguage
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
            ContextData.DataProperty = LabelText.text;
            if (BlockHeaderLanguage != null)
            {
                ContextData.LanguageId = BlockHeaderLanguage.LanguageNameId;
            }
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
                case nameof(BlockHeaderParam_Data_Label.DataProperty):
                    LabelText.text = ContextData.DataProperty;
                    break;
                case nameof(BlockHeaderParam_Data_Label.LanguageId):
                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
            LabelText.text = ContextData.DataProperty;
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