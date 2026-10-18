using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordSpreaderForFun : BaseMono
{
    [SerializeField] Material newMaterialApplyer;

    [SerializeField] string namingSuffix;

    [SerializeField] List<MeleeParentScript> genPres;

    [SerializeField] Transform emptyParent;

    [SerializeField] float alotedMass = 10f, distance = 2f;

    [SerializeField] int sideLength = 10;

    [SerializeField] bool matchingRows, doEnchantment, overrideColorsInGens;
    //List<MeleeParentScript> gens = new List<MeleeParentScript>();

    [SerializeField] List<Color> PossibleBladeColorMixers = new List<Color>(),
        PossibleHiltColorMixers= new List<Color>(),
        PossibleHanldeColorMixers = new List<Color>(),
        PossibleEnchantColorMixers = new List<Color>();

    [SerializeField] bool hardOverrideColorForReColoring = false;

    [SerializeField] Color overrideBladeColor = Color.red, overrideHiltColor = Color.green, overrideHandleColor = Color.blue, overrideEnchantColor = Color.black;

    public override void Begin()
    {
        base.Begin();

        overrideBladeColor = new Color(1, 0, 0, 0);
        overrideHiltColor = new Color(0, 1, 0, 0);
        overrideHandleColor = new Color(0, 0, 1, 0);
        overrideEnchantColor = new Color(0, 0, 0, 1);

        for (int i = 0; i < genPres.Count; i++)
        {
            genPres[i].gameObject.SetActive(true);
        }
        int inde = 0;

        for(int x = 0; x < sideLength; x++)
        {
            int randSe = randSeedQQQ();
            int randColSe = randSeedQQQ();
            int randMagSe = randSeedQQQ();
            for (int y = 0; y < genPres.Count; y++)
            {

                MeleeParentScript gen = Instantiate(genPres[inde], transform);
                gen.transform.position = Vector3.zero;
                if(!matchingRows)
                {
                    randSe = randSeedQQQ();
                    randColSe = randSeedQQQ();
                    randMagSe = randSeedQQQ();
                }

                gen.seed = randSe;
                gen.colorSeed = randColSe;
                if(gen.mparamHolder != null)
                {
                    

                    gen.mparamHolder.seed = randMagSe;

                }

                if(!doEnchantment)
                {
                    gen.mparamHolder = null;
                }

                gen.alotedMass = alotedMass;

                if (overrideColorsInGens || hardOverrideColorForReColoring)
                {
                    Color blade, hilt, handle, enchant;
                    CreateColors(randColSe, out blade, out hilt, out handle, out enchant);

                    gen.colorOverride = true;
                    gen.bladeColor = blade;
                    gen.hiltColor = hilt;
                    gen.handleColor = handle;
                    gen.enchantmentColor = enchant;

                    if (doEnchantment)
                    {
                        if(gen.mparamHolder != null)
                        {
                            gen.mparamHolder.spellColor = enchant;
                            gen.mparamHolder.randomizeColorB = false;
                        }
                        
                    }

                }

                gen.MetalMaterial = newMaterialApplyer;

                gen.Begin();

                Transform t = gen.Final.transform;

                t.parent = emptyParent;
                t.localPosition = new Vector3(x * distance, 0, y * distance);

                t.name += namingSuffix + "." + x + "."+y;

                Destroy(gen.gameObject);
                //gen.gameObject.SetActive(false);

                inde++;
                if(inde >= genPres.Count)
                {
                    inde = 0;
                }

            }
        }

        for(int i = 0; i < genPres.Count; i++)
        {
            genPres[i].gameObject.SetActive(false);
        }

    }


    int randSeedQQQ()
    {

        return (int)subrandom(-999999999, 999999999);

    }

    void CreateColors(int toUseSeed, out Color bladeColor, out Color hiltColor, out Color handleColor, out Color enchantCol)
    {
        int realSeed = seed;
        int realUsedSeed = usedSeed;
        int realRandIndex = randomIndex;

        seed = toUseSeed;
        usedSeed = toUseSeed;
        randomIndex = 0;


        if (hardOverrideColorForReColoring)
        {
            bladeColor = overrideBladeColor;
            hiltColor = overrideHiltColor;
            handleColor = overrideHandleColor;
            enchantCol = overrideEnchantColor;
        }
        else
        {

            bladeColor = mixTwoColors(PossibleBladeColorMixers[(int)subrandom(0, PossibleBladeColorMixers.Count)],
                PossibleBladeColorMixers[(int)subrandom(0, PossibleBladeColorMixers.Count)]);

            hiltColor = mixTwoColors(PossibleHiltColorMixers[(int)subrandom(0, PossibleHiltColorMixers.Count)],
                PossibleHiltColorMixers[(int)subrandom(0, PossibleHiltColorMixers.Count)]);

            handleColor = mixTwoColors(PossibleHanldeColorMixers[(int)subrandom(0, PossibleHanldeColorMixers.Count)],
                PossibleHanldeColorMixers[(int)subrandom(0, PossibleHanldeColorMixers.Count)]);

            enchantCol = mixTwoColors(PossibleEnchantColorMixers[(int)subrandom(0, PossibleEnchantColorMixers.Count)],
                PossibleEnchantColorMixers[(int)subrandom(0, PossibleEnchantColorMixers.Count)]);

        }

        seed = realSeed;
        usedSeed = realUsedSeed;
        randomIndex = realRandIndex;


    }

    Color mixTwoColors(Color col1, Color col2)
    {

        Color main = Color.Lerp(col1, col2, subrandom(0f, 1f));
        return main;
    }

}
