using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrayOutZone : MonoBehaviour
{
    public Protocol protocol;

    private void OnTriggerEnter(Collider other)
    {
        // reset textures.
        protocol.SetCueTextures();
    }
    private void OnTriggerExit(Collider other)
    {
        // reset textures.
        protocol.SetCueTextures(false);
    }
}