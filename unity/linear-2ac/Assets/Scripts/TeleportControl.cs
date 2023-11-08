
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;
using System.Threading;
using System.Threading.Tasks;
using System;
public class TeleportControl : MonoBehaviour
{

    public ActorObject actor;
    public MQTTHandler mqtt;
    public UIControl ui;
    public float teleportDarkDuration;
    public float incorrectDarkDuration;
    public float teleportFadeDuration;
    [HideInInspector]
    public bool lockOnTeleport;
    [HideInInspector]
    public bool isTeleporting;
    // Start is called before the first frame update
    void Awake() { actor.TeleportStatusChange += TeleportStatusChange; }
    public async Task TeleportToStart(Protocol.TrialStatus status)
    {
        // get dark duration depending on settings.
        float darkDuration = teleportDarkDuration;
        // adjust dark duration in case of wrong answer and teleportOnIncorrect on.
        if (status != Protocol.TrialStatus.CORRECT) { darkDuration = incorrectDarkDuration; }
        actor.Teleport(new Vector3(0, 0, 0), 0, true, true,
            (int)(darkDuration * 1000f), (int)(teleportFadeDuration * 1000f));
        // Set new completion source
        var teleportingTask = new TaskCompletionSource<bool>();
        EventHandler<ActorTeleportStatus> handler = (sender, teleportStatus) => { if (teleportStatus == ActorTeleportStatus.END) { teleportingTask.SetResult(true); } };
        actor.TeleportStatusChange += handler;
        await teleportingTask.Task;
        actor.TeleportStatusChange -= handler;
    }

    void TeleportStatusChange(object sender, ActorTeleportStatus teleportStatus)
    {
        // fade out.
        if (teleportStatus == ActorTeleportStatus.FADE_OUT)
        {
            isTeleporting = true;
            if (lockOnTeleport) { mqtt.LockOn(); }
            ui.SetTeleportNotification(true);
        }
        // Fade in
        if (teleportStatus == ActorTeleportStatus.FADE_IN) { mqtt.LockOff(); }
        // teleport done.
        if (teleportStatus == ActorTeleportStatus.END)
        {
            isTeleporting = false;
            ui.SetTeleportNotification(false);
        }
    }

}