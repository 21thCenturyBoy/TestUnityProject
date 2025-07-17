using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_避开障碍物")]
    public class ObstacleAvoidance : Steering_Seek
    {
        [AIParam_Float("检查碰撞的最小距离")]
        public float avoidDistance = 10f;

        [AIParam_Float("基于障碍物表面的距离")]
        public float lookAhead = 10f;

        private List<Navigation_Obstacle_Item> m_obstacleList = new List<Navigation_Obstacle_Item>();

        public override Vector3 GetTargetPos()
        {
            RaycastHit hit;
            if (UtilsTool.PhysicsRaycast(currentEntity.Position, currentEntity.Velocity, lookAhead, out hit))
            {
                return hit.point + (hit.normal * avoidDistance);
            }
            else
            {
                return base.GetTargetPos();
            }
        }

        [AITest_Button("创建障碍")]
        public void CeateObstacle()
        {

            Navigation_Obstacle_Item point_Obstacle = UtilsTool.CreateNavigation_Obstacle() as Navigation_Obstacle_Item;
            m_obstacleList.Add(point_Obstacle);
        }
        protected override void OnStop()
        {
            base.OnStop();

            for (int i = 0; i < m_obstacleList.Count; i++)
            {
                GameObject.Destroy(m_obstacleList[i].gameObject);
            }
            m_obstacleList.Clear();
        }
    }
}