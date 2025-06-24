using UnityEngine;

using UnityEngine.Playables;

using UnityEngine.Animations;

/// <summary>
/// 创建动画混合树，混合两个动画用
/// </summary>
[RequireComponent(typeof(Animator))]
public class MixAnimationSample : MonoBehaviour
{

    public AnimationClip clip0;
    public AnimationClip clip1;

    [Range(0f,1f)]
    public float weight;

    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;
    void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);
        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

        // Plays the Graph.
        playableGraph.Play();

    }

    void Update()
    {
        //该SetInputWeight()方法动态调整每个可播放的混合权重
        weight = Mathf.Clamp01(weight);
        mixerPlayable.SetInputWeight(0, 1.0f - weight);
        mixerPlayable.SetInputWeight(1, weight);
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}