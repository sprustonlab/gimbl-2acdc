using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class SoundManager : MonoBehaviour
{
    public enum SoundTask { ON, OFF, NONE }
    public SoundTask soundTask = SoundTask.NONE;
    public AudioSource freezeNoise;
    public void Update()
    {
        // Sounds must be played in main loop.
        if (soundTask == SoundTask.ON) { soundTask = SoundTask.NONE; freezeNoise.Play();  }
        if (soundTask == SoundTask.OFF) { soundTask = SoundTask.NONE; freezeNoise.Stop(); }
    }
    public void FreezeNoise(bool state) { if (state) { soundTask = SoundTask.ON; } else { soundTask = SoundTask.OFF; } }
}
