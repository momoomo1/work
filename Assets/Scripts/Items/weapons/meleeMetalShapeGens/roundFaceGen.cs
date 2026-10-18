using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;


public class roundFaceGen : MetalShapeGen
{
    [SerializeField] Transform ContainerParent;
    [SerializeField] Transform reverseParent;
    [SerializeField] Transform ArrangedContainersParent;
    [SerializeField] SplineContainer straightEdgeContainer;
    [SerializeField] SplineContainer CurvedContainer;
    [SerializeField] List<SplineContainer> containers = new List<SplineContainer>();
    [SerializeField] int minNumOfSides, maxNumOfSides, minRes, maxRes, minPointCount, maxPointCount;
    [SerializeField] float minSplineSharp = .01f, maxSplineSharp = .8f;
    
    [SerializeField] pinicalGenerator2 pinicGen;

    int sideCount, res, pointCount;
    bool hasRandomSharps = false;
    bool DefaultSharp;
    float sharpness1;
    public override void Begin()
    {
        base.Begin();
        sideCount = (int)subRandom(minNumOfSides, maxNumOfSides + 1);
        res = (int)subRandom(minRes, maxRes+1);
        pointCount = (int)subRandom(minPointCount, maxPointCount);

        hasRandomSharps = decider5050();
        DefaultSharp = decider5050();
        if (DefaultSharp)
        {
            sharpness1 = splineSharp;
        }
        else
        {
            sharpness1 = subRandom(minSplineSharp, maxSplineSharp);
        }

        CreateBaseSplineAndStraightSpline();
        DupAndArrangeSplineContainers();

        //CleanUp();

        width = thick*4;//THIS IS JUST FOR WEAPON GENERATOR WHEN IT READS IT FOR HANDLE GENERATION, DO THIS LAST
        thick *= 4;
    }

    void CreateBaseSplineAndStraightSpline()
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < pointCount; i++)
        {
            float percent = (float)i/(pointCount-1);

            float zPos = percent * length;
            float yPos = subRandom(0f, width);
            float xPos = subRandom(thick/4, thick);

            Vector3 v = new Vector3(xPos, yPos, zPos);
            points.Add(v);
        }
        points[0] = new Vector3(subRandom(thick / 4, thick),0,0);
        points[points.Count - 1] = new Vector3(subRandom(thick / 4, thick), 0, length);

        if (hasRandomSharps)
        {
            CurvedContainer.AddSpline(randomSharpSpline(points, minSplineSharp, maxSplineSharp));
        }
        else
        {
            CurvedContainer.AddSpline(newSpline1(points, sharpness1));

        }
        List<Vector3> straightPoints = new List<Vector3>();
        straightPoints.Add(points[0]);
        straightPoints.Add(points[points.Count - 1]);
        straightEdgeContainer.AddSpline(newSpline1(straightPoints, sharpness1));
    }
    void DupAndArrangeSplineContainers()
    {

        //Transform t = Instantiate(ContainerParent, ContainerParent);
        //t.localScale = new Vector3(-1, 1, 1);
        Instantiate(CurvedContainer.transform, reverseParent);

        Instantiate(straightEdgeContainer.transform, reverseParent);
        reverseParent.localScale = new Vector3(-1, 1, 1);

        for(int i = 0; i < sideCount; i++)
        {
            Transform cont = Instantiate(ContainerParent, ArrangedContainersParent);
            float percent = (float)i / sideCount;//a little diffrent for this

            float rot = percent * 360f;

            cont.localEulerAngles = new Vector3(0, 0, rot);



        }

        containers.AddRange(ArrangedContainersParent.GetComponentsInChildren<SplineContainer>());
        Final.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containers, res, false, false);

    }

    void CleanUp()
    {
        pinicGen.pillarSeed = seed;
        pinicGen.Begin();

        GameObject g = Instantiate(pinicGen.FinalPillar,Final.transform);
        g.transform.localScale = Vector3.one * thick*2;

        g.transform.localPosition = new Vector3(0, 0, length);


    }

}
