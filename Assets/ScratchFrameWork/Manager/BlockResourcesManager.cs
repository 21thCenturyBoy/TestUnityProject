using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;


namespace ScratchFramework
{
    /// <summary>
    /// 积木素材资源管理类
    /// </summary>
    public class BlockResourcesManager : ScratchSingleton<BlockResourcesManager>, IScratchManager
    {
        private Dictionary<ScratchBlockType, ResourcesItemData> m_TemplateTextDatas = new Dictionary<ScratchBlockType, ResourcesItemData>();

        public Dictionary<ScratchBlockType, ResourcesItemData> TemplateData => m_TemplateTextDatas;

        public override bool Initialize()
        {
            m_TemplateTextDatas.Clear();

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

        public void OnCanvasAddBlock(Block block)
        {
            if (block.BlockFucType == FucType.Variable)
            {
                if (block.GetDataRef() != null)
                {
                    var variableLabel = block.VariableLabel;
                    // var variableData = ScratchDataManager.Instance.CreateVariable(variableLabel);
                }
            }
        }

        public void OnCanvasRemoveBlock(Block block)
        {
            if (block.BlockFucType == FucType.Variable)
            {
                // var index = m_VarResourcesItem.FindIndex(data => data.Data.Name == block.GetEngineBlockData().Guid.ToString());
                //
                // if (index != -1)
                // {
                //     TempResourcesItem varItem = m_VarResourcesItem[index] as TempResourcesItem;
                //     DestroyImmediate(varItem.gameObject);
                //
                //     m_VarResourcesItem.RemoveAt(index);
                // }
            }
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

        public void RefreshResources()
        {
            if (WindowManager.Instance.SingletonIsOpen<MaterialBoxView>(out MaterialBoxView materialBoxView))
            {
                materialBoxView.Refresh();
            }
        }
    }
}