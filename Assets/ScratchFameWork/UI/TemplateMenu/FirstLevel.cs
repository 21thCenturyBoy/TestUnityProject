using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public enum FirstLevelType
    {
        None = -1,
        Event,
        Action,
        Control,
        Condition,
        GetValue,
        Variable,
        Custom,
        Search,
    }

    public class FirstLevel : MonoBehaviour
    {
        public FirstLevelType Type;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}