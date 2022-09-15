
using System;
using System.Numerics;

public struct Component : IComponent
{
    public Entity ComEntity { get; set; }
    public Entity Dependency { get; set; }
    public bool IsDestroy { get; set; }
    public bool IsActive { get; set; }
    public void OnAwake() { }
    public void OnStart() { }

    public void OnUpdtae() { }

    public void OnLateUpdtae() { }

    public void OnFixedUpdtae() { }

    public void OnDestroy() { }
}

public struct MoveComponent : IComponent
{
    public string Name;
    public Vector3 Postion;
    public Entity ComEntity { get; set; }
    public Entity Dependency { get; set; }
    public bool IsDestroy { get; set; }
    public bool IsActive { get; set; }

    public void OnAwake()
    {
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnAwake!");
    }

    public void OnStart()
    {
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnStart!");
    }

    public void OnUpdtae()
    {
        Postion = Postion + Vector3.One;
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnUpdtae!");
    }

    public void OnLateUpdtae()
    {
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnLateUpdtae!");
    }

    public void OnFixedUpdtae()
    {
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnFixedUpdtae!");
    }

    public void OnDestroy()
    {
        //Log.Info(nameof(MoveComponent) + $"{Name} {Postion} OnDestroy!");
    }
}