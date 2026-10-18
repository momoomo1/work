using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


public class MagicStaffGen : MeleeParentScript
{



    public ImprovedCrystalGen conduitGen;
    public int conGenSeed;



    [SerializeField] handleGen handleG;

    [SerializeField] MeshFilter SubstituteConduit, wrapDonutPre;
    MeshFilter top, handle, conduitMesh;

    [SerializeField] SplineContainer forwardRound;
    List<SplineContainer> thickDeterms = new List<SplineContainer>(), handleCurves = new List<SplineContainer>();

    [SerializeField] float minConduitMassPerc = .02f, maxConduitMassPerc = .1f, handleDupChance = .3f, minThickVari = .5f, maxThickVari = 1.5f,
        maxBranchOffsetPerc = 2f, minShootThickPerc = .2f, minHeadWidthPerc = 2f, maxHeadWidthPerc = 3, minHandleLengthPerc = .7f, maxHandleLengthPerc = .8f,
        minDomMult = 2, maxDomMult = 4, maxDomPosVari = .5f, minConduitWidthPerc = .75f, maxConduitWidthPerc = 1.25f, singleWrapPerc = .01f;
    float overallSharp, handleLength;


    [SerializeField] int minHandleSectionCount = 1, maxHandleSectionCount = 5, minDupSideCount = 2, maxDupSideCount = 3, minBranchPointCount = 3,
        maxBranchPointCount = 5, minThickDeterminPointCount = 3, maxThickDeterminPointCount = 10, branchSideCount = 6, branchRes = 20,
        minBranchSplits = 1, maxBranchSplits = 3, minShootPointCount = 4, maxShootPointCount = 6,minSymbolCount = 1, maxSymbolCount = 6,
        minWrapingCount = 5, maxWrapingCount = 20, minGripPerCount =1, maxGripPerCount = 3;


    [SerializeField] bool overrideConduitSeedGen;//watch out for this later

    bool naturalShaped, matchingWraping;


    public override void Begin()
    {
        base.Begin();

        if (overrideConduitSeedGen)
        {
            conGenSeed = seed;
        }

        naturalShaped = decider5050();
        if (naturalShaped)
            naturalShaped = decider5050();

        overallSharp = subRandom(.01f, .5f);
        if (decider5050())
            overallSharp = .5f;

        handleLength = subRandom(minHandleLengthPerc, maxHandleLengthPerc)*length;
        if (!naturalShaped)
        {
            float dif = length - handleLength;
            handleLength += dif / 2f;
        }
        if (conduitGen != null)
        {
            

            CreateConduit();

        }
        else
        {
            conduitMesh = SubstituteConduit;
            ReColorAMesh(conduitMesh, enchantmentColor);
            
        }

       

        CreateHandle();
        CreateTop();

        FinalAssemble();

        handleG.gameObject.SetActive(false);
        patternGen.gameObject.SetActive(false);
        magicPatternGen.gameObject.SetActive(false);
        if(wordManager != null)
        {
            wordManager.gameObject.SetActive(false);

        }
    }

    #region handleGen

