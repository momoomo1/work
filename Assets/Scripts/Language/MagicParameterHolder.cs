using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MagicParameterHolder : BaseMono
{
    public string generatedSpellInfo;
    public bool isPatternBased, randomizeColorB;
    [ColorUsage(true, true)]
    public Color spellColor;
    public float colorMult = 2;
    //input real variables here

    public override void Begin()
    {
        base.Begin();

        int realseed = seed, realUsedSeed = usedSeed, realRandIndex = randomIndex;

        if (randomizeColorB)
        {
            spellColor.r = subrandom(0, 1f);
            spellColor.g = subrandom(0, 1f);
            spellColor.b = subrandom(0, 1f);

        }


        spellColor.r *= colorMult;
        spellColor.g *= colorMult;
        spellColor.b *= colorMult;

        seed = realseed; usedSeed = realUsedSeed; randomIndex = realRandIndex;

        isPatternBased = decider5050();

        //compile real variables and output a string

    }

}
