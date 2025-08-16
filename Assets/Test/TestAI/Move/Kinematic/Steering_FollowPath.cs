using TMPro;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_路径跟随")]

    public class Steering_FollowPath : Steering_Seek
    {
        public Path_LineSegment Path_Line;//路径线段

        [AIParam_Float("路径偏移量（可负数）")]
        public float pathOffset = 0.05f; // 路径偏移量

        [AIParam_Info("当前路径位置")]
        public float currentParam = 0f; // 当前路径位置

        [AIParam_Info("运动预测时间")]
        public float predictTime = 0.1f; // 预测时间

        //Steering_Face m_steering_Face =new Steering_Face();//暂时先不考虑朝向问题

        public override SteeringOutput GetSteeringOut()
        {

            //计算预测位置
            var predictPosition = currentEntity.GetStaticStae().Position + currentEntity.Velocity * predictTime;

            //查找当前位置
            var closetParam = Path_Line.GetParam(predictPosition, pathOffset);

            //避免凹凸路线干扰，简单考虑相干性，避免出现路径跳跃（横跨路径）
            if (Mathf.Abs(currentParam - closetParam) < pathOffset + 0.1f)
            {
                currentParam = closetParam;
            }

            //添加偏移量
            var targetParam = currentParam + pathOffset;

            //获取目标位置
            targetEntity.SetPosition(Path_Line.GetPosition(targetParam));

            var res = base.GetSteeringOut();
            //res.Angular = m_steering_Face.Align().Angular;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            var res = GetSteeringOut();

            currentEntity.FixedUpdate(res, maxSpeed, FixedDeltaTime);
        }


        protected override void OnStart()
        {
            //创建沿着路径移动的目标实体
            targetEntity = UtilsTool.CreateNavigation_AI();
            targetEntity.SetColor(Color.red);

            // 生成路径线段
            GameObject path_line = new GameObject("Path_LineSegment");
            Path_Line = path_line.AddComponent<Path_LineSegment>();
            Path_Line.Points = new Navigation_Point_Item[5];

            Vector2 range = new Vector2(50, 50);
            for (int i = 0; i < 5; i++)
            {
                Navigation_Point_Item point_Item = UtilsTool.CreateNavigation_Point() as Navigation_Point_Item;

                point_Item.SetStaticStae(UtilsTool.CreateRandomStaticStae(range, false));
                point_Item.SetColor(Color.red);
                point_Item.GetComponentInChildren<TMP_Text>().text = i.ToString();
                Path_Line.Points[i] = point_Item;
            }


            //创建当前跟随实体
            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);

            //m_steering_Face.targetEntity = targetEntity;
            //m_steering_Face.currentEntity = currentEntity;
        }


        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();

            for (int i = 0; i < Path_Line.Points.Length; i++)
            {
                Navigation_Point_Item item = Path_Line.Points[i] as Navigation_Point_Item;
                GameObject.Destroy(item.gameObject);
            }

            GameObject.Destroy(Path_Line.gameObject);
        }
    }
}