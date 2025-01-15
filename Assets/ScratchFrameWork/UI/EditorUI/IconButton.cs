using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class IconButton : ScratchBehaviour
    {
        private string m_Name = String.Empty;

        private Button m_Btn;
        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_Btn = GetComponent<Button>();
                if (m_Btn == null)
                {
                    m_Btn = gameObject.AddComponent<Button>();
                }

                if (string.IsNullOrEmpty(m_Name))
                {
                    m_Name = gameObject.name;
                }

                EditorUIStyle.GetIconStyle(m_Name, (sprite) =>
                {
                    if (sprite != null)
                    {
                        m_Btn.image.sprite = sprite;
                    }
                });
            }

            return base.Initialize();
        }
        
        public void AddListener(Action action)
        {
            m_Btn.onClick.AddListener(() =>
            {
                action?.Invoke();
            });
        }
        
        public void RemoveListener(Action action)
        {
            m_Btn.onClick.RemoveListener(() =>
            {
                action?.Invoke();
            });
        }
        
        public void RemoveAllListener()
        {
            m_Btn.onClick.RemoveAllListeners();
        }
    }
}

