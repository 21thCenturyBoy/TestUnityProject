using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class MultiOutputSample : MonoBehaviour

{

    public AnimationClip animationClip;
    public AudioClip audioClip;

    PlayableGraph playableGraph;

    void Start()
    {

        playableGraph = PlayableGraph.Create();

        // Create the outputs.
        var animationOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        var audioOutput = AudioPlayableOutput.Create(playableGraph, "Audio", GetComponent<AudioSource>());

        // Create the playables.
        var animationClipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
        var audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, true);

        // Connect the playables to an output
        animationOutput.SetSourcePlayable(animationClipPlayable);
        audioOutput.SetSourcePlayable(audioClipPlayable);

        // Plays the Graph.

        playableGraph.Play();

    }

    void OnDisable()
    {

        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();

    }

}