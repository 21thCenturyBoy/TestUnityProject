using UnityEngine;

namespace TestAI.Move
{
    public class Path_LineSegment : MonoBehaviour, IPath
    {
        public IPoint[] Points;

        /// <summary>
        /// 计算路径最近点的参数。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lastParam"></param>
        /// <returns></returns>
        public float GetParam(Vector3 position, float lastParam)
        {
            //将点构成线段，计算出距离这些线段路径上最近的点
            if (Points == null || Points.Length < 2)
            {
                return 0f;
            }

            float closestParam = 0f;
            float closestDistanceSqr = float.MaxValue;

            // 遍历所有线段
            for (int i = 0; i < Points.Length - 1; i++)
            {
                Vector3 segmentStart = Points[i].GetPosition();
                Vector3 segmentEnd = Points[i + 1].GetPosition();

                // 计算线段上最近点的参数 t (0 到 1 之间)
                Vector3 segmentDirection = segmentEnd - segmentStart;//线段向量
                Vector3 toPosition = position - segmentStart;//目标到开始点

                float segmentLengthSqr = segmentDirection.sqrMagnitude;
                float t = 0f;

                if (segmentLengthSqr > Mathf.Epsilon)
                {
                    t = Mathf.Clamp01(Vector3.Dot(toPosition, segmentDirection) / segmentLengthSqr);
                }

                // 计算线段上最近点的实际位置
                Vector3 closestPointOnSegment = segmentStart + t * segmentDirection;

                // 计算距离的平方
                float distanceSqr = (position - closestPointOnSegment).sqrMagnitude;

                // 如果这是目前最近的点，更新结果
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    // 参数 = 线段索引 + 线段内的比例
                    closestParam = i + t;
                }
            }

            return closestParam;
        }

        public Vector3 GetPosition(float param)
        {
            //根据参数计算线段点的位置
            if (Points == null || Points.Length < 2)
            {
                Debug.LogWarning("Path has insufficient points.");
                return Vector3.zero;
            }

            // 限制参数在有效范围内
            param = Mathf.Clamp(param, 0, Points.Length - 1);

            // 获取整数部分(线段索引)和小数部分(线段内插值)
            int segmentIndex = Mathf.FloorToInt(param);
            float t = param - segmentIndex; // 线段内的插值参数

            // 处理边界情况
            if (segmentIndex >= Points.Length - 1)
            {
                // 如果是最后一个点或超出范围，直接返回最后一个点
                return Points[Points.Length - 1].GetPosition();
            }

            // 线性插值计算位置
            Vector3 startPoint = Points[segmentIndex].GetPosition();
            Vector3 endPoint = Points[segmentIndex + 1].GetPosition();

            return Vector3.Lerp(startPoint, endPoint, t);
        }

        private void OnDrawGizmos()
        {
            DrawPath();
        }

        /// <summary>
        /// 在编辑器中绘制路径
        /// </summary>
        public void DrawPath()
        {
#if UNITY_EDITOR
            if (Points == null || Points.Length < 2)
                return;

            // 设置Gizmos颜色
            UnityEngine.Gizmos.color = UnityEngine.Color.yellow;

            // 绘制路径点
            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 pointPos = Points[i].GetPosition();

                // 绘制路径点
                UnityEngine.Gizmos.color = UnityEngine.Color.green;
                UnityEngine.Gizmos.DrawWireSphere(pointPos, 0.5f);

                // 绘制点的索引标签
                UnityEditor.Handles.Label(pointPos + Vector3.up * 1f, i.ToString());
            }

            // 绘制连接线段
            UnityEngine.Gizmos.color = UnityEngine.Color.yellow;
            for (int i = 0; i < Points.Length - 1; i++)
            {
                Vector3 start = Points[i].GetPosition();
                Vector3 end = Points[i + 1].GetPosition();
                UnityEngine.Gizmos.DrawLine(start, end);

                // 在线段中点绘制方向箭头
                Vector3 midPoint = (start + end) * 0.5f;
                Vector3 direction = (end - start).normalized;
                Vector3 arrowHead1 = midPoint + Quaternion.Euler(0, 30, 0) * (-direction) * 0.8f;
                Vector3 arrowHead2 = midPoint + Quaternion.Euler(0, -30, 0) * (-direction) * 0.8f;

                UnityEngine.Gizmos.color = UnityEngine.Color.red;
                UnityEngine.Gizmos.DrawLine(midPoint, arrowHead1);
                UnityEngine.Gizmos.DrawLine(midPoint, arrowHead2);
            }
#endif
        }

        /// <summary>
        /// 使用Debug.DrawLine绘制路径（运行时也可见）
        /// </summary>
        public void DrawPathDebug(float duration = 0.1f)
        {
            if (Points == null || Points.Length < 2)
                return;

            // 绘制路径点
            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 pointPos = Points[i].GetPosition();

                // 绘制十字标记表示路径点
                Debug.DrawLine(pointPos + Vector3.left * 0.5f, pointPos + Vector3.right * 0.5f, UnityEngine.Color.green, duration);
                Debug.DrawLine(pointPos + Vector3.back * 0.5f, pointPos + Vector3.forward * 0.5f, UnityEngine.Color.green, duration);
                Debug.DrawLine(pointPos + Vector3.down * 0.5f, pointPos + Vector3.up * 0.5f, UnityEngine.Color.green, duration);
            }

            // 绘制连接线段
            for (int i = 0; i < Points.Length - 1; i++)
            {
                Vector3 start = Points[i].GetPosition();
                Vector3 end = Points[i + 1].GetPosition();
                Debug.DrawLine(start, end, UnityEngine.Color.yellow, duration);
            }
        }
    }
}