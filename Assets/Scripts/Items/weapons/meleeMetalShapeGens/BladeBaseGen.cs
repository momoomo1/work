using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;


public class BladeBaseGen : MetalShapeGen
{
    public SplineContainer bladeCurve;
    public float BladeWidth;
    public float BladeThick;
    public bool bladeCurveSymetry = false;
    [Range(0f, 1f)]
    [SerializeField] float minBladeCoverPercent = .1f, maxBladeCoverPercent = .35f;
    [SerializeField] SplineContainer containerPre;
    [SerializeField]int minSplinePointCount=3, maxSplinePointCount=4;
    [SerializeField] int minGaurdRes = 3, maxGaurdRes=8;
    SplineContainer mainContainer;
    SplineContainer altContainer;

    public override void Begin()
    {
        base.Begin();

        CreateProfilePre();
    }

    void CreateProfilePre()
    {
        List<Vector3> mainPoints = new List<Vector3>();
        List<Vector3> altPoints = new List<Vector3>();

        int mainCount = (int)subRandom(minSplinePointCount, maxSplinePointCount + 1);
        int altCount = (int)subRandom(minSplinePointCount, maxSplinePointCount + 1);

        float evaluateDist = subRandom(minBladeCoverPercent, maxBladeCoverPercent);
        
        for(int i = 0; i < mainCount; i++)
        {
            float perc = (float)i/(mainCount-1);


            float3 pos, st, up;

            bladeCurve.Evaluate(bladeCurve[0], perc * evaluateDist, out pos, out st, out up);

            float off = subRandom(0f, 1-perc);
            off *= BladeWidth / 2f;

            pos.y += off;
            pos.x = BladeThick;

            if (i == 0)
            {
                pos.y = BladeWidth / 2;
            }
            mainPoints.Add(pos);
        }
        

        for (int i = 0; i < altCount; i++)
        {
            float perc = (float)i / (altCount - 1);


            float3 pos, st, up;

            bladeCurve.Evaluate(bladeCurve[0], perc * evaluateDist, out pos, out st, out up);

            float off = subRandom(0f, 1-perc);

            //Debug.Log("alt off " + off);
            off *= BladeWidth / 2f;

            pos.y -= off;
            pos.x = BladeThick;

            if (i == 0)
            {
                pos.y = -BladeWidth / 2;
            }
            altPoints.Add(pos);
        }

        Spline mainSpline = randomSharpSpline(mainPoints, .01f, .5f);

        mainContainer = Instantiate(containerPre);
        mainContainer.AddSpline(mainSpline);
        altContainer = Instantiate(containerPre);
        if (bladeCurveSymetry)
        {
            altContainer.AddSpline(new Spline(mainSpline));
            altContainer.transform.localScale = new Vector3(1, -1, 1);


        }
        else
        {
            altContainer.AddSpline(randomSharpSpline(altPoints, .01f, .5f));

        }


        List<SplineContainer> containerList = new List<SplineContainer>();
        containerList.Add(mainContainer);
        containerList.Add(altContainer);

        SplineContainer m2 = Instantiate(mainContainer);
        m2.transform.localScale = new Vector3(-1, 1, 1);
        SplineContainer a2 = Instantiate(altContainer);
        a2.transform.localScale = new Vector3(-1, 1, 1);
        if (bladeCurveSymetry)
        {
            a2.transform.localScale = new Vector3(-1, -1, 1);

        }

        containerList.Add(a2);
        containerList.Add(m2);
        containerList.Add(mainContainer);

        Final.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containerList, (int)subRandom(minGaurdRes, maxGaurdRes), true, false);



    }

}
