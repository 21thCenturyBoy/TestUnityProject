using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ScratchFramework
{
    public class ScratchResourcesManager : ScratchUISingleton<ScratchResourcesManager>, IScratchManager
    {
        private Dictionary<ScratchBlockType, ResourcesItem> m_TemplateResourcesDict = new Dictionary<ScratchBlockType, ResourcesItem>();
        private List<ResourcesItem> m_VarResourcesItem = new List<ResourcesItem>();
        private Dictionary<ScratchBlockType, ResourcesItemData> m_TemplateTextDatas = new Dictionary<ScratchBlockType, ResourcesItemData>();

        public Transform TemplatePanelContent;
        public GameObject ResourcesItemPrefab;


        public Transform TemplateBlockHeader;
        public Transform TemplateVarHeader;

        public override bool Initialize()
        {
            foreach (KeyValuePair<ScratchBlockType, ResourcesItem> resourcesItem in m_TemplateResourcesDict)
            {
                GameObject.DestroyImmediate(resourcesItem.Value.gameObject);
            }

            m_TemplateResourcesDict.Clear();

            if (m_isInitialized) return false;

            OnInitialize();

            m_isInitialized = true;

            return m_isInitialized;
        }

        public void LoadAllResource(Action successCallback = null, Action failedCallback = null)
        {
            var TemplateTextDatas = ScratchConfig.Instance.TemplateDatas;

            for (int i = 0; i < TemplateTextDatas.Count; i++)
            {
                BlockData blockData = new BlockData();
                MemoryStream stream = ScratchUtils.CreateMemoryStream(TemplateTextDatas[i].bytes);
                blockData.Deserialize_Base(stream);


                ResourcesItemData data = new ResourcesItemData(TemplateTextDatas[i].bytes, TemplateTextDatas[i].name);

                data.ScratchType = blockData.ScratchType;
                data.Type = blockData.Type;
                data.BlockFucType = blockData.BlockFucType;

                m_TemplateTextDatas[data.ScratchType] = data;
            }

            //TODO 资源管理
            successCallback?.Invoke();
        }

        public ResourcesItemData GetResourcesItemData(ScratchBlockType scratchType)
        {
            if (m_TemplateTextDatas.ContainsKey(scratchType))
            {
                return m_TemplateTextDatas[scratchType];
            }

            return null;
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

            if (fucType == FucType.Undefined)
            {
                if (firstLevelType == FirstLevelType.Search)
                {
                    datas = m_TemplateTextDatas.Values.ToArray();
                }
                else if (firstLevelType == FirstLevelType.Custom)
                {
                    datas = Array.Empty<ResourcesItemData>();
                    foreach (ResourcesItem varRe in m_VarResourcesItem)
                    {
                        varRe.Active = false;
                    }

                    TemplateVarHeader.gameObject.SetActive(false);
                }
            }
            else
            {
                if (firstLevelType == FirstLevelType.Variable)
                {
                    foreach (ResourcesItem varRe in m_VarResourcesItem)
                    {
                        varRe.Active = true;
                    }

                    TemplateVarHeader.gameObject.SetActive(true);
                }
                else
                {
                    foreach (ResourcesItem varRe in m_VarResourcesItem)
                    {
                        varRe.Active = false;
                    }

                    TemplateVarHeader.gameObject.SetActive(false);
                }

                datas = m_TemplateTextDatas.Where(data => data.Value.BlockFucType == fucType)
                    .Select(data => data.Value).ToArray();
            }

            ShowBlockTemplate(datas);
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

                resourcesItem.SetParent(TemplatePanelContent, TemplateVarHeader.GetSiblingIndex());

                resourcesItem.Initialize();

                m_TemplateResourcesDict[data.ScratchType] = resourcesItem;
            }
        }


        public void OnCanvasAddBlock(Block block)
        {
            if (block.BlockFucType == FucType.Variable)
            {
                if (block.VariableLabel != null)
                {
                    var ResourcesItem = m_VarResourcesItem.FirstOrDefault(data => data.Data.Name == block.GetEngineBlockData().Guid.ToString());
                    if (ResourcesItem == null)
                    {
                        TempResourcesItem resourcesItem = CreateVariable(block);
                        resourcesItem.Active = FirstLevelMenu.Instance.CurrentShow == FirstLevelType.Variable;
                    }
                }
            }
        }

        public void OnCanvasRemoveBlock(Block block)
        {
            if (block.BlockFucType == FucType.Variable)
            {
            }
        }

        public TempResourcesItem CreateVariable(Block block)
        {
            if (block.BlockFucType != FucType.Variable) return null;

            var variableLabel = block.VariableLabel;

            var variableData = ScratchDataManager.Instance.CreateVariable(variableLabel);

            if (variableData == null)
            {
                Debug.LogError("已经创建变量！理论上不应该出现，但拦截保护一下数据！可以忽略");
                return null;
            }

            //-----获取Koala数据层-----
            var logicdata = block.GetEngineBlockData();
            IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;

            if (blockdata == null)
            {
                Debug.LogError($"不应该类型:{block.scratchType}");
                return null;
            }

            //创建变量名
            ScratchUtils.CreateVariableName(blockdata, variableData);

            //-----创建TempResourcesItem UI-----
            GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
            TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
            resourcesItem.SetVariableData(blockdata);

            var blockData = block.GetDataRef() as BlockData;
            var datas = ScratchResourcesManager.Instance.GetResourcesItemData(blockData.ScratchType).TemplateDatas;

            ResourcesItemData data = new ResourcesItemData(datas, variableData.VariableRef);
            data.ScratchType = blockData.ScratchType;
            data.Type = blockData.Type;
            data.BlockFucType = blockData.BlockFucType;

            resourcesItem.Data = data;
            resourcesItem.Active = true;
            resourcesItem.Initialize();

            if (variableData is BlockHeaderParam_Data_VariableLabel)
            {
            }

            if (variableData is BlockHeaderParam_Data_RenturnVariableLabel)
            {
                resourcesItem.NameField.interactable = false;
                resourcesItem.ValueField.gameObject.SetActive(false);
                resourcesItem.DeleteBtn.gameObject.SetActive(false);
            }

            resourcesItem.SetParent(TemplatePanelContent, TemplateVarHeader.GetSiblingIndex() + 1);

            m_VarResourcesItem.Add(resourcesItem);


            return resourcesItem;
        }


        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}