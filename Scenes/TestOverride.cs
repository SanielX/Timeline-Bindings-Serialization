using UnityEngine;
using UnityEngine.Playables;
using HostGame;

public class TestOverride : MonoBehaviour
{
    public PlayableDirector director;
    public TimelineAssetOverride timeline;

    //[NaughtyAttributes.Button]
    void Start()
    {
        timeline.SetPlayableWithOverrides(director);
    }
}
