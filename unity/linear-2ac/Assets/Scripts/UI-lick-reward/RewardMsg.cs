using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardMsg : MonoBehaviour
{
    public float destroyTime = 4.0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
