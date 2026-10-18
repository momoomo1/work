using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;


public class swordGen_Improved : MeleeParentScript
{
    [SerializeField] SplineContainer straightLine;

    [SerializeField] float maxBladeBaseOffsetPercent = .5f, minBaseCurvePointBalance = .3f, maxBaseCurvePontBalance = 1f, 
        minBaseSharp = 0.01f, maxBaseSharp = .5f, minEdgeXOFF = .2f, maxEdgeXOFF = .7f, maxRuffel_INNER_OffsetPerc_Local = .5f, maxRuffel_OUTER_OffsetPerc_Local = 1f,
        superCurveMult = 2, minGaurdVariance = .5f, maxGaurdVariance = 1.5f, minGuardRot = -30, maxGuardRot = 120, minHandleLengthPerc = .1f, maxHandleLengthPerc = .2f,
        handleWidthPerc = .25f, minPommelVariance = .1f, maxPommelVariance = .4f;

    [SerializeField] int minBaseCurvePointCount = 3, maxBaseCurvePointCount = 5, minRuffelPointCount = 3, maxRuffelPointCount = 6, topEdgePointCountBonusMult = 3,
        bladeMeshRes = 20, minGaurdQuarterCount = 1, maxGaurdQuarterCount = 3, minPommelQuarterCount = 1, maxPommelQuarterCount = 3;

    [SerializeField] bool randomSharpEdges, overrideRandomizedBools;
    [SerializeField] bool isCurved, islocalSym, isMirrored, hasRuffels, hasSuperCurves, isbalancedCurve;

    List<MeshFilter> allowedGuards = new List<MeshFilter>(), allowedCenters = new List<MeshFilter>();
    [SerializeField] MeshFilter bladeFilter, handleFilter, hiltFilter, spellFilter;

    //List<SplineContainer> containersToMesh = new List<SplineContainer>();
    SplineContainer baseCurveContainer, frontEdgeCurve, backEdgeCurve, topEdgeCurve;
    SplineContainer topFrontEdge;
    SplineContainer topBackEdge;

    Transform emptySampler;

    List<float> balances = new List<float>();

    List<float> frontBal = new List<float>(), frontXOffs = new List<float>(), backBal = new List<float>(), backXOffs = new List<float>();

    float allSharp = .5f, handleLength;
    //float localMinSharp, localMaxSharp;
    int baseCurvePointCount;


    //List<MeshFilter> enchantDuplicates = new List<MeshFilter>();


    public override void Begin()
    {
        base.Begin();

        if(!overrideRandomizedBools)
        {
            isCurved = decider5050();
            islocalSym = decider5050();
            isMirrored = decider5050();
            hasRuffels = decider5050();
            //hasRuffels = true;
            hasSuperCurves = decider5050();
            isbalancedCurve = decider5050();

            if (hasSuperCurves && !isbalancedCurve)
                isMirrored = true;
            if (!isCurved && islocalSym)
                isMirrored = false;
        }


        //localMaxSharp = subRandom(minBaseSharp, maxBaseSharp);
        //localMinSharp = subRandom(minBaseSharp, maxBaseSharp);
        if(randomSharpEdges)
        {
            allSharp = subRandom(minBaseSharp, maxBaseSharp);

        }

        baseCurveContainer = Instantiate(containerPreAAA, transform);

        emptySampler = Instantiate(emptyGameObjectAAA, transform).transform;

        allowedGuards.Add(patternGen.starArmPre.GetComponent<MeshFilter>());
        allowedGuards.Add(patternGen.hollowStarArmPre.GetComponent<MeshFilter>());
        allowedGuards.Add(patternGen.PillarPre.GetComponent<MeshFilter>());
        allowedGuards.Add(patternGen.HollowPillar.GetComponent<MeshFilter>());

        allowedCenters.Add(patternGen.PolyPre.GetComponent<MeshFilter>());
        allowedCenters.Add(patternGen.HollowPoly.GetComponent<MeshFilter>());
        allowedCenters.Add(patternGen.StarPre.GetComponent<MeshFilter>());
        allowedCenters.Add(patternGen.HollowStar.GetComponent<MeshFilter>());


        ArrangeBaseSpline();
        ArrangeFrontAndBackEdge();
        ArrangeTopEdgeContainers();
        CreateBladeMesh();

        CreateHilt();
        CreateHandle();
        CreatePommel();

        FinalCombine();
    }

