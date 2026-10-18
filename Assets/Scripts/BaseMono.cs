using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMono : MonoBehaviour
{
    public bool isTesting;
    public int seed;
    public int usedSeed;
    public int randomIndex;

    public GameObject EmptyMeshPreAAA;

    public void Start()
    {
        if (isTesting)
        {
            Begin();
        }
    }

    public virtual void Begin()
    {
        randomIndex = 0;
        usedSeed = seed;
        usedSeed = (int)subrandom(0, 999999999);

    }

    public float subrandom(float min, float max)
    {
        Random.InitState(usedSeed-randomIndex);
        randomIndex = Random.Range(0, 999999999);

        float rand = Random.Range(min, max);


        return rand;

    }
    public bool decider5050()
    {
        bool dec = false;
        if(subrandom(0f,1f) >= .5f)
        {
            dec = true;
        }
        return dec;
    }
}
