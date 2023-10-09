using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace GameUIControl
{
    /// <summary>
    /// 倒序排
    /// </summary>
    [Serializable]
    public struct HpConfig : IComparable<HpConfig>
    {
        public HpState State;
        public Color ShowColor;
        public float MinPercentage;

        public enum HpState
        {
            Low,
            Middle,
            High
        }

        public HpConfig(HpState state, Color showColor, float minPer)
        {
            State = state;
            ShowColor = showColor;
            MinPercentage = minPer;
        }
        public int CompareTo(HpConfig other)
        {
            return -MinPercentage.CompareTo(other.MinPercentage);
        }
    }

    [RequireComponent(typeof(Slider))]
    public class HpSlider : MonoBehaviour
    {
        [SerializeField]
        private List<HpConfig> m_Configs = new List<HpConfig>();

        public HpConfig.HpState CurrentState { get; protected set; }

        public event Action<HpConfig.HpState, HpConfig.HpState> OnStateChanged;

        [SerializeField]
        private bool m_UseDefalutConfig = true;

        private Slider m_Slider;
        private Image m_FillRect;

        [SerializeField]
        public TMP_Text m_Text_Val;

        // Start is called before the first frame update
        void Start()
        {
            CheckConfig();
            m_Slider.onValueChanged.AddListener(OnGetSliderVal);
        }

        private void CheckConfig()
        {
            if (m_Configs == null || m_Configs.Count == 0)
            {
                if (m_UseDefalutConfig)
                {
                    m_Configs = new List<HpConfig>();
                    m_Configs.Add(new HpConfig(HpConfig.HpState.Low, new Color(0.9607843f, 0.2705882f, 0.3019608f, 1f), 0f));
                    m_Configs.Add(new HpConfig(HpConfig.HpState.Middle, new Color(0.9411765f, 0.6156863f, 0.04313726f, 1f), 0.2f));
                    m_Configs.Add(new HpConfig(HpConfig.HpState.High, new Color(0.4980392f, 0.7647059f, 0.2235294f, 1f), 0.5f));

                    m_Configs.Sort();
                }
            }

            if (m_Slider == null) m_Slider = transform.GetComponent<Slider>();
            if (m_FillRect == null) m_FillRect = m_Slider.fillRect.GetComponent<Image>();
        }

        public Slider GetSlider() => m_Slider;

        public void ClearConfig()
        {
            m_Configs.Clear();
        }

        public void OnGetSliderVal(float hp)
        {
            if (!enabled) return;

            CheckConfig();

            float max = m_Slider.maxValue;
            float val = Mathf.Clamp01(hp / m_Slider.maxValue);
            var config = m_Configs.First(c => val >= c.MinPercentage);
            if (config.State != CurrentState)
            {
                OnStateChanged?.Invoke(CurrentState, config.State);

                CurrentState = config.State;
            }

            if (m_UseDefalutConfig) m_FillRect.color = config.ShowColor;

            if (m_Text_Val != null)
            {
                m_Text_Val.text = $"{hp}/{max}";
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HpSlider), true)]
    [CanEditMultipleObjects]
    public class PlayerHpSliderEditor : Editor
    {
        private SerializedProperty _UseDefalutConfig;
        private SerializedProperty _OffIcon;
        private SerializedProperty _IconColorActiveChanged;
        private SerializedProperty _ImageActiveColor;
        private SerializedProperty _ImageBanColor;
        private SerializedProperty _Text;
        private SerializedProperty _TextActiveColor;
        private SerializedProperty _TextBanColor;
        bool showFoldout;
        protected void OnEnable()
        {
            if (serializedObject.targetObject is GameObject obj)
            {
                obj.GetComponent<Slider>().onValueChanged.AddListener(OnValChanged);
            }
            else if (serializedObject.targetObject is Component com)
            {
                com.GetComponent<Slider>().onValueChanged.AddListener(OnValChanged);
            }
        }

        protected void OnDisable()
        {
            if (serializedObject.targetObject is GameObject obj)
            {
                obj.GetComponent<Slider>().onValueChanged.RemoveListener(OnValChanged);
            }
            else if (serializedObject.targetObject is Component com)
            {
                com.GetComponent<Slider>().onValueChanged.RemoveListener(OnValChanged);
            }
        }

        private void OnValChanged(float val)
        {
            var playerHp = (HpSlider)serializedObject.targetObject;
            playerHp.OnGetSliderVal(val);
        }
    }
#endif

}

