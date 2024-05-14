using System.Collections.Generic;

public class ClassObjectPool<T> where T : class, new()
{
    protected Stack<T> pool = new Stack<T>();
    public int Count { get => pool.Count; }

    //最大对象个数  <=0表示不限制个数
    protected int maxCount;
    public ClassObjectPool(int maxCount)
    {
        this.maxCount = maxCount;
    }

    /// <summary>
    /// 从池里面取类对象
    /// </summary>
    /// <param name="createIfPoolEmpty">如果为空是否new出来</param>
    public T Spawn(bool createIfPoolEmpty)
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
            return rtn;
        }
        else
        {
            if (createIfPoolEmpty)
            {
                T rtn = new T();
                return rtn;
            }
        }
        return null;
    }

    /// <summary>
    /// 回收类对象
    /// </summary>
    public bool Recycle(T obj)
    {
        if (obj == null)
            return false;
        if (pool.Count >= maxCount && maxCount > 0)
        {
            obj = null;
            return false;
        }
        pool.Push(obj);
        return true;
    }

    public void Clear()
    {
        pool.Clear();
        maxCount = 0;
    }
}