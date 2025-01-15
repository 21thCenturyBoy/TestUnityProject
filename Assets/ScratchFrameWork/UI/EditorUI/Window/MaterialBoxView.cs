using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class MaterialBoxView : BaseWindowsView
    {
        private Dictionary<ScratchBlockType, ResourcesItem> m_TemplateResourcesDict = new Dictionary<ScratchBlockType, ResourcesItem>();
        private List<TempResourcesItem> m_VarResourcesItem = new List<TempResourcesItem>();
        

        public Transform TemplatePanelContent;
        public GameObject ResourcesItemPrefab;

        public Transform HeaderPrefab;

        private Transform m_TemplateHeader;
        private Transform m_GlobalHeader;
        private Transform m_LocalHeader;

        
        private ToggleGroup m_toggleGroup;
        private List<FirstLevel> m_firstLevels = new List<FirstLevel>();

        private FirstLevelType m_CurrentShowType = FirstLevelType.None;
        public FirstLevelType CurrentShow => (FirstLevelType)m_CurrentShowType;
        
        public override bool Initialize()
        {
            m_TemplateHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_TemplateHeader.GetComponent<TMP_Text>().text = "-----Template-----";
            m_TemplateHeader.gameObject.SetActive(true);

            m_GlobalHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_GlobalHeader.GetComponent<TMP_Text>().text = "-----Global-----";
            m_GlobalHeader.gameObject.SetActive(true);

            m_LocalHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_LocalHeader.GetComponent<TMP_Text>().text = "-----Local-----";
            m_LocalHeader.gameObject.SetActive(true);

            foreach (KeyValuePair<ScratchBlockType, ResourcesItem> resourcesItem in m_TemplateResourcesDict)
            {
                GameObject.DestroyImmediate(resourcesItem.Value.gameObject);
            }

            m_TemplateResourcesDict.Clear();
            ShowMenu();
            if (m_isInitialized) return false;

            OnInitialize();

            m_isInitialized = true;

            return m_isInitialized;
        }

        public void ShowMenu()
        {
            FirstLevelType[] types = GetFirstLevelMenuTypes();

            FirstLevel[] firstLevels = transform.GetComponentsInChildren<FirstLevel>();

            for (int i = 0; i < firstLevels.Length; i++)
            {
                bool show = types.Contains(firstLevels[i].Type);
                FirstLevelType type = firstLevels[i].Type;

                firstLevels[i].gameObject.SetActive(show);

                var toggle = firstLevels[i].GetComponent<Toggle>();
                toggle.isOn = false;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        m_CurrentShowType = type;

                        ShowFirstLevel(type);
                        BlockResourcesManager.Instance.Active = true;
                    }
                    else
                    {
                        m_CurrentShowType = FirstLevelType.None;
                        BlockResourcesManager.Instance.Active = false;
                    }
                });
            }
        }

        private FirstLevelType[] GetFirstLevelMenuTypes()
        {
            //TODO
            var array = Enum.GetValues(typeof(FirstLevelType));
            FirstLevelType[] types = new FirstLevelType[array.Length];

            int i = 0;
            foreach (var type in array)
            {
                types[i] = (FirstLevelType)type;
                i++;
            }

            return types;
        }

        public void ShowFirstLevel(FirstLevelType firstLevelType)
        {
            foreach (KeyValuePair<ScratchBlockType, ResourcesItem> resourcesItem in m_TemplateResourcesDict)
            {
                GameObject.DestroyImmediate(resourcesItem.Value.gameObject);
            }

            m_TemplateResourcesDict.Clear();

            ResourcesItemData[] datas = null;
            FucType fucType = FucType.Undefined;
            switch (firstLevelType)
            {
                case FirstLevelType.Event:
                    fucType = FucType.Event;
                    break;
                case FirstLevelType.Action:
                    fucType = FucType.Action;
                    break;
                case FirstLevelType.Control:
                    fucType = FucType.Control;
                    break;
                case FirstLevelType.Condition:
                    fucType = FucType.Condition;
                    break;
                case FirstLevelType.GetValue:
                    fucType = FucType.GetValue;
                    break;
                case FirstLevelType.Variable:
                    fucType = FucType.Variable;
                    break;
                case FirstLevelType.Custom:
                    fucType = FucType.Undefined;
                    break;
                case FirstLevelType.Search:
                    fucType = FucType.Undefined;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(firstLevelType), firstLevelType, null);
            }


            bool showVar = firstLevelType == FirstLevelType.Variable || firstLevelType == FirstLevelType.Search;


            datas = BlockResourcesManager.Instance.TemplateData.Where(data => data.Value.BlockFucType == fucType)
                .Select(data => data.Value).ToArray();

            ShowBlockTemplate(datas);
            ShowVariable(showVar);
        }

        private TempResourcesItem CreateTempResourcesItem(IEngineBlockVariableBase blockdata)
        {
            //----- Create TempResourcesItem UI-----
            GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
            TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
            resourcesItem.SetVariableData(blockdata);

            var datas = BlockResourcesManager.Instance.GetResourcesItemData(blockdata.Type).TemplateDatas;

            ResourcesItemData data = new ResourcesItemData(datas, blockdata.Guid.ToString());
            data.ScratchType = blockdata.Type;
            data.Type = blockdata.BlockType;
            data.BlockFucType = blockdata.FucType;

            resourcesItem.Data = data;

            resourcesItem.Active = true;
            resourcesItem.Initialize();

            return resourcesItem;
        }

        private void ShowVariable(bool show)
        {
            bool isGlobal = ScratchEngine.Instance.CurrentIsGlobal;

            m_GlobalHeader.gameObject.SetActive(show);

            m_LocalHeader.gameObject.SetActive(show);

            for (int i = 0; i < m_VarResourcesItem.Count; i++)
            {
                DestroyImmediate(m_VarResourcesItem[i].gameObject);
            }

            m_VarResourcesItem.Clear();
            if (show)
            {
                ScratchEngine.Instance.SerachVariableData(out var globalVars, out var localVars);

                //Global
                for (int i = 0; i < globalVars.Length; i++)
                {
                    //-----Get Engine Data -----
                    var logicdata = globalVars[i];
                    IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;

                    //IsReturnVariable Is Local
                    if (blockdata.IsReturnVariable())
                    {
                        //Current
                        if (ScratchEngine.Instance.Current.ContainGuids(blockdata.Guid))
                        {
                            //----- Create TempResourcesItem UI-----
                            TempResourcesItem resourcesItem = CreateTempResourcesItem(blockdata);

                            resourcesItem.NameField.interactable = false;
                            resourcesItem.ValueField.gameObject.SetActive(false);
                            resourcesItem.DeleteBtn.gameObject.SetActive(false);

                            resourcesItem.SetParent(TemplatePanelContent, m_LocalHeader.GetSiblingIndex() + 1);
                            m_VarResourcesItem.Add(resourcesItem);
                        }
                    }
                    else
                    {
                        //----- Create TempResourcesItem UI-----
                        TempResourcesItem resourcesItem = CreateTempResourcesItem(blockdata);

                        resourcesItem.SetParent(TemplatePanelContent, m_GlobalHeader.GetSiblingIndex() + 1);
                        m_VarResourcesItem.Add(resourcesItem);
                    }
                }

                if (!isGlobal)
                {
                    for (int i = 0; i < localVars.Length; i++)
                    {
                        //-----获取Koala数据层-----
                        var logicdata = localVars[i];
                        IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;

                        //----- Create TempResourcesItem UI-----
                        TempResourcesItem resourcesItem = CreateTempResourcesItem(blockdata);

                        if (blockdata.IsReturnVariable())
                        {
                            resourcesItem.NameField.interactable = false;
                            resourcesItem.ValueField.gameObject.SetActive(false);
                            resourcesItem.DeleteBtn.gameObject.SetActive(false);
                        }

                        resourcesItem.SetParent(TemplatePanelContent, m_LocalHeader.GetSiblingIndex() + 1);

                        m_VarResourcesItem.Add(resourcesItem);
                    }
                }
            }
        }

        public void ShowBlockTemplate(params ResourcesItemData[] templateDatas)
        {
            for (int i = 0; i < templateDatas.Length; i++)
            {
                GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);

                ResourcesItemData data = templateDatas[i];

                ResourcesItem resourcesItem = null;

                if (data.BlockFucType == FucType.Variable)
                {
                    resourcesItem = obj.AddComponent<VariableResourcesItem>();
                }
                else
                {
                    resourcesItem = obj.AddComponent<ResourcesItem>();
                }

                resourcesItem.Data = templateDatas[i];

                resourcesItem.Active = true;

                resourcesItem.SetParent(TemplatePanelContent, m_TemplateHeader.GetSiblingIndex() + 1);

                resourcesItem.Initialize();

                m_TemplateResourcesDict[data.ScratchType] = resourcesItem;
            }
        }

        public void Refresh()
        {
            if (CurrentShow == FirstLevelType.None) return;

            ShowFirstLevel(CurrentShow);
        }
    }
}