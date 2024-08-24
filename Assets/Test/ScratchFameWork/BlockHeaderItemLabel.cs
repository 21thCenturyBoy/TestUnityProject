using System;
using System.Collections;
using System.Collections.Generic;
using ScratchFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [Serializable]
    public class BlockHeaderParam_Data_Label : BlockHeaderParam_Data
    {
        public readonly BindableProperty<string> DataProperty = new BindableProperty<string>();
    }
    // [ExecuteInEditMode]
    public class BlockHeaderItemLabel : BlockHeaderItem<BlockHeaderParam_Data_Label>
    {
        public TMP_Text LabelText;
        public override void Initialize(BlockHeaderParam_Data_Label context = null)
        {
            base.Initialize(context);
            if (context == null)
            {
                BindingContext.DataProperty.Value = LabelText.text;
            }
        }

        public override void OnUpdateLayout()
        {
            var sizeFitter = TryAddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            LabelText.raycastTarget = false;
            
            LayoutRebuilder.MarkLayoutForRebuild(RectTrans);
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            Binder.Add<string>(nameof(BindingContext.DataProperty), OnDataPropertyChanged);
        }

        private void OnDataPropertyChanged(string oldvalue, string newvalue)
        {
            LabelText.text = newvalue;
        }
    }
}