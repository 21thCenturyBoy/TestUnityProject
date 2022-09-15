using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class CoreSystem : Singleton_Class<CoreSystem>, ISystem
{
    private Dictionary<Type, ISubSystem> m_subSystems;
    public Dictionary<Type, ISubSystem> LoadSubSystem()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(CoreSystem));
        Type subtype = typeof(ISubSystem);
        Type subabtype = typeof(CoreSubSystem<>);

        var types = assembly.GetExportedTypes().Where(t => subtype.IsAssignableFrom(t) && t != subtype && t != subabtype);
        Dictionary<Type, ISubSystem> dic = new Dictionary<Type, ISubSystem>();
        foreach (Type type in types)
        {
            var subSys = (ISubSystem)assembly.CreateInstance(type.FullName);
            dic.Add(type, subSys);
            if (subSys != null) subSys.Parent = this;
        }
        return dic;
    }
    public CoreSystem()
    {

        Log.Info($"{nameof(CoreSystem)} is Generated!");
        m_subSystems = LoadSubSystem();

        foreach (Type key in m_subSystems.Keys)
        {
            Log.Info($"CoreSubSystem create:{key.Name}");
        }
    }
    public void AwakeSystem()
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.AwakeSystem();
        }
    }

    public void StartSystem()
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.StartSystem();
        }
    }

    public void UpdtaeSystem(float delta)
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.UpdtaeSystem(delta);
        }
    }

    public void LateUpdtaeSystem(float delta)
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.LateUpdtaeSystem(delta);
        }
    }

    public void FixedUpdtaeSystem(float delta)
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.FixedUpdtaeSystem(delta);
        }
    }

    public void DestroySystem()
    {
        if (m_subSystems == null) return;
        foreach (ISubSystem subSystem in m_subSystems.Values)
        {
            subSystem.DestroySystem();
        }
    }
}
