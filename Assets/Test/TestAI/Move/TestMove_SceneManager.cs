using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TestAI.Move.Kinematic;

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


        public enum TestMoveSceneType
        {
            Kinematic_Seek,
            Kinematic_Arrive,
            Kinematic_Wander,
            Steering_Seek,
            Steering_Flee,
            Steering_Arrive
        }
        // Start is called before the first frame update
        void Start()
        {
            var typeList = System.Enum.GetNames(typeof(TestMoveSceneType)).ToList();
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

            TestMoveSceneType type = (TestMoveSceneType)m_sceneTypeDropdown.value;
            switch (type)
            {
                case TestMoveSceneType.Kinematic_Seek:
                    m_currentLogic = new Kinematic_Seek();
                    break;
                case TestMoveSceneType.Kinematic_Arrive:
                    m_currentLogic = new Kinematic_Arrive();
                    break;
                case TestMoveSceneType.Kinematic_Wander:
                    m_currentLogic = new Kinematic_Wander();
                    break;
                case TestMoveSceneType.Steering_Seek:
                    m_currentLogic = new Steering_Seek();
                    break;
                case TestMoveSceneType.Steering_Flee:
                    m_currentLogic = new Steering_Flee();
                    break;
                case TestMoveSceneType.Steering_Arrive:
                    m_currentLogic = new Steering_Arrive();
                    break;
            }
            m_currentLogic.CreatAIPramUI(m_AIParm_Parent);
            m_currentLogic.Start();
        }

        void FixedUpdate()
        {
            m_currentLogic?.FixedUpdate();
        }
    }
}

