using System;
using System.Collections.Generic;
using System.Linq;

public interface IAwake
{
    void OnAwake();
}
public interface IStart
{
    void OnStart();
}
public interface IUpdtae
{
    void OnUpdtae();
}
public interface ILateUpdta
{
    void OnLateUpdtae();
}
public interface IFixedUpdtae
{
    void OnFixedUpdtae();
}
public interface IDestroy
{
    void OnDestroy();
}
public interface IComponent : IAwake, IStart, IUpdtae, ILateUpdta, IFixedUpdtae, IDestroy
{
    public Entity ComEntity { get; set; }
    public Entity Dependency { get; set; }
    public bool IsDestroy { get; set; }
    public bool IsActive { get; set; }
}

public static class ComponentSystemExtension
{
    /// <summary>
    /// 链接实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="componets"></param>
    public static T AddComponent<T>(this Entity entity) where T : struct, IComponent
    {
        T component = CommponentSystem.Create<T>();
        component.LinkEntity(entity);
        return component;
    }
    /// <summary>
    /// 链接实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="componets"></param>
    public static void LinkEntity<T>(this ref T componet, Entity entity) where T : struct, IComponent
    {
        if (entity.Guid == Guid.Empty || componet.ComEntity.Guid == Guid.Empty) return;
        if (!CommponentSystem.CommponentPool.ContainsKey(entity.Guid)) CommponentSystem.CommponentPool.Add(entity.Guid, EntityObject.CreateInstance());

        var obj = CommponentSystem.CommponentPool[entity.Guid];
        componet.Dependency = entity;
        obj.ComDicts.Add(componet.ComEntity.Guid, componet);
        CommponentSystem.CommponentPool[entity.Guid] = obj;
    }

    ///// <summary>
    ///// 移除组件
    ///// </summary>
    ///// <param name="entity"></param>
    ///// <param name="componets"></param>
    //public static bool RemoveComponents<T>(this Entity entity,out IEnumerable<T> components) where T : struct, IComponent
    //{
    //    components = GetComponents(entity);
    //    if (components == null) return true;
    //    return true;
    //}
    /// <summary>
    /// 根据组件ID获取组件
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="componets"></param>
    public static bool GetComponent<T>(this Entity entity, Guid comid, out T component) where T : struct, IComponent
    {
        component = new T();
        if (entity.Guid == Guid.Empty) return false;
        if (!CommponentSystem.CommponentPool.ContainsKey(entity.Guid) || !CommponentSystem.CommponentPool[entity.Guid].ComDicts.ContainsKey(comid)) return false;

        var obj = CommponentSystem.CommponentPool[entity.Guid].ComDicts[comid];
        if (obj is T) component = (T)obj;
        else return false;
        return true;
    }
    /// <summary>
    /// 根据组件ID获取组件
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="componets"></param>
    public static IEnumerable<T> GetComponents<T>(this Entity entity) where T : struct, IComponent
    {
        if (entity.Guid == Guid.Empty) return null;
        if (!CommponentSystem.CommponentPool.ContainsKey(entity.Guid)) return null;
        return CommponentSystem.CommponentPool[entity.Guid].ComDicts.Values.OfType<T>();
    }
    /// <summary>
    /// 提交修改
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="componet"></param>
    /// <param name="callback"></param>
    public static bool Update<T>(this ref T componet, Action<T> callback = null) where T : struct, IComponent
    {
        if (componet.ComEntity.Guid == Guid.Empty) return false;
        if (!CommponentSystem.CommponentPool.ContainsKey(componet.Dependency.Guid))
        {
            Log.Error($"实体已销毁！");
            return false;
        }
        var entityObj = CommponentSystem.CommponentPool[componet.Dependency.Guid].ComDicts;
        if (!entityObj.ContainsKey(componet.ComEntity.Guid))
        {
            Log.Warning($"修改游离状态的组件!");
            return false;
        }
        try
        {
            callback?.Invoke(componet);
        }
        catch (Exception e)
        {
            Log.Error($"Callback ERROR:{e}");
            return false;
        }
        entityObj[componet.ComEntity.Guid] = componet;
        return true;
    }
    /// <summary>
    /// 清理所有组件
    /// </summary>
    /// <param name="commponet"></param>
    /// <param name="entity"></param>
    internal static void ClearPool()
    {
        CommponentSystem.CommponentPool.Clear();
        Entity.ClearPool();
    }
}
internal class EntityObject
{
    public static EntityObject CreateInstance()
    {
        var obj = new EntityObject();
        obj.ComDicts = new Dictionary<Guid, IComponent>();
        return obj;
    }
    public Dictionary<Guid, IComponent> ComDicts { get; internal set; }
}
public class CommponentSystem : CoreSubSystem<CommponentSystem>
{
    public const string CommponentName = "Commponent";
    private static Dictionary<Guid, EntityObject> _CommponentPool = new Dictionary<Guid, EntityObject>();
    internal static Dictionary<Guid, EntityObject> CommponentPool => _CommponentPool;
    public static T Create<T>() where T : IComponent, new()
    {
        T component = new T();
        component.ComEntity = new Entity(CommponentName);
        return component;
    }

    public override void AwakeSystem()
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnAwake();
            }
        }
    }

    public override void StartSystem()
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnStart();
            }
        }
    }

    public override void UpdtaeSystem(float delta)
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnUpdtae();
            }
        }
    }

    public override void LateUpdtaeSystem(float delta)
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnLateUpdtae();
            }
        }
    }

    public override void FixedUpdtaeSystem(float delta)
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnFixedUpdtae();
            }
        }
    }

    public override void DestroySystem()
    {
        foreach (var guid in _CommponentPool.Keys)
        {
            var dict = _CommponentPool[guid].ComDicts;
            if (dict == null) continue;
            foreach (var id in dict.Keys)
            {
                dict[id].OnDestroy();
            }
        }
    }

    public override void Dispose()
    {
        ComponentSystemExtension.ClearPool();
    }

    public override void OnRegisterParent(ISystem system)
    {
        ComponentSystemExtension.ClearPool();
    }
}
