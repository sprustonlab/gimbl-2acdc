using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;
public class Log : MonoBehaviour
{
    public Protocol protocol;
    public TeleportControl teleportControl;
    public RewardIdScheduler rewardIdScheduler;

    private LoggerObject logger;         // Holds instance of logger object.

    public class SettingsMsg
    {
        public bool isGuided;
        public bool allowMultipleResponses;
        public bool useLeftRightIndicators;
        public bool freezeOnIncorrect;
        public bool playFreezeSound;
        public float freezeIncorrectDuration;
        public float teleportDarkDuration;
        public float incorrectDarkDuration;
        public float teleportFadeDuration;
        public bool lockOnTeleport;
        public float poissonLambda;
        public int maxRepeats;

        public void ReadSettings(Protocol.Settings settings, TeleportControl teleportControl, RewardIdScheduler scheduler)
        {
            isGuided = settings.isGuided;
            allowMultipleResponses = settings.allowMultipleResponses;
            useLeftRightIndicators = settings.useLeftRightIndicators;
            freezeOnIncorrect = settings.freezeOnIncorrect;
            playFreezeSound = settings.playFreezeSound;
            freezeIncorrectDuration = settings.freezeIncorrectDuration;
            teleportDarkDuration = teleportControl.teleportDarkDuration;
            incorrectDarkDuration = teleportControl.incorrectDarkDuration;
            teleportFadeDuration = teleportControl.teleportFadeDuration;
            lockOnTeleport = teleportControl.lockOnTeleport;
            poissonLambda = scheduler.poissonLambda;
            maxRepeats = scheduler.maxRepeats;
        }
        public bool Equals(SettingsMsg other)
        {
            return this.isGuided == other.isGuided & this.allowMultipleResponses == other.allowMultipleResponses &
                this.useLeftRightIndicators == other.useLeftRightIndicators &
                this.freezeOnIncorrect == other.freezeOnIncorrect & this.playFreezeSound == other.playFreezeSound &
                this.freezeIncorrectDuration == other.freezeIncorrectDuration & this.teleportDarkDuration == other.teleportDarkDuration &
                this.incorrectDarkDuration == other.incorrectDarkDuration & this.teleportFadeDuration == other.teleportFadeDuration &
                this.lockOnTeleport == other.lockOnTeleport & this.poissonLambda == other.poissonLambda & 
                this.maxRepeats==other.maxRepeats;
        }
    }
    private SettingsMsg prevSettingsMsg = new SettingsMsg();
    private SettingsMsg newSettingsMsg = new SettingsMsg();
    public class StartTrialMsg
    {
        public int trialNum;
        public string rewardSet;
        public int rewardingCueId;
    }
    private StartTrialMsg startTrialMsg = new StartTrialMsg();
    public class EndTrialMsg
    {
        public int trialNum;
        public string status;
    }
    public EndTrialMsg endTrialMsg = new EndTrialMsg();
    public class PeriodMsg
    {
        public string type;
        public string cueSet;
        public int duration;
        public bool isGuided;
    }
    public PeriodMsg periodMsg = new PeriodMsg();
    public class BallMsg
    {
        public bool status;
    }
    public BallMsg ballMsg = new BallMsg();

    public class EnvironmentMsg
    {
        public string name;
        public int indicatorPosition;
        public int indicatorSize;
        public int[] rewPositions;
        public int[] rewSizes;
        public int stopPosition;
        public int grayZonePosition;
        public int grayZoneSize;
    }
    public EnvironmentMsg envMsg= new EnvironmentMsg();

    public class FreezeMsg
    {
        public float duration;
        public bool sound;
    }
    public FreezeMsg freezeMsg = new FreezeMsg();

    public class AutoTrainerMsg
    {
        public bool guided;
    }
    public AutoTrainerMsg autoTrainerMsg = new AutoTrainerMsg();

    void Start() { logger = FindObjectOfType<LoggerObject>(); }

    private void Update()
    {
        LogSettingsChanges();
    }

    private void LogSettingsChanges()
    {
        newSettingsMsg.ReadSettings(protocol.settings, teleportControl,rewardIdScheduler);
        if (!newSettingsMsg.Equals(prevSettingsMsg))
        {
            prevSettingsMsg.ReadSettings(protocol.settings, teleportControl,rewardIdScheduler);
            logger.logFile.Log("Settings", newSettingsMsg);
        }
    }
    public void Lick() { logger.logFile.Log("Lick"); }
    public void StartTrial(int trial, CueSet set, int rewardingCueId)
    {
        startTrialMsg.trialNum = trial;
        startTrialMsg.rewardSet = set.name;
        startTrialMsg.rewardingCueId = rewardingCueId;
        logger.logFile.Log("StartTrial", startTrialMsg);
    }
    public void EndTrial(int trial, Protocol.TrialStatus status)
    {
        endTrialMsg.trialNum = trial;
        endTrialMsg.status = status.ToString();
        logger.logFile.Log("EndTrial", endTrialMsg);
    }

    public void StartPeriod(SessionSettings.TaskPeriod period)
    {
        SetPeriodToMessage(period);
        logger.logFile.Log("StartPeriod", periodMsg);
    }
    public void EndPeriod(SessionSettings.TaskPeriod period)
    {
        SetPeriodToMessage(period);
        logger.logFile.Log("EndPeriod", periodMsg);
    }
    private void SetPeriodToMessage(SessionSettings.TaskPeriod period)
    {
        periodMsg.type = period.periodType.ToString();
        periodMsg.cueSet = period.cueSet ? period.cueSet.name : "NA";
        periodMsg.duration = (int)(period.duration * 1000f);
        periodMsg.isGuided = period.isGuided;
    }

    public void BallLock(bool status) { ballMsg.status = status; logger.logFile.Log("BallLock", ballMsg); }
    public void Environment(Environment env) 
    {
        envMsg.name = env.name;
        envMsg.indicatorPosition = (int)(env.Indicator.transform.position.z * 1000f);
        envMsg.indicatorSize = (int)(env.Indicator.GetComponentInChildren<Renderer>().bounds.size.z *1000f);
        List<int> positions = new List<int>();
        List<int> sizes = new List<int>();
        foreach (PositionCue cue in env.posCues)
        {
            positions.Add((int)(cue.transform.position.z * 1000f));
            sizes.Add((int)(cue.GetComponentInChildren<Renderer>().bounds.size.z * 1000f));
        }
        envMsg.rewPositions = positions.ToArray();
        envMsg.rewSizes = sizes.ToArray();
        envMsg.stopPosition = (int)(env.stopCue.GetComponentInChildren<BoxCollider>().bounds.min.z * 1000f);
        if (env.grayZone!=null)
        {
            envMsg.grayZonePosition = (int)(env.grayZone.transform.position.z * 1000f);
            envMsg.grayZoneSize = (int)(env.grayZone.GetComponentInChildren<Collider>().bounds.size.z * 1000f);
        }
        logger.logFile.Log("Environment", envMsg);
    }

    public void LogFreeze(float duration, bool sound)
    {
        freezeMsg.duration = duration;
        freezeMsg.sound = sound;
        logger.logFile.Log("Freeze", freezeMsg);
    }

    public void LogAutoTrainer(bool guided)
    {
        autoTrainerMsg.guided = guided;
        logger.logFile.Log("AutoTrainer", autoTrainerMsg);
    }
}
