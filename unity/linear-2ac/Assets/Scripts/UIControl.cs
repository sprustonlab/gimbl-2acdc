using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class UIControl : MonoBehaviour
{
    public GameObject teleportNotification;
    public GameObject darkNotification;
    // Text. 
    public Text currentPeriodText;
    public Text periodTimerText;
    public Text sessionTimerText;
    public Text successRateText;
    public Text trialText;
    public Text cueSetText;
    public Text startCountDown;
    public Text t1CounterText;
    public Text t2CounterText;
    public Text guidedCountText;
    // Icons.
    public Texture PosCheck;
    public Texture NegCheck;
    public Texture greenBubble;
    public Texture blueBubble;
    public Texture redBubble;
    public Texture grayBubble;
    // Images
    public RawImage rewardImg;
    public RawImage isGuidedImg;
    public RawImage activeTrainerImg;
    // Performance.
    private int numSuccess = 0;
    public RawImage[] bubbles = new RawImage[20];
    private Protocol.TrialStatus[] prevTrials = new Protocol.TrialStatus[20]; // stores performance log.
    // Tracking.
    public Protocol protocol;
    private Protocol.Settings prevSettings;
    // timer
    private TimeSpan ts;
    public System.Diagnostics.Stopwatch sessionStopwatch =  new System.Diagnostics.Stopwatch();
    private TimeSpan sessionDuration;
    public void Update()
    {
        // test if settings have changed.
        if (prevSettings.Equals(protocol.settings) == false) { prevSettings = protocol.settings; UpdateSettingsWindow(); }
        // update session timer.
        UpdateTimeRemaining();
    }
    public void UpdateStartCountDown(float timeLeft)
    {
        if (timeLeft>0)
        {
            startCountDown.gameObject.SetActive(true);
            startCountDown.text = $"{Mathf.RoundToInt(timeLeft)}";
        }
        else { startCountDown.gameObject.SetActive(false); }
    }
    public void SwitchGuided() {protocol.settings.isGuided = protocol.settings.isGuided ? false : true; UpdateSettingsWindow(); }
    public void SwitchTrainer() { protocol.autoTrainer.isActive = protocol.autoTrainer.isActive ? false : true; UpdateTrainerWindow(); }
    public void SetTeleportNotification(bool enable) { if (darkNotification.activeSelf == false) { teleportNotification.SetActive(enable); } }
    public void SetDarkNotification(bool enable) { darkNotification.SetActive(enable); }
    public void SetTrialText(int trialCounter, string cueSet, Texture rewardTex) 
    {
        trialText.text = $"Trial {trialCounter}";
        cueSetText.text = $"{cueSet}";
        rewardImg.texture = rewardTex;
    }
    public void SetPerformance(Protocol.TrialStatus status, int numLaps) 
    {
        // Track success rate.
        if (status == Protocol.TrialStatus.CORRECT) { numSuccess++; }
        if (numLaps > 0) { successRateText.text = $"Success rate: {(((float)numSuccess / (float)numLaps) * 100):##0.00}%"; }
        else { successRateText.text = $"Success rate:"; }
        // put performance in array
        for (int i = 18; i >= 0; i--) { prevTrials[i + 1] = prevTrials[i]; }
        prevTrials[0] = status;
        // set buble colors.
        int counter = 0;
        foreach (RawImage bubble in bubbles)
        {
            if (prevTrials[counter] == Protocol.TrialStatus.IN_PROGRESS) { bubble.texture = grayBubble; }
            if (prevTrials[counter] == Protocol.TrialStatus.NO_RESPONSE) { bubble.texture = blueBubble; }
            if (prevTrials[counter] == Protocol.TrialStatus.CORRECT) { bubble.texture = greenBubble; }
            if (prevTrials[counter] == Protocol.TrialStatus.INCORRECT) { bubble.texture = redBubble; }
            counter++;
        }
    }
    public void UpdateSettingsWindow()
    {
        isGuidedImg.texture = prevSettings.isGuided ? PosCheck : NegCheck;
    }
    public void SetSessionText(SessionSettings.TaskPeriod period) 
    {
        ts = TimeSpan.FromMilliseconds(period.RemainingMSec());
        switch (period.periodType)
        {
            case SessionSettings.TaskPeriodType.DARK:
                {
                    currentPeriodText.text = "Dark";
                    break;
                }

            case SessionSettings.TaskPeriodType.TASK:
                {
                    currentPeriodText.text = $"{period.cueSet.name}";
                    break;
                }
        }
        periodTimerText.text = $"{ts:hh\\:mm\\:ss}";
        if (period.RemainingMSec() <= 10000) { periodTimerText.color = new Color(0.8f, 0.0f, 0.0f); }
        else { periodTimerText.color = new Color(0.2f, 0.2f, 0.2f); }
    }

    public void StartSessionTimer(SessionSettings settings)
    {
        // get total duration
        foreach (SessionSettings.TaskPeriod period in settings.periods) { sessionDuration += TimeSpan.FromMinutes(period.duration); }
        // start timer.
        sessionStopwatch.Restart();
    }

    private void UpdateTimeRemaining()
    // Controls total session time.
    {
        ts = sessionDuration - sessionStopwatch.Elapsed;
        sessionTimerText.text = (ts.TotalSeconds >= 0) ? $"{ts:hh\\:mm\\:ss}" : "00:00:00";
        sessionTimerText.color = (ts.TotalSeconds < 10f) ? new Color(0.8f, 0.0f, 0.0f) : new Color(0.2f, 0.2f, 0.2f); ;
    }

    public void UpdateTrainerWindow()
    {
        // Active indicator button.
        activeTrainerImg.texture = protocol.autoTrainer.isActive ? PosCheck : NegCheck;
        // reset counts if inactive.
        if (protocol.autoTrainer.isActive == false) { protocol.autoTrainer.Reset(); }
        // Text.
        t1CounterText.text = $"T1: {protocol.autoTrainer.t1Count}/{protocol.autoTrainer.threshold}";
        t2CounterText.text = $"T2: {protocol.autoTrainer.t2Count}/{protocol.autoTrainer.threshold}";
        if (protocol.autoTrainer.turnedOnGuided & protocol.autoTrainer.isActive)
        {
            guidedCountText.gameObject.SetActive(true);
            guidedCountText.text = $"Guided\nLeft: {protocol.autoTrainer.numGuided - protocol.autoTrainer.guidedCount}";
        }
        else
        {
            guidedCountText.gameObject.SetActive(false);
            guidedCountText.text = $"Guided\nLeft: 0";
        }
    }


}
