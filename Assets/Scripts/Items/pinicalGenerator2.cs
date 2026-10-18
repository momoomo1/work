using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class pinicalGenerator2 : MonoBehaviour
{
    //public GameObject decorationTopObj;//same thing thats at the top of arches

    public int pillarSeed;
    int randomIndex;

    public GameObject FinalPillar;
    [SerializeField] SplineContainer container;
    GameObject containerObj;
    Spline profileSpline;

    [SerializeField] int numberOfPoints;
    public bool isSquare;
    [SerializeField] int profileResolution;
    [SerializeField] int roundResolution; // if square = 4, else its equale to the number of profiles wraped around

    [SerializeField] int minRoundRes = 3, maxRoundRes = 10, minProfileRes, maxProfileRes, minNumOfPoints, maxNumOfPoints;

    [Range(0.0f, 1.0f)]
    [SerializeField] float bendSharp = .5f;

    [SerializeField] float maxProfileLean = 45;
    float profileLean = 0f;

    [SerializeField] GameObject topObj;

    float bodyPercent;

    [SerializeField] GameObject EmptyAndOffset;
    [SerializeField] GameObject pillarBody;
    [SerializeField] bool isTesting = false;

    [Range(0,10)]
    [SerializeField] int minCheckForLineUp = 3;

    [Range(0,10)]
    [SerializeField] int isSquareChance= 7;

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
        RandomizePillarTops();
        CreateArchPositions();
        //CreateProfileSplinePoints();
        CreateTop();
        //CreateMainPillarBody();
        //FinalAssemble();

        topObj.transform.localScale = new Vector3(Mathf.Sqrt(2),1, Mathf.Sqrt(2));//////////////////////////////////////////////////////////////

    }

    #region archGeneration

    private void CreateArchPositions()
    {

        float YPos = 0;

        //Vector3[] fullArchPoses = new Vector3[numberOfPoints];
        Vector3[] halfArchPoses = new Vector3[numberOfPoints];



        for (int i = 0; i < numberOfPoints - 1; i++)
        {

            YPos = (float)i / (numberOfPoints - 1);

            Vector3 pos1 = new Vector3(subrandom(0, YPos), YPos, 0);

            //fullArchPoses[i] = pos1;

            halfArchPoses[i] = new Vector3((pos1.x / 2)-.5f, YPos, 0);


        }

        halfArchPoses[numberOfPoints - 1] = new Vector3(0f, 1, 0);
        //fullArchPoses[numberOfPoints - 1] = new Vector3(1, 1, 0);

        Spline fullSpline = CreateSpline(halfArchPoses, bendSharp);

        container.AddSpline(fullSpline);


    }

    #endregion


    #region pillar generation

    void RandomizePillarTops()
    {
        numberOfPoints = (int)subrandom(minNumOfPoints, maxNumOfPoints + 1);

        float decider = subrandom(0, 10f);

        if (decider > isSquareChance)
        {
            isSquare = false;

            roundResolution = (int)subrandom(minRoundRes, maxRoundRes + 1);
        }
        else
        {
            isSquare = true;
            roundResolution = 5;

        }

        //isSquare = true;

        float decider2 = subrandom(0, 10);
        if (decider2 > 7)
        {
            profileLean = subrandom(0, maxProfileLean);
        }
        else
        {
            profileLean = 0f;
        }
        profileResolution = (int)subrandom(minProfileRes, maxProfileRes + 1);


        float decider3 = subrandom(0, 10);
        if (decider3 > minCheckForLineUp)
        {
            topObj.transform.localEulerAngles = new Vector3(0, (360 / (roundResolution - 1)) / 2, 0);
            pillarBody.transform.localEulerAngles = new Vector3(0, (360 / (roundResolution - 1)) / 2, 0);
        }

        bodyPercent = subrandom(.5f, 1);



    }


    void CreateTop()
    {

        Vector3[] verts = new Vector3[roundResolution * (profileResolution + 2)];

        int[] triangles = new int[(roundResolution - 1) * (profileResolution + 1) * 6];

        Vector2[] UVs = new Vector2[verts.Length];

        //GameObject[] rotatedSplineContainers = new GameObject[roundResolution];

        int triIndex = 0;

        for (int x = 0; x < roundResolution; x++)
        {
            //create and rotate the splines
            GameObject g = Instantiate(container.gameObject, transform);

            g.transform.localEulerAngles = new Vector3(0, ((float)x / (roundResolution - 1)) * 360, 0);



            SplineContainer cont = g.GetComponent<SplineContainer>();
            Spline spline = cont[0];

            for (int y = 0; y < profileResolution + 2; y++)
            {

                int i = (y * roundResolution) + x;

                bool skip = false;
                float3 pos, st, up;

                pos = Vector3.zero;

                if (y == 0)
                {
                    skip = true;
                    pos = Vector3.zero;


                }
                if (y == profileResolution + 1)
                {
                    skip = true;
                    pos = Vector3.up;

                }


                if (skip == false)
                {

                    float splineTime = (float)(y - 1) / (profileResolution - 1f);


                    cont.Evaluate(spline, splineTime, out pos, out st, out up);

                    //Debug.Log("spline pos " + pos + " time " + splineTime);


                    //verts[i] = pos;
                }

                verts[i] = pos;
                UVs[i] = new Vector2((float)x / roundResolution, (float)(y + 1) / (profileResolution + 2));

                if (x != roundResolution - 1 && y != profileResolution + 1) //not so sure about this (8:22)
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

        topObj.GetComponent<MeshFilter>().mesh = mesh;


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
    #endregion

}
