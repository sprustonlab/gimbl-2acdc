using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoResponse : MonoBehaviour
{

    // Triggered on entering cue zone.
    private void OnTriggerEnter(Collider other) {GetComponentInParent<PositionCue>().AutoResponse(); }
}