    #region bladeCreation

    void ArrangeBaseSpline()
    {
        List<Vector3> basePoints = new List<Vector3>();

        baseCurvePointCount = (int)subRandom(minBaseCurvePointCount, maxBaseCurvePointCount + 1);
        if(isCurved == false)
        {
            baseCurvePointCount = (int)subRandom(minBaseCurvePointCount, (maxBaseCurvePointCount *2) + 1);

        }

        balances = new List<float>();
        float totalBal = 0;
        balances.Add(0f);
        for (int i = 0; i < baseCurvePointCount; i++)
        {
            totalBal += subRandom(minBaseCurvePointBalance, maxBaseCurvePontBalance);
            balances.Add(totalBal);
        }
        for (int i = 0; i < balances.Count; i++)
        {
            balances[i] = balances[i] / totalBal;
        }

        if (isCurved)
        {
            
            if(baseCurvePointCount == maxBaseCurvePointCount)
            {
                isbalancedCurve = true;
            }


            int ind = 0;

            Vector3 averagePos = Vector3.zero;

            foreach(float bal in balances)
            {

                float zOff = length * bal;
                float xOff = subRandom(-1f, 1f);

                xOff *= width;

                if (hasSuperCurves)
                    xOff *= superCurveMult;

                if(ind == 0)
                {
                    zOff = 0;
                    xOff = 0;
                    //xOff /= 2;
                    //if (!isMirrored)
                    //    xOff = 0;
                }

                //float yOff = ind/(balances.Count-1f);
                //yOff = 1 - yOff;
                //yOff *= thick/2;

                Vector3 point = new Vector3(xOff, 0, zOff);

                basePoints.Add(point);

                if(ind < balances.Count - 1)
                {
                    averagePos += point;
                }

                ind++;
            }
            if (isbalancedCurve)
            {
                averagePos /= basePoints.Count;
                averagePos.x *= -1f;
                averagePos.y = 0;
                averagePos.z = length;
                basePoints[basePoints.Count - 1] = averagePos;

            }

        }
        else
        {
            //balances.Add(0f);
            //balances.Add(1f);

            basePoints.Add(Vector3.zero);
            basePoints.Add(Vector3.forward * length);
        }

        //baseCurveContainer.AddSpline(randomSharpSpline(basePoints, localMinSharp, localMaxSharp));
        baseCurveContainer.AddSpline(EasyCurveSpline(basePoints, allSharp));

    }

    void ArrangeFrontAndBackEdge()
    {

        frontEdgeCurve = Instantiate(containerPreAAA, transform);

        backEdgeCurve = Instantiate(containerPreAAA, transform);

        //List<float> frontBal = new List<float>();
        //List<float> frontXOffs = new List<float>();

        CreateRandomBalances(out frontBal, out frontXOffs);

        //List<float> backBal = new List<float>(), 
        //    backXOffs = new List<float>();


        if (islocalSym)
        {
            for(int i = 0; i < frontBal.Count; i++)
            {
                backBal.Add(frontBal[i]);
            }
            for(int i = 0; i < frontXOffs.Count; i++)
            {
                backXOffs.Add(frontXOffs[i]);
            }
            //backBal = frontBal;
            //backXOffs = frontXOffs;

        }
        else
        {
            CreateRandomBalances( out backBal, out backXOffs);
        }

        List<Vector3> frontPoints, backPoints;

        FindPointsAlongBase(out frontPoints, false, frontBal, frontXOffs, baseCurveContainer);
        FindPointsAlongBase(out backPoints, true, backBal, backXOffs, baseCurveContainer);

        if (randomSharpEdges)
        {
            //frontEdgeCurve.AddSpline(randomSharpSpline(frontPoints, minBaseSharp, maxBaseSharp));
            //backEdgeCurve.AddSpline(randomSharpSpline(backPoints, minBaseSharp, maxBaseSharp));
            frontEdgeCurve.AddSpline(EasyCurveSpline(frontPoints, allSharp));
            backEdgeCurve.AddSpline(EasyCurveSpline(backPoints, allSharp));
        }
        else
        {
            frontEdgeCurve.AddSpline(EasyCurveSpline(frontPoints, .5f));
            backEdgeCurve.AddSpline(EasyCurveSpline(backPoints, .5f));
        }

    }

