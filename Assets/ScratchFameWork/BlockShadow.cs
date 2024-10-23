using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class BlockShadow : ScratchUIBehaviour
    {
        private Shadow m_shadow;

        public Color DefaultColor = Color.green;

        public Shadow Shadow
        {
            get
            {
                if (m_shadow == null)
                {
                    m_shadow = TryAddComponent<Shadow>();

                    m_shadow.effectColor = DefaultColor;
                    m_shadow.effectDistance = new Vector2(-6, -6);
                }

                return m_shadow;
            }
        }

        protected override void OnVisible()
        {
            base.OnVisible();

            if (IsDestroying) return;

            Shadow.enabled = true;
        }

        protected override void OnInVisible()
        {
            base.OnInVisible();

            if (IsDestroying) return;

            Shadow.enabled = false;
        }

        public void SetColor(Color color)
        {
            if (IsDestroying) return;

            Shadow.effectColor = color;
        }
    }
}