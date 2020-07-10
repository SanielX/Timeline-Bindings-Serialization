using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using System.Reflection;

namespace HostGame
{
    [CustomPropertyDrawer(typeof(TimelineAssetOverride))]
    public class TimelineAssetOverrideEditor : PropertyDrawer
    {
        // Data used for popups menu
        // Seems like pasing data like field is not safe for property drawers
        private struct UserData
        {
            public SerializedProperty myProperty;
            public TimelineAssetOverride myTarget;
            public int num;
        }

        #region Overrides menu

        private void AddMenuItemForOverride(GenericMenu menu, string menuPath, UserData data)
        {
            // the menu item is marked as selected if it matches the current value of m_Color
            menu.AddItem(new GUIContent(menuPath), false, OnOverrideAdded, data);
        }

        private void RemoveMenuItemForOverride(GenericMenu menu, string menuPath, UserData data)
        {
            // the menu item is marked as selected if it matches the current value of m_Color
            menu.AddItem(new GUIContent(menuPath), false, OnOverrideDeleted, data);
        }

        private void OnOverrideAdded(object userData)
        {
            UserData u = (UserData)userData;

            Undo.RecordObject(u.myProperty.serializedObject.targetObject, $"Add Override to {u.myProperty.serializedObject.targetObject.name}");
            u.myTarget.Overrides[(int)u.num] = true;
            Undo.RegisterCompleteObjectUndo(u.myProperty.serializedObject.targetObject, $"Add Override to {u.myProperty.serializedObject.targetObject.name}");
        }

        private void OnOverrideDeleted(object userData)
        {
            UserData u = (UserData)userData;

            Undo.RecordObject(u.myProperty.serializedObject.targetObject, $"Delete Override to {u.myProperty.serializedObject.targetObject.name}");
            u.myTarget.Overrides[(int)u.num] = false;
        }

        #endregion Overrides menu

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect pos = position;
            pos.height = EditorGUIUtility.singleLineHeight;  // Taking position with single line height

            #region Drawing Header

            property.isExpanded = EditorGUI.Foldout(pos, property.isExpanded, label, true);

            if (!property.isExpanded)
                return;
            else
            {
                ExpandLine(ref pos);
                pos.width -= 10;
                pos.x += 10;
            }

            #endregion Drawing Header

            // Taking actual object property
            var myTarget = EditorDrawUtility.GetObject<TimelineAssetOverride>(fieldInfo, property);

            SerializedProperty timelineAssetProp = property.FindPropertyRelative("timelineAsset");
            EditorGUI.PropertyField(pos, timelineAssetProp);
            property.serializedObject.ApplyModifiedProperties();
            if (timelineAssetProp.objectReferenceValue == null)
            {
                return;
            }

            ExpandLine(ref pos);

            TimelineAsset timelineRef = myTarget.timelineAsset;
            var outputs = timelineRef.GetOutputTracks().ToList();

            SerializedProperty bindingsProperty = property.FindPropertyRelative("Bindings");
            if (myTarget.timelineAsset != myTarget.oldtimeline || outputs.Count != bindingsProperty.arraySize)
            {
                myTarget.Bindings = new UnityEngine.Object[outputs.Count];
                myTarget.Overrides = new bool[outputs.Count];
            }

            myTarget.oldtimeline = myTarget.timelineAsset;

            if (myTarget.Overrides.Length != myTarget.Bindings.Length)
            {
                property.FindPropertyRelative("Overrides").arraySize = myTarget.Bindings.Length;
            }

            #region Buttons

            Rect buttonPos = new Rect(pos);
            buttonPos.width = 50;
            buttonPos.x += position.width - 110;

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.richText = true;

            if (GUI.Button(buttonPos, new GUIContent("<b>+</b>"), style))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < outputs.Count; i++)
                {
                    UserData newData = new UserData { myTarget = myTarget, num = i, myProperty = property };
                    if (!myTarget.Overrides[i])
                        AddMenuItemForOverride(menu, $"({i}) {outputs[i].GetType().Name}", newData);
                }

