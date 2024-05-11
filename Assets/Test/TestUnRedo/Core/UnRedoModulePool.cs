using System;
using System.Collections.Generic;

namespace TestUnRedo
{
    /// <summary> 撤销还原功能模块池 </summary>
    public class UnRedoModulePool<T> : ClassObjectPool<T> where T : UnRedoModule, new()
    {
        private HashSet<T> m_refSet = new HashSet<T>();
        
        public HashSet<T> RefSet => m_refSet;
        public int RefCount => RefSet.Count;

        public new T Spawn(bool createIfPoolEmpty)
        {
            if (pool.Count > 0)
            {
                T rtn = pool.Pop();
                if (rtn == null)
                {
                    if (createIfPoolEmpty)
                    {
                        rtn = new T();
                    }
                }

                rtn.Init();
                m_refSet.Add(rtn);
                return rtn;
            }
            else
            {
                if (createIfPoolEmpty)
                {
                    T rtn = new T();

                    rtn.Init();

                    m_refSet.Add(rtn);
                    return rtn;
                }
            }

            return null;
        }

        public new bool Recycle(T obj)
        {
            if (obj == null) return false;

            obj.Release();

            if (m_refSet.Contains(obj))
            {
                m_refSet.Remove(obj);
            }

            if (pool.Count >= maxCount && maxCount > 0)
            {
                obj = null;
                return false;
            }

            pool.Push(obj);
            return true;
        }

        public void AutoRecycle()
        {
            foreach (var module in m_refSet)
            {
                module.Release();
                
                if (pool.Count >= maxCount && maxCount > 0)
                {
                    return;
                }
                pool.Push(module);
            }
            
            m_refSet.Clear();
        }
        
        public void ClearRef()
        {
            foreach (var module in m_refSet)
            {
                module.Release();
            }

            m_refSet.Clear();
        }

        public new void Clear()
        {
            foreach (var module in pool)
            {
                module.Release();
            }

            pool.Clear();
            maxCount = 0;
        }

        public UnRedoModulePool(int maxCount) : base(maxCount)
        {
        }
    }
}