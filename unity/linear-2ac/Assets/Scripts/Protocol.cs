using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;
using System.Threading.Tasks;
public class Protocol : MonoBehaviour
{
    // Object links.
    public RewardIdScheduler rewardIdScheduler;
    public ActorObject actor;
    public TeleportControl teleport;
    public AutoTrainer autoTrainer;
    public UIControl ui;
    public Log log;
    public CueSet cueSet;
    public Dash dash;
    public MQTTHandler mqtt;
    public Material leftIndicator;
    public Material rightIndicator;
    public PositionCue[] posCues;
    public StopTrigger stopCue;
    public SoundManager soundManager;
    public Texture grayTex;

    // Task variables
    private int trialCounter = -1;
    private int rewardingCueId;
    private bool isWaitingForResponse = false;

    public enum TrialStatus { IN_PROGRESS, CORRECT, INCORRECT, NO_RESPONSE, }
    [HideInInspector]
    public TrialStatus status = TrialStatus.IN_PROGRESS;
    [System.Serializable]
    public struct Settings // struct since we want to copy values between prevsettings and current settings.
    {
        [HideInInspector]
        public bool isGuided;                   // If trial in guided.
        [HideInInspector]
        public bool useLeftRightIndicators;         // use L/R scheme
        [HideInInspector]
        public bool freezeOnIncorrect;          // freeze actor on incorrect.
        [HideInInspector]
        public bool playFreezeSound;
        [HideInInspector]
        public bool allowMultipleResponses;
        public float freezeIncorrectDuration;   // Duration of incorrect freeze in seconds.
        public bool Equals(Settings other)
        {
            return this.isGuided == other.isGuided;
        }
    }

    public Settings settings;

    public async void StartTrial()
    {
        Task teleportTask = teleport.TeleportToStart(status);
        SetReward();
        SetCueTextures();
        ResetPositionCues();
        trialCounter++;
        isWaitingForResponse = true;
        status = TrialStatus.IN_PROGRESS;
        ui.SetTrialText(trialCounter, cueSet.name, GetRewardingPositionTex());
        // wait for teleport completion
        await teleportTask;
        log.StartTrial(trialCounter, cueSet, rewardingCueId);
    }
    public void FinishTrial()
    {
        if (status == TrialStatus.IN_PROGRESS) { status = TrialStatus.NO_RESPONSE; }
        log.EndTrial(trialCounter, status);
        dash.TrialComplete(trialCounter, settings.isGuided, status.ToString(), rewardingCueId.ToString(), cueSet.name);
        ui.SetPerformance(status, trialCounter + 1);
        autoTrainer.ProcessEndTrial(rewardingCueId);
    }
    void SetReward() { rewardingCueId = rewardIdScheduler.NextCondition(); }
    void ResetPositionCues()
    {
        stopCue.hasResponded = false;
        foreach (PositionCue cue in posCues)
        {
            cue.hasResponded = false;
        }
    }
    public void SetCueTextures(bool isStart = true)
    {
        bool setGray = false;
        if (isStart) { setGray = FindObjectOfType<Environment>().grayCues; }// Look up if loaded environments uses gray cues
        if (setGray)
        {
            leftIndicator.mainTexture = grayTex;
            rightIndicator.mainTexture = grayTex;
        }
        else
        {
            // One indicator scenario
            if (settings.useLeftRightIndicators)
            {
                leftIndicator.mainTexture = GetRewardingTex();
                rightIndicator.mainTexture = GetNonRewardingTex();

            }
            // LEft/Right indicator scenario.
            else
            {
                leftIndicator.mainTexture = GetRewardingTex();
                rightIndicator.mainTexture = GetRewardingTex();
            }
        }
    }

    public void ResponseTrigger(int response)
    {
        if (isWaitingForResponse | settings.allowMultipleResponses)
        {
            // Correct response
            if (response == rewardingCueId) { Correct(); }
            // Incorrect response.
            if (response !=rewardingCueId & settings.isGuided==false) { Incorrect(); }
        }
    }

    public void Correct() 
    { 
        isWaitingForResponse = false;
        if (status == TrialStatus.IN_PROGRESS) { status = TrialStatus.CORRECT; } // only change status if none have been given.
        mqtt.SendReward(); 
    }

    public void Incorrect() 
    { 
        isWaitingForResponse = false;
        status = TrialStatus.INCORRECT;
        if (settings.freezeOnIncorrect) { FreezeActor(); }
    }

    private async void FreezeActor()
    {
        actor.isActive = false;
        log.LogFreeze(settings.freezeIncorrectDuration, settings.playFreezeSound);
        if (settings.playFreezeSound) { soundManager.FreezeNoise(true); }
        await Task.Delay((int)(settings.freezeIncorrectDuration*1000f));
        soundManager.FreezeNoise(false);
        actor.isActive = true;
    }

    // Returns current rewarding texture.
    private Texture GetRewardingPositionTex() { return posCues[rewardingCueId - 1].GetComponentInChildren<Renderer>().material.mainTexture; }
    private Texture GetRewardingTex() { return cueSet.indicatorCuesTex[rewardingCueId - 1]; }
    private Texture GetNonRewardingTex() {return cueSet.indicatorCuesTex[rewardingCueId == 2 ? 0 : 1]; }
}
