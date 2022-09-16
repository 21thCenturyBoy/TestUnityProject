using System;
using System.Collections.Generic;

public interface IAwake : IBehavior
{
    void OnAwake();
}
public interface IStart : IBehavior
{
    void OnStart();
}
public interface IUpdtae : IBehavior
{
    void OnUpdtae();
}
public interface ILateUpdta : IBehavior
{
    void OnLateUpdtae();
}
public interface IFixedUpdtae : IBehavior
{
    void OnFixedUpdtae();
}
public interface IDestroy : IBehavior
{
    void OnDestroy();
}

public interface IBehavior : IComponent
{
    bool IsAwake { get; set; }
    bool IsActive { get; set; }
    bool IsDestroy { get; set; }
}
public struct BehaviorComponent : IBehavior
{
    public string Name;
    public Entity ComEntity { get; set; }
    public Entity Dependency { get; set; }
    public bool IsDestroy { get; set; }
    public bool IsAwake { get; set; }
    public bool IsActive { get; set; }

}
public static class BehaviorSystemExtension
{
    public static void BehaviorAwake<T>(this ref T behavior) where T : struct, IAwake
    {
        behavior.OnAwake();
    }
    public static void BehaviorStart<T>(this ref T behavior) where T : struct, IStart
    {
        behavior.OnStart();
    }
    public static void BehaviorUpdate<T>(this ref T behavior) where T : struct, IUpdtae
    {
        behavior.Update();
    }
    public static void BehaviorLateUpdta<T>(this ref T behavior) where T : struct, ILateUpdta
    {
        behavior.OnLateUpdtae();
    }
    public static void BehaviorFixedUpdtae<T>(this ref T behavior) where T : struct, IFixedUpdtae
    {
        behavior.OnFixedUpdtae();
    }
    public static void BehaviorDestroy<T>(this ref T behavior) where T : struct, IDestroy
    {
        behavior.OnDestroy();
    }
}
public class BehaviorSystem : CoreSubSystem<BehaviorSystem>
{
    private LinkedList<Guid> m_AwakeList = new LinkedList<Guid>();
    private LinkedList<Guid> m_StartList = new LinkedList<Guid>();
    private LinkedList<Guid> m_UpdateList = new LinkedList<Guid>();
    private LinkedList<Guid> m_LateUpdtaList = new LinkedList<Guid>();
    private LinkedList<Guid> m_FixedList = new LinkedList<Guid>();
    private LinkedList<Guid> m_DestroyList = new LinkedList<Guid>();
    public T AddBehavior<T>() where T : IBehavior, new()
    {
        T behavior = ComponentExtension.Create<T>();
        if (typeof(T).IsSubclassOf(typeof(IAwake))) m_AwakeList.AddLast(behavior.ComEntity.Guid);
        if (typeof(T).IsSubclassOf(typeof(IStart))) m_StartList.AddLast(behavior.ComEntity.Guid);
        if (typeof(T).IsSubclassOf(typeof(IUpdtae))) m_UpdateList.AddLast(behavior.ComEntity.Guid);
        if (typeof(T).IsSubclassOf(typeof(ILateUpdta))) m_LateUpdtaList.AddLast(behavior.ComEntity.Guid);
        if (typeof(T).IsSubclassOf(typeof(IFixedUpdtae))) m_FixedList.AddLast(behavior.ComEntity.Guid);
        if (typeof(T).IsSubclassOf(typeof(IDestroy))) m_DestroyList.AddLast(behavior.ComEntity.Guid);
        return behavior;
    }
    public override void AwakeSystem() { }

    public override void StartSystem() { }

    public override void UpdtaeSystem(float delta)
    {
    }
    public override void LateUpdtaeSystem(float delta) { }

    public override void FixedUpdtaeSystem(float delta) { }

    public override void DestroySystem() { }

    public override void Dispose()
    {
        ComponentExtension.ClearPool();
    }

    public override void OnRegisterParent(ISystem system)
    {
        ComponentExtension.ClearPool();
    }
}
