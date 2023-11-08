using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrainer : MonoBehaviour
{
    public Protocol protocol;
    public UIControl ui;
    [HideInInspector]
    public bool isActive = true;
    [HideInInspector]
    public PositionCue[] cues;

    [HideInInspector]
    public int t1Count = 0;
    [HideInInspector]
    public int t2Count = 0;
    [HideInInspector]
    public int guidedCount = 0;
    [HideInInspector]
    public bool turnedOnGuided = false;

    public int threshold = 3;
    public int numGuided = 5;


    // Start is called before the first frame update
    void Start()
    {
        cues = FindObjectsOfType<PositionCue>();
        protocol.ui.UpdateTrainerWindow();
    }

    public void ProcessEndTrial(int rewardingCueId)
    {
        if (turnedOnGuided)
        {
            guidedCount++;
            if (guidedCount>=numGuided) 
            {
                // Turn off guided.
                protocol.settings.isGuided = false;
                turnedOnGuided = false;
                guidedCount = 0;
                protocol.log.LogAutoTrainer(false);
            }
        }
        if (isActive)
        {
            // Check if reward was given
            bool wasRewarded = false;
            foreach (PositionCue cue in cues)
            {
                if ((cue.id == rewardingCueId) & (cue.hasResponded)) { wasRewarded = true; }
            }
            // Reset related counter in case of reward
            if (wasRewarded)
            {
                if (rewardingCueId == 1) { t1Count = 0; }
                if (rewardingCueId == 2) { t2Count = 0; }
            }
            // add to count in case of no reward.
            else
            {
                if (rewardingCueId == 1) { t1Count++; }
                if (rewardingCueId == 2) { t2Count++; }
            }

            // Check if threshold reached
            if (t1Count>=threshold | t2Count>=threshold)
            {
                // reset
                t1Count = 0;
                t2Count = 0;
                // Set to guided.
                protocol.settings.isGuided = true;
                turnedOnGuided = true;
                protocol.log.LogAutoTrainer(true);
            }
        }
        ui.UpdateTrainerWindow();
    }

    public void Reset()
    {
        turnedOnGuided = false;
        guidedCount = 0;
        t1Count = 0;
        t2Count = 0;
    }
}
