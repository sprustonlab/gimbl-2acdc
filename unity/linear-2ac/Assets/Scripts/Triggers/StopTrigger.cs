using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopTrigger : MonoBehaviour
{
    public Protocol protocol;
    public bool hasResponded = false;
    private void OnTriggerEnter(Collider other) { 
        if (hasResponded == false) {
            hasResponded = true;
            protocol.FinishTrial(); 
            protocol.StartTrial(); 
        }
    }
}
