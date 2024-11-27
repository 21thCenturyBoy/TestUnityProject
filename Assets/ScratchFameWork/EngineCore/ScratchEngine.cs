using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchEngine : Singleton<ScratchEngine>
    {
        IEngineCoreInterface m_engineCore;
        public IEngineCoreInterface Core => m_engineCore ??= new TestEngineCore();

        /// <summary>
        /// 编辑器模式&运行时下获取默认值
        /// </summary>
        public string GetDefaultType(ScratchValueType type)
        {
            String value = string.Empty;
            switch (type)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    value = "false";
                    break;
                case ScratchValueType.Byte:
                    value = "0";
                    break;
                case ScratchValueType.Integer:
                    value = "0";
                    break;
                case ScratchValueType.Float:
                    value = "0.00";
                    break;
                case ScratchValueType.Vector2:
                    value = "(0.00,0.00)";
                    break;
                case ScratchValueType.Vector3:
                    value = "(0.00,0.00,0.00)";
                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return value;
        }

        private EngineBlockCanvasGroup m_CurrentGroup;

        public EngineBlockCanvasGroup CurrentGroup
        {
            get => m_CurrentGroup;
            set
            {
                m_CurrentGroup = value;
            }
        }

        private EngineBlockCanvas m_Current;

        public EngineBlockCanvas Current
        {
            get => m_Current;
            set => m_Current = value;
        }

        public Dictionary<int, IEngineBlockBaseData> GetAllBlocksRef()
        {
            if (m_Current == null) return new Dictionary<int, IEngineBlockBaseData>();
            return m_Current.BlockDataDicts;
        }

        public IEngineBlockBaseData GetBlocksDataRef(int guid, Action<IEngineBlockBaseData> callback = null)
        {
            if (m_Current.BlockDataDicts.ContainsKey(guid))
            {
                callback?.Invoke(m_Current.BlockDataDicts[guid]);
                return m_Current.BlockDataDicts[guid];
            }
            return null;
        }
        
        public bool AddBlocksData(IEngineBlockBaseData data)
        {
            if (m_Current.BlockDataDicts.ContainsKey(data.Guid))
            {
                return false;
            }

            m_Current.BlockDataDicts[data.Guid] = data;
            return true;
        }

        public bool RemoveBlocksData(IEngineBlockBaseData data)
        {
            if (!m_Current.BlockDataDicts.ContainsKey(data.Guid))
            {
                return false;
            }

            return m_Current.BlockDataDicts.Remove(data.Guid);
        }
    }
}