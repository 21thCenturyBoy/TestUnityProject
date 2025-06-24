using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

/// <summary>
/// 控制树的时间
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayWithTimeControlSample : MonoBehaviour
{

    public AnimationClip clip;
    public float time;

    PlayableGraph playableGraph;
    AnimationClipPlayable playableClip;

    void Start()
    {
        playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable
        playableClip = AnimationClipPlayable.Create(playableGraph, clip);

        // Connect the Playable to an output
        playableOutput.SetSourcePlayable(playableClip);

        // Plays the Graph.
        playableGraph.Play();

        // Stops time from progressing automatically.
        playableClip.SetPlayState(PlayState.Paused);

    }

    void Update()
    {
        // Control the time manually
        playableClip.SetTime(time);
    }

    void OnDisable()
    {

        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}