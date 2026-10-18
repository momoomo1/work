using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class gunGenerator : MeleeParentScript
{

    [SerializeField] Transform emptyRotater;
    Transform preFinal;

    public float minStockHandleThickPercent = .1f, maxStockHandleThickPercent = .3f;

    [SerializeField] float stockMetalMassThresh = 1, minBarrelRadiusPercent, maxBarrelRadiusPercent, 
        minBarrelLengthPercent, maxBarrelLengthPercent, minB_BandLengthPercent=.025f, maxB_BandLengthPercent=.05f, bbOverrideMinPercent = .25f,
        minBandEdgeThickPercent = .07f, maxBandEdgeThickPercent = 1.25f, minStickUpPercent = .3f, maxStickUpPercnet = .8f,
        minMuzzelThickPercent = .5f, maxMuzzelThickPercent = 1.5f, minMuzzelBarrelLengthPercent = .075f, maxMuzzelBarrelLengthPercent = .15f,
        minStockHeightWidthPercent = 2, maxStockEndWidthPercent = 3, stockEndMaxOffsetPercent = 3, stockMidMaxOffsetPercent = 1.5f, 
        minStockLengthPercent = .2f, maxStockLengthPercent = .5f, minGripPosPercent= .25f, maxGripPosPercent = .75f, 
        minHandleThickPercent = .3f, maxHandleThickPercent = .7f, minCrossSecOffsetPercent = .75f, maxCrossSecOffsetPercent = 1.25f,
        minHandleBarrelLengthPercent = .25f, maxHandleBarrelLengthPercent = .75f,
        triggerMinThickPerc=.1f, triggerMaxThickPerc=.2f;

    [SerializeField] int minNumberOfBarrels = 1, maxNumberOfBarrels = 4, minBarrelBandCount, maxBarrelBandCount,
        cylinderResolution, minCylQuarterPointCount = 3, maxCylQuarterPointCount = 5,
        stockLengthResolution = 25, stockWrapResolution = 12;

    [SerializeField] bool hasMultipleBarrels = false;
    [SerializeField] bool barrelsDiffrentLength = false, createWithoutBarrel;
    bool hasStock;
    //bool hasOverHandle;
    [SerializeField] bool irregularBarrelShape;
    [SerializeField] bool matchingBarrels;

    [SerializeField] bool barrelBandsStickUp, barrelBandSizeOverride, b_BandsPerfectlyVertical;

    SplineContainer LockAndTriggerPreToBe;

    //Material mainMetal, altMetal, woodMat;

    float stockGripDist, barrelStartRadius, barrelEndRadius, longestBarrelDist, shortestBarrelDist;

    float bandLengthPercent, frontTiltDistPercent, bandEdgePercentThick, stickUpPos,
        muzzelNeckThickPercent, muzzelTipThickPercent, muzNeckBalance, muzzelLengthPercent;

    int bandCount;
    int numOfBarrels = 1;

    public float3 NeededEdgePoint;

    //List<MeshFilter> finalFilters = new List<MeshFilter>();

    public override void Begin()
    {
        base.Begin();

        //mainMetal = inMats[2];
        //altMetal = inMats[6];
        //woodMat = inMats[5];

        preFinal = Instantiate(emptyMeshPreAAA, transform).transform;

        hasMultipleBarrels = decider5050();
        barrelsDiffrentLength = decider5050();
        irregularBarrelShape = decider5050();
        matchingBarrels = decider5050();

        barrelBandsStickUp = decider5050();
        barrelBandsStickUp = true;
        barrelBandSizeOverride=decider5050();
        b_BandsPerfectlyVertical = decider5050();

        if (alotedMass >= stockMetalMassThresh)
        {
            hasStock = true;
        }

        stickUpPos = subRandom(0f,1f);

        if(width > thick)
        {
            thick = width;
        }
        else
        {
            width = thick;
        }
        muzNeckBalance = subRandom(0, 1f);
        muzzelLengthPercent = subRandom(minMuzzelBarrelLengthPercent, maxMuzzelBarrelLengthPercent);
        bandCount = (int)subRandom(minBarrelBandCount, maxBarrelBandCount+1);

        bandLengthPercent = subRandom(minB_BandLengthPercent, maxB_BandLengthPercent);

        bandEdgePercentThick = subRandom(minBandEdgeThickPercent, maxBandEdgeThickPercent);

        muzzelNeckThickPercent = subRandom(minMuzzelThickPercent, maxMuzzelThickPercent);
        muzzelTipThickPercent = subRandom(minMuzzelThickPercent, maxMuzzelThickPercent);

        if (barrelBandSizeOverride)
        {
            bandLengthPercent = 1f / bandCount;

            bandLengthPercent *= subRandom(bbOverrideMinPercent, 1f);
        }

        if (!b_BandsPerfectlyVertical)
        {
            frontTiltDistPercent = subRandom(0f, 1f);
        }
        else
        {
            frontTiltDistPercent = 0;
        }

        CreateBarrel();
        CreateStockOrHandle();
        //CreateMuzzel();
        //CreateLock();
        //CreateTrigger();

        MeshFilter[] filts = preFinal.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> filters = new List<MeshFilter>();
        filters.AddRange(filts);
        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        Final.GetComponent<MeshRenderer>().material = MetalMaterial;

        //Destroy(Final);

        //Final = preFinal.gameObject;

    }

    void CreateBarrel()
    {
        if (hasMultipleBarrels)
        {
            numOfBarrels = (int)subRandom(minNumberOfBarrels, maxNumberOfBarrels);
        }

        Transform barrelBase = Instantiate(emptyGameObjectAAA, preFinal).transform;

        barrelBase.position = Vector3.zero;

        barrelStartRadius = subRandom(minBarrelRadiusPercent, maxBarrelRadiusPercent);
        barrelEndRadius = subRandom(minBarrelRadiusPercent, barrelStartRadius);

        barrelStartRadius *= width;
        barrelEndRadius *= width;

        float uBarStart = barrelStartRadius/numOfBarrels;
        float uBarEnd = barrelEndRadius/numOfBarrels;

        longestBarrelDist = 0;
        shortestBarrelDist = maxBarrelLengthPercent * length;

        float remainingPercent = 1f;

        List<float> lengths = new List<float>();

        for(int i = 0; i < numOfBarrels; i++)
        {
            float usedP = 1f/(numOfBarrels);

            if (barrelsDiffrentLength)
            {
                usedP = subRandom(0f, 1f) * remainingPercent;
            }

            remainingPercent -= usedP;

            float bLength = Mathf.Lerp(minBarrelLengthPercent, maxBarrelLengthPercent, usedP) * length;

            lengths.Add(bLength);

            if(bLength > longestBarrelDist)
            {
                longestBarrelDist = bLength;
            }
            if(bLength < shortestBarrelDist)
            {
                shortestBarrelDist = bLength;
            }

        }

        int quarterRes = (int)subRandom(minCylQuarterPointCount, maxCylQuarterPointCount+1);


        if (!irregularBarrelShape)
        {
            quarterRes = minCylQuarterPointCount;
        }

        SplineContainer mainbarrelShape = Instantiate(containerPreAAA, transform);

        //SplineContainer quCon = Instantiate(containerPreAAA, transform);

        //List<Vector3> quarterPoints = new List<Vector3>();

        Vector3[] proPoints = new Vector3[(quarterRes*4)];

        for(int i = 0; i < quarterRes; i++)//base spline must be about a radius of .5 (diameter of 1) so it can be scaled correctly later
        {
            float time = (float)i / (quarterRes);

            float offsetPercent = subRandom(minBarrelRadiusPercent, maxBarrelRadiusPercent);

            emptyRotater.transform.localEulerAngles = new Vector3(0, 0, -90f * time);

            Vector3 basePos = emptyRotater.up * offsetPercent * .5f;

            //repeating not symetrical

            proPoints[i] = basePos;//top right
            proPoints[i + quarterRes] = new Vector3(basePos.y, -basePos.x, basePos.z);// bottom right

            proPoints[i + (quarterRes*2)] = new Vector3(-basePos.x,-basePos.y,basePos.z);// bottom left
            proPoints[i + (quarterRes*3)] = new Vector3(-basePos.y, basePos.x, basePos.z);// top left



        }
        //proPoints[proPoints.Length - 1] = proPoints[0];

        List<Vector3> pointConversion = new List<Vector3>();
        pointConversion.AddRange(proPoints);

        mainbarrelShape.AddSpline(newSpline1(pointConversion, .5f));
        mainbarrelShape[0].Closed = true;

        int lInd = 0;

        foreach (float f in lengths)
        {
            SplineContainer startContainer = Instantiate(containerPreAAA, transform);

            if (!matchingBarrels)
            {
                proPoints = new Vector3[(quarterRes * 4)];
                pointConversion.Clear();
                for (int i = 0; i < quarterRes; i++)//base spline must be about a radius of .5 (diameter of 1) so it can be scaled correctly later
                {
                    float time = (float)i / (quarterRes);

                    float offsetPercent = subRandom(minBarrelRadiusPercent, maxBarrelRadiusPercent);

                    emptyRotater.transform.localEulerAngles = new Vector3(0, 0, -90f * time);

                    Vector3 basePos = emptyRotater.up * offsetPercent * .5f;

                    proPoints[i] = basePos;//top right
                    proPoints[i + quarterRes] = new Vector3(basePos.y, -basePos.x, basePos.z);// bottom right

                    proPoints[i + (quarterRes * 2)] = new Vector3(-basePos.x, -basePos.y, basePos.z);// bottom left
                    proPoints[i + (quarterRes * 3)] = new Vector3(-basePos.y, basePos.x, basePos.z);// top left




                }
                //proPoints[proPoints.Length - 1] = proPoints[0];

                pointConversion = new List<Vector3>();
                pointConversion.AddRange(proPoints);

                startContainer.AddSpline(newSpline1(pointConversion,.5f));
                startContainer[0].Closed = true;
            }
            else
            {
                startContainer.AddSpline(mainbarrelShape[0]);
            }

            SplineContainer endContainer = Instantiate(startContainer, transform);

            endContainer.transform.localPosition = new Vector3(0, 0, f);

            startContainer.transform.localScale = Vector3.one * uBarStart;
            endContainer.transform.localScale = Vector3.one * uBarEnd;

            GameObject g = Instantiate(emptyMeshPreAAA, barrelBase.transform);

            SplineContainer capContainer = Instantiate(startContainer, transform);
            capContainer.transform.localScale = Vector3.zero;

            List<SplineContainer> conts = new List<SplineContainer>();
            conts.Add(capContainer);
            conts.Add(startContainer);
            conts.Add(endContainer);

            if (!createWithoutBarrel)
            {
                g.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(conts, cylinderResolution, false, true);
                //g.GetComponent<MeshRenderer>().material = mainMetal;
                g.GetComponent<MeshRenderer>().material = MetalMaterial;
                ReColorAMesh(g.GetComponent<MeshFilter>(), bladeColor);

            }

            float percentage = (float)lInd / (lengths.Count);

            CreateBarrelBands(g.transform, f, startContainer, uBarStart, uBarEnd);
            if (!createWithoutBarrel)
            {
                CreateMuzzel(endContainer, f, g.transform);

            }

            if (!matchingBarrels)
            {
                g.transform.localEulerAngles = new Vector3(0, 0, subRandom(0, 360));
            }
            else
            {
                g.transform.localEulerAngles = new Vector3(0, 0, 360f * percentage);
            }

            //Debug.Log("percent " + lInd);

            emptyRotater.transform.localEulerAngles = new Vector3(0, 0, 360f * percentage);

            if(lengths.Count > 1)
            {
                g.transform.localPosition = emptyRotater.up * (uBarStart / 2);

            }


            lInd++;

        }

        if(lengths.Count != 3)
        {
            barrelBase.transform.localEulerAngles = new Vector3(0, 0, 360f*(1f / (lengths.Count*2)));

        }
    }

    void CreateBarrelBands(Transform finalParent, float currentLength, SplineContainer crossSec, float startThick,float endThick)
    {
        //create base circle or loop or half

        List<MeshFilter> BandMeshToCombine = new List<MeshFilter>();

        float scaleToBe = Mathf.Lerp(1, 1.41421f, frontTiltDistPercent);
        scaleToBe += .01f;//just so there isn't any clipping issues
        float xRot = Mathf.Lerp(0f, 45f, frontTiltDistPercent);

        for (int i =0; i < bandCount; i++)
        {


            float Percent = ((float)i ) / (bandCount );

            Transform baseT = Instantiate(emptyMeshPreAAA, transform).transform;


            Transform a = Instantiate(crossSec.transform, baseT);
            Transform b = Instantiate(crossSec.transform, baseT);
            a.localEulerAngles = new Vector3(xRot, 0, 0);
            b.localEulerAngles = new Vector3(xRot, 0, 0);
            float perPartStartScale = scaleToBe;
            float perPartStartXScale = 1.01f;

            perPartStartScale *= Mathf.Lerp(startThick, endThick, Percent - (bandLengthPercent/2));
            perPartStartXScale *= Mathf.Lerp(startThick, endThick, Percent - (bandLengthPercent / 2));

            float perPartEndScale = scaleToBe * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 2));
            float perPartEndXScale = 1.01f * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 2));

            a.localScale = new Vector3(perPartStartXScale, perPartStartScale, perPartStartScale);
            b.localScale = new Vector3(perPartEndXScale, perPartEndScale, perPartEndScale);

            a.localPosition = new Vector3(0, 0, -bandLengthPercent * (currentLength)/2f);
            b.localPosition = new Vector3(0, 0, bandLengthPercent * (currentLength)/2f);

            //Debug.Log("length " + currentLength);

            MeshFilter bandEdgeA = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
            MeshFilter bandEdgeB = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();



            List<SplineContainer> containers = new List<SplineContainer>();
            containers.Add(a.GetComponent<SplineContainer>());
            containers.Add(b.GetComponent<SplineContainer>());

            baseT.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containers, cylinderResolution, false, false);

            baseT.localPosition = Vector3.forward * currentLength * Percent;


            bandEdgeA.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartStartScale * bandEdgePercentThick, false,
                a.GetComponent<SplineContainer>(), a.GetComponent<SplineContainer>()[0], Vector3.one);
            bandEdgeB.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartEndScale * bandEdgePercentThick, false,
                b.GetComponent<SplineContainer>(), b.GetComponent<SplineContainer>()[0], Vector3.one);


            if (barrelBandsStickUp)
            {
                //create smaller copy of the edgebands, and place it right between the other 2 bands
                //then make the smaller one move up, so that it looks like a sight(keep the forward rot)

                SplineContainer stickUpContainer = Instantiate(a, baseT).GetComponent<SplineContainer>();
                stickUpContainer.transform.localScale /= 2f;
                //stickUpContainer.transform.localPosition = Vector3.Lerp(a.localPosition, b.localPosition, stickUpPos);


                float3 pos, st, up;

                b.GetComponent<SplineContainer>().Evaluate(b.GetComponent<SplineContainer>()[0], .5f, out pos, out st, out up);

                pos.z = Mathf.Lerp(a.position.z, b.position.z, stickUpPos);

                pos.y -= stickUpContainer.transform.localScale.y / 2;

                stickUpContainer.transform.position = pos;

                //stickUpContainer.transform.localPosition += Vector3.up * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 5));
                MeshFilter stickUpBand = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

                stickUpBand.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartEndScale * bandEdgePercentThick *.5f, false,
                stickUpContainer, stickUpContainer[0], Vector3.one);
                BandMeshToCombine.Add(stickUpBand);
            }



            BandMeshToCombine.Add(baseT.GetComponent<MeshFilter>());
            BandMeshToCombine.Add(bandEdgeA);
            BandMeshToCombine.Add(bandEdgeB);

            //BandMeshToCombine.Add(//and also the stick up band);!!!!!!!


        }

        MeshFilter combineAllTheseMeshes = Instantiate(emptyMeshPreAAA, preFinal).GetComponent<MeshFilter>();

        List<Texture2D> ts = null;

        combineAllTheseMeshes.mesh = UNWEIGHTEDCombineMeshsAndTextures(BandMeshToCombine, null, out ts);

        combineAllTheseMeshes.transform.parent = finalParent;

        combineAllTheseMeshes.transform.localPosition += new Vector3(0, 0, bandLengthPercent * (currentLength) / 2f);

        //combineAllTheseMeshes.GetComponent<MeshRenderer>().material = altMetal;
        combineAllTheseMeshes.GetComponent<MeshRenderer>().material = MetalMaterial;
        ReColorAMesh(combineAllTheseMeshes.GetComponent<MeshFilter>(), hiltColor);

        for (int i = BandMeshToCombine.Count-1; i >= 0; i--)
        {
            Destroy(BandMeshToCombine[i].gameObject);
        }

    }

    void CreateStockOrHandle()
    {
        //create vertical half cross section spline

        int crossSecQuarterPointCount = (int)subRandom(minCylQuarterPointCount, maxCylQuarterPointCount + 1);
        //float widthToUse = (width / 2f) * ((numOfBarrels - 1f) / numOfBarrels);
        float widthToUse = barrelStartRadius;
        //barrelStartRadius
        if (numOfBarrels >= 2)
        {
            widthToUse /=2;

        }

        List<Vector3> points = new List<Vector3>();

        for(int i = 0; i < crossSecQuarterPointCount; i++)
        {
            float percent = (float)i/(crossSecQuarterPointCount - 1);

            emptyRotater.localEulerAngles = new Vector3(0, 0, percent * 90);

            Vector3 offPos = emptyRotater.up * (subRandom(minCrossSecOffsetPercent, maxCrossSecOffsetPercent));

            points.Add(offPos);
        }

        SplineContainer qu = Instantiate(containerPreAAA, transform);

        qu.AddSpline(randomSharpSpline(points, .01f, .5f));
        SplineContainer qu2 = Instantiate(qu, transform);

        qu2.transform.localScale = new Vector3(-1, 1, 1);

        List<SplineContainer> qu3 = new List<SplineContainer>();
        qu3.Add(qu);
        qu3.Add(qu2);

        Transform CrossSec = Instantiate(emptyGameObjectAAA, transform).transform;
        Transform topHalfParent = Instantiate(emptyGameObjectAAA, CrossSec).transform;
        Transform botHalfParent = Instantiate(emptyGameObjectAAA, CrossSec).transform;

        

        SplineContainer tHalf = Instantiate(containerPreAAA, topHalfParent);
        tHalf.AddSpline(connectAlternatingSplinesRough(qu3, crossSecQuarterPointCount * 4, false));


        SplineContainer bHalf = Instantiate(tHalf, botHalfParent);
        bHalf.transform.localScale = new Vector3(1, -1, 1);

        

        tHalf.transform.localEulerAngles = new Vector3(90, 0, 0);
        bHalf.transform.localEulerAngles = new Vector3(-90, 0, 0);

        LockAndTriggerPreToBe = Instantiate(bHalf, transform);

        //LockAndTriggerPreToBe.transform.localEulerAngles += new Vector3(0, 0, 90);

        //create main stock curve spline

        SplineContainer mainStockContainer = Instantiate(containerPreAAA, transform);

        float handleLengthPercent = subRandom(minHandleBarrelLengthPercent, maxHandleBarrelLengthPercent);
        float stockLengthPercent = subRandom(minStockLengthPercent, maxStockLengthPercent);

        float fullPer = handleLengthPercent + stockLengthPercent;

        float realHandlePercent = handleLengthPercent / (fullPer);
        float realStockPercent = stockLengthPercent / (fullPer);

        Vector3 HandleTip = Vector3.forward * handleLengthPercent * shortestBarrelDist;
        Vector3 center = Vector3.zero;
        Vector3 stockEnd = -Vector3.forward * stockLengthPercent * length;
        stockEnd.y = -subRandom(0, stockEndMaxOffsetPercent) * width;
        Vector3 stockCurve = -Vector3.forward * stockLengthPercent * length / 2;
        stockCurve.y = subRandom(0, stockEnd.y);

        List<Vector3> mainCurPoints = new List<Vector3>();
        mainCurPoints.Add(HandleTip);
        mainCurPoints.Add(center + Vector3.forward * .001f);
        mainCurPoints.Add(center);
        mainCurPoints.Add(stockCurve);
        mainCurPoints.Add(stockEnd);

        mainStockContainer.AddSpline(newSpline1(mainCurPoints, .5f));

        //create top stock curve

        float stockEndHeightHalf = subRandom(minStockHeightWidthPercent, maxStockEndWidthPercent) * thick;
        stockEndHeightHalf /= 2f;
        Vector3 topEdgeHandle = Vector3.forward * handleLengthPercent * shortestBarrelDist;
        Vector3 topMidBottomEdge = Vector3.zero;
        //Vector3 topMidTopEdge = Vector3.up * widthToUse;
        Vector3 topMidTopEdge = Vector3.up * barrelStartRadius/2;

        Vector3 topEdgeStockEnd = stockEnd;
        topEdgeStockEnd.y += stockEndHeightHalf;

        List<Vector3> topEdgePoints = new List<Vector3>();
        topEdgePoints.Add(topEdgeHandle);
        topEdgePoints.Add(topMidBottomEdge + Vector3.forward * .01f);

        topEdgePoints.Add(topMidBottomEdge);
        topEdgePoints.Add(topMidTopEdge);

        topEdgePoints.Add(stockCurve + (Vector3.up * Mathf.Lerp(thick / 2, stockEndHeightHalf, .5f)));

        topEdgePoints.Add(topEdgeStockEnd);

        SplineContainer topEdgeCont = Instantiate(containerPreAAA, transform);
        topEdgeCont.AddSpline(newSpline1(topEdgePoints, .5f));

        //create bottom stock curve

        float handleThick = subRandom(minHandleThickPercent, maxHandleThickPercent);

        Vector3 botEdgeHandle = Vector3.forward * handleLengthPercent * shortestBarrelDist;
        Vector3 botEdgeEndLow = Vector3.forward * handleLengthPercent * shortestBarrelDist * .9f;
        Vector3 botEdgeThickest = Vector3.down * handleThick * thick;
        botEdgeEndLow += Vector3.down * subRandom(0f, handleThick * thick);


        //botEdgeHandle.y -= width / 2;
        botEdgeEndLow.y -= widthToUse;
        botEdgeThickest.y -= widthToUse;
        botEdgeThickest.z = shortestBarrelDist * subRandom(minGripPosPercent, maxGripPosPercent);
        Vector3 BotEdgeStockEnd = stockEnd;
        BotEdgeStockEnd += Vector3.down * stockEndHeightHalf;
        Vector3 BotEdgeStockCurveCon = stockCurve- (Vector3.up * Mathf.Lerp(handleThick * thick, stockEndHeightHalf, .5f));

        //Vector3 BotEdgeGrip = Vector3.Lerp(botEdgeThickest, BotEdgeStockCurveCon, subRandom(minGripPosPercent, maxGripPosPercent));
        Vector3 BotEdgeGrip = Vector3.zero;

        BotEdgeGrip.y = Mathf.Lerp( -widthToUse, botEdgeThickest.y, subRandom(0, 1f));

        List<Vector3> botEdgePoints = new List<Vector3>();

        botEdgePoints.Add(botEdgeHandle);
        botEdgePoints.Add(botEdgeEndLow);
        botEdgePoints.Add(botEdgeThickest);
        botEdgePoints.Add(BotEdgeGrip);
        botEdgePoints.Add(BotEdgeStockCurveCon);
        botEdgePoints.Add(BotEdgeStockEnd);


        SplineContainer botEdgeCont = Instantiate(containerPreAAA, transform);
        botEdgeCont.AddSpline(newSpline1(botEdgePoints, .5f));

        LockAndTriggerPreToBe.transform.position = BotEdgeGrip;
        float trigScale = maxStockHandleThickPercent * thick;
        LockAndTriggerPreToBe.transform.localScale = trigScale * Vector3.one;

        //place an array of spline cross sections along the main spline curves,
        //then scale the vertical halves of cross secs so they match up with 
        //top and bottom stock curves

        List<SplineContainer> TopCrossSecs = new List<SplineContainer>();
        List<SplineContainer> BotCrossSecs = new List<SplineContainer>();

        float stockHandleEndThickPercent = subRandom(minStockHandleThickPercent, maxStockHandleThickPercent);
        //float stockHandleEndThickPercent = 0;

        //topEdgeCont.transform.localEulerAngles = Vector3.forward * 90;
        topEdgeCont.transform.localPosition = Vector3.up * .01f*thick;

        //mainStockContainer.transform.localEulerAngles = Vector3.forward * 90;
        //botEdgeCont.transform.localEulerAngles = Vector3.forward * 90;

        for (int i = 0; i < stockLengthResolution; i++)
        {
            float percent = (float)i/(stockLengthResolution - 1);

            Transform currentCrossSec = Instantiate(CrossSec, transform);


            float3 main_pos, st, up, top_pos, bot_pos;

            mainStockContainer.Evaluate(mainStockContainer[0], percent, out main_pos, out st, out up);

            topEdgeCont.Evaluate(topEdgeCont[0], percent, out top_pos, out st, out up);

            botEdgeCont.Evaluate(botEdgeCont[0], percent, out bot_pos, out st, out up);


            currentCrossSec.position = main_pos;

            float thickPercent = Mathf.Lerp(stockHandleEndThickPercent, maxStockHandleThickPercent, (1f - percent) / realStockPercent);

            if (percent <= realHandlePercent)
            {
                thickPercent = Mathf.Lerp(stockHandleEndThickPercent, maxStockHandleThickPercent, percent / realHandlePercent);
            }
            

            Transform top = currentCrossSec.GetChild(0);
            Transform bot = currentCrossSec.GetChild(1);

            Vector3 topDir = top_pos - main_pos;
            topDir.Normalize();

            Vector3 botDir = bot_pos - main_pos;
            botDir.Normalize();

            top.rotation = Quaternion.LookRotation(topDir, Vector3.forward);//may cause the transform to do a 180 if up is normal up
            bot.rotation = Quaternion.LookRotation(botDir, Vector3.forward);

            float topYZ_scale = Vector3.Distance(main_pos, top_pos);
            float botYZ_scale = Vector3.Distance(main_pos, bot_pos);

            float x_scale = thickPercent * thick;

            top.localScale = new Vector3(x_scale, topYZ_scale, topYZ_scale);
            bot.localScale = new Vector3(x_scale, botYZ_scale, botYZ_scale);

            if (i == 0)
            {
                Transform t = Instantiate(currentCrossSec, transform);
                t.localScale = Vector3.zero;
                TopCrossSecs.Add(t.GetChild(0).GetComponentInChildren<SplineContainer>());
                BotCrossSecs.Add(t.GetChild(1).GetComponentInChildren<SplineContainer>());
            }

            TopCrossSecs.Add(top.GetComponentInChildren<SplineContainer>());
            BotCrossSecs.Add(bot.GetComponentInChildren<SplineContainer>());

            if (i == stockLengthResolution - 1)
            {
                Transform t = Instantiate(currentCrossSec, transform);
                t.localScale = Vector3.zero;
                TopCrossSecs.Add(t.GetChild(0).GetComponentInChildren<SplineContainer>());
                BotCrossSecs.Add(t.GetChild(1).GetComponentInChildren<SplineContainer>());
            }


        }
        MeshFilter topStockMesh = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        topStockMesh.mesh = GenerateMeshFromSplineContainerList(TopCrossSecs, stockWrapResolution, false, true);

        //topStockMesh.transform.localEulerAngles = Vector3.forward * -90;

        MeshFilter botStockMesh = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        botStockMesh.mesh = GenerateMeshFromSplineContainerList(BotCrossSecs, stockWrapResolution, false, true);

        //botStockMesh.transform.localEulerAngles = Vector3.forward * -90;

        List<MeshFilter> finalStock = new List<MeshFilter>();
        finalStock.Add(topStockMesh);
        finalStock.Add(botStockMesh);

        List<Texture2D> ts;

        MeshFilter finalStockMesh = Instantiate(emptyMeshPreAAA, preFinal).GetComponent<MeshFilter>();
        finalStockMesh.mesh = UNWEIGHTEDCombineMeshsAndTextures(finalStock, null, out ts);
        finalStockMesh.GetComponent<MeshRenderer>().material = MetalMaterial;
        ReColorAMesh(finalStockMesh.GetComponent<MeshFilter>(), handleColor);

        Destroy(topStockMesh.gameObject);
        Destroy(botStockMesh.gameObject);
    }
    void CreateMuzzel(SplineContainer barrelTipContainer, float currentBarrelLength, Transform parentToBe)
    {
        float muzzelNeck = muzzelNeckThickPercent;
        float muzzelTip = muzzelTipThickPercent;
        if (!matchingBarrels)
        {
            muzzelNeck = subRandom(minMuzzelThickPercent, maxMuzzelThickPercent);
            muzzelTip = subRandom(minMuzzelThickPercent, maxMuzzelThickPercent);
            muzNeckBalance = subRandom(0f, 1f);
            muzzelLengthPercent = subRandom(minMuzzelBarrelLengthPercent, maxMuzzelBarrelLengthPercent);
        }

        Transform neck = Instantiate(barrelTipContainer, transform).transform;
        Transform tip = Instantiate(barrelTipContainer, transform).transform;
        Transform hole = Instantiate(barrelTipContainer, transform).transform;
        Transform back = Instantiate(barrelTipContainer, transform).transform;

        List<SplineContainer> containers = new List<SplineContainer>();

        containers.Add(barrelTipContainer);
        containers.Add(neck.GetComponent<SplineContainer>());
        containers.Add(tip.GetComponent<SplineContainer>());
        containers.Add(hole.GetComponent<SplineContainer>());
        containers.Add(back.GetComponent<SplineContainer>());


        neck.localScale *= muzzelNeck;
        tip.localScale *= muzzelTip;
        hole.localScale *= minMuzzelThickPercent;
        back.localScale *= 0;

        tip.localPosition += Vector3.forward * currentBarrelLength * muzzelLengthPercent;
        neck.localPosition = Vector3.Lerp(neck.localPosition, tip.localPosition, muzNeckBalance);
        hole.localPosition = tip.localPosition;
        back.localPosition = Vector3.zero;

        MeshFilter barrelEndRing = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        MeshFilter MuzzelTipRing = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        MeshFilter MuzzelMesh = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        barrelEndRing.mesh = nonTaporStemMesh1(6, cylinderResolution, bandEdgePercentThick * barrelTipContainer.transform.localScale.y,
            false, barrelTipContainer, barrelTipContainer[0], Vector3.one);

        MuzzelTipRing.mesh = nonTaporStemMesh1(6, cylinderResolution, bandEdgePercentThick * barrelTipContainer.transform.localScale.y,
            false, tip.GetComponent<SplineContainer>(), tip.GetComponent<SplineContainer>()[0], Vector3.one);

        MuzzelMesh.mesh = GenerateMeshFromSplineContainerList(containers, cylinderResolution, false, false);

        List<MeshFilter> combineFilters = new List<MeshFilter>();
        combineFilters.Add(barrelEndRing);
        combineFilters.Add(MuzzelTipRing);
        combineFilters.Add(MuzzelMesh);

        List<Texture2D> t = null;

        MeshFilter ultamiteMesh = Instantiate(emptyMeshPreAAA, parentToBe).GetComponent<MeshFilter>();
        ultamiteMesh.mesh = UNWEIGHTEDCombineMeshsAndTextures(combineFilters, null, out t);

        //ultamiteMesh.GetComponent<MeshRenderer>().material = altMetal;
        ultamiteMesh.GetComponent<MeshRenderer>().material = MetalMaterial;
        ReColorAMesh(ultamiteMesh.GetComponent<MeshFilter>(), hiltColor);

        for (int i = combineFilters.Count - 1; i >= 0; i--)
        {
            Destroy(combineFilters[i].gameObject);
        }

    }
    void CreateLock()
    {

    }
    void CreateTrigger()
    {
        MeshFilter trigger = Instantiate(emptyMeshPreAAA, preFinal).GetComponent<MeshFilter>();
        trigger.mesh = nonTaporStemMesh1(6, cylinderResolution, subRandom(triggerMinThickPerc, triggerMaxThickPerc) * thick,false,
            LockAndTriggerPreToBe, LockAndTriggerPreToBe[0], Vector3.one);
    }
}
