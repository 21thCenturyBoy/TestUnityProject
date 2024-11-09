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
    public interface IBlockHeaderVariableLabel
    {
        void UpdateVariablName(string name);
        IHeaderParamVariable GetVariableData();
    }

    public interface IHeaderParamVariable
    {
        DataType DataType { get; }
        string VariableInfo { get; set; }
        string VariableRef { get; set; }
    }

    [Serializable]
    public class BlockHeaderParam_Data_VariableLabel : BlockHeaderParam_Data<BlockHeaderParam_Data_VariableLabel>, IScratchRefreshRef, IHeaderParamVariable
    {
        private string _VariableInfo = string.Empty;

        public string VariableInfo
        {
            get => _VariableInfo;
            set
            {
                if (value == _VariableInfo) return;
                _VariableInfo = value;
                OnPropertyChanged();
            }
        }

        //变量引用信息
        private string _VariableRef = String.Empty;

        public string VariableRef
        {
            get => _VariableRef;
            set
            {
                if (value == _VariableRef) return;
                _VariableRef = value;
                OnPropertyChanged();
            }
        }

        public override DataType DataType => DataType.VariableLabel;

        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            stream.Write(ScratchUtils.ScratchSerializeString(VariableInfo));
            stream.Write(ScratchUtils.ScratchSerializeString(VariableRef));

            return stream.ToArray();
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            VariableInfo = memoryStream.ScratchDeserializeString();
            VariableRef = memoryStream.ScratchDeserializeString();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(VariableInfo)}: {VariableInfo}, {nameof(VariableRef)}: {VariableRef}";
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
        }
    }


    public class BlockHeaderItem_VariableLabel : BlockHeaderItem<BlockHeaderParam_Data_VariableLabel>, IBlockHeaderVariableLabel
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
            ContextData.VariableInfo = LabelText.text;
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
                case nameof(BlockHeaderParam_Data_VariableLabel.VariableInfo):
                    LabelText.text = ContextData.VariableInfo;
                    break;
                case nameof(BlockHeaderParam_Data_VariableLabel.VariableRef):
                    if (!string.IsNullOrEmpty(ContextData.VariableRef))
                    {
                        int Guid = int.Parse(ContextData.VariableRef);
                        IEngineBlockBaseData baseData = ScratchEngine.Instance.Core.GetBlocksDataRef(Guid);
                        if (baseData != null && baseData.TryGetBlockVariableName(out var variableName))
                        {
                            ContextData.VariableInfo = variableName;
                        }
                    }
                    else
                    {
                        LabelText.text = ContextData.VariableInfo;
                    }

                    break;
                default:
                    break;
            }
        }

        public override void RefreshUI()
        {
            LabelText.text = ContextData.VariableInfo;
        }

        protected override void OnDestroy()
        {
            ScratchDataManager.Instance.RemoveVariableLabelRef(this);

            base.OnDestroy();
        }

        public void UpdateVariablName(string name)
        {
            ContextData.VariableInfo = name;
        }

        public IHeaderParamVariable GetVariableData() => ContextData;
    }
}