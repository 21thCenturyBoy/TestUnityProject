using System;
using System.Collections.Generic;

/// <summary>
/// 安全回调（在同一个命名空间下Invoke）
/// </summary>
namespace Utils
{
    public class SafeListener<T1, T2, T3>
    {
        private List<Action<T1, T2, T3>> m_ActionCBList = new List<Action<T1, T2, T3>>();
        public event Action<T1, T2, T3> Listener
        {
            add
            {
                if (m_ActionCBList.Contains(value)) return;
                m_ActionCBList.Add(value);
            }
            remove
            {
                m_ActionCBList.Remove(value);
            }
        }
        internal void Invoke(T1 t1, T2 t2, T3 t3)
        {
            var array = m_ActionCBList.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                m_ActionCBList[i].Invoke(t1, t2, t3);
            }
        }
    }
    public class SafeListener<T1, T2>
    {
        private List<Action<T1, T2>> m_ActionCBList = new List<Action<T1, T2>>();
        public event Action<T1, T2> Listener
        {
            add
            {
                if (m_ActionCBList.Contains(value)) return;
                m_ActionCBList.Add(value);
            }
            remove
            {
                m_ActionCBList.Remove(value);
            }
        }
        internal void Invoke(T1 t1, T2 t2)
        {
            var array = m_ActionCBList.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                m_ActionCBList[i].Invoke(t1, t2);
            }
        }
    }
    public class SafeListener<T1>
    {
        private List<Action<T1>> m_ActionCBList = new List<Action<T1>>();
        public event Action<T1> Listener
        {
            add
            {
                if (m_ActionCBList.Contains(value)) return;
                m_ActionCBList.Add(value);
            }
            remove
            {
                m_ActionCBList.Remove(value);
            }
        }
        internal void Invoke(T1 t1)
        {
            var array = m_ActionCBList.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                m_ActionCBList[i].Invoke(t1);
            }
        }
    }
    public class SafeListener
    {
        private List<Action> m_ActionCBList = new List<Action>();
        public event Action Listener
        {
            add
            {
                if (m_ActionCBList.Contains(value)) return;
                m_ActionCBList.Add(value);
            }
            remove
            {
                m_ActionCBList.Remove(value);
            }
        }
        internal void Invoke()
        {
            var array = m_ActionCBList.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                m_ActionCBList[i].Invoke();
            }
        }
    }
}
