using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RandomPatternGen : MetalShapeGen
{
    public List<SplineContainer> inputGuideCurves = new List<SplineContainer>();
    public int inputRowCount;
    public float inputGuideSharp;
    public MeshFilter InputBasicPatternPre;

    public List<Transform> useForPattern = new List<Transform>();
    //List<SplineContainer> GuideCurvesGuide, int rowCount, float guideSharp
    [SerializeField] handleGen solidPillar;
    [SerializeField] SplineContainer circle;
    [SerializeField] Transform emptyTran;

    [SerializeField] float minStarWidthPerc = .1f, maxStarWidthPerc = .5f, meshThick = .1f, minGenaricPerc = .2f, maxGenaricPerc = .5f,
        minStarArmBalance = .25f, maxStarArmBalance = .75f, hollowPillarWrapVariance = .4f;
    [SerializeField] int distCheckRes = 50, maxPatterningCount = 100, minPointRes = 3, maxPointRes = 5, starWingMeshRes = 10,
        minPolySideCount = 4, maxPolySideCount = 9, minSubPillarCount = 1, maxSubPillarCount = 4, minHollowWrapPill = 2, maxHollowWrapPill = 5,
        minQuarterLength = 2, maxQuarterLength = 2, minFullPatternLength = 2, maxFullPatternLength = 5, minHollowPillPointRes = 3, maxHollowPillPointRes = 6;

    List<Texture2D> ts;
    List<MeshFilter> patternFinalMeshes = new List<MeshFilter>();

    [SerializeField] Transform emptyMover, SplineParent, FinalSectionPattern;

    [SerializeField] bool quarterCustomRotation, overrideSingleShapeForPattern;

    public Transform StarPre, PolyPre, PillarPre, HollowStar, HollowPoly, HollowPillar, starArmPre, hollowStarArmPre, emptyPre;
    [SerializeField] bool[] PresAllowForPattern = new bool[9];

    float3 pos, st, up;

    float starWidthPerc;

    int polySideCount, numOfSubPills, quarterLength, fullPatternRes;

    bool hasStarted, balanceEnds, useingCustomPattern;

    
    public override void Begin()
    {
        base.Begin();

        if(SplineParent != null)
        {
            Destroy(SplineParent.gameObject);
        }

        SplineParent = Instantiate(emptyGameObjectAAA, transform).transform;

        polySideCount = (int)subRandom(minPolySideCount, maxPolySideCount + 1);
        numOfSubPills = (int)subRandom(minSubPillarCount, maxSubPillarCount + 1);
        quarterLength = (int)subRandom(minQuarterLength, maxQuarterLength + 1);
        fullPatternRes = (int)subRandom(minFullPatternLength, maxFullPatternLength + 1);

        balanceEnds = decider5050();

        if (patternFinalMeshes.Count > 0)
        {
            for (int i = patternFinalMeshes.Count - 1; i >= 0; i--)
            {
                Destroy(patternFinalMeshes[i].gameObject);
            }
            patternFinalMeshes.Clear();
        }

        emptyMover = Instantiate(emptyTran, transform);

        CreateBaseShapes();

        if(useForPattern.Count == 0)
        {
            if (PresAllowForPattern[0])
            {
                useForPattern.Add(StarPre);

            }
            if (PresAllowForPattern[1])
            {
                useForPattern.Add(PolyPre);
            }
            if (PresAllowForPattern[2])
            {
                useForPattern.Add(PillarPre);
            }
            if (PresAllowForPattern[3])
            {
                useForPattern.Add(HollowStar);
            }
            if (PresAllowForPattern[4])
            {
                useForPattern.Add(HollowPoly);
            }
            if (PresAllowForPattern[5])
            {
                useForPattern.Add(HollowPillar);
            }
            if (PresAllowForPattern[6])
            {
                useForPattern.Add(starArmPre);
            }
            if (PresAllowForPattern[7])
            {
                useForPattern.Add(hollowStarArmPre);
            }
            if (PresAllowForPattern[8])
            {
                useForPattern.Add(emptyPre);
            }

            if (overrideSingleShapeForPattern)
            {

                Transform t = useForPattern[(int)subRandom(0, useForPattern.Count)];

                useForPattern.Clear();
                useForPattern.Add(t);
            }

                //public Transform StarPre, PolyPre, PillarPre, HollowStar, HollowPoly, HollowPillar, starArmPre, hollowStarArmPre;

    //useForPattern.Add(PolyPre); //to blocky
    //useForPattern.Add(PillarPre);
    //        useForPattern.Add(HollowStar);
    //        useForPattern.Add(HollowPoly);
    //        useForPattern.Add(HollowPillar);

        }
        else
        {
            useingCustomPattern = true;
        }


        CreateBasePattern();

        hasStarted = true;

        if (InputBasicPatternPre == null)
        {
            InputBasicPatternPre = FinalSectionPattern.GetComponent<MeshFilter>();
        }

        /*
        if (inputGuideCurves.Count > 0)
        {
            if(InputBasicPatternPre == null)
            {
                InputBasicPatternPre = FinalSectionPattern.GetComponent<MeshFilter>();
            }

            MeshFilter m;

            ArrangeAlongCurves(inputGuideCurves, inputRowCount, inputGuideSharp, out m);

            Final = m.gameObject;

            //Debug.Log("Arrangment check");

        }
        */


        //SubstituteVoid();

        if(Final == null)
        {
            Final = InputBasicPatternPre.gameObject;
        }
    }



    void CreateBaseShapes()
    {
        CreateStarAndHollowStar();
        CreatePolyAndHollowPolyPre();
        CreatePillarAndHollowPillar();

    }

    #region baseShape nitty gritty!!!!!!!!!!!!!!!!!!!!!!!

    void CreateStarAndHollowStar()
    {
        //create corner mesh
        float pointPerc = subRandom(0, 1f);

        float starBalance = subRandom(minStarArmBalance, maxStarArmBalance);
        float inversBalance = 1f - starBalance;
        //int baseCurvePointCount = (int)subRandom(minPointRes, maxPointRes + 1);
        int baseCurvePointCount = (int)Mathf.Lerp(minPointRes+0f, maxPointRes + 1f, pointPerc);
        starWidthPerc = Mathf.Lerp(minStarWidthPerc, maxStarWidthPerc, pointPerc);


        SplineContainer baseCurve = Instantiate(containerPreAAA, SplineParent);

        List<Vector3> basePoints = new List<Vector3>();

        Vector3 averagePos = Vector3.zero;


        basePoints.Add(Vector3.zero);

        float maxOffPerc = 1 / (baseCurvePointCount - 1f);

        bool straightCenter = decider5050();

        for (int i = 1; i < baseCurvePointCount-1; i++)
        {

            float perc = i / (baseCurvePointCount-1f);

            float ZOff = subRandom(-.5f, .5f) * maxOffPerc;

            if (i > 1 && i < baseCurvePointCount - 1)
                ZOff *= 2;

            ZOff += perc;

            float XOff111 = subRandom(-.5f, .5f) * starWidthPerc;

            Vector3 v = new Vector3(XOff111,0,ZOff);

            if (straightCenter)
                v = Vector3.forward * perc;

            basePoints.Add(v);

            averagePos += v;

        }
        if (balanceEnds)
        {
            basePoints.Add(new Vector3((-averagePos / (baseCurvePointCount - 1f)).x, 0, 1f));
        }
        else
        {
            if (straightCenter)
                basePoints.Add(Vector3.forward);
            else
                basePoints.Add(new Vector3(subRandom(-.5f, .5f), 0, 1f));

        }

        baseCurve.AddSpline(EasyCurveSpline(basePoints, .5f));

        List<Vector3> leftPoints = new List<Vector3>();
        List<Vector3> rightPoints = new List<Vector3>();

        float XOff = subRandom(0f, .5f) * starWidthPerc;
        if (decider5050())
            XOff = .5f * starWidthPerc;
        leftPoints.Add(basePoints[0] - (Vector3.right * XOff));
        rightPoints.Add(basePoints[0] + (Vector3.right * XOff));

        for (int i = 1; i < baseCurvePointCount - 1; i++)
        {

            float perc = i / (baseCurvePointCount - 1f);

            baseCurve.Evaluate(baseCurve[0], perc, out pos, out st, out up);

            emptyMover.position = pos;
            emptyMover.rotation = Quaternion.LookRotation(st);

            float XOff222 = subRandom(.1f, .5f) * starWidthPerc;

            leftPoints.Add((Vector3)pos - (emptyMover.right * XOff222));
            rightPoints.Add((Vector3)pos + (emptyMover.right * XOff222));

        }

        leftPoints.Add(basePoints[basePoints.Count-1]);
        rightPoints.Add(basePoints[basePoints.Count - 1]);

        SplineContainer TopLeftHalf = Instantiate(containerPreAAA, SplineParent);
        SplineContainer TopRightHalf = Instantiate(containerPreAAA, SplineParent);

        TopLeftHalf.AddSpline(EasyCurveSpline(leftPoints, .5f));
        TopRightHalf.AddSpline(EasyCurveSpline(rightPoints, .5f));

        SplineContainer BotLeftHalf = Instantiate(TopLeftHalf, SplineParent);
        SplineContainer BotRightHalf = Instantiate(TopRightHalf, SplineParent);

        TopLeftHalf.transform.position += Vector3.up * meshThick / 2;
        TopRightHalf.transform.position += Vector3.up * meshThick / 2;
        BotLeftHalf.transform.position -= Vector3.up * meshThick / 2;
        BotRightHalf.transform.position -= Vector3.up * meshThick / 2;


        List<SplineContainer> solidWing = new List<SplineContainer>()
        {
            TopLeftHalf,
            TopRightHalf,
            BotRightHalf,
            BotLeftHalf,
            TopLeftHalf
        };

        MeshFilter starWingPre = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        starWingPre.mesh = GenerateMeshFromSplineContainerList(solidWing, starWingMeshRes, true, false);

        MeshFilter HollowStarWingPre = Instantiate(starWingPre, transform).GetComponent<MeshFilter>();
        HollowStarWingPre.transform.localScale -= Vector3.up * .1f;

        starArmPre = starWingPre.transform;
        hollowStarArmPre = HollowStarWingPre.transform;

        //straightCenter = false;

        if (straightCenter)
        {
           
            MeshFilter rightHalf = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
            TopRightHalf.transform.localPosition = Vector3.zero;
            rightHalf.mesh = nonTaporStemMesh1(4, starWingMeshRes, meshThick/2, false, TopRightHalf, TopRightHalf[0], Vector3.one);

            MeshFilter leftHalf = Instantiate(rightHalf, transform);
            Vector3 v3 = leftHalf.transform.localScale;
            v3.x *= -1;
            leftHalf.transform.localScale = v3;

            List<MeshFilter> mfs = new List<MeshFilter>()
            {
                rightHalf,
                leftHalf

            };

            HollowStarWingPre.mesh = UNWEIGHTEDCombineMeshsAndTextures(mfs, null, out ts);
            
            HollowStarWingPre.transform.localScale = Vector3.one;

        }
        else
        {
            Vector3 v1 = HollowStarWingPre.transform.localScale;
            v1.x *= -1f;
            HollowStarWingPre.transform.localScale = v1;

            List<MeshFilter> mfs = new List<MeshFilter>()
            {
                starWingPre,
                HollowStarWingPre

            };

            HollowStarWingPre.mesh = UNWEIGHTEDCombineMeshsAndTextures(mfs, null, out ts);

            HollowStarWingPre.transform.localScale = Vector3.one;


        }



        starWingPre.transform.localPosition = Vector3.zero;
        starWingPre.transform.localScale = Vector3.one;

        int starSideCount = (int)subRandom(minPolySideCount, maxPolySideCount + 1);
        if (decider5050())
            starSideCount = polySideCount;

        MeshFilter poly, hollow;
        float sideL;

        PolyAndHollow(starSideCount, out poly, out hollow, out sideL);

        poly.transform.localScale = new Vector3(starBalance, 1, starBalance);
        hollow.transform.localScale = new Vector3(starBalance, 1, starBalance);

        List<MeshFilter> starFilters = new List<MeshFilter>();
        starFilters.Add(poly);
        List<MeshFilter> hollowFilters = new List<MeshFilter>();
        hollowFilters.Add(hollow);

        float rotAdd = 90;
        if (decider5050())
            rotAdd += (.5f / (starSideCount - 1f)) * 360;

        //Vector3 scaleToBe = new Vector3((sideL) / 2, 1, (sideL) / 2);
        Vector3 scaleToBe = new Vector3((inversBalance) / 2, 1, (inversBalance) / 2);

        for (int i=0; i<starSideCount-1f; i++)
        {

            float YRot = i/(starSideCount-1f);
            YRot *= 360;
            YRot += rotAdd;

            //YRot += (1f / (starSideCount - 1f))*90;

            Transform polyWing = Instantiate(starWingPre, transform).transform;
            Transform hollowWing = Instantiate(HollowStarWingPre, transform).transform;

            polyWing.localEulerAngles = Vector3.up * YRot;
            hollowWing.localEulerAngles = Vector3.up * YRot;

            Vector3 posToBe = polyWing.forward * (starBalance/2);

            polyWing.localScale = scaleToBe;
            hollowWing.localScale = scaleToBe;

            polyWing.localPosition = posToBe;
            hollowWing.localPosition = posToBe;



            starFilters.Add(polyWing.GetComponent<MeshFilter>());
            hollowFilters.Add(hollowWing.GetComponent<MeshFilter>());

        }

        StarPre = Instantiate(emptyMeshPreAAA, transform).transform;
        StarPre.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(starFilters, null, out ts);

        HollowStar = Instantiate(emptyMeshPreAAA, transform).transform;
        HollowStar.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(hollowFilters, null, out ts);
    }
    void CreatePolyAndHollowPolyPre()
    {
        MeshFilter polyShape;
        MeshFilter hollowShape;

        List<MeshFilter> poly111 = new List<MeshFilter>();
        List<MeshFilter> hollow = new List<MeshFilter>();

        bool multiPoly = decider5050();
        float sideL;

        PolyAndHollow(polySideCount, out polyShape, out hollowShape, out sideL);

        poly111.Add(polyShape);
        hollow.Add(hollowShape);

        if (multiPoly)
        {

            float CenterScaleToBe = subRandom(minStarArmBalance, maxStarArmBalance);

            MeshFilter polyShape2;
            MeshFilter hollowShape2;

            int altPolyCount = (int)subRandom(minPolySideCount, maxPolySideCount + 1);

            if (decider5050())
                altPolyCount = polySideCount;

            PolyAndHollow(altPolyCount, out polyShape2, out hollowShape2, out sideL);

            polyShape.transform.localScale = new Vector3(CenterScaleToBe, 1, CenterScaleToBe);
            hollowShape.transform.localScale = new Vector3(CenterScaleToBe, 1, CenterScaleToBe);

            poly111.Add(hollowShape2);
            hollow.Add(hollowShape2);

        }


        PolyPre = Instantiate(emptyMeshPreAAA, transform).transform;
        HollowPoly = Instantiate(emptyMeshPreAAA, transform).transform;

        PolyPre.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(poly111, null, out ts);
        HollowPoly.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(hollow, null, out ts);

        //if multipoly, make anouther polygone(hollow), that wraps the first one we made

    }
    void PolyAndHollow(int sideCount, out MeshFilter poly, out MeshFilter hollow, out float sideLength)
    {
        List<Vector3> corners = new List<Vector3>();
        for (int i = 0; i < sideCount; i++)
        {
            float perc = i / (sideCount - 1f);
            circle.Evaluate(circle[0], perc, out pos, out st, out up);
            corners.Add(pos);
        }

        Vector3 di = Vector3.Lerp(corners[0], corners[1], .5f);
        float scaleToBe = .5f/(Vector3.Distance(di, Vector3.zero));

        circle.transform.localScale = Vector3.one * scaleToBe;


        corners.Clear();
        for(int i = 0; i < sideCount; i++)
        {
            float perc = i/(sideCount-1f);
            circle.Evaluate(circle[0], perc, out pos, out st, out up);
            corners.Add(pos);
        }

        sideLength = Vector3.Distance(corners[0], corners[1]);

        SplineContainer top = Instantiate(containerPreAAA, SplineParent);
        top.AddSpline(EasyCurveSpline(corners, .01f));
        SplineContainer bot = Instantiate(top, SplineParent);

        //poly = new MeshFilter();
        hollow = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        hollow.mesh = nonTaporStemMesh1(4, sideCount, meshThick/2, false, top, top[0], Vector3.one);


        int[] tris = new int[corners.Count * 3];
        Vector2[] uvs = new Vector2[corners.Count];

        int TriOff = 0;
        for(int verts = 0; verts < corners.Count-1; verts++)
        {
            tris[TriOff] = 0;
            tris[TriOff + 1] = verts;
            tris[TriOff + 2] = verts+1;

            uvs[verts] = new Vector2(corners[verts].x,corners[verts].z);

            TriOff += 3;
            

        }

        top.transform.localPosition = Vector3.up * meshThick / 2;
        bot.transform.localPosition = Vector3.down * meshThick / 2;

        List<SplineContainer> cons = new List<SplineContainer>()
        {
            top,bot
        };

        MeshFilter sides = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        sides.mesh = GenerateMeshFromSplineContainerList(cons, sideCount-1, false, false);

        MeshFilter topFace = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        topFace.mesh.vertices = corners.ToArray();
        topFace.mesh.triangles = tris;
        topFace.mesh.uv = uvs;
        topFace.mesh.RecalculateNormals();

        MeshFilter botFace = Instantiate(topFace, transform);
        Vector3 v4 = botFace.transform.localScale;
        v4.y *= -1;
        botFace.transform.localScale = v4;

        topFace.transform.localPosition = Vector3.up * meshThick/2;
        botFace.transform.localPosition = Vector3.down * meshThick/2;

        List<MeshFilter> polysToBe = new List<MeshFilter>()
        {
            topFace, sides, botFace
        };



        poly = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        poly.mesh = UNWEIGHTEDCombineMeshsAndTextures(polysToBe, null, out ts);

        circle.transform.localScale = Vector3.one;

    }

    void CreatePillarAndHollowPillar()
    {
        List<MeshFilter> solidFilters = new List<MeshFilter>();

        solidPillar.pillarSeed = seed;
        solidPillar.Begin();
        solidPillar.FinalPillar.transform.localScale = new Vector3(meshThick, 1, meshThick);
        solidPillar.FinalPillar.transform.localPosition = new Vector3(0, 0, -.5f);

        foreach (MeshFilter filter in solidPillar.FinalPillar.GetComponentsInChildren<MeshFilter>())
        {
            solidFilters.Add(filter);
        }



        Transform t = Instantiate(emptyMeshPreAAA, transform).transform;
        t.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(solidFilters, null, out ts);

        List<MeshFilter> subPillars = new List<MeshFilter>();

        float pillarLineWidth = numOfSubPills * meshThick;

        for(int i = 0; i < numOfSubPills; i++)
        {
            Transform pil = Instantiate(t, transform);

            float Off = 0;
            if (numOfSubPills > 1)
                Off = i / (numOfSubPills - 1f);

            Off -= .5f;
            Off *= pillarLineWidth;

            pil.localPosition = Vector3.right * Off;


            subPillars.Add(pil.GetComponent<MeshFilter>());

        }

        PillarPre = Instantiate(emptyMeshPreAAA, transform).transform;
        PillarPre.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(subPillars, null, out ts);


        List<Vector3> points = new List<Vector3>();

        int hollowWraps = (int)subRandom(minHollowWrapPill, maxHollowWrapPill + 1);
        int pointRes = (int)subRandom(minHollowPillPointRes, maxHollowPillPointRes + 1);

        points.Add(Vector3.zero);
        bool hasXOff = decider5050();
        for (int i = 1; i < pointRes-1; i++)
        {

            float perc = i/(pointRes-1f);

            float YOFF = subRandom(-hollowPillarWrapVariance, hollowPillarWrapVariance);

            float xOFF = 0;
            if(hasXOff)
                xOFF = subRandom(-hollowPillarWrapVariance, hollowPillarWrapVariance);

            points.Add(new Vector3(xOFF, YOFF, perc));

        }

        points.Add(Vector3.forward);

        SplineContainer wrapSpline = Instantiate(containerPreAAA, SplineParent);

        wrapSpline.AddSpline(EasyCurveSpline(points, .5f));

        MeshFilter singleWrap = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        singleWrap.mesh = nonTaporStemMesh1(4, starWingMeshRes, meshThick/3, false, wrapSpline, wrapSpline[0], Vector3.one);

        List<MeshFilter> singleHollow = new List<MeshFilter>();

        for(int i = 0; i < hollowWraps; i++)
        {

            float perc = ((float)i/(hollowWraps));

            MeshFilter m = Instantiate(singleWrap, transform);
            m.transform.localEulerAngles = Vector3.forward * perc * 360;
            m.transform.position = m.transform.up * meshThick / 2;

            singleHollow.Add(m);

        }

        MeshFilter singleHollowPillar = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        singleHollowPillar.mesh = UNWEIGHTEDCombineMeshsAndTextures(singleHollow, null, out ts);
        singleHollowPillar.transform.localEulerAngles = Vector3.forward * 90;
        List<MeshFilter> finalHollowPillars = new List<MeshFilter>();

        for(int i = 0; i < numOfSubPills; i++)
        {
            MeshFilter hol = Instantiate(singleHollowPillar, transform);


            float Off = 0;
            if(numOfSubPills > 1)
                Off = i / (numOfSubPills - 1f);

            Off -= .5f;
            Off *= pillarLineWidth;


            hol.transform.localPosition = Vector3.right * Off;
            hol.transform.localPosition -= Vector3.forward * .5f;


            finalHollowPillars.Add(hol.GetComponent<MeshFilter>());

        }

        HollowPillar = Instantiate(emptyMeshPreAAA, transform).transform;
        HollowPillar.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(finalHollowPillars, null, out ts);

    }

    #endregion


    void CreateBasePattern()
    {

        Transform t = null;
        if (decider5050())
        {
            t = Instantiate(useForPattern[(int)subRandom(0, useForPattern.Count)], transform);

        }
        //CreateQuarter(out FinalSectionPattern, useForPattern, quarterLength, t, true);

        CreateQuarter(out FinalSectionPattern, useForPattern, quarterLength, t,false);

    }

    void CreateQuarter(out Transform q, List<Transform> possiblePeices, int QuarterRes, Transform centerOverride,bool needsOffSet)
    {

        List<Transform> usedPeices = new List<Transform>();

        for(int i =0; i < QuarterRes+1; i++)
        {
            Transform t = possiblePeices[(int)subRandom(0, possiblePeices.Count)];
            usedPeices.Add(t);
        }

        MeshFilter QuarterMesh = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        List<MeshFilter> filters = new List<MeshFilter>();

        int customRot = (int)subRandom(0, 2 + 1);

        for (int x = 0; x < QuarterRes; x++)
        {

            for(int z = 0; z < QuarterRes; z++)
            {

                float xPer = x / (QuarterRes - 1f);
                float zPer = z / (QuarterRes - 1f);

                Transform t = Instantiate(usedPeices[(int)subRandom(0, usedPeices.Count)], transform);

                t.localPosition = new Vector3(x, 0, z);
                t.localPosition -= new Vector3(.5f * (QuarterRes-1), 0, .5f * (QuarterRes - 1));
                if (needsOffSet)
                {
                    t.localPosition += new Vector3(.5f, 0, .5f);
                }

                if (useingCustomPattern)
                {
                    if(customRot == 0)
                    {
                        
                        if(xPer < .4f)
                        {
                            t.rotation = Quaternion.LookRotation(Vector3.right);
                        }
                        if(xPer > .6f)
                        {
                            t.rotation = Quaternion.LookRotation(-Vector3.right);
                        }

                    }
                    if (customRot == 1)
                    {

                        if (zPer < .4f)
                        {
                            t.rotation = Quaternion.LookRotation(Vector3.forward);
                        }
                        if (zPer > .6f)
                        {
                            t.rotation = Quaternion.LookRotation(-Vector3.forward);
                        }

                    }
                    if (customRot == 2)
                    {
                        t.rotation = Quaternion.LookRotation(t.localPosition - new Vector3(QuarterRes / 2f, 0, QuarterRes / 2f));
                    }
                }

                filters.Add(t.GetComponent<MeshFilter>());

            }

        }

        if (centerOverride!=null)
        {
            Transform t = Instantiate(centerOverride, transform);

            //t.localScale = Vector3.one * subRandom(.5f * quarterLength, quarterLength);
            t.localScale = Vector3.one * (quarterLength);

            Vector3 v = t.localScale;
            v.y = 2;
            t.localScale = v;

            //t.localPosition = new Vector3((float)quarterLength / 2, 0, (float)quarterLength / 2);
            t.localPosition =Vector3.zero;

            filters.Add(t.GetComponent<MeshFilter>());
        }

        QuarterMesh.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        QuarterMesh.transform.localScale = Vector3.one / (QuarterRes);
        //QuarterMesh.transform.localScale -= Vector3.one / (QuarterRes - 1f);

        Vector3 v2 = QuarterMesh.transform.localScale;
        v2.y = 1;
        QuarterMesh.transform.localScale = v2;
        QuarterMesh.transform.localEulerAngles= new Vector3(0,0, 90);

        filters.Clear();
        filters.Add(QuarterMesh);
        QuarterMesh.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);//resets scale on mesh

        q = QuarterMesh.transform;
        q.localScale = Vector3.one;
    }

    public void arrangeAlongTwoSplines(SplineContainer innerCon, SplineContainer outerCon, out MeshFilter mf1)
    {
        mf1 = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        float innerConLength = determineSplineLength(innerCon);

        float totalPercentUsed = 0f;

        float3 pos1;

        int saftyCheck = 0;
        List<MeshFilter> filts = new List<MeshFilter>();

        while (totalPercentUsed < 1f && saftyCheck < maxPatterningCount)
        {
            innerCon.Evaluate(innerCon[0], totalPercentUsed, out pos, out st, out up);
            outerCon.Evaluate(outerCon[0], totalPercentUsed, out pos1, out st, out up);

            float dist = Vector3.Distance(pos, pos1);

            Transform lT = Instantiate(InputBasicPatternPre, transform).transform;


            ArrangeObjectBetweenTwoSplines(innerCon, outerCon, totalPercentUsed, totalPercentUsed, lT, true);

            filts.Add(lT.GetComponent<MeshFilter>());

            saftyCheck++;


            float usedPerc = dist / innerConLength;
            //usedPerc /= 2f;

            if (usedPerc < 1f / maxPatterningCount)
            {
                usedPerc = 1f / maxPatterningCount;
            }

            totalPercentUsed += usedPerc;

        }

        mf1.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);

        //for (int i = letterPresInOrder.Count - 1; i >= 0; i--)
        //{
        //    Destroy(letterPresInOrder[i].gameObject);
        //}
    }


    float determineSplineLength(SplineContainer spl)
    {
        float length = 0;
        int measureRes = 100;

        Vector3 prevPos = Vector3.zero;
        for (int i = 0; i < measureRes; i++)
        {
            spl.Evaluate(spl[0], i / (measureRes - 1f), out pos, out st, out up);

            if (i > 0)
            {
                length += Vector3.Distance(prevPos, pos);
            }
            prevPos = pos;
        }


        return length;
    }


    public void arrangeAlongPerpendicularSplineGrid(List<SplineContainer> GuideCurvesGuide, int rowCount, float guideSharp, out MeshFilter masterFilter)
    {
        if (rowCount < 2)
            rowCount = 2;

        if(patternFinalMeshes.Count > 0)
        {
            for(int i = patternFinalMeshes.Count-1; i >= 0; i--)
            {
                Destroy(patternFinalMeshes[i].gameObject);
            }
            patternFinalMeshes.Clear();
        }

        if (hasStarted)
        {
            //if (SplineParent != null)
            //{
            //    Destroy(SplineParent.gameObject);
            //}
            //else
            //{
            //    SplineParent = Instantiate(emptyTran, transform);
            //}

            List<SplineContainer> GuideCurves = new List<SplineContainer>(); 

            for(int i = 0; i < rowCount; i++)
            {
                float perc = i / (rowCount - 1f);

                List<Vector3> points = new List<Vector3>();

                foreach(SplineContainer c in GuideCurvesGuide)
                {
                    c.Evaluate(c[0], perc, out pos, out st, out up);
                    points.Add(pos);
                }

                SplineContainer gCon = Instantiate(containerPreAAA, SplineParent);
                gCon.AddSpline(EasyCurveSpline(points, guideSharp));

                GuideCurves.Add(gCon);
            }

            List<float> avrgGuideDist = new List<float>();

            foreach(SplineContainer c in GuideCurves)
            {
                float guideDist = 0;

                c.Evaluate(c[0], 0f, out pos, out st, out up);
                Vector3 prevPos = pos;

                for (int i = 1; i < distCheckRes; i++)
                {
                    float checkTime = i/(distCheckRes-1f);
                    c.Evaluate(c[0], checkTime, out pos, out st, out up);

                    guideDist += Vector3.Distance(prevPos, pos);

                    prevPos = pos;

                }

                avrgGuideDist.Add(guideDist);

            }

            for(int i = 0; i < GuideCurves.Count-1; i++)
            {
                SplineContainer startCurve = GuideCurves[i];
                SplineContainer endCurve = GuideCurves[i+1];

                float startCurveDist = avrgGuideDist[i];
                float endCurveDist = avrgGuideDist[i+1];

                float checkPercent = 0f;
                int patterningCount = 1;
                while(checkPercent < 1f && patterningCount < maxPatterningCount)
                {
                    startCurve.Evaluate(startCurve[0], checkPercent, out pos, out st, out up);
                    Vector3 startPos = pos;
                    endCurve.Evaluate(endCurve[0], checkPercent, out pos, out st, out up);
                    Vector3 endPos = pos;

                    float distBetweenSpline = Vector3.Distance(startPos, endPos);

                    //arrange transform here
                    /////

                    Transform t = Instantiate(InputBasicPatternPre, transform).transform;

                    ArrangeObjectBetweenTwoSplines(startCurve, endCurve, checkPercent, checkPercent, t, true);

                    Vector3 v1 = t.localScale.z * Vector3.one;
                    t.localScale = v1;

                    patternFinalMeshes.Add(t.GetComponent<MeshFilter>());

                    float bbb = 0;

                    /////
                    if(distBetweenSpline / startCurveDist < distBetweenSpline / endCurveDist)
                    {
                        checkPercent += distBetweenSpline / startCurveDist;
                        bbb = distBetweenSpline / startCurveDist;
                    }
                    else
                    {
                        checkPercent += distBetweenSpline / endCurveDist;
                        bbb= distBetweenSpline / endCurveDist;
                    }

                    if(bbb < 1f/maxPatterningCount)
                    {
                        checkPercent += 1f / maxPatterningCount;
                    }

                    patterningCount++;
                }

            }


        }

        masterFilter = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        masterFilter.mesh = UNWEIGHTEDCombineMeshsAndTextures(patternFinalMeshes, null, out ts);
    }

    void ArrangeObjectBetweenTwoSplines(SplineContainer start, SplineContainer end, float basePercent, float destPercent, Transform tran, bool zScaler)
    {
        float3 pos, pos2, st, up;

        start.Evaluate(start[0], basePercent, out pos, out st, out up);
        end.Evaluate(end[0], destPercent, out pos2, out st, out up);

        //tran.position = pos;

        tran.position = (pos + pos2) / 2;

        Vector3 dir = ((Vector3)(pos2 - pos)).normalized;

        //tran.rotation = Quaternion.LookRotation(dir, new Vector3(1,0.01f,0).normalized);//may need to change the up to a variable
        tran.rotation = Quaternion.LookRotation(dir, st);//may need to change the up to a variable

        if (zScaler)
        {
            Vector3 scale = tran.localScale;
            scale.z = Vector3.Distance(pos, pos2);
            scale.y = scale.z;
            scale.x = scale.z/2.5f;
            tran.localScale = scale;

        }

    }

}
