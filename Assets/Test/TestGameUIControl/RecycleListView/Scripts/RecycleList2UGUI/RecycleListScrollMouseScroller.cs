using UnityEngine;

namespace GameUIControl.RecycleListView.UI
{

    public class RecycleListScrollMouseScroller<TView> : MonoBehaviour where TView : RecycleListScrollViewUI
    {
        public TView View;

        protected bool m_Scrolling;
        protected Vector3 m_StartPosition;
        protected float m_StartOffset;
        void Update()
        {
            HandleInput();
        }
        protected virtual void StartScrolling(Vector3 start)
        {
            if (m_Scrolling) return;
            m_Scrolling = true;
            m_StartPosition = start;
            m_StartOffset = View.ScrollOffset;

        }

        protected virtual void Scroll(Vector3 position)
        {
            if (m_Scrolling)
            {
                Vector3 delta = m_StartPosition - position;
                View.ScrollOffset = m_StartOffset + Vector3.Dot(delta, View.NegativeOrientation);
            }

        }

        protected virtual void StopScrolling()
        {
            m_Scrolling = false;
        }

        protected float m_ListDepth;

        protected virtual void HandleInput()
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