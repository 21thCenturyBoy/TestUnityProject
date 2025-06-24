using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// 播放动画片段示例
/// </summary>
[RequireComponent(typeof(Animator))]

public class PlayAnimationSample : MonoBehaviour

{

    public AnimationClip clip;
    PlayableGraph playableGraph;

    void Start()

    {
        playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        // Wrap the clip in a playable
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        // Connect the Playable to an output
        playableOutput.SetSourcePlayable(clipPlayable);
        // Plays the Graph.
        playableGraph.Play();

    }

    void OnDisable()

    {
        // Destroys all Playables and PlayableOutputs created by the graph.
        playableGraph.Destroy();
    }

}