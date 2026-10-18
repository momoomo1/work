using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class axeBladeGen : MetalShapeGen
{
    public float outPutStartWidth;

    [SerializeField] SplineContainer upperEdgeContainer;
    [SerializeField] SplineContainer lowerEdgeContainer;
    [SerializeField] SplineContainer bladeEdgeContainer;

    [SerializeField] int minSplinePointCount, maxSplinePointCount;
    [SerializeField] float minEdgeDisplace_Percent;
    [SerializeField] float maxEdgeDisplace_Percent;
    [SerializeField] float minEdgeStartOff_Percent, maxEdgeStartOff_Percent;
    [SerializeField] float minEdgeEndOff_Percent, maxEdgeEndOff_Percent;
    [SerializeField] float minGapDist_Percent = .25f, maxGapDist_Percent = .75f;
    [SerializeField] float maxBladeEdgeDisplace_Percent = .5f;
    [SerializeField] float minSplineSharp, maxSplineSharp;
    [SerializeField] float maxBladeDisplace_Percent;
    public bool symetryOverride = false;
    [SerializeField] int splineCountRes;
    [SerializeField] int meshResAlongSpline;
    [SerializeField] SplineContainer emptyContainerPre;

    SplineContainer trueTopContainer;
    SplineContainer trueBotContainer;
    List<Vector3> topCPoints;
    List<Vector3> botCPoints;

    float cSplineSharp;

    [SerializeField] MeshFilter topMesh;
    [SerializeField] MeshFilter botMesh;
    [SerializeField] Transform backQuad;
    [SerializeField] MeshFilter rightHalf;

    //bool similarGaps = false;

    public override void Begin()
    {
        base.Begin();
        if(symetryOverride == false)
        {
            symetryOverride = decider5050();

        }
        CreateEdges();
        OpenEdgeFillIn();



        //THIS IS JUST FOR THE WEAPON GEN, BE SURE TO DO ALL NESSISARY FUNCTIONS INVOLVING WIDTH BEFORE THIS
        thick *= 3.5f;

        width = thick;
    }

    void CreateEdges()
    {

        int edge1PointCount = (int)subRandom(minSplinePointCount, maxSplinePointCount);

       

        cSplineSharp = splineSharp;

        if (decider5050())
        {
            cSplineSharp = subRandom(minSplineSharp, maxSplineSharp);
        }

        List<Vector3> edge1Points = new List<Vector3>();

        int edge2PointCount = (int)subRandom(minSplinePointCount, maxSplinePointCount);
        List<Vector3> edge2Points = new List<Vector3>();


        float topLength = subRandom(minGapDist_Percent, maxGapDist_Percent);
        float bottomLength = 1 - topLength;
        bottomLength *= length;
        topLength *= length;
        //float bottomLength = length - topLength;

        for (int i = 0; i < edge1PointCount; i++)
        {

            float lengthPos = (float)i / (edge1PointCount - 1);
            float thickPos = (1-lengthPos) * thick;
            lengthPos *= topLength;

            edge1Points.Add(new Vector3(thickPos, subRandom(minEdgeDisplace_Percent, maxEdgeDisplace_Percent)*(width/2), lengthPos));
        }


        if (!symetryOverride)
        {

            for (int i = 0; i < edge2PointCount; i++)
            {


                float lengthPos = (float)i / (edge2PointCount - 1);
                float thickPos = (1 - lengthPos) * thick;
                lengthPos *= bottomLength;

                edge2Points.Add(new Vector3(thickPos, -subRandom(minEdgeDisplace_Percent, maxEdgeDisplace_Percent)*(width/2), lengthPos));


            }
        }




        float startOffset = subRandom(minEdgeStartOff_Percent, maxEdgeStartOff_Percent)*(width/2);
        float endOffset = subRandom(minEdgeEndOff_Percent, maxEdgeEndOff_Percent) * (width / 2);

        edge1Points[0] = new Vector3(thick, startOffset, 0);
        edge1Points[edge1Points.Count - 1] = new Vector3(0, endOffset, topLength);



        float startOffset2 = subRandom(minEdgeStartOff_Percent, maxEdgeStartOff_Percent) * (width / 2);
        float endOffset2 = subRandom(minEdgeEndOff_Percent, maxEdgeEndOff_Percent) * (width / 2);

        if (!symetryOverride)
        {
            edge2Points[0] = new Vector3(thick, -startOffset2, 0);
            edge2Points[edge2Points.Count - 1] = new Vector3(0, -endOffset2, bottomLength);


        }

        if (symetryOverride)
        {

            foreach (Vector3 v in edge1Points)
            {
                edge2Points.Add(new Vector3(v.x, -v.y, v.z));
            }

        }


        upperEdgeContainer.AddSpline(newSpline1(edge1Points, cSplineSharp));
        lowerEdgeContainer.AddSpline(newSpline1(edge2Points, cSplineSharp));


        Vector3 startPoint = edge1Points[edge1Points.Count-1];
        Vector3 endPoint = edge2Points[edge2Points.Count - 1];

        int frontEdgeSplineCount = (int)subRandom(minSplinePointCount,maxSplinePointCount);

        if (symetryOverride)
        {
            frontEdgeSplineCount /= 2;
            frontEdgeSplineCount *= 2;//ensures it is an even number
        }

        List<Vector3> bladeEdgePos = new List<Vector3>();
        for(int i = 0; i < frontEdgeSplineCount; i++)
        {
            float time1 = (float)i / (frontEdgeSplineCount - 1);

            Vector3 defaultPos = Vector3.Lerp(startPoint, endPoint, time1);

            float zOff = subRandom(-maxBladeDisplace_Percent, maxBladeDisplace_Percent) *(defaultPos.z);


            Vector3 finalPos = new Vector3(0,0,zOff);

            if(symetryOverride && time1 > .5f)
            {

                int revInd = frontEdgeSplineCount - i;
                finalPos = new Vector3(0,0, bladeEdgePos[revInd - 1].z-defaultPos.z);

            }

            bladeEdgePos.Add(finalPos+defaultPos);

        }

        bladeEdgePos[0] = startPoint;
        bladeEdgePos[bladeEdgePos.Count - 1] = endPoint;

        bladeEdgeContainer.AddSpline(newSpline1(bladeEdgePos, cSplineSharp));

        List<SplineContainer> containersToMeshify = new List<SplineContainer>();

        for(int i = 0; i < splineCountRes; i++)
        {
            SplineContainer cont = Instantiate(emptyContainerPre, transform);
            //lets go from left to right

            List<Vector3> points = new List<Vector3>();

           


            for (int x = 0; x < maxSplinePointCount; x++)
            {

                float splineTime = (float)x / (maxSplinePointCount-1);


                float3 pos, st, up;

                upperEdgeContainer.Evaluate(upperEdgeContainer[0], splineTime, out pos, out st, out up);

                Vector3 topPos = pos;

                lowerEdgeContainer.Evaluate(lowerEdgeContainer[0], splineTime, out pos, out st, out up);

                Vector3 bottomPos = pos;

                bladeEdgeContainer.Evaluate(bladeEdgeContainer[0], (float)i / (splineCountRes-1), out pos, out st, out up);
                Vector3 bEdgePos = pos;

                Vector3 finalPos1 = Vector3.Lerp(topPos, bottomPos, (float)i/(splineCountRes-1));
                if(splineTime > .75f)
                {
                    finalPos1 = Vector3.Lerp(finalPos1, bEdgePos, splineTime);

                }

                points.Add(finalPos1);

            }

            cont.AddSpline(newSpline1(points, cSplineSharp));
            containersToMeshify.Add(cont);

            if(i == 0)
            {
                trueTopContainer = cont;
                topCPoints = points;
            }
            if(i == splineCountRes - 1)
            {
                trueBotContainer = cont;
                botCPoints = points;
            }

        }

        rightHalf.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containersToMeshify, meshResAlongSpline, true, true);

    }

    void OpenEdgeFillIn()
    {
        float localWidth = topCPoints[0].x;

        for (int i=0;i < topCPoints.Count; i++)
        {
            topCPoints[i] = new Vector3(0, topCPoints[i].y, topCPoints[i].z);
        }

        for (int i = 0; i < botCPoints.Count; i++)
        {
            botCPoints[i] = new Vector3(0, botCPoints[i].y, botCPoints[i].z);
        }

        SplineContainer sctop = Instantiate(emptyContainerPre, transform);
        SplineContainer scbot = Instantiate(emptyContainerPre, transform);
        Spline flatTop = newSpline1(topCPoints, cSplineSharp);
        Spline flatBot = newSpline1(botCPoints, cSplineSharp);

        sctop.AddSpline(flatTop);
        scbot.AddSpline(flatBot);

        List<SplineContainer> topFill = new List<SplineContainer>();
        List<SplineContainer> botFill = new List<SplineContainer>();

        topFill.Add(sctop);
        topFill.Add(trueTopContainer);

        botFill.Add(scbot);
        botFill.Add(trueBotContainer);

        topMesh.mesh = GenerateMeshFromSplineContainerList(topFill, meshResAlongSpline, true, true);
        botMesh.mesh = GenerateMeshFromSplineContainerList(botFill, meshResAlongSpline, false, true);


        float midYPos = (topCPoints[0].y + botCPoints[0].y) / 2f;
        float backHeight = topCPoints[0].y - botCPoints[0].y;

        backQuad.localPosition = new Vector3(localWidth/2, midYPos, 0);
        backQuad.localScale = new Vector3(localWidth, backHeight, 1);
        outPutStartWidth = backHeight;

        float3 pos, st, up;

        trueTopContainer.Evaluate(trueTopContainer[0], 0, out pos, out st, out up);


        rightHalf.transform.localPosition = new Vector3(0, - pos.y, 0);

        Transform t = Instantiate(rightHalf.gameObject, transform).transform;

        t.localScale = new Vector3(-1, 1, 1);

        List<MeshFilter> filters = new List<MeshFilter>();

        filters.AddRange(rightHalf.GetComponentsInChildren<MeshFilter>());
        filters.AddRange(t.GetComponentsInChildren<MeshFilter>());

        List<Texture2D> ts = new List<Texture2D>();

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters,null,out ts);


    }


}
