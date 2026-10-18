using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;


public class SwordBladeGen : MetalShapeGen
{
    public bool symetryOverride;
    public Material blankMetalMat;

    [SerializeField] SplineContainer bladeCurve;
    [SerializeField] SplineContainer outline1Container;
    [SerializeField] SplineContainer outline2Container;
    [SerializeField] SplineContainer quarter1Profile;
    [SerializeField] SplineContainer quarter2Profile;

    [SerializeField] GameObject profileParentPre;

    //List<Transform> profileLocationsAlongLength = new List<Transform>();
    List<SplineContainer> frontProfiles = new List<SplineContainer>();
    List<SplineContainer> backProfiles = new List<SplineContainer>();

    [SerializeField] bool symetricalProfile;
    [SerializeField] bool symetricalOutline;
    [SerializeField] bool straightCurve;
    [SerializeField] bool doubleEdged;
    [SerializeField] bool consistantProfileTapor;
    //[SerializeField] bool consistantOutlineTapor;
    [SerializeField] bool sharpEdgeOutline;
    [SerializeField] bool flatEdgeTop;
    bool canBeReallyCurvy = false;

    [SerializeField] int minBladeCurvePoints, maxBladeCurvePoints;
    [Header("(no need for min, since it's just turned negative)")]
    [SerializeField] float maxBladeCurveOff_Percent;

    [SerializeField] int maxEdgeCurvePoints;

    int curvePointCount;

    [SerializeField] int minProfileQuarterPointCount, maxProfileQuarterPointCount;

    int out1PointCount;
    int out2PointCount;

    [SerializeField] int minLengthResolution, maxLengthResolution;
    [SerializeField] int minWidthRes, maxWidthRes;

    [SerializeField] GameObject frontObj;
    [SerializeField] GameObject backObj;

    float startWidth;
    float startThick;

    public override void Begin()
    {
        base.Begin();

        symetricalProfile = decider5050();
        symetricalOutline = decider5050();
        straightCurve = decider5050();
        doubleEdged = decider5050();
        consistantProfileTapor = decider5050();
        sharpEdgeOutline = decider5050();
        flatEdgeTop = decider5050();
        canBeReallyCurvy = decider5050();
        //flatEdgeTop = false;
        //consistantProfileTapor = false;
        if (symetryOverride)
        {
            symetricalProfile = true;
            symetricalOutline = true;
            straightCurve = true;
            doubleEdged = true;
        }

        CreateBladeCurve();
        CreateOutlineHalves();
        CreateAndArrangeProfile();
        GenerateMesh();




        GameObject g = Instantiate(GenerateAGuardForABlade(decider5050(), decider5050(), decider5050(), straightCurve, startWidth, startThick, bladeCurve, blankMetalMat),Final.transform);

        thick *= 2;
        width *= 2;

    }

    void CreateBladeCurve()
    {

        curvePointCount = (int)subRandom(minBladeCurvePoints, maxBladeCurvePoints + 1);

        if(curvePointCount == 2)
        {
            straightCurve = true;
        }

        List<Vector3> curvePoints = new List<Vector3>();

        for(int i = 0; i < curvePointCount; i++)
        {
            float yPos = (float)i / (curvePointCount-1)*length;

            float xPos = 0;

            if (canBeReallyCurvy)
            {
                xPos = subRandom(-maxBladeCurveOff_Percent, maxBladeCurveOff_Percent)*length/4;
                minLengthResolution = maxLengthResolution;
            }
            else
            {
                xPos = subRandom(-maxBladeCurveOff_Percent, maxBladeCurveOff_Percent)*width;
            }

            if(straightCurve)
            {
                xPos = 0;
            }

            //curvePoints.Add(new Vector3(xPos, yPos,0));
            curvePoints.Add(new Vector3(0, xPos, yPos));

        }
        curvePoints[0] = Vector3.zero;
        bladeCurve.AddSpline(newSpline1(curvePoints, splineSharp));
    }

    void CreateOutlineHalves()
    {
        //create each blade half at same time. so that even if it's not a straight blade, but the blades are suppose to be symetrical,
        //we can just use the same distance for both halves

        int topSub = 1;
        if (flatEdgeTop)
        {
            topSub = 2;
        }

        out1PointCount = (int) subRandom(curvePointCount, maxEdgeCurvePoints + 1);
        out2PointCount = (int) subRandom(curvePointCount, maxEdgeCurvePoints + 1);

        List<Vector3> out1Points = new List<Vector3>();
        List<Vector3> out2Points = new List<Vector3>();

        if(out1PointCount < 3 || out2PointCount < 3)
        {
            topSub = 1;
            flatEdgeTop = false;
        }

        if(symetricalOutline == true)
        {
            out2PointCount = out1PointCount;
        }

        //Debug.Log("out1 point count " + out1Points.Count);

        for(int i = 0; i < out1PointCount; i++)
        {

            float3 pos, st, up;

            bladeCurve.Evaluate(bladeCurve[0], (float)i/(out1PointCount-topSub), out pos, out st, out up);


            float localWidth = subRandom(minW, width);

            

            if (i == out1PointCount - 1)
            {
                localWidth = 0;
            }
            Vector3 o1Pos = pos + (up* localWidth);
            out1Points.Add(o1Pos);
            if (symetricalOutline)
            {
                Vector3 o2Pos = pos + (-up* localWidth);
                out2Points.Add(o2Pos);
            }

            //if (localWidth > startWidth)
            //    startWidth = localWidth;

            if(i == 0)
            {
                startWidth = localWidth;
            }

        }


        if(symetricalOutline == false)
        {

            for (int i = 0; i < out2PointCount; i++)
            {

                float3 pos, st, up;

                bladeCurve.Evaluate(bladeCurve[0], (float)i / (out2PointCount -topSub), out pos, out st, out up);

                float localWidth = subRandom(minW, width);

                if(i == 0)
                {

                    startWidth = (startWidth + localWidth) / 2;

                }

                if (i == out2PointCount - 1)
                {
                    localWidth = 0;
                }


                Vector3 o2Pos = pos + (-up * localWidth);



                out2Points.Add(o2Pos);

                if (localWidth > startWidth)
                    startWidth = localWidth;

            }

            

        }

        float sharp = splineSharp;

        if (sharpEdgeOutline)
        {
            sharp = subRandom(.001f, splineSharp);
        }

        outline1Container.AddSpline(newSpline1(out1Points, sharp));
        outline2Container.AddSpline(newSpline1(out2Points, sharp));

    }

