using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchEngine : Singleton<ScratchEngine>
    {
        IEngineCoreInterface m_engineCore;
        public IEngineCoreInterface Core => m_engineCore ??= new TestEngineCore();

        private EngineBlockCanvasGroup m_CurrentGroup;

        public EngineBlockCanvasGroup CurrentGroup
        {
            get => m_CurrentGroup;
            set { m_CurrentGroup = value; }
        }

        private EngineBlockCanvas m_Current;

        public EngineBlockCanvas Current
        {
            get => m_Current;
            set
            {
                if (m_Current == value) return;
                m_Current = value;

                ScratchUtils.SetDirtyType(BlocksDataDirtyType.Change);
            }
        }

        public bool CurrentIsGlobal => m_Current == m_CurrentGroup.GlobalCanvas;

        public bool ContainGuids(int guid)
        {
            return m_Current.ContainGuids(guid);
        }
        
        public void SerachVariableData(out List<IEngineBlockBaseData> listGlobal, out List<IEngineBlockBaseData> listLocal)
        {
            listGlobal = CurrentGroup.GlobalCanvas.BlockDataDicts.Where(pair => pair.Value is IEngineBlockVariableBase).Select(pair => pair.Value).ToList();
            listLocal = Current.BlockDataDicts.Where(pair => pair.Value is IEngineBlockVariableBase).Select(pair => pair.Value).ToList();
        }

        public bool AddBlocksData(IEngineBlockBaseData data)
        {
            return m_Current.AddBlocksData(data);
        }

        public bool RemoveBlocksData(IEngineBlockBaseData data)
        {
            return m_Current.RemoveBlocksData(data);
        }
    }
}