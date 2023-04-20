using UnityEngine;

namespace TestRecycleListView
{
    public abstract class RecycleListInputHandler : MonoBehaviour
    {
        public ListViewControllerBase listView;

        void Update()
        {
            HandleInput();
        }

        protected abstract void HandleInput();
    }
    /// <summary>
    /// 滚动视图输入
    /// </summary>
    public abstract class RecycleListScroller : RecycleListInputHandler
    {
        protected bool m_Scrolling;
        protected Vector3 m_StartPosition;
        protected float m_StartOffset;

        protected virtual void StartScrolling(Vector3 start)
        {
            if (m_Scrolling)
                return;
            m_Scrolling = true;
            m_StartPosition = start;
            m_StartOffset = listView.scrollOffset;
        }

        protected virtual void Scroll(Vector3 position)
        {
            if (m_Scrolling)
                listView.scrollOffset = m_StartOffset + position.x - m_StartPosition.x;
        }

        protected virtual void StopScrolling()
        {
            m_Scrolling = false;
        }
    }
    public class RecycleListMouseScroller : RecycleListScroller
    {
        float m_ListDepth;

        protected override void HandleInput()
        {
            Vector3 screenPoint = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    ItemBase item = hit.collider.GetComponent<ItemBase>();
                    if (item)
                    {
                        m_ListDepth = (hit.point - Camera.main.transform.position).magnitude;
                        screenPoint.z = m_ListDepth;
                        StartScrolling(Camera.main.ScreenToWorldPoint(screenPoint));
                    }
                }
            }
            else
            {
                StopScrolling();
            }
            screenPoint.z = m_ListDepth;
            Scroll(Camera.main.ScreenToWorldPoint(screenPoint));
        }
    }
}