    void ArrangeTopEdgeContainers()
    {
        topEdgeCurve = Instantiate(containerPreAAA, transform);
        
        List<Vector3> points = new List<Vector3>();

        int topCount = topEdgePointCountBonusMult * balances.Count;

        for (int i = 0; i < topCount; i++)
        {

            float baseTime = i/(topCount - 1f);
            baseTime = Mathf.Lerp(0, 1f-((width/5)/length), baseTime);

            baseCurveContainer.Evaluate(baseCurveContainer[0], baseTime, out pos, out st, out up);

            //pos.y += thick / 2f;

            points.Add(pos);

        }
        topEdgeCurve.AddSpline(EasyCurveSpline(points, .1f));
        
        topFrontEdge = Instantiate(containerPreAAA, transform);
        topBackEdge = Instantiate(containerPreAAA, transform);

        float3 pos2;


        topEdgeCurve.Evaluate(topEdgeCurve[0], 1, out pos, out st, out up);
        frontEdgeCurve.Evaluate(baseCurveContainer[0], 1, out pos2, out st, out up);


        float idealGap = Vector3.Distance(pos, pos2);

        int frontCount = topEdgePointCountBonusMult * frontBal.Count;

        points.Clear();

        for(int i =0; i < frontCount; i++)
        {
            float baseTime = i / (frontCount - 1f);

            topEdgeCurve.Evaluate(topEdgeCurve[0], baseTime, out pos, out st, out up);
            frontEdgeCurve.Evaluate(frontEdgeCurve[0], baseTime, out pos2, out st, out up);

            float percDist = Vector3.Distance(pos, pos2);

            percDist = idealGap / percDist;
            percDist = Mathf.Clamp(percDist, 0f, 1f);

            points.Add(Vector3.Lerp(pos2, pos, percDist));

        }

        topFrontEdge.AddSpline(EasyCurveSpline(points, .1f));

        int backCount = topEdgePointCountBonusMult * backBal.Count;

        points.Clear();

        for (int i = 0; i < backCount; i++)
        {
            float baseTime = i / (backCount - 1f);

            topEdgeCurve.Evaluate(topEdgeCurve[0], baseTime, out pos, out st, out up);
            backEdgeCurve.Evaluate(backEdgeCurve[0], baseTime, out pos2, out st, out up);

            float percDist = Vector3.Distance(pos, pos2);

            percDist = idealGap / percDist;
            percDist = Mathf.Clamp(percDist, 0f, 1f);

            points.Add(Vector3.Lerp(pos2, pos, percDist));

        }
        topBackEdge.AddSpline(EasyCurveSpline(points, .1f));

        topFrontEdge.transform.localPosition += Vector3.up * thick / 2;
        topBackEdge.transform.localPosition += Vector3.up * thick / 2;


    }


