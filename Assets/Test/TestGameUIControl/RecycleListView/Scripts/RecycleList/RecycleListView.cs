using System.Collections.Generic;
using UnityEngine;

namespace GameUIControl.RecycleListView
{
    public interface IViewController
    {
        void Initialize();
        void ViewUpdate();
        bool ComputeConditions();
        void UpdateItems();
    }

    public abstract class ListViewControllerBase : MonoBehaviour, IViewController
    {
        //滚动便宜
        public float ScrollOffset;
        //间距
        public float Padding = 0.01f;
        //可视范围
        public float Range = 1;

        public GameObject[] Templates;

        protected int m_DataOffset;
        protected int m_NumItems;
        protected Vector3 m_LeftSide;
        protected Vector3 m_ItemSize;

        protected readonly Dictionary<string, ItemTemplatePool> m_Templates = new Dictionary<string, ItemTemplatePool>();

        public Vector3 ItemSize
        {
            get { return m_ItemSize; }
        }

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            ViewUpdate();
        }

        public virtual void Initialize()
        {
            if (Templates.Length < 1)
            {
                Debug.LogError("No templates!");
            }
            foreach (var template in Templates)
            {
                if (m_Templates.ContainsKey(template.name))
                    Debug.LogError("Two templates cannot have the same name");
                m_Templates[template.name] = new ItemTemplatePool(template);
            }
        }

        public virtual void ViewUpdate()
        {
            ComputeConditions();
            UpdateItems();
        }

        public virtual bool ComputeConditions()
        {
            if (Templates.Length > 0)
            {
                m_ItemSize = GetObjectSize(Templates[0]);
            }

            m_NumItems = Mathf.RoundToInt(Range / m_ItemSize.x); 
            Range = m_NumItems * m_ItemSize.x;

            m_LeftSide = transform.position + Vector3.left * Range * 0.5f;

            m_DataOffset = (int)(ScrollOffset / ItemSize.x) + 1;
            if (ScrollOffset <= 0)
            {
                m_DataOffset--;
            }
 
            return true;
        }

        public abstract void UpdateItems();

        public virtual void ScrollNext()
        {
            ScrollOffset += m_ItemSize.x;
        }

        public virtual void ScrollPrev()
        {
            ScrollOffset -= m_ItemSize.x;
        }

        public virtual void ScrollTo(int index)
        {
            ScrollOffset = index * ItemSize.x;
        }

        protected virtual void Positioning(Transform t, int offset)
        {
            t.position = m_LeftSide + (offset * m_ItemSize.x + ScrollOffset) * Vector3.right;
        }

        protected virtual Vector3 GetObjectSize(GameObject g)
        {
            Vector3 itemSize = Vector3.one;
            //TODO: Better method for finding object size
            Renderer rend = g.GetComponentInChildren<Renderer>();
            if (rend)
            {
                itemSize.x = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).x * 2 + Padding;
                itemSize.y = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).y * 2 + Padding;
                itemSize.z = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).z * 2 + Padding;
            }
            return itemSize;
        }

        public virtual void RecycleItem(string template, MonoBehaviour item)
        {
            if (item == null || template == null)
                return;
            m_Templates[template].Pool.Add(item);
            item.gameObject.SetActive(false);
        }
    }
    public class RecycleListView<DataType, ItemType> : ListViewControllerBase where DataType : ItemInspectorData where ItemType : RecycleListItem<DataType>
    {
        [Tooltip("Source Data")]
        public DataType[] Data;

        public override void UpdateItems()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                if (i + m_DataOffset < 0)
                {
                    ExtremeLeft(Data[i]);
                }
                else if (i + m_DataOffset > m_NumItems)
                {
                    ExtremeRight(Data[i]);
                }
                else
                {
                    ListMiddle(Data[i], i);
                }
            }
        }

        protected virtual void ExtremeLeft(DataType data)
        {
            RecycleItem(data.TemplateName, data.Item);
            data.Item = null;
        }

        protected virtual void ExtremeRight(DataType data)
        {
            RecycleItem(data.TemplateName, data.Item);
            data.Item = null;
        }

        protected virtual void ListMiddle(DataType data, int offset)
        {
            if (data.Item == null)
            {
                data.Item = GetItem(data);
            }
            Positioning(data.Item.transform, offset);
        }

        protected virtual ItemType GetItem(DataType data)
        {
            if (data == null)
            {
                Debug.LogWarning("Tried to get item with null data");
                return null;
            }
            if (!m_Templates.ContainsKey(data.TemplateName))
            {
                Debug.LogWarning("Cannot get item, template " + data.TemplateName + " doesn't exist");
                return null;
            }
            ItemType item = null;
            if (m_Templates[data.TemplateName].Pool.Count > 0)
            {
                item = (ItemType)m_Templates[data.TemplateName].Pool[0];
                m_Templates[data.TemplateName].Pool.RemoveAt(0);

                item.gameObject.SetActive(true);
                item.Initialize(data);
            }
            else
            {
                item = Instantiate(m_Templates[data.TemplateName].Prefab).GetComponent<ItemType>();
                item.transform.parent = transform;
                item.Initialize(data);
            }
            return item;
        }
    }

    public class RecycleListView : RecycleListView<ItemInspectorData, RecycleListItem3D>
    {
    }
}

