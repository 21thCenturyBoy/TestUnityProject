using Codice.Client.BaseCommands.BranchExplorer;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TestAI.Move.Kinematic;
using UnityEngine;
namespace TestAI.Move.Flocking
{
    public class PrioritySteering : SteeringLogic
    {
        public List<BlendedSteering> groups;

        [AIParam_Float("加速度阈值")]
        public float epsilon = 0.1f;

        public override SteeringOutput GetSteeringOut()
        {
            int index = 0;
            foreach (BlendedSteering group in groups)
            {
                var res = group.GetSteeringOut();
                index++;
                if (res.Linear.magnitude > epsilon || MathF.Abs(res.Angular) > epsilon)
                {
                    Debug.Log($"PrioritySteering use group {index}+{epsilon}:{res.Linear}:{res.Angular}");
                    return res;
                }
            }
            return new SteeringOutput();
        }
    }
}