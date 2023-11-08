using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;
using System.Threading.Tasks;
public class Session : MonoBehaviour
{
    public TaskConfiguration taskConfig;
    public Protocol protocol;
    public MQTTHandler mqtt;
    public UIControl ui;
    public TeleportControl teleport;
    public Log log;
    [HideInInspector]
    public SessionSettings settings;

    private SessionSettings.TaskPeriod period;
    private int cPeriod = -1;
    private float waitForStartDelay = 10f; // Wait this long before starting.
    private float passedTime = 0f;

    private void Start()
    {
        settings = taskConfig.sessionSettings;
        ApplySettings();
        ActivateEnvironment(taskConfig.environment);
        ui.UpdateSettingsWindow();
        protocol.actor.isActive = false;
    }

    void ApplySettings()
    {
        teleport.lockOnTeleport = settings.lockOnTeleport;
        protocol.settings.useLeftRightIndicators = settings.useLeftRightIndicators;
        protocol.settings.freezeOnIncorrect = settings.freezeOnIncorrect;
        protocol.settings.playFreezeSound = settings.playFreezeSound;
        protocol.settings.allowMultipleResponses = settings.allowMultipleResponses;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we have started.
        if (period == null) { 
            passedTime += Time.deltaTime;
            ui.UpdateStartCountDown(waitForStartDelay - passedTime);
            // Wait for start 
            if (passedTime > waitForStartDelay) { ui.StartSessionTimer(settings); NextPeriod(); } }
        else
        {
            // Check if timer is still running
            if (period.stopwatch.IsRunning)
            {
                // update time.
                ui.SetSessionText(period);
                // Check if we reached the end.
                if (period.RemainingMSec() <= 0)
                {
                    // go to next period if actor is not teleporting (safety)
                    if (teleport.isTeleporting == false) { NextPeriod(); }
                }
            }
        }
    }

    private async void NextPeriod()
    {
        // stop actor.
        protocol.actor.isActive = false;
        // Wrap up previous period.
        EndPeriod();
        // Test for next period.
        cPeriod++;
        if (cPeriod <= settings.periods.Length - 1)
        {
            period = settings.periods[cPeriod];
            // Add some logging here.
            switch (period.periodType)
            {
                case SessionSettings.TaskPeriodType.TASK:
                    {
                        StartTaskPeriod();
                        break;
                    }

                case SessionSettings.TaskPeriodType.DARK:
                    {
                        await StartDarkPeriod();
                        break;
                    }
            }
            log.StartPeriod(period);
            // Start stopwatch.
            period.stopwatch.Restart();
            ui.sessionStopwatch.Start();
        }
        // Last period done: end session.
        else { mqtt.StopSession(); }
    }
    void EndPeriod()
    {
        if (period != null)
        {
            // stop stopwatch.
            period.stopwatch.Stop();
            ui.sessionStopwatch.Stop();
            // Wrap up previous task.
            switch (period.periodType)
            {
                case SessionSettings.TaskPeriodType.TASK:
                    {
                        protocol.FinishTrial();
                        break;
                    }
                case SessionSettings.TaskPeriodType.DARK:
                    {
                        EndDarkPeriod();
                        break;
                    }
            }
            log.EndPeriod(period);
            // Reset autotrainer.
            protocol.autoTrainer.Reset();
            ui.UpdateTrainerWindow();
        }
    }
    void StartTaskPeriod()
    {
        protocol.cueSet = period.cueSet;
        protocol.settings.isGuided = period.isGuided;
        protocol.StartTrial();
    }

    // Start a new dark period.
    private async Task StartDarkPeriod()
    {
        // freeze actor.
        protocol.actor.isActive = false;
        ui.SetDarkNotification(true);
        if (taskConfig.sessionSettings.lockOnDark) { mqtt.LockOn(); }
        else { mqtt.LockOff(); }
        // fade in 
        await protocol.actor.FadeOut(protocol.actor.display, 3000f);
        protocol.actor.Teleport(new Vector3(0, 0, 0), 0, true, false);
        protocol.actor.isActive = false;
    }
    private void EndDarkPeriod()
    {
        // Actor will become active again at start of trial.
        ui.SetDarkNotification(false);
        mqtt.LockOff();
    }

    // Activates selected environment and turns off others.
    private void ActivateEnvironment(Environment selectedEnv)
    {
        foreach (Environment env in Resources.FindObjectsOfTypeAll(typeof(Environment)) as Environment[])
        {
            if (env == selectedEnv) {
                env.gameObject.SetActive(true);
                protocol.posCues = env.posCues;
                protocol.stopCue = env.stopCue;
                log.Environment(env); 
            }
            else { env.gameObject.SetActive(false); }
        }
    }
}