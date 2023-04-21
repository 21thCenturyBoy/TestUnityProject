using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestRecycleListView.UI
{
    public abstract class RecycleListScrollViewUI : MonoBehaviour
    {
        public float ScrollOffset;
    }

    public abstract class RecycleListScrollViewUI<TDataType, TDataSet, TItemType> : RecycleListScrollViewUI, IViewController where TItemType : RecycleListScrollViewItem<TDataType> where TDataType : ItemData where TDataSet : IEnumerable<TDataType>
    {
        //间距
        public float Padding = 0.01f;
        //可视范围
        public float Range = 1;

        public GameObject Templates;

        protected int m_DataOffset;
        protected int m_NumItems;

        protected Vector3 m_LeftSide;
        protected Vector3 m_ItemSize;

        protected float m_MinScrollOffset = 0;
        protected float m_MaxScrollOffset = 0;

        protected ItemTemplatePool m_Templates;

        public Vector3 ItemSize
        {
            get { return m_ItemSize; }
        }

        [Tooltip("Source Data")]
        public TDataSet Data;


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
            m_Templates = new ItemTemplatePool(Templates);

            m_ItemSize = GetObjectSize(Templates);
        }

        public virtual void ViewUpdate()
        {
            if (!Data.Any()) return;
            ComputeConditions();
            UpdateItems();
        }
        public abstract void AddData(TDataType data);
        public abstract void RemoveData(TDataType data);
        public float GetTotalSzie()
        {
            return Data.Count() * ItemSize.x;
        }
        /// <summary>
        /// 限制滚动
        /// </summary>
        /// <returns></returns>
        private float ClampScrollOffset()
        {
            m_MinScrollOffset = m_MaxScrollOffset - GetTotalSzie() + Range;
            if (ScrollOffset < m_MinScrollOffset)
            {
                m_DataOffset = -(Data.Count() - m_NumItems);
                return m_MinScrollOffset;
            }
            else if (ScrollOffset > m_MaxScrollOffset)
            {
                m_DataOffset = 0;
                return m_MaxScrollOffset;
            }
            else
            {
                m_DataOffset--;
                return ScrollOffset;
            }
        }

        public virtual bool ComputeConditions()
        {
            m_ItemSize = GetObjectSize(Templates);

            m_NumItems = Mathf.FloorToInt(Range / m_ItemSize.x);
            Range = m_NumItems * m_ItemSize.x;

            m_LeftSide = transform.position + Vector3.left * Range * 0.5f;

            m_DataOffset = (int)(ScrollOffset / ItemSize.x) + 1;

            ScrollOffset = ClampScrollOffset();

            return true;
        }



        public virtual void UpdateItems()
        {
            int i = 0;

            foreach (TDataType data in Data)
            {
                if (i + m_DataOffset < 0)
                {
                    ExtremeLeft(data);
                }
                else if (i + m_DataOffset > m_NumItems)
                {
                    ExtremeRight(data);
                }
                else
                {
                    ListMiddle(data, i);
                }
                i++;
            }
        }

        protected virtual void ExtremeLeft(TDataType data)
        {
            RecycleItem(data.Item);
            data.Item = null;
        }

        protected virtual void ExtremeRight(TDataType data)
        {
            RecycleItem(data.Item);
            data.Item = null;
        }

        protected virtual void ListMiddle(TDataType data, int offset)
        {
            if (data.Item == null)
            {
                data.Item = GetItem(data);
            }
            Positioning(data.Item.transform, offset);
        }

        protected virtual TItemType GetItem(TDataType data)
        {
            if (data == null)
            {
                Debug.LogWarning("Tried to get item with null data");
                return null;
            }

            TItemType item = null;
            if (m_Templates.Pool.Count > 0)
            {
                item = (TItemType)m_Templates.Pool[0];
                m_Templates.Pool.RemoveAt(0);

                item.gameObject.SetActive(true);
                item.ShowData(data);
            }
            else
            {
                item = Instantiate(m_Templates.Prefab).GetComponent<TItemType>();
                item.transform.parent = transform;
                item.ShowData(data);
            }
            return item;
        }

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

        protected virtual Vector2 GetObjectSize(GameObject g)
        {
            Vector3 itemSize = g.GetComponent<RectTransform>().sizeDelta;
            return itemSize;
        }

        public virtual void RecycleItem(MonoBehaviour item)
        {
            if (item == null) return;
            m_Templates.Pool.Add(item);
            item.gameObject.SetActive(false);
        }
    }
}

