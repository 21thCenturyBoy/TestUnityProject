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
    public class BlockHeaderParam_Data_RenturnVariableLabel : BlockHeaderParam_Data<BlockHeaderParam_Data_RenturnVariableLabel>, IHeaderParamVariable, IScratchRefreshRef
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

        public override DataType DataType => DataType.RenturnVariableLabel;


        protected override byte[] OnSerialize()
        {
            var stream = ScratchUtils.CreateMemoryStream();

            stream.Write(ScratchUtils.ScratchSerializeString(VariableInfo));
            stream.Write(ScratchUtils.ScratchSerializeString(VariableRef));
            stream.Write(ScratchUtils.ScratchSerializeInt(LanguageId));

            return stream.ToArray();
        }

        protected override void OnDeserialize(MemoryStream memoryStream, int version = -1)
        {
            VariableInfo = memoryStream.ScratchDeserializeString();
            VariableRef = memoryStream.ScratchDeserializeString();
            LanguageId = memoryStream.ScratchDeserializeInt();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(VariableInfo)}: {VariableInfo}, {nameof(VariableRef)}: {VariableRef}";
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
        }
    }

    public class BlockHeaderItem_RenturnVariableLabel : BlockHeaderItem<BlockHeaderParam_Data_RenturnVariableLabel>, IBlockHeaderVariableLabel, IBlockLanguage
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
            ContextData.VariableInfo = LabelText.text;
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
                case nameof(BlockHeaderParam_Data_RenturnVariableLabel.VariableInfo):
                    LabelText.text = ContextData.VariableInfo;
                    break;
                case nameof(BlockHeaderParam_Data_RenturnVariableLabel.VariableRef):
                    if (!string.IsNullOrEmpty(ContextData.VariableRef))
                    {
                        int Guid = int.Parse(ContextData.VariableRef);
                        IEngineBlockVariableBase baseData = ScratchEngine.Instance.Core.GetBlocksDataRef(Guid) as IEngineBlockVariableBase;
                        if (baseData != null)
                        {
                            ContextData.VariableInfo = baseData.VariableName;
                        }
                    }
                    else
                    {
                        LabelText.text = ContextData.VariableInfo;
                    }

                    break;
                case nameof(BlockHeaderParam_Data_Label.LanguageId):
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
            Debug.LogError("不允许改名！");
        }

        public IHeaderParamVariable GetVariableData() => ContextData;

        public void SetLanguageId(int id)
        {
            if (ContextData != null)
            {
                ContextData.LanguageId = id;
            }
        }
    }
}