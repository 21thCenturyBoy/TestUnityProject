using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [ExecuteInEditMode]
    public class BlockImage : ScratchUIBehaviour
    {
        private Image m_Image;

        public Image Image
        {
            get
            {
                if (m_Image == null)
                {
                    m_Image = TryAddComponent<Image>();
                    m_Image.type = Image.Type.Sliced;
                    m_Image.pixelsPerUnitMultiplier = 2;
                }

                return m_Image;
            }
        }

        private bool m_lockedInVisible;

        public void LockedInVisible()
        {
            m_lockedInVisible = true;
            Visible = false;
        }

        protected override void OnEnable()
        {
            if (m_lockedInVisible) return;
            Visible = true;
        }

        protected override void OnDisable()
        {
            if (m_lockedInVisible) return;
            Visible = false;
        }

        protected override void OnVisible()
        {
            base.OnVisible();

            if (IsDestroying) return;
            Image.enabled = true;
        }

        protected override void OnInVisible()
        {
            base.OnInVisible();

            if (IsDestroying) return;
            Image.enabled = false;
        }

        public void SetColor(Color color)
        {
            if (IsDestroying) return;

            Image.color = color;
        }
    }
}