using System.Collections.Generic;
using UnityEngine;

namespace GameUIControl.RecycleListView
{
    public interface IData { }
    public class ItemData : IData
    {
        public MonoBehaviour Item;
    }

    [System.Serializable]
    public class ItemInspectorData : ItemData
    {
        public string TemplateName;
    }
    public class ItemBase : MonoBehaviour { }
    public class RecycleListItem<DataType> : ItemBase where DataType : ItemInspectorData
    {
        public DataType data;
        public virtual void Initialize(DataType data)
        {
            this.data = data;
            data.Item = this;
        }
    }
    public class ItemTemplatePool
    {
        public readonly GameObject Prefab;
        public readonly List<MonoBehaviour> Pool = new List<MonoBehaviour>();

        public ItemTemplatePool(GameObject prefab)
        {
            if (prefab == null) Debug.LogError("Template prefab cannot be null");
            this.Prefab = prefab;
        }
    }
    [RequireComponent(typeof(Collider))]
    public class RecycleListItem3D : RecycleListItem<ItemInspectorData>
    {
    }
}
