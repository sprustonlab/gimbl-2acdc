using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gimbl;

public class LickRewardSpawner : MonoBehaviour
{
    public GameObject lickPrefab;
    public GameObject rewardPrefab;
    public Canvas canvas;
    // MQTT channels.
    private MQTTChannel lick;           // Subscribed to "LickPort/".
    private MQTTChannel reward;         // Subscribed to "Gimbl/Reward/"
    private bool showLick = false;      // Toggles lick msg creation.
    private bool showReward = false;    // Toggles reward msg creation.

    // Start is called before the first frame update
    void Start()
    {
        // Setup mqtt channel.
        lick = new MQTTChannel("LickPort/");
        lick.Event.AddListener(OnLick);
        reward = new MQTTChannel("Gimbl/Reward/");
        reward.Event.AddListener(OnReward);
    }

    void OnLick() { showLick = true; }
    void OnReward() { showReward = true; }
    private void CreateLickMsg()
    {
        showLick = false;
        Instantiate(lickPrefab, canvas.transform);
    }
    private void CreateRewardMsg()
    {
        showReward = false;
        Instantiate(rewardPrefab, canvas.transform);
    }
    // Update is called once per frame
    void Update()
    {
        if (showLick) { CreateLickMsg(); }
        if (showReward) { CreateRewardMsg(); }
    }
}
