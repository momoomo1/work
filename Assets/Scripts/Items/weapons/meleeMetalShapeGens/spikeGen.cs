using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class spikeGen : MetalShapeGen
{
    [SerializeField] SplineContainer mainCurve;
    [SerializeField] SplineContainer emptyContainerPre;

    [SerializeField] GameObject emptyMeshPre;
    [SerializeField] Transform PreMeshParent;
    [SerializeField] Transform rotator, offseter;
    [SerializeField] Transform ribParent;

    [SerializeField] float minSharp = .01f, maxSharp = .8f;
    [SerializeField] float maxMainOffset_Percent;
    [SerializeField] float minRibPointOffset, maxRibPointOffset;
    [SerializeField] float slightScaleChange = .01f;
    [SerializeField] float minThickWidthOffset_Percent, maxThickWidthOffset_Percent;
    [SerializeField] int minMainCurvePointCount, maxMainCurvePointCount, minMinorCurvePointCount, maxMinorCurvePointCount, minMinorSplines=2, maxMinorSplines=10;
    [SerializeField] int minMeshRes=5, maxMeshRes=10;
    [SerializeField] int minAxisOfSyms, maxAxisOfSyms;

    float maxMainOff;

    int meshRes;
    int numOfSymAxis;

    [SerializeField] bool isStraight = false;
    [SerializeField] bool isRightLeftSym = false;
    [SerializeField] bool isUpDownSym = false;
    bool sameMinorSpline = false;
    bool variedSharpness = false;
    bool singleNonNormalSharpness = false;
    bool variedThicknesses=false;
    [SerializeField] bool hasAxisSym = false;

    public override void Begin()
    {
        base.Begin();

        isStraight = decider5050();
        isRightLeftSym = decider5050();
        isUpDownSym = decider5050();
        sameMinorSpline = decider5050();
        variedSharpness = decider5050();
        singleNonNormalSharpness = decider5050();
        variedThicknesses = decider5050();
        maxMainOff = length * maxMainOffset_Percent;
        meshRes = (int)subRandom(minMeshRes, maxMeshRes + 1);
        if (!isStraight)
        {
            hasAxisSym = decider5050();
            numOfSymAxis = (int)subRandom(minAxisOfSyms, maxAxisOfSyms + 1);
        }

        //if (variedSharpness)
        //{
        //    if (isUpDownSym || isRightLeftSym)
        //    {
        //        variedSharpness = false;
        //    }
        //}


        CreateMainSpline();
        CreateSplineCage();
        CombineAllMeshes();
    }

    void CreateMainSpline()
    {
        int mainPointCount = (int)subRandom(minMainCurvePointCount, maxMainCurvePointCount + 1);

        List<Vector3> mainPoints = new List<Vector3>();

        if (isStraight)
        {
            mainPoints.Add(Vector3.zero);
            mainPoints.Add(Vector3.forward * length);
        }
        else
        {
            for (int i = 0; i < mainPointCount; i++)
            {
                float percent = (float)i / (mainPointCount - 1);

                float xPos = subRandom(-maxMainOff, maxMainOff);
                float yPos = subRandom(-maxMainOff, maxMainOff);
                float zPos = percent * length;
                mainPoints.Add(new Vector3(xPos, yPos, zPos));

            }

            mainPoints[0] = Vector3.zero;
            mainPoints[mainPoints.Count - 1] = Vector3.forward * length;

        }

        mainCurve.AddSpline(CreateBasedOnParameters(mainPoints));

        

    }

    void CreateSplineCage()
    {
        int splineRibCount = (int)subRandom(minMinorCurvePointCount, maxMinorCurvePointCount + 1);

        if (sameMinorSpline)
        {
            List<Vector3> ribCopyPointList = CreateRibPointList(); 

            Transform copContainer = Instantiate(emptyContainerPre, ribParent).transform;

            copContainer.GetComponent<SplineContainer>().AddSpline(CreateBasedOnParameters(ribCopyPointList));

            copContainer.GetComponent<SplineContainer>()[0].Closed = true;

            List<SplineContainer> ribContainers = new List<SplineContainer>();

            for (int i = 0; i < splineRibCount; i++)
            {
                float percent = (float)i/(splineRibCount - 1);

                Vector3 localScale = new Vector3(thick, width, (thick+width)/2);

                localScale *= (1-percent);

                if (variedThicknesses)
                {
                    localScale *= subRandom(minThickWidthOffset_Percent, maxThickWidthOffset_Percent);
                }

                float3 pos, st, up;

                mainCurve.Evaluate(mainCurve[0], percent, out pos, out st, out up);

                Transform t = Instantiate(copContainer, ribParent).transform;
                t.localPosition = pos;
                t.localScale = localScale;
                t.localRotation = Quaternion.LookRotation(st);

                ribContainers.Add(t.GetComponent<SplineContainer>());
            }



            GameObject g = Instantiate(emptyMeshPre, PreMeshParent);
            g.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(ribContainers, meshRes, true, false);

        }
        else
        {
           

            List<SplineContainer> ribContainers = new List<SplineContainer>();

            for (int i = 0; i < splineRibCount; i++)
            {
                List<Vector3> ribCopyPointList = CreateRibPointList();

                Transform copContainer = Instantiate(emptyContainerPre, ribParent).transform;

                copContainer.GetComponent<SplineContainer>().AddSpline(CreateBasedOnParameters(ribCopyPointList));

                copContainer.GetComponent<SplineContainer>()[0].Closed = true;


                float percent = (float)i / (splineRibCount - 1);

                Vector3 localScale = new Vector3(thick, width, (thick + width) / 2);

                localScale *= (1 - percent);

                if (variedThicknesses)
                {
                    localScale *= subRandom(minThickWidthOffset_Percent, maxThickWidthOffset_Percent);
                }

                float3 pos, st, up;

                mainCurve.Evaluate(mainCurve[0], percent, out pos, out st, out up);

                Transform t = Instantiate(copContainer, ribParent).transform;
                t.localPosition = pos;
                t.localScale = localScale;
                t.localRotation = Quaternion.LookRotation(st);

                if(i == 0)
                {
                    Transform end = Instantiate(t, ribParent);
                    end.localScale = Vector3.zero;
                    ribContainers.Add(end.GetComponent<SplineContainer>());

                }

                ribContainers.Add(t.GetComponent<SplineContainer>());
            }



            GameObject g = Instantiate(emptyMeshPre, PreMeshParent);
            g.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(ribContainers, meshRes, true, false);

        }

    }


    void CombineAllMeshes()
    {
        if (hasAxisSym)
        {
            List<Transform> ts = new List<Transform>();
            for(int i =1; i < numOfSymAxis; i++)//set to 1 since the first one is already done
            {
                Transform t = Instantiate(PreMeshParent);
                t.localScale = Vector3.one - (Vector3.one * (i * slightScaleChange));

                float percent = (float)i / (numOfSymAxis);

                t.localEulerAngles = new Vector3(0,0,percent*360);
                ts.Add(t);
            }

            for(int i = 0; i < ts.Count; i++)
            {
                ts[i].parent = PreMeshParent;
            }


        }

        List<Texture2D> emptyTex;
        List<MeshFilter> filters = new List<MeshFilter>();
        filters.AddRange(PreMeshParent.GetComponentsInChildren<MeshFilter>());

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out emptyTex);

    }


    Spline CreateBasedOnParameters(List<Vector3> points)
    {
        Spline spline;

        if (variedSharpness)
        {
            spline = (randomSharpSpline(points, minSharp, maxSharp));
        }
        else
        {
            if (singleNonNormalSharpness)
            {
                spline = (newSpline1(points, subRandom(minSharp, maxSharp)));
            }
            else
            {
                spline = (newSpline1(points, splineSharp));

            }
        }

        return spline;
    }


    List<Vector3> CreateRibPointList()
    {
        int pointCount = (int)subRandom(minMinorCurvePointCount, maxMinorCurvePointCount + 1);
        List<Vector3> points = new List<Vector3>();

        if (isUpDownSym && !isRightLeftSym)
        {
            pointCount /= 2;
            pointCount *= 2;
            if (pointCount < 2)
                pointCount = 2;

            List<Vector3> halfPoints = new List<Vector3>();

            for (int i = 0; i < pointCount / 2; i++)
            {

                float percent = (float)i / (pointCount);//not -1, since its a close loop anyway
                float rot = (360 * percent)+90;

                float off = subRandom(minRibPointOffset, maxRibPointOffset);

                rotator.localEulerAngles = new Vector3(0, 0, rot);
                offseter.localPosition = Vector3.up * off;                      //MAY NEED TO CHANGE AXIS OFFSET

                Vector3 point = offseter.position;
                points.Add(point);
                halfPoints.Add(point);
            }
            for (int i = 0; i < pointCount / 2; i++)
            {
                //+x-y//reversed order


                Vector3 ajustedQPoint = halfPoints[halfPoints.Count - (i + 1)];
                ajustedQPoint.y *= -1;
                points.Add(ajustedQPoint);

            }
        }
        if (isRightLeftSym && !isUpDownSym)
        {
            pointCount /= 2;
            pointCount *= 2;
            if (pointCount < 2)
                pointCount = 2;

            List<Vector3> halfPoints = new List<Vector3>();

            for(int i =0; i < pointCount/2; i++)
            {

                float percent = (float)i / (pointCount);//not -1, since its a close loop anyway
                float rot = 360 * percent;

                float off = subRandom(minRibPointOffset, maxRibPointOffset);

                rotator.localEulerAngles = new Vector3(0, 0, rot);
                offseter.localPosition = Vector3.up * off;                      //MAY NEED TO CHANGE AXIS OFFSET

                Vector3 point = offseter.position;
                points.Add(point);
                halfPoints.Add(point);
            }
            for (int i = 0; i < pointCount / 2; i++)
            {
                //-x+y//reversed order


                Vector3 ajustedQPoint = halfPoints[halfPoints.Count - (i + 1)];
                ajustedQPoint.x *= -1;
                points.Add(ajustedQPoint);

            }

        }
        if (isRightLeftSym && isUpDownSym)
        {
            pointCount /= 4;
            pointCount *= 4;
            if (pointCount < 4)
                pointCount = 4;

            List<Vector3> QuarterPoints = new List<Vector3>();

            for(int i = 0; i < pointCount/4; i++)//top right //rotating clockwise
            {
                //+x+y

                float percent = (float)i / (pointCount);//not -1, since its a close loop anyway
                float rot = 360 * percent;

                float off = subRandom(minRibPointOffset, maxRibPointOffset);

                rotator.localEulerAngles = new Vector3(0, 0, rot);
                offseter.localPosition = Vector3.up * off;                      //MAY NEED TO CHANGE AXIS OFFSET

                Vector3 point = offseter.position;
                points.Add(point);
                QuarterPoints.Add(point);
            }

            for (int i = 0; i < pointCount / 4; i++)//bottom right
            {
                //+x-y//reversed

                Vector3 ajustedQPoint = QuarterPoints[QuarterPoints.Count-(i+1)];
                ajustedQPoint.y *= -1;
                points.Add(ajustedQPoint);

            }
            for (int i = 0; i < pointCount / 4; i++)//bottom left
            {
                //-x-y//not reversed
                Vector3 ajustedQPoint = QuarterPoints[i];
                ajustedQPoint.x *= -1;
                ajustedQPoint.y *= -1;
                points.Add(ajustedQPoint);
            }
            for (int i = 0; i < pointCount / 4; i++)//top left
            {
                //-x+y//reversed

                Vector3 ajustedQPoint = QuarterPoints[QuarterPoints.Count - (i + 1)];
                ajustedQPoint.x *= -1;
                points.Add(ajustedQPoint);

            }

        }
        if (!isRightLeftSym && !isUpDownSym)
        {
            Debug.Log("it is the final case");

            for (int i = 0; i < pointCount; i++)
            {
                float percent = (float)i / (pointCount);//not -1, since its a close loop anyway
                float rot = 360 * percent;

                float off = subRandom(minRibPointOffset, maxRibPointOffset);

                rotator.localEulerAngles = new Vector3(0, 0, rot);
                offseter.localPosition = Vector3.up * off;                      //MAY NEED TO CHANGE AXIS OFFSET

                Vector3 point = offseter.position;
                points.Add(point);
            }
        }


        return points;
        
    }

}
