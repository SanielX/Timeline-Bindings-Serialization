# Timeline Overrides Serialization
 Easy way to serialize timeline generic references in editor
Video: https://www.reddit.com/r/Unity3D/comments/gcwb2m/ive_made_a_tool_for_setting_timeline_bindings_at/

By default it serializes everything by track ID so it can work strangely with marker track I guess. Also if you change number of tracks bindings will clear.

I made this script for myself after all

# How to use
Just add TimelineAssetOverride as your property to script:
```csharp
public class A : MonoBehaviour {
    public TimelineAssetOverride Overrides;

    void Start(){
        // And then to set overrides:
        var dir = GetComponent<PlayableDirector>();

        // This will set timeline asset AND overrides
        // Though it won't apply empty overrides
        Overrides.SetPlayableWithOverrides(dir);
    }
}
```
