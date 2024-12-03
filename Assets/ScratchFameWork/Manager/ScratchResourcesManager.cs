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
        private List<TempResourcesItem> m_VarResourcesItem = new List<TempResourcesItem>();
        private Dictionary<ScratchBlockType, ResourcesItemData> m_TemplateTextDatas = new Dictionary<ScratchBlockType, ResourcesItemData>();

        public Transform TemplatePanelContent;
        public GameObject ResourcesItemPrefab;


        public Transform HeaderPrefab;

        private Transform m_TemplateHeader;
        private Transform m_GlobalHeader;
        private Transform m_LocalHeader;

        public override bool Initialize()
        {
            m_TemplateHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_TemplateHeader.GetComponent<TMP_Text>().text = "-----积木模版-----";
            m_TemplateHeader.gameObject.SetActive(true);

            m_GlobalHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_GlobalHeader.GetComponent<TMP_Text>().text = "-----全局变量-----";
            m_GlobalHeader.gameObject.SetActive(true);

            m_LocalHeader = GameObject.Instantiate(HeaderPrefab, TemplatePanelContent);
            m_LocalHeader.GetComponent<TMP_Text>().text = "-----局部变量-----";
            m_LocalHeader.gameObject.SetActive(true);

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

        public ResourcesItemData[] GetTemplateData(FucType type)
        {
            return m_TemplateTextDatas.Values.Where(data => data.BlockFucType == type).ToArray();
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


            datas = m_TemplateTextDatas.Where(data => data.Value.BlockFucType == fucType)
                .Select(data => data.Value).ToArray();

            ShowBlockTemplate(datas);
            ShowVariable(showVar);
        }

        private void ShowVariable(bool show)
        {
            bool isGlobal = ScratchEngine.Instance.CurrentIsGlobal;

            m_GlobalHeader.gameObject.SetActive(show);

            m_LocalHeader.gameObject.SetActive(show && !isGlobal);

            for (int i = 0; i < m_VarResourcesItem.Count; i++)
            {
                DestroyImmediate(m_VarResourcesItem[i].gameObject);
            }

            m_VarResourcesItem.Clear();
            if (show)
            {
                ScratchEngine.Instance.SerachVariableData(out var globalVars, out var localVars);

                for (int i = 0; i < globalVars.Count; i++)
                {
                    //-----获取Koala数据层-----
                    var logicdata = globalVars[i];
                    IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;

                    //-----创建TempResourcesItem UI-----
                    GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
                    TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
                    resourcesItem.SetVariableData(blockdata);

                    var datas = ScratchResourcesManager.Instance.GetResourcesItemData(blockdata.Type).TemplateDatas;

                    ResourcesItemData data = new ResourcesItemData(datas, blockdata.Guid.ToString());
                    data.ScratchType = blockdata.Type;
                    data.Type = blockdata.BlockType;
                    data.BlockFucType = blockdata.FucType;

                    resourcesItem.Data = data;
                    resourcesItem.Active = true;
                    resourcesItem.Initialize();

                    if (blockdata.IsReturnVariable())
                    {
                        resourcesItem.NameField.interactable = false;
                        resourcesItem.ValueField.gameObject.SetActive(false);
                        resourcesItem.DeleteBtn.gameObject.SetActive(false);
                    }

                    resourcesItem.SetParent(TemplatePanelContent, m_GlobalHeader.GetSiblingIndex() + 1);

                    m_VarResourcesItem.Add(resourcesItem);
                }


                if (!isGlobal)
                {
                    for (int i = 0; i < localVars.Count; i++)
                    {
                        //-----获取Koala数据层-----
                        var logicdata = localVars[i];
                        IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;

                        //-----创建TempResourcesItem UI-----
                        GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
                        TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
                        resourcesItem.SetVariableData(blockdata);

                        var datas = ScratchResourcesManager.Instance.GetResourcesItemData(blockdata.Type).TemplateDatas;

                        ResourcesItemData data = new ResourcesItemData(datas, blockdata.Guid.ToString());
                        data.ScratchType = blockdata.Type;
                        data.Type = blockdata.BlockType;
                        data.BlockFucType = blockdata.FucType;

                        resourcesItem.Data = data;
                        resourcesItem.Active = true;
                        resourcesItem.Initialize();

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


        public void OnCanvasAddBlock(Block block)
        {
            // if (block.BlockFucType == FucType.Variable)
            // {
            //     if (block.VariableLabel != null)
            //     {
            //         var ResourcesItem = m_VarResourcesItem.FirstOrDefault(data => data.Data.Name == block.GetEngineBlockData().Guid.ToString());
            //         if (ResourcesItem == null)
            //         {
            //             TempResourcesItem resourcesItem = CreateVariable(block);
            //             resourcesItem.Active = FirstLevelMenu.Instance.CurrentShow == FirstLevelType.Variable;
            //         }
            //     }
            // }
        }

        public void OnCanvasRemoveBlock(Block block)
        {
            // if (block.BlockFucType == FucType.Variable)
            // {
            //     var index = m_VarResourcesItem.FindIndex(data => data.Data.Name == block.GetEngineBlockData().Guid.ToString());
            //
            //     if (index != -1)
            //     {
            //         TempResourcesItem varItem = m_VarResourcesItem[index] as TempResourcesItem;
            //         DestroyImmediate(varItem.gameObject);
            //
            //         m_VarResourcesItem.RemoveAt(index);
            //     }
            // }
        }


        // public TempResourcesItem CreateVariable(Block block)
        // {
        //     if (block.BlockFucType != FucType.Variable) return null;
        //
        //     var variableLabel = block.VariableLabel;
        //
        //     var variableData = ScratchDataManager.Instance.CreateVariable(variableLabel);
        //
        //     if (variableData == null)
        //     {
        //         Debug.LogError("已经创建变量！理论上不应该出现，但拦截保护一下数据！可以忽略");
        //         return null;
        //     }
        //
        //     //-----获取Koala数据层-----
        //     var logicdata = block.GetEngineBlockData();
        //     IEngineBlockVariableBase blockdata = logicdata as IEngineBlockVariableBase;
        //
        //     if (blockdata == null)
        //     {
        //         Debug.LogError($"不应该类型:{block.scratchType}");
        //         return null;
        //     }
        //
        //     //创建变量名
        //     ScratchUtils.CreateVariableName(blockdata, variableData);
        //
        //     //-----创建TempResourcesItem UI-----
        //     GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
        //     TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
        //     resourcesItem.SetVariableData(blockdata);
        //
        //     var blockData = block.GetDataRef() as BlockData;
        //     var datas = ScratchResourcesManager.Instance.GetResourcesItemData(blockData.ScratchType).TemplateDatas;
        //
        //     ResourcesItemData data = new ResourcesItemData(datas, variableData.VariableRef);
        //     data.ScratchType = blockData.ScratchType;
        //     data.Type = blockData.Type;
        //     data.BlockFucType = blockData.BlockFucType;
        //
        //     resourcesItem.Data = data;
        //     resourcesItem.Active = true;
        //     resourcesItem.Initialize();
        //
        //     if (variableData is BlockHeaderParam_Data_VariableLabel)
        //     {
        //     }
        //
        //     if (variableData is BlockHeaderParam_Data_RenturnVariableLabel)
        //     {
        //         resourcesItem.NameField.interactable = false;
        //         resourcesItem.ValueField.gameObject.SetActive(false);
        //         resourcesItem.DeleteBtn.gameObject.SetActive(false);
        //     }
        //
        //     resourcesItem.SetParent(TemplatePanelContent, TemplateVarHeader.GetSiblingIndex() + 1);
        //
        //     m_VarResourcesItem.Add(resourcesItem);
        //
        //
        //     return resourcesItem;
        // }


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

        public void RefreshResources()
        {
            if (FirstLevelMenu.Instance.CurrentShow == FirstLevelType.None) return;

            ShowFirstLevel(FirstLevelMenu.Instance.CurrentShow);
        }
    }
}