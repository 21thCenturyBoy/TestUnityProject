﻿using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

class ApplyVelocitySample : MonoBehaviour
{
    public struct VelocityJob : IJobParallelForTransform
    {
        // Jobs declare all data that will be accessed in the job
        // By declaring it as read only, multiple jobs are allowed to access the data in parallel
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        // Delta time must be copied to the job since jobs generally don't have a concept of a frame.
        // The main thread waits for the job same frame or next frame, but the job should do work deterministically
        // independent on when the job happens to run on the worker threads.
        public float deltaTime;

        // The code actually running on the job
        public void Execute(int index, TransformAccess transform)
        {
            // Move the transforms based on delta time and velocity
            var ang = transform.rotation.eulerAngles;
            ang += velocity[index] * deltaTime;
            transform.rotation = Quaternion.Euler(ang);
        }
    }

    // Assign transforms in the inspector to be acted on by the job
    [SerializeField] public Transform[] m_Transforms;
    TransformAccessArray m_AccessArray;

    void Awake()
    {
        if (m_Transforms.Length == 0)
        {
            List<Transform> m_temp = new List<Transform>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.SetParent(this.transform);
                    obj.transform.position = new Vector3(i * 2, j * 2, 0);
                    m_temp.Add(obj.transform);
                }
            }
            m_Transforms = m_temp.ToArray();
        }

        // Store the transforms inside a TransformAccessArray instance,
        // so that the transforms can be accessed inside a job.
        m_AccessArray = new TransformAccessArray(m_Transforms);
    }

    void OnDestroy()
    {
        // TransformAccessArrays must be disposed manually.
        m_AccessArray.Dispose();
    }

    public void Update()
    {
        var velocity = new NativeArray<Vector3>(m_Transforms.Length, Allocator.Persistent);

        for (var i = 0; i < velocity.Length; ++i)
            velocity[i] = new Vector3(0f, 10f, 0f);

        // Initialize the job data
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            velocity = velocity
        };

        // Schedule a parallel-for-transform job.
        // The method takes a TransformAccessArray which contains the Transforms that will be acted on in the job.
        JobHandle jobHandle = job.Schedule(m_AccessArray);

        // Ensure the job has completed.
        // It is not recommended to Complete a job immediately,
        // since that reduces the chance of having other jobs run in parallel with this one.
        // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
        //确保任务已经完成。
        //不建议立即完成任务。
        //因为这样可以减少其他作业与此作业并行运行的机会。
        //你最好在一个帧的早期调度一个作业，然后在该帧的后期等待它。
        jobHandle.Complete();

        Debug.Log(m_Transforms[0].position);

        // Native arrays must be disposed manually.
        velocity.Dispose();
    }
}