    void CreateBladeMesh()
    {
        List<MeshFilter> bladeFilters = new List<MeshFilter>();
        List<SplineContainer> frontEdgeT = new List<SplineContainer>(), flatT = new List<SplineContainer>(), backEdgeT = new List<SplineContainer>();

        MeshFilter frontET = Instantiate(emptyMeshPreAAA,transform).GetComponent<MeshFilter>(),
            flatTop = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>(),
            backET = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        //Final = frontET.gameObject;

        frontEdgeT.Add(frontEdgeCurve);
        frontEdgeT.Add(topFrontEdge);

        bool worldUV = true;

        frontET.mesh = GenerateMeshFromSplineContainerList(frontEdgeT, bladeMeshRes, false, worldUV);


        flatT.Add(topFrontEdge);
        flatT.Add(topBackEdge);
        flatTop.mesh = GenerateMeshFromSplineContainerList(flatT, bladeMeshRes, false, worldUV);


        backEdgeT.Add(topBackEdge);
        backEdgeT.Add(backEdgeCurve);
        backET.mesh = GenerateMeshFromSplineContainerList(backEdgeT, bladeMeshRes, false, worldUV);

        bladeFilters.Add(frontET);
        bladeFilters.Add(backET);
        bladeFilters.Add(flatTop);


        frontET.mesh = UNWEIGHTEDCombineMeshsAndTextures(bladeFilters, null, out ts);

        //Color[] bladeCols = new Color[frontET.mesh.vertices.Length];
        //for (int i = 0; i < bladeCols.Length; i++)
        //{
        //    bladeCols[i] = bladeColor;
        //}
        //frontET.mesh.colors = bladeCols;
        ReColorAMesh(frontET, bladeColor);

        bladeFilters.Clear();

        ///////////////////////////////////
        //topFrontEdge.transform.localPosition = Vector3.zero;
        //topBackEdge.transform.localPosition = Vector3.zero;

        //add blade enchantment here
        
        enchantmentPlacement(topFrontEdge, topBackEdge, transform, false);

        bladeFilters.Add(frontET);
        if(outputEnchantmentFilter != null)
        {
            

            bladeFilters.Add(outputEnchantmentFilter);


        }

        frontET.mesh = UNWEIGHTEDCombineMeshsAndTextures(bladeFilters, null, out ts);
        ////////////////////////////

        bladeFilters.Clear();


        MeshFilter otherHalf = Instantiate(frontET,transform).GetComponent<MeshFilter>();
        otherHalf.transform.localScale = new Vector3(1,-1,1);

        bladeFilters.Add(frontET);
        bladeFilters.Add(otherHalf);

        frontET.mesh = UNWEIGHTEDCombineMeshsAndTextures(bladeFilters, null, out ts);



        bladeFilters.Clear();

        if(isMirrored)
        {
            MeshFilter mir = Instantiate(frontET, transform);
            mir.transform.localScale = new Vector3(-1, 1.01f, 1);

            bladeFilters.Add(mir); bladeFilters.Add(frontET);

            frontET.mesh = UNWEIGHTEDCombineMeshsAndTextures(bladeFilters, null, out ts);

        }

        bladeFilter = frontET;

        frontET.GetComponent<MeshRenderer>().material = MetalMaterial;



    }

    #endregion

    #region handleCreation
    void CreateHilt()
    {
        bool multpleGaurds = decider5050();

        bool gaurdXUp = decider5050();

        List<MeshFilter> filtsToAllow = new List<MeshFilter>();

        bool straightGaurd = false;
        bool perfectSym = false;
        if (isMirrored)
        {
            perfectSym = true;
        }
        if(islocalSym && !isCurved)
        {
            perfectSym = true;
        }

        if (perfectSym)
        {
            straightGaurd = decider5050();
        }

        MeshFilter HalfOrQuart;

        if (straightGaurd)
        {
            filtsToAllow.Add(allowedGuards[2]);
            //filtsToAllow.Add(allowedGuards[3]);
            HalfOrQuart = arrangeGaurdPieces(minGaurdQuarterCount, maxGaurdQuarterCount, 90, 90, filtsToAllow, minGaurdVariance, maxGaurdVariance, false);
        }
        else
        {
            filtsToAllow.Add(allowedGuards[0]);
            filtsToAllow.Add(allowedGuards[1]);
            HalfOrQuart = arrangeGaurdPieces(minGaurdQuarterCount, maxGaurdQuarterCount, minGuardRot, maxGuardRot, filtsToAllow, minGaurdVariance, maxGaurdVariance, false);
        }



        List<MeshFilter> combFilts = new List<MeshFilter>();


        if(!gaurdXUp && !straightGaurd)
        {
            combFilts.Add(HalfOrQuart);

            MeshFilter m2 = Instantiate(HalfOrQuart, transform);
            m2.transform.localScale = new Vector3(1,1,-1);

            combFilts.Add(m2);

            HalfOrQuart.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);

            combFilts.Clear();
        }

