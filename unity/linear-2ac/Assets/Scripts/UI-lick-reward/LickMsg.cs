using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LickMsg : MonoBehaviour
{
    public float destroyTime = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

}
