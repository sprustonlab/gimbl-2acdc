using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCue : MonoBehaviour
{
    //object links.
    public Protocol protocol;
    public MQTTHandler mqtt;
    // task parameters.
    public int id;
    public bool inZone = false;
    public bool hasResponded = false;
    void Start()
    {
        // Subscribe to lick event.
        mqtt.LickEvent += OnLick;
    }
    private void OnLick()
    {
        if (inZone & hasResponded==false)
        {
            SendResponse();
        }
    }
    public void AutoResponse() { if (protocol.settings.isGuided & hasResponded==false) { SendResponse(); } }
    public void SendResponse() { hasResponded = true;  protocol.ResponseTrigger(id); }
    // Triggered on entering cue zone.
    private void OnTriggerEnter(Collider other) { inZone = true; }

    // Triggered on exitting cue zone.
    private void OnTriggerExit(Collider other) { inZone = false;}

}
