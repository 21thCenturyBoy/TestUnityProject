using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestAI.Move
{
    public class Kinematic_Seek : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxSpeed = 0.5f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutputVelocity Seek()
        {

            var res = new SteeringOutputVelocity();

            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Line = res.Line.normalized;//归一化
            res.Line *= maxSpeed;

            //面向要移动的方向
            var current_stae = currentEntity.GetStaticStae();
            float currentOrientation = current_stae.Orientation;

            res.Angular = 0;

            float targetOrientation = UtilsTool.NewOrientation(currentOrientation, res.Line);
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

    public class Kinematic_Arrive : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxSpeed = 0.5f;
        [AIParm_Float]
        public float slowRadius = 5;//减速范围
        [AIParm_Float]
        public float targetRadius = 1;//目标半径范围
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutputVelocity Arrive()
        {
            var res = new SteeringOutputVelocity();
            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;
            //计算距离
            float distance = res.Line.magnitude;
            if (distance < targetRadius)
            {
                res.Line = Vector3.zero;
                return res;
            }
            //这里可以使用线性插值来计算速度，也可以根据时间来计算速度
            //如果在减速范围内，计算速度
            if (distance < slowRadius)
            {
                res.Line = res.Line.normalized * maxSpeed * (distance / slowRadius);
            }
            else
            {
                res.Line = res.Line.normalized * maxSpeed;
            }
            //面向要移动的方向
            var current_stae = currentEntity.GetStaticStae();
            float currentOrientation = current_stae.Orientation;
            res.Angular = 0;
            float targetOrientation = UtilsTool.NewOrientation(currentOrientation, res.Line);
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

    public class Kinematic_Wander : KinematicLogic
    {
        private IKinematicEntity currentEntity;
        [AIParm_Float]
        public float maxSpeed = 0.2f;
        [AIParm_Float]
        public float maxRotate = 0.1f;//最大旋转
        protected override void OnStart()
        {
            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }
        protected override void OnFixedUpdate()
        {
            Wander();
        }
        public SteeringOutputVelocity Wander()
        {
            var res = new SteeringOutputVelocity();

            var current_stae = currentEntity.GetStaticStae();

            //从方向的向量形式获取速度
            res.Line = maxSpeed * current_stae.OrientationToVector();//获取当前方向的速度向量
            res.Angular = UnityEngine.Random.Range(-1f, 1f) * maxRotate;//随机旋转

            //更新实体状态
            current_stae.SteeringOutputApply(res);

            currentEntity.SetStaticStae(current_stae);

            return res;
        }
        protected override void OnStop()
        {
            currentEntity.Destroy();
        }
    }

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

