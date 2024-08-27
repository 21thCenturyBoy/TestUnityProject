using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Object = System.Object;

namespace ScratchFramework
{
    public enum ScratchEventDefine
    {
        Default,
        
        //Block
        OnBlockBeginDrag,
        OnBlockDrag,
        OnBlockEndDrag,
    }

    public class SafeListener { }

    public class SafeListener<T> : SafeListener
    {
        private List<Action<T>> m_ActionCBList;

        public event Action<T> Listener
        {
            add
            {
                if (m_ActionCBList == null)
                {
                    m_ActionCBList = new List<Action<T>>();
                }
                else
                {
                    if (m_ActionCBList.Contains(value)) return;
                }

                m_ActionCBList.Add(value);
            }
            remove => m_ActionCBList?.Remove(value);
        }

        internal void Invoke(T data)
        {
            if (m_ActionCBList != null)
            {
                for (int i = 0; i < m_ActionCBList.Count; i++)
                {
                    m_ActionCBList[i]?.Invoke(data);
                }
            }
        }
    }

    public class ScratchEventManager : Singleton_Class<ScratchEventManager>,IScratchManager
    {
        private Dictionary<ScratchEventDefine, SafeListener> m_eventMap = new();
        
        public void AddListener<T>(ScratchEventDefine eventKey, Action<T> callback)
        {
            if (!m_eventMap.ContainsKey(eventKey))
            {
                m_eventMap.Add(eventKey, new SafeListener<T>());
            }

            if (callback != null && m_eventMap[eventKey] is SafeListener<T> eventListener)
            {
                eventListener.Listener += callback;
            }
        }

        public void RemoveListener<T>(ScratchEventDefine eventKey, Action<T> callback) where T : new()
        {
            if (!m_eventMap.ContainsKey(eventKey))
            {
                m_eventMap.Add(eventKey, new SafeListener<object>());
            }

            if (callback != null && m_eventMap[eventKey] is SafeListener<T> eventListener)
            {
                eventListener.Listener -= callback;
            }
        }

        public void SendEvent<T>(ScratchEventDefine eventDefine, T data)
        {
            if (m_eventMap.ContainsKey(eventDefine))
            {
                if (m_eventMap[eventDefine] is SafeListener<T> eventListener)
                {
                    eventListener?.Invoke(data);
                }
            }
            
            try
            {
                //TODO CustomSendEvent
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public bool Initialize()
        {
            return true;
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
            return true;
        }
    }
}