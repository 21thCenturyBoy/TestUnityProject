using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// 通过创建Playable播放动画队列
/// </summary>
public class PlayQueuePlayable : PlayableBehaviour
{

    private int m_CurrentClipIndex = -1;
    private float m_TimeToNextClip;//计算切换到一下片段的时间

    private Playable mixer;
    private float m_AllClipWeight;
    public void Initialize(AnimationClip[] clipsToPlay, Playable owner, PlayableGraph graph)
    {

        owner.SetInputCount(1);
        mixer = AnimationMixerPlayable.Create(graph, clipsToPlay.Length);

        graph.Connect(mixer, 0, owner, 0);
        owner.SetInputWeight(0, 1);

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            graph.Connect(AnimationClipPlayable.Create(graph, clipsToPlay[clipIndex]), 0, mixer, clipIndex);
            mixer.SetInputWeight(clipIndex, 1.0f);
        }
    }

    public override void PrepareFrame(Playable owner, FrameData info)
    {

        if (mixer.GetInputCount() == 0) return;

        // Advance to next clip if necessary
        m_TimeToNextClip -= (float)info.deltaTime;

        if (m_TimeToNextClip <= 0.0f)
        {
            //开始播放下一片段

            m_CurrentClipIndex++;
            if (m_CurrentClipIndex >= mixer.GetInputCount()) m_CurrentClipIndex = 0;

            var currentClip = (AnimationClipPlayable)mixer.GetInput(m_CurrentClipIndex);
            // Reset the time so that the next clip starts at the correct position
            //重置当前播放权重索引

            currentClip.SetTime(0);

            m_TimeToNextClip = currentClip.GetAnimationClip().length;
        }

        // Adjust the weight of the inputs
        //调整输入的权重
        int otherWeight = mixer.GetInputCount() - 1;//其他剪辑权重均摊
        float preWeight = (1 - m_AllClipWeight) / otherWeight;
        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            if (clipIndex == m_CurrentClipIndex) mixer.SetInputWeight(clipIndex, m_AllClipWeight);
            else mixer.SetInputWeight(clipIndex, preWeight);
        }
    }

    public void SetCurrentClipWeight(float weight)
    {
        m_AllClipWeight = Mathf.Clamp01(weight);
    }
}

[RequireComponent(typeof(Animator))]

public class PlayQueueSample : MonoBehaviour
{
    public AnimationClip[] clipsToPlay;


    [Range(0f, 1f)]
    public float CurrentClipWeight = 1f;//控制当前剪辑权重

    PlayableGraph playableGraph;
    PlayQueuePlayable playQueue;
    void OnEnable()
    {

        playableGraph = PlayableGraph.Create();
        var playQueuePlayable = ScriptPlayable<PlayQueuePlayable>.Create(playableGraph);

        playQueue = playQueuePlayable.GetBehaviour();

        playQueue.Initialize(clipsToPlay, playQueuePlayable, playableGraph);
        playQueue.SetCurrentClipWeight(CurrentClipWeight);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        playableOutput.SetSourcePlayable(playQueuePlayable,0);

        playableGraph.Play();

    }
    void Update()
    {
        playQueue.SetCurrentClipWeight(CurrentClipWeight);
    }

    void OnDisable()
    {

        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}