using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class BlockSpot_Input : BlockSpot
    {
        #region property

        public readonly Color DefaultColor = Color.green;
        public readonly Vector2 DefaultEffectDistance= new Vector2(5,3);
        private Outline m_Outline;

        public Outline Outline
        {
            get
            {
                if (m_Outline == null)
                {
                    m_Outline = TryAddComponent<Outline>();
                    m_Outline.effectColor = DefaultColor;
                    m_Outline.effectDistance = DefaultEffectDistance;
                }

                return m_Outline;
            }
        }

        #endregion

        protected override void OnVisible()
        {
            base.OnVisible();

            ShowOutline(false);
        }

        protected override void OnInVisible()
        {
            base.OnInVisible();
            
            ShowOutline(false);
        }
        
        public void ShowOutline(bool active)
        {
            if (IsDestroying) return;

            Outline.enabled = active;
        }
    }
}