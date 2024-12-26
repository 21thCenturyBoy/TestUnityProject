using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    [ExecuteAlways]
    public class BlockLayout_Input : ScratchUIBehaviour
    {
        [SerializeField] private Button m_inputButton;

        public Button InputButton
        {
            get
            {
                if (m_inputButton == null)
                {
                    m_inputButton = GetComponent<Button>();
                }

                return m_inputButton;
            }
        }

        [SerializeField] private RectTransform m_WarnIconRect;

        public RectTransform WarnIconRect
        {
            get
            {
                if (m_WarnIconRect == null)
                {
                    m_WarnIconRect = GetComponent<RectTransform>();
                }

                return m_WarnIconRect;
            }
        }

        [SerializeField] private TMP_Text m_InputText;

        public TMP_Text InputText
        {
            get
            {
                if (m_InputText == null)
                {
                    m_InputText = GetComponent<TMP_Text>();
                }

                return m_InputText;
            }
        }
    }
}