using System;
using System.Linq;
using TestAI.Move.Kinematic;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace TestAI.Move
{

    public class TestMove_SceneManager : MonoBehaviour
    {

        [SerializeField]
        private TMP_Dropdown m_sceneTypeDropdown = null;

        [SerializeField]
        private Button m_playBtn = null;

        [SerializeField]
        private Transform m_AIParm_Parent = null;

        [SerializeField]
        private KinematicLogic m_currentLogic = null;

        // Start is called before the first frame update
        void Start()
        {

            var typeList = UtilsTool.GetKinematicLogicTypeCache().Keys.ToList();
            m_sceneTypeDropdown.ClearOptions();
            m_sceneTypeDropdown.AddOptions(typeList);
            m_sceneTypeDropdown.value = 0;

            //注册回调
            m_sceneTypeDropdown.onValueChanged.AddListener(SceneTypeDropdownOnValueChanged);
            m_playBtn.onClick.AddListener(PlayBtnOnClick);

        }

        private void SceneTypeDropdownOnValueChanged(int arg0)
        {
        }

        private void PlayBtnOnClick()
        {
            if (m_currentLogic != null)
            {
                m_currentLogic.Stop();
            }
            m_currentLogic = UtilsTool.CreateKinematicLogic(m_sceneTypeDropdown.options[m_sceneTypeDropdown.value].text);
            m_currentLogic.CreatAIPramUI(m_AIParm_Parent);
            m_currentLogic.Start();
        }

        void FixedUpdate()
        {
            m_currentLogic?.FixedUpdate();
        }
    }
}

