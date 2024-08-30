using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ScratchFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public class BlockHeaderParam_Data_Label : BlockHeaderParam_Data<BlockHeaderParam_Data_Label>
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
    }

    public class BlockHeaderItem_Label : BlockHeaderItem<BlockHeaderParam_Data_Label>
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

        public override bool Initialize(BlockHeaderParam_Data_Label context = null)
        {
            if (base.Initialize(context))
            {
                if (context == null)
                {
                    ContextData.DataProperty = LabelText.text;
                }
            }

            return Inited;
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
                default:
                    break;
            }
        }
    }
}