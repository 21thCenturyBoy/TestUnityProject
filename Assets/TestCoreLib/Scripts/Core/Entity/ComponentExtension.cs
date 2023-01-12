namespace TestCoreLib
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public static class ComponentExtension
    {
        public const string CommponentName = "Commponent";
        private static Dictionary<Guid, EntityObject> _CommponentPool = new Dictionary<Guid, EntityObject>();
        internal static Dictionary<Guid, EntityObject> CommponentPool => _CommponentPool;

        static ComponentExtension()
        {
        }


        public static T Create<T>() where T : IComponent, new()
        {
            T component = new T
            {
                ComEntity = new Entity(CommponentName)
            };
            return component;
        }
        /// <summary>
        /// 链接实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static T AddComponent<T>(this Entity entity) where T : struct, IComponent
        {
            T component = Create<T>();
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
            if (!CommponentPool.ContainsKey(entity.Guid)) CommponentPool.Add(entity.Guid, EntityObject.CreateInstance());

            var obj = CommponentPool[entity.Guid];
            componet.Dependency = entity;
            obj.ComDicts.Add(componet.ComEntity.Guid, componet);
            CommponentPool[entity.Guid] = obj;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static void RemoveComponents<T>(this Entity entity, out IEnumerable<T> components) where T : struct, IComponent
        {
            components = GetComponents<T>(entity);
            if (components == null) return;
            foreach (T component in components)
            {
                CommponentPool[entity.Guid].ComDicts.Remove(component.ComEntity.Guid);
            }
        }
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static int RemoveComponents<T>(this Entity entity) where T : struct, IComponent
        {
            var guids = GetComponents<T>(entity).Select(com => com.ComEntity.Guid).ToArray();
            foreach (Guid id in guids)
            {
                CommponentPool[entity.Guid].ComDicts.Remove(id);
            }
            return guids.Length;
        }
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static bool RemoveComponent(this Entity entity, Guid comid)
        {
            if (entity.Guid == Guid.Empty) return false;
            if (!CommponentPool.ContainsKey(entity.Guid) || !CommponentPool[entity.Guid].ComDicts.ContainsKey(comid)) return false;
            CommponentPool[entity.Guid].ComDicts.Remove(comid);
            return true;
        }
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static bool RemoveComponent<T>(this Entity entity, T component) where T : struct, IComponent
        {
            return entity.RemoveComponent(component.ComEntity.Guid);
        }
        /// <summary>
        /// 根据组件ID获取组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static bool GetComponent<T>(this Entity entity, Guid comid, out T component) where T : struct, IComponent
        {
            component = new T();
            if (entity.Guid == Guid.Empty) return false;
            if (!CommponentPool.ContainsKey(entity.Guid) || !CommponentPool[entity.Guid].ComDicts.ContainsKey(comid)) return false;

            var obj = CommponentPool[entity.Guid].ComDicts[comid];
            if (obj is T com) component = com;
            else return false;
            return true;
        }
        /// <summary>
        /// 根据组件ID获取组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componets"></param>
        public static T[] GetComponents<T>(this Entity entity) where T : struct, IComponent
        {
            if (entity.Guid == Guid.Empty) return null;
            if (!CommponentPool.ContainsKey(entity.Guid)) return null;
            return CommponentPool[entity.Guid].ComDicts.Values.OfType<T>().ToArray();
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
            EntityObject obj = null;
            if (!CommponentPool.TryGetValue(componet.Dependency.Guid, out obj))
            {
                Log.Error($"实体已销毁！");
                return false;
            }
            var comDicts = obj.ComDicts;
            if (!comDicts.ContainsKey(componet.ComEntity.Guid))
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
            comDicts[componet.ComEntity.Guid] = componet;
            return true;
        }
        /// <summary>
        /// 清理所有组件
        /// </summary>
        /// <param name="commponet"></param>
        /// <param name="entity"></param>
        internal static void ClearPool()
        {
            CommponentPool.Clear();
            Entity.ClearPool();
        }
    }
}