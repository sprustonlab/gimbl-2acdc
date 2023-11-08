using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Gimbl;

public class Dash : MonoBehaviour
{
    private MQTTChannel<TrialDashMsg> trial;           // Subscribed to "Dash/Trial".
    private MQTTChannel<StartDashMsg> start;           // Subscribed to "Dash/Start".

    public class TrialDashMsg
    {
        public int trial_num;
        public bool is_guided;
        public string status;
        public string condition;
        public string cue_set;
    }
    private TrialDashMsg trialDashMsg = new TrialDashMsg();
    public class StartDashMsg
    {
        public string name;
    }
    private StartDashMsg startDashMsg = new StartDashMsg();
    void Start()
    {
        trial = new MQTTChannel<TrialDashMsg>("Dash/Trial/");
        start = new MQTTChannel<StartDashMsg>("Dash/Start/");
        // Send start message.
        SendStart();
    }
    private void SendStart()
    {
        startDashMsg.name = Path.GetFileNameWithoutExtension(FindObjectOfType<LoggerObject>().logFile.filePath);
        start.Send(startDashMsg);
    }

    public void TrialComplete(int trialNum, bool isGuided, string status, string condition, string set)
    {
        trialDashMsg.trial_num = trialNum;
        trialDashMsg.is_guided = isGuided;
        trialDashMsg.status = status;
        trialDashMsg.condition = condition;
        trialDashMsg.cue_set = set;
        trial.Send(trialDashMsg);
    }
}
