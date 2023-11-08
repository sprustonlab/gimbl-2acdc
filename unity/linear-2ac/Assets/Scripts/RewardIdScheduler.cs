using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardIdScheduler : MonoBehaviour
{
    public float poissonLambda = 0.7f;
    public int maxRepeats = 3;
    private int currentRewardId;
    private int currentRepeats = 0;
    private void Start()
    {
        SelectStartingArm();
    }

    private void SelectStartingArm() { currentRewardId = Random.value > 0.5 ? 1 : 2; }

    public int NextCondition()
    {
        // If done with repeats.
        if (currentRepeats == 0)
        {
            // switch arms.
            currentRewardId = currentRewardId == 1 ? 2 : 1;
            // new repeats.
            currentRepeats = NewRepeats();
        }
        currentRepeats--; // substract one repeat. 
        return currentRewardId;
    }

    private int NewRepeats()
    {
        // poisson sample
        int repeats = funPoissonSingle(poissonLambda) + 1; // +1 since we dont want zero repeats.
        if (repeats > maxRepeats) { repeats = maxRepeats; } // threshold.
        return repeats;
    }

    //Poisson function -- returns a single Poisson random variable
    private int funPoissonSingle(float lambda)
    {
        double exp_lambda = Mathf.Exp(-lambda); //constant for terminating loop
        double randUni; //uniform variable
        double prodUni; //product of uniform variables
        int randPoisson; //Poisson variable

        //initialize variables
        randPoisson = -1;
        prodUni = 1;
        do
        {
            randUni = Random.value; //generate uniform variable
            prodUni = prodUni * randUni; //update product
            randPoisson++; // increase Poisson variable

        } while (prodUni > exp_lambda);

        return randPoisson;
    }
}