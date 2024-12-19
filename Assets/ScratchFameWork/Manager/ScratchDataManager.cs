using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchDataManager : Singleton_Class<ScratchDataManager>, IScratchManager
    {
        protected Dictionary<Guid, ScratchVMData> m_Dict = new Dictionary<Guid, ScratchVMData>();
        public Dictionary<Guid, ScratchVMData> DataDict => m_Dict;

        private int m_DataCurrentIndex = ScratchVMData.UnallocatedId;

        private Dictionary<int, Guid> m_IdDict = new();

        public bool Initialize()
        {
            //清除冗余数据
            DataDict.Clear();
            return true;
        }

        public ScratchVMData GetDataById(int data)
        {
            if (m_IdDict.ContainsKey(data))
            {
                if (m_Dict.ContainsKey(m_IdDict[data]))
                {
                    return m_Dict[m_IdDict[data]];
                }
            }

            return null;
        }

        public void AddData(ScratchVMData vmdata)
        {
            if (!m_Dict.ContainsKey(vmdata.Guid))
            {
                m_Dict[vmdata.Guid] = vmdata;
                m_DataCurrentIndex++;
                vmdata.IdPtr = m_DataCurrentIndex;

                m_IdDict[vmdata.IdPtr] = vmdata.Guid;
            }
            else
            {
                // Debug.LogError($" Data Is Added!");
            }
        }

        public void RemoveData(ScratchVMData vmdata)
        {
            if (m_Dict.ContainsKey(vmdata.Guid))
            {
                m_Dict.Remove(vmdata.Guid);
                
                m_IdDict.Remove(vmdata.IdPtr);
            }
        }
        

        public bool Active { get; set; }

        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            m_DataCurrentIndex = ScratchVMData.UnallocatedId;

            m_Dict.Clear();
            m_IdDict.Clear();
            return true;
        }

        public void Save()
        {
            ScratchEngine.Instance.Core.SaveCanvasGroup(ScratchEngine.Instance.CurrentGroup, (res) => { Debug.Log("Save Success:" + res); });
        }
    }
}