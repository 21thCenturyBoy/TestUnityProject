using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestPlayables
{
    public class TestLoadAnimClicp : MonoBehaviour
    {
        public AnimationClip Clip;
        // Start is called before the first frame update
        void Start()
        {
            Clip = Resources.Load<AnimationClip>("Anim@Run 1");
            Debug.LogError(Clip.name);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