    void CreateHandle()
    {
        List<MeshFilter> filters = new List<MeshFilter>();

        //naturalShaped = true;

        if(naturalShaped)
        {
            MeshFilter branch = curvyBranch();

            float decide = subRandom(0f, 1f);
            if(decide < handleDupChance)
            {
                int handleDupCount = (int)subRandom(minDupSideCount, maxDupSideCount);
                matchingWraping = decider5050();
                if (matchingWraping)
                {
                    //matching branches wraped

                    for(int i = 0; i < handleDupCount; i++)
                    {
                        float rot = i / (handleDupCount - 0f);

                        MeshFilter branch33 = Instantiate(branch,transform);
                        branch33.transform.localEulerAngles = new Vector3(0,0,rot);

                        SplineContainer handC = Instantiate(handleCurves[0], transform);
                        SplineContainer thickC = Instantiate(thickDeterms[0], transform);

                        handC.transform.localEulerAngles = new Vector3(0, 0, rot);

                        handleCurves.Add(handC);
                        thickDeterms.Add(thickC);

                        filters.Add(branch33);
                    }

                }
                else
                {
                    //diffrent branches wraped

                    filters.Add(branch);

                    for (int i = 1; i < handleDupCount; i++)
                    {
                        float rot = i / (handleDupCount - 0f);

                        MeshFilter branch33 = curvyBranch();
                        branch33.transform.localEulerAngles = new Vector3(0, 0, rot);
                        filters.Add(branch33);
                    }
                }

            }
            else
            {
                filters.Add(branch);

            }

        }
        else
        {
            handleG.pillarSeed = seed;
            handleG.Begin();

            MeshFilter[] filts = handleG.FinalPillar.GetComponentsInChildren<MeshFilter>();
            ReColorAMesh(filts[0], hiltColor);
            ReColorAMesh(filts[1], handleColor);
            ReColorAMesh(filts[2], hiltColor);
            filters.AddRange(filts);

            MeshFilter handlePre = emptyFilt_AAA();
            handlePre.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
            filters.Clear();
            int handleSectionCount = (int)subRandom(minHandleSectionCount, maxHandleSectionCount + 1);
            if(decider5050())
                handleSectionCount = (int)subRandom(minHandleSectionCount, handleSectionCount + 1);


            List<float> percents = new List<float>();

            if (decider5050())
            {
                float total = 0;

                for (int i = 0; i < handleSectionCount; i++)
                {
                    float perc = subRandom(.2f, 1f);

                    total += perc;

                    percents.Add(perc);

                }
                for (int i = 0; i < handleSectionCount; i++)
                {
                    percents[i] /= total;

                }
            }
            else
            {
                for (int i = 0; i < handleSectionCount; i++)
                {
                    percents.Add(1 / (handleSectionCount-0f));
                }
            }

            float previousTotal = 0f;

            //bool isGridDup = decider5050();

            for (int i = 0; i < handleSectionCount; i++)
            {

                Transform t = Instantiate(handlePre, transform).transform;

                t.localScale = new Vector3(1, 1, percents[i]);
                t.localPosition = Vector3.forward * previousTotal;
                t.localEulerAngles = new Vector3(180, 0, 0);
                previousTotal += percents[i];

                float duplicateDecider = subRandom(0, 1f);
                if(duplicateDecider <= handleDupChance)
                {
                    //duplicate
                    if(decider5050())
                    {
                        filters.Add(GridSection(t));
                    }
                    else
                    {
                        filters.Add(RoundSection(t));
                    }

                    Destroy(t.gameObject);

                }
                else
                {
                    filters.Add(t.GetComponent<MeshFilter>());
                }


            }

        }

        handle = emptyFilt_AAA();
        handle.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        if (!naturalShaped)
        {
            handle.transform.localScale = new Vector3(thick, thick, handleLength);
        }

        handle.GetComponent<MeshRenderer>().material = MetalMaterial;

    }

    MeshFilter GridSection(Transform t)
    {
        int dupSideCount = (int)subRandom(minDupSideCount, maxDupSideCount + 1);

        List<MeshFilter> filters = new List<MeshFilter>();

        for (int x = 0; x < dupSideCount; x++)
        {
            for (int y = 0; y < dupSideCount; y++)
            {

                Transform t22 = Instantiate(t, transform);

                Vector3 v = t22.localScale;
                v.x *= (1f / dupSideCount);
                v.y *= (1f / dupSideCount);
                t22.localScale = v;

                Vector3 v22 = t22.localPosition;

                float xOff = x / (dupSideCount + 0f);
                xOff -= .5f;
                xOff += .5f / dupSideCount;

                float yOff = y / (dupSideCount - 0f);
                yOff -= .5f;
                yOff += .5f / dupSideCount;


                v22.x = xOff;
                v22.y = yOff;
                t22.localPosition = v22;

                filters.Add(t22.GetComponent<MeshFilter>());

            }
        }

        MeshFilter m = emptyFilt_AAA();
        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);


