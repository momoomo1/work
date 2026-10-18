using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ImprovedCrystalGen : MeleeParentScript
{
    [SerializeField] SplineContainer forwardCircle;
    SplineContainer taporControl;
    //public Color overrideColor;

    [SerializeField] float maxSplineScaleVairPerc = .2f, maxRotVariance = 30f, minProfThickPerc = .2f, minClumpVariance = .2f, maxClumpVariance = 1.2f, 
        maxClumpOffsetVariance;

    [SerializeField] int minCrystalCount = 2, maxCrystalCount = 10, minCrystalSideCount = 3, maxCrystalSideCount = 9,
        minProfileCount = 3, maxProfileCount = 5, minTaporPointCount = 3, maxTaporPointCount = 5;
    int sideCount;

    public bool isGem;

    public override void Begin()
    {
        base.Begin();

        //isGem = decider5050();
        if (!isGem)
        {
            length *= 2f;
            width *= .75f;
            thick *= .75f;
        }


        //if (!overrideTheColor)
        //{
        //    RandomizeColors();
        //}


        sideCount = (int)subRandom(minCrystalSideCount, maxCrystalSideCount + 1);

        taporControl = makeTapor();

        AssembleCrystal();

    }

    void AssembleCrystal()
    {
        int crystalCount = 1;
        if (decider5050() && !isGem)
        {
            crystalCount = (int)subRandom(minCrystalCount, maxCrystalCount+1);
        }

        List<MeshFilter> filters = new List<MeshFilter>();

        bool doOffset = decider5050();

        for(int i = 0; i < crystalCount; i++)
        {
            float xRot = 0, yRot= 0, zRot = 0;
            if(crystalCount > 1)
            {

                xRot = subRandom(0, 360);
                yRot = subRandom(0, 360);
                zRot = subRandom(0, 360);

            }






            MeshFilter m = createIndividualCrystal(length, width, thick);
            m.transform.localEulerAngles = new Vector3(xRot, yRot, zRot);

            if(crystalCount > 1)
            {
                m.transform.localScale = Vector3.one * subRandom(minClumpVariance, maxClumpVariance);
                if(doOffset)
                {
                    m.transform.localPosition = new Vector3(subRandom(-1, 1f)*maxClumpOffsetVariance * width,
                    subRandom(-1, 1f) * maxClumpOffsetVariance * thick,
                    subRandom(-1, 1f) * maxClumpOffsetVariance * (width+thick)/2);
                }

            }

            if (!isGem)
            {
                m.transform.localScale = Vector3.one / 2f;

            }

            //m.transform.localPosition = Vector3.back * length / 2;



            filters.Add(m);
        }

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        MakeMeshShadeFlat(Final.GetComponent<MeshFilter>());

        ReColorAMesh(Final.GetComponent<MeshFilter>(), bladeColor);
        Final.GetComponent<MeshRenderer>().material = MetalMaterial;
    }


    MeshFilter createIndividualCrystal(float lengthToUse, float thickToUse, float widthToUse)
    {

        int profileCount = (int)subRandom(minProfileCount, maxProfileCount+1);

        bool consistantRot = decider5050();
        float xRotAmount = subRandom(-maxRotVariance, maxRotVariance);
        float yRotAmount = subRandom(-maxRotVariance, maxRotVariance);

        List<SplineContainer> containers = new List<SplineContainer>();

        SplineContainer Oscale = Instantiate(forwardCircle, transform);
        Oscale.transform.localPosition -= Vector3.forward * .5f * lengthToUse;

        Oscale.transform.localScale = Vector3.zero;

        containers.Add(Oscale);
        for (int i = 0;i < profileCount; i++)
        {
            float perc = i/(profileCount-1f);
            SplineContainer con = Instantiate(forwardCircle, transform);


            taporControl.Evaluate(taporControl[0], perc, out pos, out st, out up);

            con.transform.localScale = new Vector3(widthToUse * pos.y, thickToUse * pos.y, 1);

            Vector3 prevPos = containers[containers.Count-1].transform.localPosition;
            Vector3 prevScale = containers[containers.Count - 1].transform.localScale;

            float possPosOff = Mathf.Abs(prevScale.x - pos.x)/2;

            con.transform.localPosition = Vector3.forward * (perc * lengthToUse) + new Vector3(prevPos.x, prevPos.y,0);
            con.transform.localPosition -= Vector3.forward * .5f * lengthToUse;



            if (!isGem)
            {
                float xMin = subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * widthToUse;

                con.transform.localScale += new Vector3(minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * widthToUse, width / 20),
                    minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * thickToUse, thickToUse / 20),
                    minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * Mathf.Lerp(widthToUse, thickToUse, .5f), Mathf.Lerp(widthToUse, thickToUse, .5f) / 20));

                //con.transform.localScale += new Vector3(minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * widthToUse, 0),
                //    minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * thickToUse, 0),
                //    minCheck(subRandom(-maxSplineScaleVairPerc, maxSplineScaleVairPerc) * Mathf.Lerp(widthToUse, thickToUse, .5f), 0));

                if (!consistantRot)
                {
                    xRotAmount = subRandom(-maxRotVariance, maxRotVariance);
                    yRotAmount = subRandom(-maxRotVariance, maxRotVariance);
                }
                
                con.transform.localEulerAngles = new Vector3(xRotAmount, yRotAmount,0);

                con.transform.localPosition += new Vector3(subRandom(-possPosOff, possPosOff)/2, subRandom(-possPosOff, possPosOff)/2, 0);


            }

            containers.Add(con);

        }
        SplineContainer Oscale2 = Instantiate(forwardCircle, transform);
        Oscale2.transform.localScale = Vector3.zero;
        Oscale2.transform.localPosition = containers[containers.Count-1].transform.localPosition;
        //Oscale2.transform.localPosition -= Vector3.forward * .5f * lengthToUse;

        Oscale2.transform.localEulerAngles = containers[containers.Count - 1].transform.localEulerAngles;

        containers.Add(Oscale2);

        

        MeshFilter f = emptyFilt_AAA();
        f.mesh = GenerateMeshFromSplineContainerList(containers, sideCount, true, false);
        
        //f.transform.localPosition = Vector3.back * .5f * lengthToUse;
        return f;

    }

    SplineContainer makeTapor()
    {

        int profileCount = (int)subRandom(minTaporPointCount, maxTaporPointCount + 1);
        List<Vector3> points = new List<Vector3>();

        bool bothTapored = decider5050();

        if (!isGem&&bothTapored)
        {
            points.Add(Vector3.forward * (-1));

        }

        if (decider5050() && isGem)
        {
            points.Add(Vector3.forward * (-1));

        }

        for (int i = 0; i < profileCount; i++)
        {

            Vector3 v = new Vector3(0, subRandom(minProfThickPerc, 1f), i);
            points.Add(v);
            

        }

        if(bothTapored)
        {
            points.Add(Vector3.forward * (profileCount));

        }

        if (decider5050()&&isGem)
        {
            points.Add(Vector3.forward * (profileCount));

        }

        SplineContainer con = emptyCont_AAA();
        con.AddSpline(EasyCurveSpline(points, subRandom(.01f, .5f)));
        return con;
    }

    float minCheck(float check, float min)
    {
        if(check < min)
        {
            check = min;
        }
        return check;

    }


}
