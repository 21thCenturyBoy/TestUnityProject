using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestRecycleListView.UI
{
    public abstract class RecycleListScrollViewUI : MonoBehaviour
    {
        public float ScrollOffset;

        public Vector3 Orientation = Vector3.right;

        public Vector3 NormalOrientation
        {
            get => (Vector3.Normalize(Orientation));
        }
        public Vector3 NegativeOrientation
        {
            get => (-NormalOrientation);
        }

        public virtual void ScrollNext() { }

        public virtual void ScrollPrev() { }

        public virtual void ScrollTo(int index) { }
        protected abstract void Positioning(Transform t, int offset);

    }

    public abstract class RecycleListScrollViewUI<TDataType, TDataSet, TItemType> : RecycleListScrollViewUI, IViewController where TItemType : RecycleListScrollViewItem<TDataType> where TDataType : ItemData where TDataSet : IEnumerable<TDataType>
    {
        //间距
        public Vector3 Padding = Vector3.zero;
        //最佳
        public bool OptimumRange = true;
        public float Range = 1;

        public GameObject Templates;

        protected int m_DataOffset;
        protected int m_NumItems;

        protected Vector3 m_LeftSide;
        protected Vector3 m_ItemSize;

        protected float m_MinScrollOffset = 0;
        protected float m_MaxScrollOffset = 0;

        protected ItemTemplatePool m_Templates;

        public float ItemLength
        {
            get => Mathf.Abs(Vector3.Dot(m_ItemSize, NormalOrientation));
        }

        //[HideInInspector]
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
            return Data.Count() * ItemLength;
        }
        /// <summary>
        /// 限制滚动
        /// </summary>
        /// <returns></returns>
        private float ClampScrollOffset()
        {
            if (Data.Count() <= m_NumItems)
            {
                m_DataOffset = 0;
                return m_MaxScrollOffset;
            }

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
            m_NumItems = Mathf.FloorToInt(Range / ItemLength);
            if(OptimumRange) Range = m_NumItems * ItemLength;

            m_LeftSide = transform.position + NegativeOrientation * Range * 0.5f;

            m_DataOffset = (int)(ScrollOffset / ItemLength) + 1;

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

        protected virtual TItemType GetItem(TDataType data, Transform parent = null)
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
                if (parent == null)
                {
                    parent = transform;
                }

                item = Instantiate(m_Templates.Prefab, parent).GetComponent<TItemType>();

                item.ShowData(data);
            }
            return item;
        }

        public override void ScrollNext()
        {
            ScrollOffset += ItemLength;
        }

        public override void ScrollPrev()
        {
            ScrollOffset -= ItemLength;
        }

        public override void ScrollTo(int index)
        {
            ScrollOffset = index * ItemLength;
        }

        protected override void Positioning(Transform t, int offset)
        {
            t.position = m_LeftSide + (offset * ItemLength + ScrollOffset) * Orientation;
        }

        protected virtual Vector2 GetObjectSize(GameObject g)
        {
            Vector3 itemSize = Padding;
            itemSize.x += g.GetComponent<RectTransform>().sizeDelta.x;
            itemSize.y += g.GetComponent<RectTransform>().sizeDelta.y;
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

