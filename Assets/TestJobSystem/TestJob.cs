using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class TestJob : MonoBehaviour
{
    public struct VelocityJob : IJob
    {
        //job声明job中将要访问的所有数据  
        //通过声明为只读，允许多个作业并行访问数据 
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        //默认情况下，容器被认为是可读和可写的 
        public NativeArray<Vector3> position;

        //增量时间必须复制到作业，因为作业通常没有框架的概念。  
        //主线程在同一帧或下一帧等待作业，但作业应该等待  
        //在工作线程上运行时，以确定和独立的方式执行工作。 
        public float deltaTime;

        // 实际运行在作业上的代码
        public void Execute()
        {
            // 根据时间差和速度移动位置 
            for (var i = 0; i < position.Length; i++) position[i] = position[i] + velocity[i] * deltaTime;
        }
    }
    public void Update()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);

        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
        for (var i = 0; i < velocity.Length; i++) velocity[i] = new Vector3(0, 10, 0);


        // 初始化作业数据
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };

        // 调度作业，返回以后可以等待的JobHandle 
        JobHandle jobHandle = job.Schedule();

        //初始化作业数据,确保作业已完成
        //不建议立即完成任务，  
        //因为它没有实际的并行性
        //你最好在一个帧的早期安排一个任务，然后在稍后的帧中等待它。 
        jobHandle.Complete();

        Debug.Log(job.position[0]);

        // 必须手动清除本机数组
        position.Dispose();
        velocity.Dispose();
    }
}