    void CreateAndArrangeProfile()
    {

        int profile1PointCount = (int)subRandom(minProfileQuarterPointCount, maxProfileQuarterPointCount + 1);
        int profile2PointCount = (int)subRandom(minProfileQuarterPointCount, maxProfileQuarterPointCount + 1);

        List<Vector3> pro1Points = new List<Vector3>();
        List<Vector3> pro2Points = new List<Vector3>();

        for(int i = 0; i < profile1PointCount; i++)
        {
            //height is along the y
            //displace along the x

            float yPos = (float)i / (profile1PointCount - 1);

            float xPos = subRandom(.01f, 1f);

            pro1Points.Add(new Vector3(xPos, yPos, 0f));

        }
        pro1Points[pro1Points.Count-1] = Vector3.up;
        if (symetricalProfile)
        {
            pro2Points = pro1Points;
        }
        else
        {
            for (int i = 0; i < profile2PointCount; i++)
            {
                //height is along the y
                //displace along the x

                float yPos = (float)i / (profile2PointCount - 1);

                float xPos = subRandom(.01f, 1f);

                pro2Points.Add(new Vector3(xPos, yPos, 0f));

            }
            pro2Points[0] = pro1Points[0];

            pro2Points[pro2Points.Count - 1] = Vector3.up;

            if (!doubleEdged)
            {
                pro2Points[pro2Points.Count - 2] = new Vector3(1,1,0);

            }

        }

        float sharp = splineSharp;

        if (sharpEdgeOutline)
        {
            sharp = subRandom(.001f, splineSharp);
        }

        quarter1Profile.AddSpline(newSpline1(pro1Points, sharp));
        quarter2Profile.AddSpline(newSpline1(pro2Points, sharp));

        quarter2Profile.transform.localScale = new Vector3(1,-1,1);

        int profileDupCount = (int)subRandom(minLengthResolution,maxLengthResolution+1);
        //if(out2PointCount > out1PointCount)
        //{
        //    profileDupCount = out2PointCount;
        //}

        for(int i = 0; i < profileDupCount; i++)
        {
            Transform t = Instantiate(profileParentPre, transform).transform;

            float Percent = (float)i / (profileDupCount - 1);

            float3 pos, st, up;

            outline1Container.Evaluate(outline1Container[0], Percent, out pos, out st, out up);

            Vector3 frontEdge = pos;

            outline2Container.Evaluate(outline2Container[0], Percent, out pos, out st, out up);

            Vector3 backEdge = pos;

            float edgeDist = Vector3.Distance(frontEdge, backEdge);

            float localThick = edgeDist / (width * 2);
            localThick *= thick;

            if (consistantProfileTapor)
            {
                localThick = (1 - Percent) * thick;
            }

            //if (localThick > startThick)
            //    startThick = localThick;
            if(i == 0)
            {
                startThick = localThick;
            }

            Vector3 averagePos = (frontEdge + backEdge) / 2f;

            t.localScale = new Vector3(localThick,edgeDist,edgeDist)/2;

            t.localPosition = averagePos;

            Vector3 dir = (backEdge-frontEdge).normalized;
            //Debug.Log("dir pre " + dir);



            dir = new Vector3(dir.x, dir.z, dir.y);
            //Debug.Log("dir post " + dir);
            t.rotation = Quaternion.LookRotation(dir);
            t.eulerAngles += new Vector3(0, 180, 0);

            frontProfiles.Add(t.GetChild(0).GetComponent<SplineContainer>());
            backProfiles.Add(t.GetChild(1).GetComponent<SplineContainer>());

            //profileLocationsAlongLength.Add(t);
        }
    }

    void GenerateMesh()
    {

        int res = (int)subRandom(minWidthRes, maxWidthRes+1);

        frontObj.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(frontProfiles, res, true,true);
        backObj.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(backProfiles, res, false,true);

        GameObject g = Instantiate(frontObj, transform);
        g.transform.localScale = new Vector3(-1, 1, 1);


        GameObject g1 = Instantiate(backObj, transform);
        g1.transform.localScale = new Vector3(-1, 1, 1);

        List<MeshFilter> filters = new List<MeshFilter>();
        filters.Add(frontObj.GetComponent<MeshFilter>());
        filters.Add(backObj.GetComponent<MeshFilter>());

        filters.Add(g.GetComponent<MeshFilter>());
        filters.Add(g1.GetComponent<MeshFilter>());

        List<Texture2D> tex = null;

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out tex);



    }

}
