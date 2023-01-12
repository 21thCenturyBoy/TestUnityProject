namespace TestCoreLib
{
    using System;
    using System.Numerics;
    public interface IComponent
    {
        Entity ComEntity { get; set; }
        Entity Dependency { get; set; }
    }

    public struct Component : IComponent
    {
        public string Name;
        public Entity ComEntity { get; set; }
        public Entity Dependency { get; set; }
    }
}