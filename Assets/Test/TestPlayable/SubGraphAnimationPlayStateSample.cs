using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

/// <summary>
/// 控制树的播放状态
/// </summary>
[RequireComponent(typeof(Animator))]
public class SubGraphAnimationPlayStateSample : MonoBehaviour
{
    public AnimationClip clip0;
    public AnimationClip clip1;

    [Range(0f, 1f)]
    public float weight;

    public PlayState Clip1_PlayState;

    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    AnimationClipPlayable clipPlayable0;
    AnimationClipPlayable clipPlayable1;
    void Start()
    {

        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);

        playableOutput.SetSourcePlayable(mixerPlayable);
        // Creates AnimationClipPlayable and connects them to the mixer.

        clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);


        // Plays the Graph.

        playableGraph.Play();

    }

    void Update()
    {
        weight = Mathf.Clamp01(weight);
        mixerPlayable.SetInputWeight(0, 1.0f - weight);
        mixerPlayable.SetInputWeight(1, weight);

        if (clipPlayable1.GetPlayState() != Clip1_PlayState)
        {
            clipPlayable1.SetPlayState(Clip1_PlayState);
        }
    }

    void OnDisable()
    {

        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }

}