using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HostGame
{
    [System.Serializable]
    public class TimelineAssetOverride
    {
        // Idk how to store per object editor parameters else
        #region editor

#if UNITY_EDITOR

        /// <summary>
        /// Not accessable at runtime
        /// </summary>
        public TimelineAsset oldtimeline;

        /// <summary>
        /// Not accessable at runtime
        /// </summary>
        public bool[] Overrides;

#endif

        #endregion editor

        public TimelineAsset timelineAsset;
        public Object[] Bindings = new Object[0];

        /// <summary>
        /// Find overrides that correspond to specific timeline
        /// </summary>
        /// <param name="array"></param>
        /// <param name="toFind"></param>
        /// <returns></returns>
        public static TimelineAssetOverride FindByAsset(TimelineAssetOverride[] array, TimelineAsset toFind)
        {
            foreach (var item in array)
            {
                if (item.timelineAsset == toFind)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Assign specific object to specific track of timeline asset
        /// </summary>
        /// <param name="trackName"></param>
        /// <param name="dir"></param>
        /// <param name="obj"></param>
        public static void SetOverride(int trackIndex, PlayableDirector dir, Object obj)
        {
            var track = (TimelineAsset)dir.playableAsset;
            var trackT = track.GetOutputTrack(trackIndex);
            dir.SetGenericBinding(trackT, obj);
        }

        /// <summary>
        /// Same as: <seealso cref="SetOverride(string, PlayableDirector, Object)"/> but also returns track asset it found, so can be reused without comparing strings
        /// </summary>
        /// <param name="trackName"></param>
        /// <param name="dir"></param>
        /// <param name="obj"></param>
        /// <param name="trackAsset"></param>
        public static void SetOverride(string trackName, PlayableDirector dir, Object obj, out TrackAsset trackAsset)
        {
            TrackAsset outAsset = null;
            var t = (TimelineAsset)dir.playableAsset;

            foreach (var track in t.GetOutputTracks())
            {
                if (string.CompareOrdinal(trackName, track.name) == 0)
                    outAsset = track;
            }

            dir.SetGenericBinding(outAsset, obj);
            trackAsset = outAsset;
        }

        /// <summary>
        /// Assign specific object to specific track of timeline asset
        /// </summary>
        /// <param name="trackName"></param>
        /// <param name="dir"></param>
        /// <param name="obj"></param>
        public static void SetOverride(string trackName, PlayableDirector dir, Object obj)
        {
            TrackAsset outAsset = null;
            var t = (TimelineAsset)dir.playableAsset;

            foreach (var trackk in t.GetOutputTracks())
            {
                if (string.CompareOrdinal(trackName, trackk.name) == 0)
                    outAsset = trackk;
            }

            dir.SetGenericBinding(outAsset, obj);
        }

        /// <summary>
        /// Set playable director generic bindings based on overrided ones
        /// </summary>
        /// <param name="director"></param>
        public void SetPlayableWithOverrides(PlayableDirector director)
        {
            director.playableAsset = timelineAsset;

            for (int i = 0; i < Bindings.Length; i++)
            {
                if (!Bindings[i])
                    continue;

                var track = timelineAsset.GetOutputTrack(i);
                director.SetGenericBinding(track, Bindings[i]);
            }
        }

        /// <summary>
        /// Setting overrides without assigning new timeline asset
        /// </summary>
        /// <param name="director"></param>
        public void SetOverridesOnly(PlayableDirector director)
        {
            for (int i = 0; i < Bindings.Length; i++)
            {
                if (!Bindings[i])
                    continue;

                var track = timelineAsset.GetOutputTrack(i);
                director.SetGenericBinding(track, Bindings[i]);
            }
        }
    }
}