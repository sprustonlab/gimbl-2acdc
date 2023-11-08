using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SessionSettings : MonoBehaviour
{
    public enum TaskPeriodType { DARK, TASK, }
    [System.Serializable]
    public class TaskPeriod
    {
        public TaskPeriodType periodType;
        public float duration;
        public CueSet cueSet;
        public bool isGuided;
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public long RemainingMSec()
        {
            long remaining = (long)(duration * 60f * 1000f) - stopwatch.ElapsedMilliseconds;
            if (remaining < 0) { remaining = 0; }
            return remaining;
        }
    }

    [HideInInspector]
    public bool locked; // used for locking in GUI.
    public bool allowMultipleResponses=true;
    public bool lockOnDark=true;
    public bool lockOnTeleport;
    public bool useLeftRightIndicators;
    public bool freezeOnIncorrect;
    public bool playFreezeSound;
    public TaskPeriod[] periods;

}

[CustomEditor(typeof(SessionSettings))]
public class MyClassInspector : Editor
{
    public override void OnInspectorGUI()
    {
        SessionSettings targ = target as SessionSettings;
        targ.locked = EditorGUILayout.Toggle("Locked", targ.locked);
        GUI.enabled = !targ.locked;
        DrawDefaultInspector();
    }
}