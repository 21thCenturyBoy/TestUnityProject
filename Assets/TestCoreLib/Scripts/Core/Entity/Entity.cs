using System;
using System.Collections.Generic;

public struct Entity
{
    public const string EntityName = "Entity";
    private Guid m_uuid;
    public Guid Guid => m_uuid;
    public string Id => m_uuid.ToString();
    public string Name { get; set; }

    private static Dictionary<Guid, Entity> m_pool = new Dictionary<Guid, Entity>();

    public Entity(string name = EntityName) : this()
    {
        Name = name;
        GenerateID(ref this);
    }
    private static Entity GenerateID(ref Entity entity)
    {
        Guid uuid = Guid.NewGuid();
        entity.m_uuid = uuid;
        if (m_pool.ContainsKey(uuid)) return GenerateID(ref entity);
        else m_pool.Add(uuid, entity);
        return entity;
    }
    internal static void ClearPool()
    {
        m_pool.Clear();
    }
}
