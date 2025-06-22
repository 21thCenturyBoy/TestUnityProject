using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestAI.Move
{
    public class Navigation_Seek : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        public float maxSpeed = 0.5f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutput Seek() {

            var res = new SteeringOutput();

            //获取目标的方向
            res.Velocity = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Velocity = res.Velocity.normalized;//归一化
            res.Velocity *= maxSpeed;

            //面向要移动的方向
            var current_stae = currentEntity.GetStaticStae();
            float currentOrientation = current_stae.Orientation;

            res.Angular = 0;
            
            float targetOrientation = UtilsTool.NewOrientation(currentOrientation, res.Velocity);
            current_stae.Orientation = targetOrientation;

            current_stae.SteeringOutputApply(res);

            currentEntity.SetStaticStae(current_stae);

            return res;
        }

        protected override void OnFixedUpdate()
        {
            Seek();
        }

        protected override void OnStart()
        {
            targetEntity = UtilsTool.CreateNavigation_AI();
            Vector2 range = new Vector2(50, 50);
            targetEntity.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
            targetEntity.SetColor(Color.red);
            targetEntity.AllowDrag(true);

            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }

        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }

    public class Navigation_Arrive : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;
        public float maxSpeed = 0.5f;

        public float slowRadius = 5;
        public float targetRadius = 1;
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutput Arrive() {
            var res = new SteeringOutput();
            //获取目标的方向
            res.Velocity = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;
            //计算距离
            float distance = res.Velocity.magnitude;
            if (distance < targetRadius)
            {
                res.Velocity = Vector3.zero;
                return res;
            }
            //如果在减速范围内，计算速度
            if (distance < slowRadius)
            {
                res.Velocity = res.Velocity.normalized * maxSpeed * (distance / slowRadius);
            }
            else
            {
                res.Velocity = res.Velocity.normalized * maxSpeed;
            }
            //面向要移动的方向
            var current_stae = currentEntity.GetStaticStae();
            float currentOrientation = current_stae.Orientation;
            res.Angular = 0;
            float targetOrientation = UtilsTool.NewOrientation(currentOrientation, res.Velocity);
            current_stae.Orientation = targetOrientation;

            current_stae.SteeringOutputApply(res);

            currentEntity.SetStaticStae(current_stae);
            return res;
        }
        protected override void OnFixedUpdate()
        {
            Arrive();
        }
        protected override void OnStart()
        {
            targetEntity = UtilsTool.CreateNavigation_AI();
            Vector2 range = new Vector2(50, 50);
            targetEntity.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
            targetEntity.SetColor(Color.red);
            targetEntity.AllowDrag(true);

            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }
        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }

    public class TestMove_SceneManager : MonoBehaviour
    {

        [SerializeField]
        private TMP_Dropdown m_sceneTypeDropdown = null;

        [SerializeField]
        private Button m_playBtn = null;

        private IKinematicLogic m_currentLogic;
        public enum TestMoveSceneType
        {
            Navigation_Seek,
            Navigation_Arrive,
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
                case TestMoveSceneType.Navigation_Seek:
                    m_currentLogic = new Navigation_Seek();
                    break;
                case TestMoveSceneType.Navigation_Arrive:
                    m_currentLogic = new Navigation_Arrive();
                    break;
            }

            m_currentLogic.Start();
        }

        void FixedUpdate()
        {
            m_currentLogic?.FixedUpdate();
        }
    }
}

