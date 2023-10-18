using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayAnimationSample : MonoBehaviour
{
    public AnimationClip clip;
    private PlayableGraph graph;
    // Start is called before the first frame update
    void Start()
    {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);//设置游戏更新

        var clipplayable = AnimationClipPlayable.Create(graph,clip);

        var output = AnimationPlayableOutput.Create(graph, "Anim", GetComponent<Animator>());
        output.SetSourcePlayable(clipplayable);

        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
