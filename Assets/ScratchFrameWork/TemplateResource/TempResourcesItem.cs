using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace ScratchFramework
{
    public class TempResourcesItem : ResourcesItem
    {
        public Button DeleteBtn;
        public TMPro.TMP_InputField NameField;
        public TMPro.TMP_InputField ValueField;
        public Button RefBtn;

        private IEngineBlockVariableBase m_VariableData;

        private string m_PreVal = string.Empty;

        public void SetVariableData(IEngineBlockVariableBase variableBase)
        {
            m_VariableData = variableBase;
        }

        protected override void OnInitialize()
        {
            if (NameField == null) NameField = transform.Find("NameField").GetComponent<TMPro.TMP_InputField>();
            if (DeleteBtn == null) DeleteBtn = transform.Find("DeleteBtn").GetComponent<Button>();
            if (ValueField == null) ValueField = transform.Find("ValueField").GetComponent<TMPro.TMP_InputField>();
            if (RefBtn == null) RefBtn = transform.Find("RefBtn").GetComponent<Button>();

            DeleteBtn.onClick.RemoveAllListeners();
            DeleteBtn.onClick.AddListener(OnDelete);

            ValueField.onEndEdit.RemoveAllListeners();
            ValueField.onSelect.RemoveAllListeners();
            ValueField.onSelect.AddListener(OnValueFieldSelect);
            ValueField.onEndEdit.AddListener(OnValueFieldEndEdit);

            NameField.onEndEdit.RemoveAllListeners();
            NameField.onEndEdit.AddListener(OnNameFieldEndEdit);

            RefBtn.onClick.RemoveAllListeners();
            RefBtn.onClick.AddListener(OnRefBtn);

            DeleteBtn.gameObject.SetActive(true);
            NameField.gameObject.SetActive(true);
            ValueField.gameObject.SetActive(true);
            RefBtn.gameObject.SetActive(true);

            if (m_VariableData != null)
            {
                ValueField.text = ScratchUtils.EngineVariableToString(m_VariableData);
                NameField.text = m_VariableData.VariableName;
            }
        }

        private void OnRefBtn()
        {
            if (BlockResourcesManager.Instance.GetResourcesItemData(m_VariableData.Type) != null)
            {
                Block block = ScratchUtils.DeserializeBlock(Data.TemplateDatas, BlockCanvasUIManager.Instance.RectTrans);

                block.Position = TempCanvasUIManager.Instance.CanvasCenter.Position;

                var variableLabel = block.VariableLabel;
                if (variableLabel != null)
                {
                    var variableData = variableLabel.GetVariableData();
                    //绑定数据
                    variableData.VariableRef = m_VariableData.Guid.ToString();
                    
                    BlockFragmentDataRef dataRef = DataRefDirector.Create(m_VariableData);
                    ScratchEngine.Instance.AddFragmentDataRef(dataRef);

                    block.SetKoalaBlock(dataRef);
                }
            }
            else
            {
                Debug.LogError("没资源！");
            }
        }

        private void OnValueFieldSelect(string arg0)
        {
            m_PreVal = arg0;
        }

        public virtual void OnDelete()
        {
            Debug.LogError("还没做！");
        }

        public virtual void OnNameFieldEndEdit(string strInfo)
        {
            foreach (var scData in BlockDataUIManager.Instance.DataDict.Values)
            {
                if (scData is IHeaderParamVariable paramData)
                {
                    if (m_VariableData.Guid.ToString() == paramData.VariableRef)
                    {
                        paramData.VariableInfo = strInfo;
                        m_VariableData.VariableName = strInfo;
                    }
                }
            }
        }

        public virtual void OnValueFieldEndEdit(string strInfo)
        {
            if (ScratchUtils.String2VariableEngine(strInfo, m_VariableData))
            {
            }
            else
            {
                ValueField.SetTextWithoutNotify(m_PreVal);
                Debug.LogError("无法改值！");
            }
        }
    }
}