        //if (hasCenterPeice)
        //{
        //    MeshFilter cen = Instantiate(allowedCenters[(int)subRandom(0, allowedCenters.Count)], transform);

        //    ReColorAMesh(cen, hiltColor);

        //    //Vector3 sca = Vector3.one * subRandom(minGaurdVariance, maxGaurdVariance) * width;
        //    //Vector3 sca = Vector3.one * subRandom(.5f,1f)* width/2.6f;
        //    //Vector3 sca = Vector3.one * subRandom(minGaurdVariance, 1)* width/2.5f;
        //    Vector3 sca = Vector3.one * width;

        //    //sca.y = thick * 7.5f;
        //    cen.transform.localScale = sca;
        //    cen.transform.localEulerAngles = new Vector3(0, 90, 0);
        //    //cen.transform.localScale = new Vector3(width * 2, thick * 15, width * 2);
        //    combFilts.Clear();
        //    combFilts.Add(HalfOrQuart);
        //    combFilts.Add(cen);


        //    HalfOrQuart.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);

        //}


        MeshFilter otherHalf;

        if (perfectSym)
        {
            if (!straightGaurd)
            {
                otherHalf = Instantiate(HalfOrQuart, transform);
                //Debug.Log("111");
            }
            else
            {
                otherHalf = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
                //Debug.Log("222");
            }
            
        }
        else
        {
            otherHalf = arrangeGaurdPieces(minGaurdQuarterCount, maxGaurdQuarterCount, minGuardRot, maxGuardRot, filtsToAllow, minGaurdVariance, maxGaurdVariance, false);


            if (!gaurdXUp)
            {
                combFilts.Clear();

                combFilts.Add(otherHalf);

                MeshFilter m2 = Instantiate(otherHalf, transform);
                m2.transform.localScale = new Vector3(1, 1.03f, -1);

                combFilts.Add(m2);

                otherHalf.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);

                combFilts.Clear();
            }

        }
        otherHalf.transform.localScale = new Vector3(-1, 1.02f, 1);

        combFilts.Clear();

        combFilts.Add(HalfOrQuart);
        combFilts.Add(otherHalf);
        

        HalfOrQuart.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);

        HalfOrQuart.GetComponent<MeshRenderer>().material = MetalMaterial;



        HalfOrQuart.transform.localScale = new Vector3(width * 2, thick * 15, width * 2);

        if (straightGaurd)
        {
            HalfOrQuart.transform.localScale = new Vector3(width * 4, thick * 15, width * 4);

        }


        combFilts.Clear();
        combFilts.Add(HalfOrQuart);
        HalfOrQuart.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);
        HalfOrQuart.transform.localScale = Vector3.one;
        HalfOrQuart.transform.localEulerAngles = Vector3.zero;
        ReColorAMesh(HalfOrQuart, hiltColor);

        hiltFilter = HalfOrQuart;

    }

    void CreateHandle()
    {

        List<MeshFilter> comFilts = new List<MeshFilter>();

        Transform t = patternGen.GetComponentInChildren<handleGen>().FinalPillar.transform;

        comFilts.AddRange(t.GetComponentsInChildren<MeshFilter>());

        Vector3 v = t.localScale;
        t.localScale = Vector3.one;

        handleFilter = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        handleFilter.mesh = UNWEIGHTEDCombineMeshsAndTextures(comFilts, null, out ts);

        t.localScale = v;

        handleLength = subRandom(minHandleLengthPerc, maxHandleLengthPerc) * length;

        Vector3 handleScale = new Vector3(handleWidthPerc * width, thick, handleLength);
        handleFilter.transform.localScale = handleScale;
        handleFilter.transform.localPosition = Vector3.forward * -handleLength / 2;

        ReColorAMesh(handleFilter, handleColor);
        handleFilter.GetComponent<MeshRenderer>().material = MetalMaterial;
    }

    void CreatePommel()
    {
        bool complexPommel = decider5050();

        List<MeshFilter> comFilts = new List<MeshFilter>();
        MeshFilter pommelFilt = emptyFilt_AAA();

        bool sym = isMirrored;

        if (!isCurved && islocalSym)
        {
            sym = true;
        }
        List<MeshFilter> filtsToAllow = new List<MeshFilter>();
        if (complexPommel)
        {
            
            filtsToAllow.Add(allowedGuards[0]);
            filtsToAllow.Add(allowedGuards[1]);
            pommelFilt = arrangeGaurdPieces(minPommelQuarterCount, maxPommelQuarterCount, 0f, 360f, filtsToAllow, minPommelVariance, maxPommelVariance, false);

            comFilts.Add(pommelFilt);


            if(sym)
            {

                MeshFilter mir = Instantiate(pommelFilt, transform);
                mir.transform.localScale = new Vector3(-1,1,1);
                comFilts.Add(mir);
            }

        }

        float pomWidth = subRandom(handleWidthPerc * 2, handleWidthPerc * 2);

        MeshFilter centerFilt = arrangeGaurdPieces(1, 1, 0, 0, allowedCenters, pomWidth, pomWidth, false);
        centerFilt.transform.localEulerAngles = new Vector3(0, 90, 0);
        comFilts.Add(centerFilt);

        if (sym)
        {

            MeshFilter mir = Instantiate(centerFilt, transform);
            mir.transform.localScale = new Vector3(-1, 1, 1);
            comFilts.Add(mir);
        }

        pommelFilt.mesh = UNWEIGHTEDCombineMeshsAndTextures(comFilts, null, out ts);
        ReColorAMesh(pommelFilt, hiltColor);

        pommelFilt.transform.localPosition = Vector3.forward * -handleLength;
        //Debug.Log("pos " + pommelFilt.transform.localPosition);
       

        pommelFilt.transform.localScale = new Vector3(1, 2.2f, 1) * width;
        comFilts.Clear();

        comFilts.Add(pommelFilt);
        comFilts.Add(hiltFilter);

        hiltFilter.mesh = UNWEIGHTEDCombineMeshsAndTextures(comFilts, null, out ts);
        AddCenterPiece();
    }

    void AddCenterPiece()
    {
        bool hasCenterPeice = decider5050();
        hasCenterPeice = true;

        if (hasCenterPeice)
        {

            List<MeshFilter> combFilts = new List<MeshFilter>();
            float pomWidth = subRandom(handleWidthPerc*.8f, handleWidthPerc*1.2f)/4;


            MeshFilter cen = arrangeGaurdPieces(1, 1, 0, 0, allowedCenters, pomWidth, pomWidth, false);

            cen.transform.localScale = new Vector3(1,.5f,1);
            cen.transform.localEulerAngles = new Vector3(0,90,0);
            combFilts.Clear();
            combFilts.Add(hiltFilter);
            combFilts.Add(cen);


            hiltFilter.mesh = UNWEIGHTEDCombineMeshsAndTextures(combFilts, null, out ts);

            ReColorAMesh(hiltFilter, hiltColor);

        }
    }


    #endregion

    void FinalCombine()
    {

        GameObject par = Instantiate(emptyGameObjectAAA, transform);
        bladeFilter.transform.parent = par.transform;
        hiltFilter.transform.parent = par.transform;
        handleFilter.transform.parent = par.transform;

        par.transform.localEulerAngles = new Vector3(0, 0, 90);


        List<MeshFilter> finalCom = new List<MeshFilter> ();
        finalCom.Add(bladeFilter); 
        finalCom.Add(hiltFilter);
        finalCom.Add(handleFilter);


        //Final = emptyFilt_AAA().gameObject;
        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(finalCom, null, out ts);
        Final.GetComponent<MeshRenderer>().material = MetalMaterial;

        MakeEnchantedDuplicate();

        //Final.GetComponent<weaponControllerAA>().enchantmentFilters.AddRange(enchantDuplicates);

        //bladeFilter, handleFilter, hiltFilter, spellFilter;

    }

    // 0 - 180 will point roughly to the right (90 = right)
    MeshFilter arrangeGaurdPieces(int minGCount, int maxGCount, float minRot, float maxRot, List<MeshFilter> allowedFilts, float minScale, float maxScale, bool ajustYScale)
    {
        int gQuarterCount = (int)subRandom(minGCount, maxGCount + 1);

        List<MeshFilter> guardFilters = new List<MeshFilter>();

        for (int i = 0; i < gQuarterCount; i++)
        {

            int randInd = (int)subRandom(0, allowedFilts.Count);

            float yRot = subRandom(minRot, maxRot);
            Vector3 scaleToBe = new Vector3(subRandom(minScale, maxScale), 1, subRandom(minScale, maxScale));
            if(ajustYScale)
            {
                scaleToBe.y = subRandom(minScale, maxScale);
            }
            scaleToBe.y -= scaleToBe.y * .01f * subRandom(0,10f);//to avoid clipping issues

            MeshFilter m = Instantiate(allowedFilts[randInd], transform);
            m.transform.localScale = scaleToBe;
            m.transform.localEulerAngles = Vector3.up * yRot;

            guardFilters.Add(m);
        }

        MeshFilter outFilt = Instantiate(emptyMeshPreAAA,transform).GetComponent<MeshFilter>();
        outFilt.mesh = UNWEIGHTEDCombineMeshsAndTextures(guardFilters, null, out ts);

        return outFilt;

    }



    void FindPointsAlongBase(out List<Vector3> points, bool invert, List<float> balances111, List<float> xOffs, SplineContainer baseCon111)
    {
        points = new List<Vector3>();



        for(int i = 0; i < balances111.Count; i++)
        {

            baseCon111.Evaluate(baseCon111[0], balances111[i], out pos, out st, out up);
            emptySampler.position = pos;

            emptySampler.localRotation = Quaternion.LookRotation(st, Vector3.up);

            if(invert)
            {
                emptySampler.localPosition -= Vector3.right * xOffs[i];

            }
            else
            {
                emptySampler.localPosition += Vector3.right * xOffs[i];

            }

            points.Add(emptySampler.localPosition);

        }
    }

    void CreateRandomBalances(out List<float> trueBalances, out List<float> trueXOffs)
    {
        List<float> MacroXOffs = new List<float>();

        int pointCount = balances.Count;

        for (int i = 0; i < pointCount; i++)
        {

            float xOff = subRandom(minEdgeXOFF, maxEdgeXOFF) * width;
            MacroXOffs.Add(xOff);
        }


        float ruffelChance = 1f / (pointCount-1f);

        int ruffelPointCount = (int)subRandom(minRuffelPointCount, maxRuffelPointCount + 1);

        trueBalances = new List<float>();
        trueXOffs = new List<float>();


        for (int i = 0; i < pointCount-1; i++)//-1 is important
        {

            trueBalances.Add(balances[i]);
            trueXOffs.Add(MacroXOffs[i]);

            float ruffelDecider = subRandom(0, 1f);

            if (hasRuffels && ruffelDecider <= ruffelChance)
            {
                //float dist = balances[i + 1] - balances[i];

                for (int r = 1; r < ruffelPointCount + 1; r++)
                {
                    float perc = (float)r / (ruffelPointCount + 1f);

                    float bal = Mathf.Lerp(balances[i], balances[i + 1], perc);
                    float xOff = Mathf.Lerp(MacroXOffs[i], MacroXOffs[i + 1], perc);

                    //xOff -= subRandom(0, maxRuffelOffsetPerc_Local) * dist;
                    if(r == 1 || r == ruffelPointCount)
                    {
                        xOff -= subRandom(0, maxRuffel_INNER_OffsetPerc_Local) * xOff;
                        //xOff += 0;
                    }
                    else
                    {
                        xOff += subRandom(0,maxRuffel_OUTER_OffsetPerc_Local) * xOff;

                    }

                    trueBalances.Add(bal);
                    trueXOffs.Add(xOff);
                }
            }

        }
        trueBalances.Add(1f);
        trueXOffs.Add(0f);
    }

}