        return m;
    }

    MeshFilter RoundSection(Transform t)
    {
        forwardRound.transform.position = t.position;
        List<MeshFilter> filters = new List<MeshFilter>();

        int subCount = (int)subRandom(minDupSideCount, maxDupSideCount*2);

        float scale2B = (subRandom(1f,2f) / 3.141f);

        for (int i = 0; i < subCount; i++)
        {

            float time = i/(subCount-0f);

            Transform t22 = Instantiate(t, transform);
            t22.localScale = new Vector3(scale2B, scale2B, t.localScale.z);

            forwardRound.Evaluate(forwardRound[0], time, out pos, out st, out up);

            t22.position = pos;
            t22.localEulerAngles += new Vector3(0, 0, time * 360);

            filters.Add(t22.GetComponent<MeshFilter>());
        }

        MeshFilter m = emptyFilt_AAA();
        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null , out ts);
        return m;
    }

    MeshFilter curvyBranch()
    {
        SplineContainer thicknessDeterm = emptyCont_AAA();
        SplineContainer mainCurve = emptyCont_AAA();

        int thickDePointCount = (int)subRandom(minThickDeterminPointCount, maxThickDeterminPointCount + 1);
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < thickDePointCount; i++)
        {

            points.Add(new Vector3(0, subRandom(minThickVari, maxThickVari), i));


        }
        thicknessDeterm.AddSpline(EasyCurveSpline(points, overallSharp));

        int mainCurvePointCount = (int)subRandom(minBranchPointCount, maxBranchPointCount + 1);
        List<float> percents;
        if (decider5050())//can have crazy curves
        {
            percents = variedPercentsAAA(mainCurvePointCount, true, 0f);

        }
        else//regular curves
        {
            percents = variedPercentsAAA(mainCurvePointCount, true, .2f);
        }
        points.Clear();
        for(int i = 0; i < mainCurvePointCount; i++)
        {
            
            float perc = percents[i];

            points.Add(new Vector3(subRandom(-maxBranchOffsetPerc/2, maxBranchOffsetPerc/2) * width, 
                subRandom(-maxBranchOffsetPerc / 2, maxBranchOffsetPerc / 2) * width, 
                perc * handleLength));

        }


        mainCurve.AddSpline(EasyCurveSpline(points, overallSharp));



        MeshFilter m = emptyFilt_AAA();

        m.mesh = PartialSplineDrivenTapor(branchSideCount, branchRes, false, mainCurve, thicknessDeterm, thick / 2, 1f);

        ReColorAMesh(m, handleColor);

        thickDeterms.Add(thicknessDeterm);
        handleCurves.Add(mainCurve);

        List<MeshFilter> filters = new List<MeshFilter>();

        int gripCount = (int)subRandom(minGripPerCount, maxGripPerCount + 1);

        for (int i = 0; i < gripCount; i++)
        {
            filters.Add(CreateWrappings(mainCurve, thicknessDeterm, thick, 0));
        }

        filters.Add(m);
        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        return m;
    }


    MeshFilter CreateWrappings(SplineContainer mainCurve, SplineContainer widthDeterm, float maxThick, int maxWrap)
    {
        List<MeshFilter> filters = new List<MeshFilter>();

        int wrapCount = maxWrap;
        if (wrapCount == 0)
        {
            wrapCount = (int)subRandom(minWrapingCount, maxWrapingCount + 1);
        }

        float percentRange = singleWrapPerc * wrapCount;
        float startPercent = subRandom(0f, 1f - percentRange);

        forwardRound.transform.localPosition = Vector3.zero;
        forwardRound.transform.localEulerAngles = Vector3.zero;
        forwardRound.transform.localScale = Vector3.one;
        float next = 0f;
        for (int i = 0; i < wrapCount; i++)
        {
            float perc = startPercent + (i * singleWrapPerc);

            mainCurve.Evaluate(mainCurve[0], perc, out pos, out st, out up);

            SplineContainer con = Instantiate(forwardRound, transform);
            con.transform.localPosition = pos;
            con.transform.rotation = Quaternion.LookRotation(st, up);
            if (i < wrapCount - 1)
            {
                Vector3 curr = pos;

                mainCurve.Evaluate(mainCurve[0], perc + singleWrapPerc, out pos, out st, out up);

                next = Vector3.Distance(curr, pos);

            }



            widthDeterm.Evaluate(widthDeterm[0], perc, out pos, out st, out up);
            con.transform.localScale = Vector3.one * pos.y * maxThick;



            MeshFilter m = emptyFilt_AAA();
            m.mesh = nonTaporStemMesh1(4, 7, next, false, con, con[0], Vector3.one);
            filters.Add(m);
        }


        MeshFilter grip = emptyFilt_AAA();
        grip.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        ReColorAMesh(grip, hiltColor);
        return grip;
    }



    #endregion

    #region top

    void CreateTop()
    {
        List<MeshFilter> filters = new List<MeshFilter>();


        if (naturalShaped)
        {
            

            if (matchingWraping)
            {
                //make sure each top gets matching head

                //foreach handleDup { 
                //naturalTop();

                MeshFilter m0 = naturalTop(thickDeterms[0]);

                int inde = 0;
                foreach (SplineContainer handCurv in handleCurves)
                {

                    MeshFilter m = Instantiate(m0, transform);

                    handCurv.Evaluate(handCurv[0], 0f, out pos, out st, out up);

                    m.transform.localPosition = pos;
                    m.transform.rotation = Quaternion.LookRotation(-st, -up);
                    //m.transform.localEulerAngles += new Vector3(0, 180, 0);
                    filters.Add(m);

                    inde++;
                }

            }
            else
            {
                //create unique head
                int inde = 0;
                foreach(SplineContainer handCurv in handleCurves)
                {

                    MeshFilter m = naturalTop(thickDeterms[inde]);

                    handCurv.Evaluate(handCurv[0], 0f, out pos, out st, out up);

                    m.transform.localPosition = pos;
                    m.transform.rotation = Quaternion.LookRotation(-st, -up);

                    filters.Add(m);

                    inde++;
                }

            }

            conduitMesh.transform.localScale = Vector3.one * subRandom(minConduitWidthPerc, maxConduitWidthPerc) * width;
            handleCurves[0].Evaluate(handleCurves[0][0], 0f, out pos, out st, out up);

            st = math.normalize(st);

            conduitMesh.transform.localPosition = st * (length - handleLength)/2;
            conduitMesh.transform.rotation = Quaternion.LookRotation(-st, -up);
        }
        else
        {


            filters.Add(UnNaturalTop(false));
            if (decider5050())
            {
                filters.Add(UnNaturalTop(true));
            }
            conduitMesh.transform.localScale = Vector3.one * subRandom(minConduitWidthPerc, maxConduitWidthPerc) * width;

            conduitMesh.transform.localPosition = Vector3.forward * (length - handleLength) / 2f;

        }

        bool hadDom = decider5050();

        if(naturalShaped)
        {
            hadDom = false;
        }

        if (hadDom)//add a dominant pattern piece from pat gen
        {
            List<Transform> possibleDoms = new List<Transform>()
            {
                patternGen.HollowStar,
                patternGen.HollowPoly,
                patternGen.PillarPre
            };

            MeshFilter dom = Instantiate(possibleDoms[(int)subRandom(0, possibleDoms.Count)], transform).GetComponent<MeshFilter>();

            float zPos = subRandom((length - handleLength) * minDomMult, (length - handleLength) * maxDomMult);//not just used for position

            dom.transform.localScale = Vector3.one * zPos;

            zPos = length - handleLength;
            zPos /= 2f;
            if (decider5050())
            {
                zPos += subRandom(-maxDomPosVari, maxDomPosVari) * zPos;

            }

            dom.transform.localPosition = Vector3.back * zPos;

            if (decider5050())
                zPos = 90f;
            else
                zPos = 0f;

            dom.transform.localEulerAngles = new Vector3(zPos, 90, 0);

            ReColorAMesh(dom, hiltColor);

            filters.Add(dom);



        }

        top = emptyFilt_AAA();
        top.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        top.GetComponent<MeshRenderer>().material = MetalMaterial;

    }

    MeshFilter UnNaturalTop(bool isSecoundTime)
    {
        List<MeshFilter> filters = new List<MeshFilter>();

        int symCount = (int)subRandom(minSymbolCount, maxSymbolCount + 1);

        int decider = (int)subRandom(0, 2 + 1);
        if (isSecoundTime)
        {
            decider = (int)subRandom(0, 1 + 1);

        }
        List<Transform> usableSymbols = new List<Transform>()
        {
            patternGen.HollowStar, patternGen.HollowPoly
        };
        if (isSecoundTime)
        {
            usableSymbols.Add(patternGen.PillarPre);
            usableSymbols.Add(patternGen.hollowStarArmPre);
            usableSymbols.Add(patternGen.starArmPre);

        }


        Transform symbPre = Instantiate(usableSymbols[(int)subRandom(0, usableSymbols.Count)], transform);

        float headLength = length - handleLength;

        if(decider == 0)//conjoined
        {
            float mult = subRandom(1, 2f);

            float possibleOffset = subRandom(0, headLength)/2;

            bool offsetSym = decider5050();
            if(symCount == 1)
                offsetSym = false;

            bool oblong = decider5050();

            

            for(int i = 0; i < symCount; i++)
            {
                float perc = i/(symCount-0f);

                Transform t = Instantiate(symbPre, transform);

                if (oblong)
                {
                    t.localScale= new Vector3(headLength, headLength, headLength* mult);
                }
                else
                {
                    t.localScale = Vector3.one * headLength;
                }
                

                t.localPosition = Vector3.back * (length - handleLength) / 2f;

                if(offsetSym)
                {
                    t.localEulerAngles = new Vector3(0, 0, perc * 360);

                }
                else
                {
                    t.localEulerAngles = new Vector3(0, 0, perc * 180);

                }

                if (offsetSym)
                {
                    t.localPosition += t.right * possibleOffset;
                }
                filters.Add(t.GetComponent<MeshFilter>());

            }

        }
        if(decider == 1)//fenced
        {

            Transform emp = Instantiate(emptyGameObjectAAA, transform).transform;
            emp.transform.localPosition = Vector3.forward * -headLength / 2;
            float fenceRot = subRandom(0, 360f);
            if (decider5050())
                fenceRot = 0f;

            float headWidth = subRandom(minHeadWidthPerc, maxHeadWidthPerc) * width;

            float xyScale = (3.14f* headWidth) / symCount;


            for(int i = 0; i < symCount;i++)
            {

                float perc = i/(symCount-0f);

                emp.transform.localEulerAngles = new Vector3(0, 0, perc * 360);

                Transform t = Instantiate(symbPre, emp);
                t.transform.localEulerAngles = new Vector3(fenceRot, 0, 0);
                t.parent = transform;
                t.localScale = new Vector3(xyScale, xyScale, headLength);
                if(symCount > 1)
                {
                    t.localPosition = emp.localPosition+(emp.up * headWidth / 2);

                }

                filters.Add(t.GetComponent<MeshFilter>());

            }


        }
        if(decider == 2)//stacked
        {
            Transform emp = Instantiate(emptyGameObjectAAA, transform).transform;
            emp.localEulerAngles = new Vector3(90,0,0);
            float xzScale = subRandom(minHeadWidthPerc, maxHeadWidthPerc)*width;

            bool altRot = decider5050();
            bool rotDirB = false;

            for (int i = 0; i < symCount; i++)
            {
                float perc = i / (symCount - 1f);

                Transform t = Instantiate(symbPre, emp);
                if(rotDirB)
                {
                    t.localEulerAngles = new Vector3(0, 90, 0);

                }
                else
                {
                    t.localEulerAngles = new Vector3(0, -90, 0);

                }
                t.parent = transform;

                t.localScale = new Vector3(xzScale, xzScale, xzScale);
                if (symCount > 1)
                {
                    t.localPosition = Vector3.back * perc * headLength;

                }
                else
                {
                    t.localPosition = Vector3.back * .5f * headLength;

                }

                filters.Add(t.GetComponent<MeshFilter>());

                if(altRot)
                {
                    rotDirB = !rotDirB;
                }

            }

        }



        MeshFilter m = emptyFilt_AAA();
        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        ReColorAMesh(m, hiltColor);
        m.transform.localEulerAngles = new Vector3(0, 0, 90);
        return m;
    }

    MeshFilter naturalTop(SplineContainer handleThickDe)
    {

        int splits = 1;

        SplineContainer thickDeter = emptyCont_AAA();
        int pointCount = (int)subRandom(minThickDeterminPointCount, maxThickDeterminPointCount);

        List<Vector3> points = new List<Vector3>();

        handleThickDe.Evaluate(handleThickDe[0], 0f, out pos, out st, out up);


        points.Add(pos);
        for(int i = 1; i < pointCount-1; i++)
        {
            float perc = i/(pointCount-1f);
            points.Add(new Vector3(0, subRandom(minShootThickPerc, perc* maxThickVari), i));
        }
        points.Add(Vector3.forward * pointCount);

        thickDeter.AddSpline(EasyCurveSpline(points, overallSharp));

        List<MeshFilter> filters = new List<MeshFilter>();

        if (decider5050())
        {
            splits = (int)subRandom(minBranchSplits, maxBranchSplits + 1);

            for(int i = 0; i < splits; i++)
            {
                MeshFilter shoot = emptyFilt_AAA();
                SplineContainer shootCurve = randSpline();

                float xy = subRandom(minHeadWidthPerc, maxHeadWidthPerc) * width;
                Vector3 scale2b = new Vector3(xy, xy, length - handleLength);

                shootCurve.transform.localScale = scale2b;
                shoot.mesh = PartialSplineDrivenTapor(branchSideCount, branchRes, false, shootCurve, thickDeter, thick / 2f, 1f);

                ReColorAMesh(shoot, handleColor);
                filters.Add(shoot);

            }


        }
        else
        {
            //just one
            MeshFilter shoot = emptyFilt_AAA();
            SplineContainer shootCurve = randSpline();

            float xy = subRandom(minHeadWidthPerc, maxHeadWidthPerc) * width;
            Vector3 scale2b = new Vector3(xy, xy, length - handleLength);

            shootCurve.transform.localScale = scale2b;
            shoot.mesh = PartialSplineDrivenTapor(branchSideCount, branchRes, false, shootCurve, thickDeter, thick / 2f, 1f);

            ReColorAMesh(shoot, handleColor);
            filters.Add(shoot);
        }

        MeshFilter m = emptyFilt_AAA();

        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        return m;

    }


    SplineContainer randSpline()
    {

        List<Vector3> points = new List<Vector3>();
        int pointCount = (int)subRandom(minShootPointCount, maxShootPointCount+1);

        points.Add(Vector3.zero);
        points.Add(Vector3.forward * .1f);

        for(int i = 2; i < pointCount; i++)
        {

            Vector3 v = new Vector3();
            v.x = subRandom(-.5f, .5f);
            v.y = subRandom(-.5f, .5f);
            v.z = subRandom(.11f, 1f);

            points.Add(v);

        }

        SplineContainer con = emptyCont_AAA();
        con.AddSpline(EasyCurveSpline(points, overallSharp));
        
        return con;

    }

    #endregion

    void CreateConduit()
    {

        conduitGen.alotedMass = 3f;//i think this will produce something about 1m cubed
        conduitGen.seed = conGenSeed;
        conduitGen.colorSeed = colorSeed;

        if (naturalShaped)
        {
            conduitGen.isGem = false;
        }
        else
        {
            conduitGen.isGem = true;

        }

        conduitGen.Begin();

        conduitMesh = conduitGen.Final.GetComponent<MeshFilter>();

        ReColorAMesh(conduitMesh, enchantmentColor);
        //Debug.Log("staff enchant color " + enchantmentColor);
    }


    void FinalAssemble()
    {
        handle.transform.localScale *= -1f;
        top.transform.localScale *= -1f;
        List<MeshFilter> filters = new List<MeshFilter>()
        {
            handle, top
        };
        if (conduitMesh != null)
            filters.Add(conduitMesh);

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        Final.GetComponent<MeshRenderer>().material = MetalMaterial;

        MakeEnchantedDuplicate();
    }


}
