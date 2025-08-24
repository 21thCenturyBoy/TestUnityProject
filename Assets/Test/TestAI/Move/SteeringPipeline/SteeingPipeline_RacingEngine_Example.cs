using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

namespace TestAI.Move.SteeringPipeline
{
    //追逐目标
    public class ChaseTargeter : Targeter
    {
        public IKinematicEntity chasedEntity;

        //控制移动的提前距离，预判目标位置
        public float lookAheadTime = 0.5f;

        public override IPipeline_Goal GetSumGoal(IKinematicEntity entity)
        {
            Pipeline_Goal goal = new Pipeline_Goal();
            if (chasedEntity == null) return goal;
            //计算预判位置
            Vector3 targetPosition = chasedEntity.Position + chasedEntity.Velocity * lookAheadTime;
            goal.position = targetPosition;
            goal.hasPositionData = true;
            return goal;
        }
    }

    public class RacingMapGraph
    {
        //将位置转换为节点
        public RacingMapGraph_Node GetNode(Vector3 pos)
        {
            // TODO 实现具体的节点查找逻辑
            return null;
        }
        public Vector3 GetPosition(RacingMapGraph_Node node)
        {
            // TODO 实现具体的节点位置获取逻辑
            return Vector3.zero;
        }
    }
    public class RacingMapGraph_Node
    {

    }

    //基于路径规划的分解器
    public class PlanningDecomposer : Decomposer
    {
        RacingMapGraph graph;
        Func<RacingMapGraph_Node, RacingMapGraph_Node, float> heuristicFunc;//启发式函数，估算从某节点到目标节点的代价
        public override IPipeline_Goal Decompose(IKinematicEntity entity, IPipeline_Goal sumGoal)
        {
            Pipeline_Goal goal = (Pipeline_Goal)sumGoal;

            // 可以量化当前的位置和目标位置
            RacingMapGraph_Node start = graph.GetNode(entity.Position);
            RacingMapGraph_Node end = graph.GetNode(goal.position);


            // 如果起始节点和终点相同，直接返回总目标，无需规划路径
            if (start == end)
                return goal;

            RacingMapGraph_Node[] path = PathFindAStar(graph, start, end, heuristicFunc);
            Vector3 targetPos = graph.GetPosition(path[0]);

            //更新该目标位置并但会结果
            goal.position = targetPos;
            goal.hasPositionData = true;

            return goal;
        }

        // A*路径查找算法
        public RacingMapGraph_Node[] PathFindAStar(RacingMapGraph racingMap, RacingMapGraph_Node start, RacingMapGraph_Node end, Func<RacingMapGraph_Node, RacingMapGraph_Node, float> heuristicFunc)
        {
            return null;
        }
    }

    //避障约束
    public class AvoidObstacleConstraint : Constraint
    {
        // 保存障碍物边界球体
        public Vector3[] obstacleCenters;
        public float obstacleRadius = 1.0f; // 障碍物半径

        // 安全距离，确保实体与障碍物之间有一定的缓冲区（与障碍物半径有关）
        public float safeMargin = 1.5f; // 安全距离系数

        public float SafeDistance => obstacleRadius * safeMargin;

        private int m_problemIndex;//出现违反约束条件的情况，则存储导致问题的路径点索引

        public override bool IsViolated(IPath path)
        {
            // 使用枚举器遍历路径上的点
            foreach (var item in path)
            {
                
            }

            // TODO 实现具体的路径检测逻辑
            return false;
        }
        public override IPipeline_Goal Suggest(IKinematicEntity entity, IPipeline_Goal goal, IPath path)
        {
            Pipeline_Goal newGoal = (Pipeline_Goal)goal;
            // TODO 实现具体的避障建议逻辑
            return newGoal;
        }


        //点到线段的最短距离
        private Vector3 PointToSegmentDistance(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            float segmentLengthSq = direction.sqrMagnitude;

            // 计算参数t，表示线段上最近点的位置
            float t = Vector3.Dot(point - start, direction) / segmentLengthSq;

            // 将t限制在[0,1]范围内（确保点在线段上而不是延长线上）
            t = Mathf.Clamp01(t);

            // 计算线段上最近的点
            Vector3 closestPoint = start + t * direction;

            return closestPoint;
        }
        private int CheckSegment(Vector3 start, Vector3 end)
        {
            if (obstacleCenters == null || obstacleCenters.Length == 0)
                return -1;

            Vector3 direction = end - start;
            float segmentLengthSq = direction.sqrMagnitude;

            float minDistance = float.MaxValue;
            int closestObstacleIndex = -1;
            // 遍历所有障碍物
            for (int i = 0; i < obstacleCenters.Length; i++)
            {
                Vector3 center = obstacleCenters[i];

                // 计算线段上最近的点
                Vector3 closestPoint = PointToSegmentDistance(center, start, end);

                // 计算障碍物中心到线段最近点的距离
                float distance = Vector3.Distance(center, closestPoint);

                // 如果距离小于安全距离，则线段与障碍物相交
                if (distance < SafeDistance)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestObstacleIndex = i;
                    }
                }
            }

            // 确保所有代码路径都有返回值
            return closestObstacleIndex;
        }
    }
}