                menu.ShowAsContext();
            }

            buttonPos.x += 50;
            if (GUI.Button(buttonPos, new GUIContent("<b>-</b>"), style))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < outputs.Count; i++)
                {
                    UserData newData = new UserData { myTarget = myTarget, num = i, myProperty = property };

                    if (myTarget.Overrides[i])
                        RemoveMenuItemForOverride(menu, $"({i}) {outputs[i].GetType().Name}", newData);
                }

                menu.ShowAsContext();
            }

            #endregion Buttons

            ExpandLine(ref pos);
            pos.y += 5;

            for (int i = 0; i < outputs.Count; i++)
            {
                if (myTarget.Overrides[i])
                {
                    SerializedProperty element = bindingsProperty.GetArrayElementAtIndex(i);

                    Rect poss = new Rect(pos);
                    poss.width -= 2;
                    var referenceValue = element.objectReferenceValue;
                    DrawPropertyOfTimelineType(outputs[i].GetType(), ref referenceValue, poss, i);
                    element.objectReferenceValue = referenceValue;
                    ExpandLine(ref pos);
                    pos.y += 1;

                    element.serializedObject.ApplyModifiedProperties();
                }
            }

            property.serializedObject.Update();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var timelineRef = (TimelineAsset)property.FindPropertyRelative("timelineAsset").objectReferenceValue;
            if (timelineRef == null)
                return EditorGUIUtility.singleLineHeight * 2;

            int overrided = 0;
            var myTarget = EditorDrawUtility.GetObject<TimelineAssetOverride>(fieldInfo, property);

            for (int i = 0; i < myTarget.Overrides.Length; i++)
            {
                if (myTarget.Overrides[i])
                    overrided++;
            }
            return EditorGUIUtility.singleLineHeight * (overrided + 3.5f) + 5;
        }

        public void ExpandLine(ref Rect pos)
        {
            pos.y += EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Draws property field based on timeline track type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="pos"></param>
        /// <param name="num"></param>
        private void DrawPropertyOfTimelineType(Type type, ref UnityEngine.Object obj, Rect pos, int num)
        {
#pragma warning disable CS0618 // Тип или член устарел

            if (type == typeof(ActivationTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Activation Track"), obj, typeof(GameObject));
            else
            if (type == typeof(AudioTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Audio Track"), obj, typeof(AudioSource));
            else
            if (type == typeof(AnimationTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Animation Track"), obj, typeof(Animator));
            else
                // These are from Standard Timelines pack from unity
          /*  if (type == typeof(LightControlTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Light Control Track"), obj, typeof(LightControlTrack));
            else
            if (type == typeof(TransformTweenTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Transform Tween Track"), obj, typeof(Transform));
            else
            if (type == typeof(TimeControllerTrack) || type == typeof(TimeDilationTrack))
                EditorGUILayout.LabelField($"({num}) Time Control Track");
            else
            if (type == typeof(ScreenFaderTrack))
                obj = EditorGUI.ObjectField(pos, new GUIContent($"({num}) Screen Fader Track"), obj, typeof(UnityEngine.UI.Image));
            else*/
                EditorGUI.LabelField(pos, $"({num}) Track type is not supported! ({type.Name})");

#pragma warning restore CS0618 // Тип или член устарел
        }
    }
}

public static class EditorDrawUtility
{
    public static void ExpandLine(ref Rect r)
    {
        r.y += EditorGUIUtility.singleLineHeight;
    }

    public static T GetObject<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
    {
        var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
        if (obj == null) { return null; }

        T actualObject = null;
        if (obj.GetType().IsArray)
        {
            var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
            actualObject = ((T[])obj)[index];
        }
        else
        {
            actualObject = obj as T;
        }
        return actualObject;
    }
}
