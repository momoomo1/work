using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class handleGen : MonoBehaviour
{
    //public GameObject decorationTopObj;//same thing thats at the top of arches

    public Material blankMetal;
    public Material HandleMaterial;
    public bool hasPommel = false;
    public int pillarSeed;
    int randomIndex;

    public GameObject FinalPillar;
    [SerializeField] SplineContainer container;
    GameObject containerObj;
    Spline profileSpline;

    [SerializeField] int numberOfPoints;
    [SerializeField] bool isSquare = true, useOverrideBools = false;
    [SerializeField] int profileResolution;
    [SerializeField] int roundResolution; // if square = 4, else its equale to the number of profiles wraped around

    [SerializeField] int minRoundRes=3, maxRoundRes=10,minProfileRes,maxProfileRes, minNumOfPoints, maxNumOfPoints;

    [Range(0.0f, 1.0f)]
    [SerializeField] float bendSharp = .5f;

    [SerializeField] float maxProfileLean = 45;
    float profileLean = 0f;

    [SerializeField] GameObject topObj;

    public float bodyPercent;
    [SerializeField] float minbodPercent = .5f, maxBodPercent = .9f;

    [SerializeField] GameObject EmptyAndOffset;
    [SerializeField] GameObject pillarBody;

    [SerializeField] float minPommelBonus=1f,maxPommelBonus=5f;
    float pommelBonus;

    [SerializeField] bool isTesting = false;

    private void Start()
    {
        if (isTesting)
        {
            Begin();
        }
    }

    public void Begin()
    {
        randomIndex = 0;


        pillarBody.GetComponent<MeshRenderer>().material = HandleMaterial;
        topObj.GetComponent<MeshRenderer>().material = blankMetal;

        RandomizePillarTops();


        float bodDiff = maxBodPercent - minbodPercent;
        float bodPerc = bodyPercent - minbodPercent;
        bodPerc = bodPerc / bodDiff;

        float pommelDiff = maxPommelBonus - minPommelBonus;
        float pommPercent = bodPerc;



        pommelBonus = (pommPercent * pommelDiff) + minPommelBonus;


        CreateProfileSplinePoints();
        CreateTop();
        CreateMainPillarBody();
        FinalAssemble();
    }

    void RandomizePillarTops()
    {
        numberOfPoints = (int)subrandom(minNumOfPoints, maxNumOfPoints+1);

        if (!useOverrideBools)
        {

            float decider = subrandom(0, 10f);

            if (decider > 7)
            {
                isSquare = false;

            }
            else
            {
                isSquare = true;

            }
        }

        if (isSquare)
        {
            roundResolution = 5;

        }
        else
        {
            roundResolution = (int)subrandom(minRoundRes, maxRoundRes + 1);

        }

        float decider2 = subrandom(0, 10);
        if(decider2 > 7)
        {
            profileLean = subrandom(0, maxProfileLean);
        }
        else
        {
            profileLean = 0f;
        }
        profileResolution = (int)subrandom(minProfileRes, maxProfileRes+1);


        float decider3 = subrandom(0, 10);
        if(decider3 > 3)
        {
            topObj.transform.localEulerAngles = new Vector3(0, (360 / (roundResolution - 1)) / 2, 0);
            pillarBody.transform.localEulerAngles = new Vector3(0, (360 / (roundResolution - 1)) / 2, 0);
        }
        //Debug.Log("Body percent" + bodyPercent);
        if(bodyPercent == 0)
        {
            bodyPercent = subrandom(minbodPercent, maxBodPercent);

        }



    }

    void CreateProfileSplinePoints()
    {

        float maxOffset = Mathf.Sqrt(2f)/2;
        float minOffset = .25f;


        Vector3[] poses = new Vector3[numberOfPoints];//extra 2 are ment to close it all up

        for (int i = 0; i < poses.Length; i++)
        {
            //bool skip = false;

            Vector3 pos = Vector3.zero;

            //if (i == 0)
            //{
            //    skip = true;
            //    pos = Vector3.zero;
            //}
            //if (i == poses.Length)
            //{
            //    skip = true;
            //    pos = new Vector3(0, 1, 0);
            //}

            //if (!skip)


            {
                float percent = (float)(i) / (poses.Length-1);

                pos = new Vector3(0, percent, subrandom(minOffset, maxOffset));

            }

            poses[i] = pos;
        }

        profileSpline = CreateSpline(poses, bendSharp);

        container.AddSpline(profileSpline);


    }

    void CreateTop()
    {



        Vector3[] verts = new Vector3[roundResolution * (profileResolution+2)];

        int[] triangles = new int[(roundResolution - 1) * (profileResolution +1) * 6];

        Vector2[] UVs = new Vector2[verts.Length];

        GameObject[] rotatedSplineContainers = new GameObject[roundResolution];

        int triIndex = 0;

        for (int x = 0; x < roundResolution; x++)
        {
            //create and rotate the splines
            GameObject g = Instantiate(container.gameObject, transform);

            g.transform.localEulerAngles = new Vector3(0, ((float)x / (roundResolution - 1)) * 360, 0);



            SplineContainer cont = g.GetComponent<SplineContainer>();
            Spline spline = cont[0];

            for (int y = 0; y < profileResolution+2; y++)
            {

                int i = (y * roundResolution) + x;

                bool skip = false;
                float3 pos, st, up;

                pos = Vector3.zero;

                if(y == 0)
                {
                    skip = true;
                    pos = Vector3.zero;


                }
                if (y == profileResolution+1)
                {
                    skip = true;
                    pos = Vector3.up;

                }


                if (skip == false)
                {

                    float splineTime = (float)(y - 1) / (profileResolution-1f);


                    cont.Evaluate(spline, splineTime, out pos, out st, out up);

                    //Debug.Log("spline pos " + pos+ " time "+splineTime);


                    //verts[i] = pos;
                }

                verts[i] = pos;
                UVs[i] = new Vector2((float)x / roundResolution, (float)(y+1) / (profileResolution+2));

                if (x != roundResolution - 1 && y != profileResolution +1) //not so sure about this (8:22)
                {
                    triangles[triIndex] = i;//triangleOffset
                    triangles[triIndex + 1] = i + roundResolution + 1;
                    triangles[triIndex + 2] = i + roundResolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + roundResolution + 1;

                    triIndex += 6;
                }

            }


        }

        //Mesh mesh = activeLOD.GetComponent<MeshFilter>().mesh;

        //activeLOD.vertices = verts;
        //activeLOD.triangles = triangles;
        //activeLOD.uv = UVs;
        //activeLOD.RecalculateNormals();

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();

        topObj.GetComponent<MeshFilter>().mesh = mesh;


    }

    void CreateMainPillarBody()
    {


        Vector3[] verts = new Vector3[roundResolution * (2 + 2)];

        int[] triangles = new int[(roundResolution - 1) * (2 + 1) * 6];

        Vector2[] UVs = new Vector2[verts.Length];

        GameObject[] rotatedSplineContainers = new GameObject[roundResolution];

        int triIndex = 0;

        for (int x = 0; x < roundResolution; x++)
        {
            //create and rotate the splines
            GameObject g = Instantiate(EmptyAndOffset, transform);

            g.transform.localEulerAngles = new Vector3(0, ((float)x / (roundResolution - 1)) * 360, 0);

            for (int y = 0; y < 2 + 2; y++)
            {

                int i = (y * roundResolution) + x;

                bool skip = false;
                float3 pos, st, up;

                pos = Vector3.zero;

                if (y == 0)
                {
                    skip = true;
                    pos = -Vector3.up / 2;


                }
                if (y == 2 + 1)
                {
                    skip = true;
                    pos = Vector3.up/2;

                }


                if (skip == false)
                {

                    //float splineTime = (float)(y - 1) / (profileResolution - 1f);

                    float HeightPercent = ((float)(y - 1) / (2 - 1f)) - .5f;


                    pos = g.transform.GetChild(0).position;
                    pos = new float3(pos.x, HeightPercent, pos.z);

                    //cont.Evaluate(spline, splineTime, out pos, out st, out up);

                    //Debug.Log("spline pos " + pos + " time " + splineTime);


                    //verts[i] = pos;
                }

                verts[i] = pos;
                UVs[i] = new Vector2((float)x / roundResolution, (float)(y + 1) / (2 + 2));

                if (x != roundResolution - 1 && y != 2 + 1) //not so sure about this (8:22)
                {
                    triangles[triIndex] = i;//triangleOffset
                    triangles[triIndex + 1] = i + roundResolution + 1;
                    triangles[triIndex + 2] = i + roundResolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + roundResolution + 1;

                    triIndex += 6;
                }

            }


        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();

        pillarBody.GetComponent<MeshFilter>().mesh = mesh;
    }

    void FinalAssemble()
    {

        pillarBody.transform.localScale = new Vector3(1, bodyPercent, 1);
        pillarBody.transform.localPosition = Vector3.up / 2f;
        topObj.transform.localScale = new Vector3(1, (1 - bodyPercent) / 2, 1);

        float ammount = 1f;

        GameObject topTop = Instantiate(topObj, FinalPillar.transform);
        topTop.transform.localScale = -topTop.transform.localScale;

        if (hasPommel)
        {
            float pommelYPosAdd = ((((1 - bodyPercent) / 2) * pommelBonus)) - ((1 - bodyPercent) / 2);

            Vector3 tt = topTop.transform.localScale;
            topTop.transform.localScale = new Vector3(tt.x*1.25f,tt.y* pommelBonus, tt.x*1.25f);

            ammount += pommelYPosAdd;

        }

        topTop.transform.localPosition = Vector3.up * ammount;
    }

    //will need to make 



    float subrandom(float min, float max)
    {
        UnityEngine.Random.InitState(pillarSeed - randomIndex);
        randomIndex++;

        float val = UnityEngine.Random.Range(min, max);

        return val;

    }


    Spline CreateSpline(Vector3[] pointsToBe, float bendSharpness)
    {

        BezierKnot[] test = new BezierKnot[pointsToBe.Length];

        Spline trunkSpline = new Spline();



        trunkSpline.CopyTo(test, 0);

        trunkSpline.Clear();

        for (int j = 0; j < pointsToBe.Length; j++)//positions
        {
            test[j].Position = pointsToBe[j];
            //trunkSpline.Add(test[j]);
        }

        for (int i = 0; i < test.Length; i++)//rotation
        {

            if (i != test.Length - 1)
            {

                Vector3 cPos = new Vector3(test[i].Position.x, test[i].Position.y, test[i].Position.z);
                Vector3 nPos = new Vector3(test[i + 1].Position.x, test[i + 1].Position.y, test[i + 1].Position.z);

                Vector3 forward1 = (nPos - cPos).normalized;

                test[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
            else
            {

                Vector3 cPos = new Vector3(test[i].Position.x, test[i].Position.y, test[i].Position.z);
                Vector3 pPos = new Vector3(test[i - 1].Position.x, test[i - 1].Position.y, test[i - 1].Position.z);

                Vector3 forward1 = (pPos - cPos).normalized;
                forward1 *= -1;

                test[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
        }

        for (int j = 0; j < test.Length; j++)//tangents
        {
            if (j == 0)
            {

                Vector3 cPos = new float3(test[j].Position.x, test[j].Position.y, test[j].Position.z);
                Vector3 nPos = new float3(test[j + 1].Position.x, test[j + 1].Position.y, test[j + 1].Position.z);

                float forDist = Vector3.Distance(cPos, nPos) * bendSharpness;


                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, -forDist);

                test[j].TangentIn = backward1;
                test[j].TangentOut = forward1;
            }
            if (j == test.Length - 1)
            {

                Vector3 cPos = new float3(test[j].Position.x, test[j].Position.y, test[j].Position.z);
                Vector3 pPos = new float3(test[j - 1].Position.x, test[j - 1].Position.y, test[j - 1].Position.z);

                float backDist = Vector3.Distance(cPos, pPos) * bendSharpness;


                float3 forward1 = new float3(0, 0, backDist);
                float3 backward1 = new float3(0, 0, -backDist);

                test[j].TangentIn = backward1;
                test[j].TangentOut = forward1;
            }
            if (j != 0 && j != test.Length - 1)
            {

                Vector3 cPos = new float3(test[j].Position.x, test[j].Position.y, test[j].Position.z);
                Vector3 nPos = new float3(test[j + 1].Position.x, test[j + 1].Position.y, test[j + 1].Position.z);
                Vector3 pPos = new float3(test[j - 1].Position.x, test[j - 1].Position.y, test[j - 1].Position.z);


                float forDist = Vector3.Distance(cPos, nPos) * bendSharpness;
                float backDist = Vector3.Distance(cPos, pPos) * bendSharpness;

                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, backDist);
                backward1 *= -1;

                test[j].TangentIn = backward1;
                test[j].TangentOut = forward1;
            }

            trunkSpline.Add(test[j]);

        }
        return trunkSpline;

    }

}
