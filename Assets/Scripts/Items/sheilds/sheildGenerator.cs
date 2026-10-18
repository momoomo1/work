using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class sheildGenerator : MeleeParentScript
{
    //public int materialSeed = 0;
    //[SerializeField] MaterialRandomizer matRand;

    [SerializeField] Transform RidgeAndEdgeHolder;

    [Range(0f, 2f)]
    [SerializeField] float minRadPercent = .5f, maxRadPercent = 1f, minSidePercent = .5f, maxSidePercent = 1,
        minTopShare = .25f, maxTopShare = .5f, minSideShare = .5f, maxSideShare = 1.5f, minEdgeThickPercent, maxEdgeThickPercent,
        sheildActualThickPercent = .5f, minCenterObjPercent = .1f, maxCenterObjPercent = .2f, sheildDepthOffPercent = .25f,
        centerOBJChance = .8f;
    [SerializeField] int minQuaterPointCount = 3, maxQuaterPointCount = 4, meshResolution = 10, minQuarterRidges = 0, maxQuarterRidges = 3,
        minRoundRes = 3, maxRoundRes = 6, edgeMeshResolution = 50, numberOfShrinkingCenters = 4, centerObjMeshRes = 20,
        minConnecterCount = 2, maxConnecterCount = 8;
    bool verticalSymetry = false;
    bool quaterSymetry = false;
    bool hasRidges;
    bool straightRidges;
    [SerializeField] bool RidgeSymetry;
    float edgeThick,ridgeThick;
    //bool hasSharps = false;

    bool centerShapeMatches, hasEdgeContact, topEdgeContact;

    SplineContainer fullContainer;

    [SerializeField] GameObject emptyMeshPre;

    //GameObject topMesh, bottomMesh;

    //bool topRound = false;
    //bool bottomRount = false;
    //bool horizontalSymetry = false;

    [SerializeField] Transform emptyRotater;

    [SerializeField] SplineContainer containerPre;

    List<SplineContainer> topHalf = new List<SplineContainer>(), bottomHalf = new List<SplineContainer>();

    List<SplineContainer> fullRound;

    List<MeshFilter> finalFilters= new List<MeshFilter>();

    [SerializeField] bool overrideBool, hasCenterObj;
    
    public override void Begin()
    {

        base.Begin();



        verticalSymetry = decider5050();
        //hasSharps = decider5050();
        centerShapeMatches = decider5050();
        quaterSymetry = decider5050();
        RidgeSymetry = decider5050();
        hasRidges = decider5050();
        straightRidges = decider5050();
        centerShapeMatches = decider5050();
        hasEdgeContact = decider5050();
        topEdgeContact = decider5050();

        if (!overrideBool)
        {
            


            float chance = subRandom(0, 1f);
            if(chance < centerOBJChance)
            {
                hasCenterObj = true;
            }

        }

        

        //RidgeSymetry = true;

        edgeThick = subRandom(minEdgeThickPercent, maxEdgeThickPercent);
        ridgeThick = subRandom(minEdgeThickPercent, edgeThick);
        edgeThick *= thick;
        ridgeThick *= thick;

        float sideDist;

        CreateBasicShape(out sideDist);
        CreateEdgeAndRiges();
        if (hasCenterObj)
        {
            CreateCenterShape(sideDist);

        }

        //MeshFilter[] filts = Final.GetComponentsInChildren<MeshFilter>();
        //List<MeshFilter> filters = new List<MeshFilter>();
        //filters.AddRange(filts);

        //MeshFilter trueFinal = emptyFilt_AAA();
        MeshFilter trueFinal = Final.GetComponent<MeshFilter>();

        trueFinal.mesh = UNWEIGHTEDCombineMeshsAndTextures(finalFilters, null, out ts);

        trueFinal.GetComponent<MeshRenderer>().material = MetalMaterial;

        //Destroy(Final);
        //Final = trueFinal.gameObject;

        InactivateParaphanalia();

    }

    void CreateBasicShape(out float sideDist)
    {

        float topDist, bottomDist;

        topDist = subRandom(minTopShare, maxTopShare);
        bottomDist = 1 - topDist;

        topDist *= length;
        bottomDist *= length;

        sideDist = subRandom(minSideShare, maxSideShare) * (width/2);


        if (verticalSymetry)
        {
            topDist = length / 2;
            bottomDist = topDist;
        }
        if (quaterSymetry)
        {
            sideDist = topDist;
        }

        //Debug.Log("topDist " + topDist + " side dist " + sideDist);

        SplineContainer rh, lh, rh2, lh2;

        CreateHalf(out rh, out lh, topDist, sideDist);

        topHalf.Add(rh);
        topHalf.Add(lh);

        CreateHalf(out rh2, out lh2, bottomDist, sideDist);

        if (verticalSymetry)
        {
            rh2 = Instantiate(rh, transform);
            lh2 = Instantiate(lh, transform);
        }

        Vector3 rS = rh2.transform.localScale;
        rh2.transform.localScale = new Vector3(rS.x, -rS.y, rS.z);

        Vector3 lS = lh2.transform.localScale;
        lh2.transform.localScale = new Vector3(lS.x, -lS.y, lS.z);

        

        bottomHalf.Add(rh2);
        bottomHalf.Add(lh2);

        fullRound = new List<SplineContainer>();

        //fullRound.Add(topHalf[0]);
        //fullRound.Add(topHalf[1]);
        //fullRound.Add(bottomHalf[1]);
        //fullRound.Add(bottomHalf[0]);

        fullRound.Add(topHalf[1]);
        fullRound.Add(topHalf[0]);
        fullRound.Add(bottomHalf[0]);
        fullRound.Add(bottomHalf[1]);

        foreach (SplineContainer sc in fullRound)
        {
            sc.transform.localEulerAngles = new Vector3(0, 0, 90);
        }

        fullContainer = Instantiate(containerPre, transform);
        fullContainer.AddSpline(connectAlternatingSplinesRough(fullRound, 100, false));
        fullContainer[0].Closed = true;



        GameObject mainMesh = Instantiate(emptyMeshPre, transform);

        SplineContainer innerContainer = Instantiate(fullContainer, transform);
        innerContainer.transform.localScale = Vector3.zero;
        List<SplineContainer> containers = new List<SplineContainer>();
        containers.Add(fullContainer);
        containers.Add(innerContainer);

        mainMesh.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containers, meshResolution, false, true);
        mainMesh.transform.localEulerAngles = new Vector3(0, 0, -90);
        fullContainer.transform.localEulerAngles = new Vector3(0, 0, -90);


        ReColorAMesh(mainMesh.GetComponent<MeshFilter>(), bladeColor);


        mainMesh.GetComponent<MeshRenderer>().material = MetalMaterial;
        GameObject rev = Instantiate(mainMesh, transform);
        rev.transform.localScale = new Vector3(1, -1, 1);

        finalFilters.Add(mainMesh.GetComponent<MeshFilter>());
        finalFilters.Add(rev.GetComponent<MeshFilter>());

    }

    void CreateHalf(out SplineContainer topRight, out SplineContainer topLeft, float topDist, float sideDist)
    {

        int topQuarterCount = (int)subRandom(minQuaterPointCount, maxQuaterPointCount + 1);

        List<Vector3> points = new List<Vector3>();

        bool isRadial = decider5050(); //round or more square style



        if (isRadial)
        {
            for (int i = 0; i < topQuarterCount; i++)
            {

                float percent = (float)i / (topQuarterCount - 1);

                float maxOffset = Mathf.Lerp(topDist, sideDist, percent);

                emptyRotater.localEulerAngles = Vector3.right * percent * 90f;

                Vector3 pos = emptyRotater.up * subRandom(minRadPercent, maxRadPercent) * maxOffset;

                points.Add(pos);

            }
            points[points.Count - 1] = Vector3.forward * sideDist;
        }
        else
        {
            points.Add(new Vector3(0, topDist, 0));

            for (int i = 0; i < topQuarterCount; i++)
            {
                float percent = (float)i / (topQuarterCount - 1);

                float offset = subRandom(minSidePercent, maxSidePercent) * sideDist;

                if (i == 0)
                {
                    offset = subRandom(0f, maxSidePercent) * sideDist;
                }

                Vector3 point = new Vector3(0, (1f - percent) * topDist, offset);
                points.Add(point);
            }

            points[points.Count - 1] = Vector3.forward * sideDist;
        }


        topRight = Instantiate(containerPre, transform);
        topRight.AddSpline(randomSharpSpline(points, .01f, splineSharp));

        topLeft = Instantiate(topRight, transform);
        topLeft.transform.localScale = new Vector3(1, 1, -1);


    }

    void CreateEdgeAndRiges()
    {

        int ridgeCount = (int)subRandom(minQuarterRidges, maxQuarterRidges);
        int roundRes = (int)subRandom(minRoundRes, maxRoundRes);
        List<float> baseTimes = new List<float>();
        List<float> offsetStrDir = new List<float>();
        for(int i = 0; i < ridgeCount; i++)
        {
            baseTimes.Add(subRandom(0f, 1f));
            offsetStrDir.Add(subRandom(-.5f, .5f));
            if (straightRidges)
            {
                offsetStrDir[i] = 0f;
            }
        }

        float edgeSharp = subRandom(.01f, .5f);

        //edges first

        
        MeshFilter edgeMesh = CreateOuterEdge(fullContainer);
        edgeMesh.transform.parent = transform;

        finalFilters.Add(edgeMesh);


    }

    

    void CreateCenterShape(float sideDist)
    {

        sideDist *= 2f;

        bool hasSmallCenter = decider5050();
        bool hasLargeCenter = !hasSmallCenter;



        List<Transform> potenLargeCenter = new List<Transform>()
        {
            patternGen.HollowStar,patternGen.HollowPoly
        };
        List<Transform> potenSmallCenter = new List<Transform>()
        {
            patternGen.PolyPre, patternGen.StarPre
        };
        List<Transform> connecters = new List<Transform>()
        {
            patternGen.starArmPre, patternGen.hollowStarArmPre
        };

        if (hasSmallCenter)
        {
            Transform sc = Instantiate(potenSmallCenter[(int)subRandom(0, potenSmallCenter.Count)], transform);
            sc.transform.localScale = new Vector3(width/2, thick*1.25f, width/2);
            sc.transform.localEulerAngles = new Vector3(0, 0, 90);
            sc.transform.localPosition = Vector3.right * thick/4;

            ReColorAMesh(sc.GetComponent<MeshFilter>(), hiltColor);

            bool connect = decider5050();
            if (!connect)
            {
                connect = decider5050();
            }

            if(connect)
            {
                makeConnecters(connecters[(int)subRandom(0, connecters.Count)], fullContainer);
            }

            finalFilters.Add(sc.GetComponent<MeshFilter>());

        }
        if(hasLargeCenter)
        {
            Transform sc = Instantiate(potenLargeCenter[(int)subRandom(0, potenLargeCenter.Count)], transform);
            sc.transform.localScale = new Vector3(length, thick*.9f, length);
            sc.transform.localEulerAngles = new Vector3(0, 0, 90);
            sc.transform.localPosition = Vector3.right * thick / 4;

            ReColorAMesh(sc.GetComponent<MeshFilter>(), hiltColor);

            finalFilters.Add(sc.GetComponent <MeshFilter>());

        }


    }

    void makeConnecters(Transform conPre, SplineContainer outerCon)
    {
        int conCount = (int)subRandom(minConnecterCount, maxConnecterCount + 1);

        float fCon = conCount;
        fCon /= 2f;
        fCon -= .01f;
        if( fCon > conCount / 2)
        {
            conCount++;
        }

        for(int i = 0; i < conCount; i++)
        {
            float perc = i/(conCount-0f);

            outerCon.Evaluate(outerCon[0], perc, out pos, out st, out up);

            Transform t = Instantiate(conPre, transform);

            float dist = Vector3.Distance(Vector3.zero, pos);

            t.localScale = new Vector3(dist, thick*.8f-(.01f * i), dist);
            t.rotation = Quaternion.LookRotation(math.normalize(pos), Vector3.right);

            t.localPosition = Vector3.right * thick / 4;

            ReColorAMesh(t.GetComponent<MeshFilter>(), handleColor);

            finalFilters.Add(t.GetComponent <MeshFilter>());

        }

    }


    MeshFilter CreateOuterEdge(SplineContainer fullCon)
    {
        
        SplineContainer frontInner = Instantiate(fullCon, transform);
        SplineContainer frontOuter = Instantiate(fullCon, transform);
        SplineContainer backInner = Instantiate(fullCon, transform);
        SplineContainer backOuter = Instantiate(fullCon, transform);


        frontInner.transform.localPosition = Vector3.right * thick / 2;
        frontOuter.transform.localPosition = Vector3.right * thick / 2;
        frontOuter.transform.localScale += Vector3.one * edgeThick*2;


        backInner.transform.localPosition = -Vector3.right * thick / 2;
        backOuter.transform.localPosition = -Vector3.right * thick / 2;
        backOuter.transform.localScale += Vector3.one * edgeThick*2;

        List<SplineContainer> containers = new List<SplineContainer>()
        {
            frontInner, frontOuter, backOuter, backInner, frontInner
        };

        MeshFilter m = emptyFilt_AAA();
        m.mesh = GenerateMeshFromSplineContainerList(containers, meshResolution, false, false);
        MakeMeshShadeFlat(m);
        ReColorAMesh(m, hiltColor);

        return m;
    }

}
