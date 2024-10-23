using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{

    [ExecuteAlways]
    public class BlockLayout_Input : ScratchUIBehaviour,IScratchModifyLayout
    {
        private TMP_InputField m_inputField;

        public TMP_InputField InputField
        {
            get
            {
                if (m_inputField == null)
                {
                    m_inputField = GetComponent<TMP_InputField>();
                }

                return m_inputField;
            }
        }
        public float minWidth = 70;
        public float widthOffset = 35;
        
        public float maxWidth = 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            InputField.onValueChanged.AddListener(Resize);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            m_inputField.onValueChanged.RemoveAllListeners();
        }
        
        public void Resize(string value)
        {
            float width = widthOffset + InputField.textComponent.GetPreferredValues(value).x;
            if (width < minWidth)
                width = minWidth;

            if (maxWidth > 0 && width > maxWidth)
                width = maxWidth;

            SetSize(new Vector2(width, GetSize().y));

            InputField.textComponent.transform.localPosition = Vector3.zero;
        }

        public void UpdateLayout()
        {
            Resize(InputField.text);
        }

        public Vector2 SetSize(Vector2 size)
        {
            if (RectTrans == null) return Vector2.zero;
            RectTrans.sizeDelta = size;
            return size;
        }
    }
}

