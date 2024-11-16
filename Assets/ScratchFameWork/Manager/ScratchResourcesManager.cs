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
        private List<ResourcesItem> m_TemplateResourcesItem = new List<ResourcesItem>();

        public Dictionary<ScratchBlockType, ResourcesItem> TemplateResourcesDict = new Dictionary<ScratchBlockType, ResourcesItem>();

        public Transform TemplatePanelContent;
        public GameObject ResourcesItemPrefab;


        public Transform TemplateBlockHeader;
        public Transform TemplateVarHeader;

        public override bool Initialize()
        {
            for (int i = 0; i < m_TemplateResourcesItem.Count; i++)
            {
                GameObject.DestroyImmediate(m_TemplateResourcesItem[i].gameObject);
                m_TemplateResourcesItem[i] = null;
            }

            m_TemplateResourcesItem.Clear();

            if (m_isInitialized) return false;

            OnInitialize();

            m_isInitialized = true;

            return m_isInitialized;
        }


        public void LoadAllResource(Action successCallback = null, Action failedCallback = null)
        {
            TemplateResourcesDict.Clear();
            //TODO 资源管理
            successCallback?.Invoke();

            var TemplateTextDatas = ScratchConfig.Instance.TemplateDatas;
            for (int i = 0; i < TemplateTextDatas.Count; i++)
            {
                GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);

                BlockData blockData = new BlockData();
                MemoryStream stream = ScratchUtils.CreateMemoryStream(TemplateTextDatas[i].bytes);
                blockData.Deserialize_Base(stream);

                ResourcesItem resourcesItem = null;

                if (blockData.BlockFucType == FucType.Variable)
                {
                    resourcesItem = obj.AddComponent<VariableResourcesItem>();
                }
                else
                {
                    resourcesItem = obj.AddComponent<ResourcesItem>();
                }

                ResourcesItemData data = new ResourcesItemData(TemplateTextDatas[i].bytes, TemplateTextDatas[i].name);

                data.ScratchType = blockData.ScratchType;
                data.Type = blockData.Type;
                data.BlockFucType = blockData.BlockFucType;

                resourcesItem.Data = data;

                resourcesItem.Active = true;

                resourcesItem.SetParent(TemplatePanelContent, TemplateVarHeader.GetSiblingIndex());

                resourcesItem.Initialize();

                m_TemplateResourcesItem.Add(resourcesItem);

                TemplateResourcesDict[blockData.ScratchType] = resourcesItem;
            }

            ClearAllTempData();
        }

        public int ClearAllTempData()
        {
            return m_TemplateResourcesItem.RemoveAll(item =>
            {
                if (item is TempResourcesItem tempResourcesItem)
                {
                    GameObject.DestroyImmediate(tempResourcesItem.gameObject);
                    return true;
                }

                return false;
            });
        }


        public void OnCanvasAddBlock(Block block)
        {
            if (block.BlockFucType == FucType.Variable)
            {
                if (block.VariableLabel != null)
                {
                    var ResourcesItem = m_TemplateResourcesItem.FirstOrDefault(data => data.Data.Name == block.GetEngineBlockData().Guid.ToString());
                    if (ResourcesItem == null)
                    {
                        CreateVariable(block);
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
            if (string.IsNullOrEmpty(blockdata.VariableName))
            {
                if (variableData is BlockHeaderParam_Data_VariableLabel variable)
                {
                    string variableRef = blockdata.Guid.ToString();
                    switch (blockdata.Type)
                    {
                        case ScratchBlockType.IntegerValue:
                            blockdata.VariableName = $"[int]{variableRef}";
                            break;
                        case ScratchBlockType.VectorValue:
                            blockdata.VariableName = $"[Vector3]{variableRef}";
                            break;
                        case ScratchBlockType.EntityValue:
                            blockdata.VariableName = $"[Entity]{variableRef}";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (variableData is BlockHeaderParam_Data_RenturnVariableLabel returnVariable)
                {
                    blockdata.VariableName = $"[Entity]{returnVariable.VariableInfo}";
                }
            }

            //绑定数据
            variableData.VariableRef = blockdata.Guid.ToString();

            //-----创建TempResourcesItem UI-----
            GameObject obj = GameObject.Instantiate(ResourcesItemPrefab, TemplatePanelContent);
            TempResourcesItem resourcesItem = obj.AddComponent<TempResourcesItem>();
            resourcesItem.SetVariableData(blockdata);

            var blockData = block.GetDataRef() as BlockData;
            var datas = ScratchResourcesManager.Instance.TemplateResourcesDict[blockData.ScratchType].Data.TemplateDatas;

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

            m_TemplateResourcesItem.Add(resourcesItem);


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