using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestRecycleListView
{
    public class ItemData
    {
        public string template;
        public MonoBehaviour item;
    }
    [System.Serializable]
    public class ItemInspectorData : ItemData { }
    [RequireComponent(typeof(Collider))]
    public class ItemBase : MonoBehaviour { }
    public class RecycleListItem<DataType> : ItemBase where DataType : ItemData
    {
        public DataType data;
        public virtual void Setup(DataType data)
        {
            this.data = data;
            data.item = this;
        }
    }
    public class ListViewItemTemplate
    {
        public readonly GameObject prefab;
        public readonly List<MonoBehaviour> pool = new List<MonoBehaviour>();

        public ListViewItemTemplate(GameObject prefab)
        {
            if (prefab == null)
                Debug.LogError("Template prefab cannot be null");
            this.prefab = prefab;
        }
    }
    public class RecycleListItem : RecycleListItem<ItemInspectorData>
    {
    }
}
