using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class weaponGenerator : MonoBehaviour
{
    public int seed;
    int randomIndex = 0;

    //public Material inputCompareMat;

    public GameObject finalObj;
    public List<Material> matsForMetal;
    //types: 0=blank 
    //1= front edge //2= back edge
    //3= top edge // 4 = bottom edge
    //5 = front and top //6=front and bottom
    //7= back and top 8 = back and bottom

    [SerializeField] int xRes;
    [SerializeField] int yRes;

    [SerializeField] float minMetalMass, maxMetalMass;
    //[SerializeField] float minHandleLength, maxHandleLength;

    float metalMass;
    [SerializeField] float handleLength;


    int metalShapeCount = 0;
    [SerializeField] int minNumOfMetalShapes = 0, maxNumOfMetalShapes = 4, minNumOfClubSpikes, maxNumOfClubSpikes;

    [SerializeField] GameObject SphereBase;
    [SerializeField] GameObject CubeBase;
    [SerializeField] GameObject CylinderBase;

    [SerializeField] GameObject swordBladeBuilderPre;
    [SerializeField] GameObject axeBladeBuilderPre;
    [SerializeField] GameObject roundBluntBuilderPre;
    [SerializeField] GameObject flatBluntBuilderPre;
    [SerializeField] GameObject spikeBuilderPre;


    [SerializeField] GameObject handleBuilderPre;

    //[SerializeField] float minLengthForSidewaysSwordBlade;

    [SerializeField] float handleToWidthThickProportion = .5f;

    [SerializeField] float minWidthAndThickInProportionToHeight = .05f;

    [SerializeField] float minShortLength, maxShortLength;
    [SerializeField] float minMidLength, maxMidLength;
    [SerializeField] float minLongLength, maxLongLength;
    [Range(0,1f)]
    [SerializeField] float swordHandleToSwordLengthPercent = .1f;
    [SerializeField] float multipleBladesShrinkPercent = .25f;
    public bool isSingleBlade = false;

    [SerializeField] bool isTesting;

    // Start is called before the first frame update
    void Start()
    {
        if (isTesting)
            Begin();
    }

    public void Begin()
    {

        randomIndex = 0;
        GenerateRandomWeapon();
    }

    //damage type shape
    //blade
        //sword type
        //axe type
    //blunt
        //round face
        //flat face
    //spike

    void GenerateRandomWeapon()
    {


        float symDecider = subRandom(-1f, 1f);
        bool isSym = false;
        if (symDecider > 0)
        {
            isSym = true;
        }

        //float3 ribPos, st, up;

        ////hullCurveContainer.Evaluate(baseHullSpline, frontBalance, out ribPos, out st, out up);
        //hullCurveContainer.Evaluate(baseHullSpline, 1f, out ribPos, out st, out up);

        float decider100 = subRandom(0, 1f);
        if(decider100 > 0&&decider100 < .3f)
        {
            handleLength = subRandom(minShortLength, maxShortLength);

        }
        if (decider100 >= .3f && decider100 < .6f)
        {
            handleLength = subRandom(minMidLength, maxMidLength);

        }
        if (decider100 >= .6f)
        {
            handleLength = subRandom(minLongLength, maxLongLength);

        }


        float decider2 = subRandom(-1f, 1f);
        float decider3 = subRandom(-1f, 1f);
        if(decider3 > 0)
        {
            isSingleBlade = true;
        }

        if(decider2 > 0f)
        {
            handleLength = subRandom(minShortLength, maxShortLength);
        }

        metalMass = subRandom(minMetalMass, maxMetalMass);

        metalShapeCount = (int)subRandom(minNumOfMetalShapes, maxNumOfMetalShapes + 1);

        //metalMass *= metalShapeCount;

        if (isSingleBlade)
        {
            metalShapeCount = 1;
        }

        if(metalShapeCount > 3)
        {

            float decider12 = subRandom(-1f, 1f);
            if(decider12 > 0)
            {
                metalShapeCount = 5;
            }
            else
            {
                metalShapeCount = (int)subRandom(minNumOfClubSpikes,maxNumOfClubSpikes + 1);
            }

        }

        List<GameObject> metalShapeBuilders = new List<GameObject>();


        List<Transform> metalShapes = new List<Transform>();

        float widthToPick = 0, thickToPick = 0;

        float customBodyBalance = 0;

        bool hasPommel = false;

        if (metalShapeCount <= 3)
        {

            //damage type shape
            //blade
            //sword type
            //axe type
            //blunt
            //round face
            //flat face
            //spike

            //Material swordM;      0
            //Material flatFaceM;   1
            //Material roundFaceM;  2
            //Material spikeM;      3
            //Material axeM;        4

            float splitMass = metalMass;
            //float splitMass = metalMass / metalShapeCount;
            //float splitMass = metalMass - (metalMass*(multipleBladesShrinkPercent*(metalShapeCount-1)));


            List<int> picks = new List<int>();
            picks.Add(0);picks.Add(1);picks.Add(2);

            bool alreadyHasBlade = false;

            for(int i = 0; i < metalShapeCount; i++)
            {
                int MaterialType = 0;

                bool earlyCompletion = false;//for axe

                int dirPickInd = (int)subRandom(0, picks.Count);
                
                int dirPick = picks[dirPickInd];

                Vector3 dir = Vector3.up;

                if (dirPick == 1)
                    dir = Vector3.forward;
                if (dirPick == 2)
                    dir = -Vector3.forward;

                MetalShapeGen msg = null;


                //int decider = (int)subRandom(1, 5 + 1);
                int decider = (int)subRandom(1, 5+1);

                if (decider == 1)//sword type
                {

                    //if (alreadyHasBlade && handleLength < minMidLength)
                    //{
                    //    break;
                    //}

                    if(metalShapeCount == 1)
                    {
                        hasPommel = true;
                        handleLength = subRandom(minShortLength, maxShortLength);
                    }


                    if (handleLength < minLongLength)
                    {
                        dirPick = 0;
                        dir = Vector3.up;

                        
                    }

                    GameObject g = Instantiate(swordBladeBuilderPre, transform);
                    msg = g.GetComponent<MetalShapeGen>();

                    SwordBladeGen sbg = g.GetComponent<SwordBladeGen>();
                    sbg.blankMetalMat = matsForMetal[6];

                    alreadyHasBlade = true;

                    MaterialType = 0;

                }


                if (decider == 3)
                {
                    //flat face

                    GameObject g = Instantiate(flatBluntBuilderPre, transform);
                    msg = g.GetComponent<MetalShapeGen>();

                    MaterialType = 1;


                    if (metalShapeCount == 1)
                    {
                        dir = Vector3.forward;
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxMidLength);
                        }
                    }
                    else
                    {
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxLongLength);
                        }
                    }
                }

                if (decider ==4)
                {
                    //round face

                    GameObject g = Instantiate(roundBluntBuilderPre, transform);
                    msg = g.GetComponent<MetalShapeGen>();

                    //handleLength = subRandom(minLengthForSidewaysSwordBlade, maxHandleLength);

                    MaterialType = 2;


                    if (metalShapeCount == 1)
                    {
                        dir = Vector3.up;
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxMidLength);
                        }
                    }
                    else
                    {
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxLongLength);
                        }
                    }
                }

                if (decider == 5)
                {
                    //spike type
                    MaterialType = 3;

                    if (handleLength < minMidLength)
                    {
                        dirPick = 0;
                        dir = Vector3.up;
                    }

                    if (metalShapeCount == 1)
                    {
                        dir = Vector3.up;
                    }

                    GameObject g = Instantiate(spikeBuilderPre, transform);
                    msg = g.GetComponent<MetalShapeGen>();
                }


                if (decider == 2)
                {
                    //axe type

                    MaterialType = 4;


                    GameObject g = Instantiate(axeBladeBuilderPre, transform);
                    msg = g.GetComponent<MetalShapeGen>();

                    if (metalShapeCount == 1)
                    {
                        dir = Vector3.forward;
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxMidLength);
                        }
                    }
                    else
                    {
                        if (handleLength <= maxShortLength)
                        {
                            handleLength = subRandom(minMidLength, maxLongLength);
                        }
                    }

                    msg.seed = seed - i;

                    if (isSym)
                    {
                        msg.seed = seed;
                    }

                    msg.alotedMass = splitMass;
                    msg.Begin();

                    float a = g.GetComponent<axeBladeGen>().outPutStartWidth*2;


                    if (a > customBodyBalance)
                    {
                        customBodyBalance = a;
                    }

                    if (metalShapeCount == 1)
                    {
                        dir = Vector3.forward;
                    }

                    earlyCompletion = true;
                }

                if (!earlyCompletion)
                {

                    msg.seed = seed - i;

                    if (isSym)
                    {
                        msg.seed = seed;
                    }

                    msg.alotedMass = splitMass;
                    msg.Begin();
                }



                if (msg.width > widthToPick)
                {
                    widthToPick = msg.width;
                }
                if(msg.thick > thickToPick)
                {
                    thickToPick = msg.thick;
                }


                Transform t = Instantiate(msg.Final,finalObj.transform).transform;

                

                if (t.GetComponent<MeshRenderer>() != null)
                {
                    //filtersToAddToBe.Add(t.GetComponent<MeshFilter>());
                    t.GetComponent<MeshRenderer>().material = matsForMetal[MaterialType];
                }
                else
                {
                    MeshRenderer[] renderers = t.GetComponentsInChildren<MeshRenderer>();
                    renderers[0].material = matsForMetal[MaterialType];

                }


                t.localRotation = Quaternion.LookRotation(dir);

                picks.Remove(dirPick);

            }


        }
       
        if(thickToPick * handleToWidthThickProportion < handleLength * minWidthAndThickInProportionToHeight)
        {
            thickToPick = (handleLength * minWidthAndThickInProportionToHeight) / handleToWidthThickProportion;
        }
        if(widthToPick* handleToWidthThickProportion < handleLength *minWidthAndThickInProportionToHeight)
        {
            widthToPick = (handleLength * minWidthAndThickInProportionToHeight) / handleToWidthThickProportion;
        }

        //if (isSym)
        //{
        //    Transform mir = Instantiate(finalObj, finalObj.transform).transform;
        //    mir.localScale = new Vector3(1, 1, -1f);
        //}


        Vector3 handleScale = new Vector3(thickToPick*handleToWidthThickProportion,handleLength,widthToPick* handleToWidthThickProportion);

        handleGen handB = Instantiate(handleBuilderPre, transform).GetComponent<handleGen>();

        handB.pillarSeed = seed;
        handB.HandleMaterial = matsForMetal[5];
        handB.blankMetal = matsForMetal[2];
        handB.hasPommel = hasPommel;
        if (hasPommel)
        {
            handB.blankMetal = matsForMetal[6];

        }

        if (customBodyBalance > 0)
        {
            customBodyBalance = customBodyBalance / handleLength;
            customBodyBalance = 1f - customBodyBalance;
        }

        //Debug.Log(" custom balance " + customBodyBalance + " handle length " + handleLength);

        handB.bodyPercent = customBodyBalance;
        handB.Begin();

        Transform finalHandle = Instantiate(handB.FinalPillar, finalObj.transform).transform;
        finalHandle.localScale = handleScale;
        finalHandle.localEulerAngles = new Vector3(180, 0, 0);


    }



    #region qualityOf life


    float subRandom(float min, float max)
    {
        UnityEngine.Random.InitState(seed - randomIndex);

        randomIndex++;

        return UnityEngine.Random.Range(min, max);


    }


    #endregion


}
