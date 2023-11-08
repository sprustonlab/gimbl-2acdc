using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;
public class MQTTHandler : MonoBehaviour
{
    public Log log;
    // MQTT channels.
    private MQTTChannel lick;           // Subscribed to "LickPort/".
    private MQTTChannel reward;         // Subscribed to "Gimbl/Reward/"
    private MQTTChannel lockOn;         // Subscribed to "miniBCS/Lock/On"
    private MQTTChannel lockOff;        // Subscribed to "miniBCS/Lock/Off"
    private MQTTChannel stopSession;    // Subscribed to "Matlab/StopSession".
    // Lick Event.
    public delegate void Notify();  // delegate
    public event Notify LickEvent;

    // Start is called before the first frame update
    void Start()
    {
        // Setup mqtt channel.
        lick = new MQTTChannel("LickPort/");
        lick.Event.AddListener(OnLick);
        reward = new MQTTChannel("Gimbl/Reward/");
        lockOn = new MQTTChannel("miniBCS/Lock/On");
        lockOn.Event.AddListener(OnLockOn);
        lockOff = new MQTTChannel("miniBCS/Lock/Off");
        lockOff.Event.AddListener(OnLockOff);
        stopSession = new MQTTChannel("Matlab/StopSession");
    }

    public void StopSession() { stopSession.Send(); }
    public void SendReward() { reward.Send(); }
    public void OnLick() { log.Lick(); LickEvent.Invoke(); }
    public void LockOn() { lockOn.Send(); }
    public void LockOff() { lockOff.Send();}
    public void OnLockOn() { log.BallLock(true); }
    public void OnLockOff() { log.BallLock(false); }
}

