using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using static UnityEditor.PlayerSettings;

public class MetalShapeGen : MonoBehaviour
{
    public GameObject Final;
    public int seed;
    public int colorSeed;
    public int randomIndex;
    public bool isTesting = false;
    public float alotedMass;
    [Range(0f, 2f)]
    public float massModifier = 1f;
    
    public float minLength_Percent, maxLength_Percent, minWidth_Percent, maxWidth_Percent, minThick_Percent, maxThick_Percent;
    public float length, width, thick;
    public float minL, minW, minT;

    public float splineSharp = .5f;

    public bool hasCrossGuard;
    public bool hasBasketTypeGuard;

    public List<Material> inMats = new List<Material>();
    public MaterialRandomizer randomizerAAA;

    public GameObject emptyMeshPreAAA;
    public SplineContainer containerPreAAA;
    public GameObject emptyGameObjectAAA;

    [Header("last inherited var")]

    public GameObject crossGuardBuilder;


    public virtual void Start()
    {
        if (isTesting)
        {
            Begin();
        }
    }

    public virtual void Begin()
    {
        alotedMass *= massModifier;

        length = subRandom(minLength_Percent, maxLength_Percent);

        width = subRandom(minWidth_Percent, maxWidth_Percent);

        thick = subRandom(minThick_Percent, maxThick_Percent);

        float percentDiffrence = length + width + thick;

        length /= percentDiffrence;
        width /= percentDiffrence;
        thick /= percentDiffrence;

        length *= alotedMass;
        width *= alotedMass;
        thick *= alotedMass;

        //percentDiffrence = minLength_Percent + minWidth_Percent + minThick_Percent;
        minL = minLength_Percent / percentDiffrence;
        minW = minWidth_Percent / percentDiffrence;
        minT = minThick_Percent / percentDiffrence;

        minL *= alotedMass;
        minW *= alotedMass;
        minT *= alotedMass;

        //if(randomizerAAA != null)
        //{
        //    randomizerAAA.MakeMaterialTypes(out inMats, colorSeed);
        //}

    }


    public float subRandom(float min, float max)
    {
        UnityEngine.Random.InitState(seed - randomIndex);

        //randomIndex++;
        randomIndex = UnityEngine.Random.Range(-999999999, 999999999);
        return UnityEngine.Random.Range(min, max);

    }

    public bool decider5050()
    {
        float decider = subRandom(-1f, 1f);
        bool dec = false;
        if (decider > 0)
        {
            dec = true;
        }
        return dec;
    }


    public Spline newSpline1(List<Vector3> pos, float sharp)
    {
        BezierKnot[] knots = new BezierKnot[pos.Count];

        Spline spline = new Spline();
        spline.CopyTo(knots, 0);
        spline.Clear();


        for (int j = 0; j < pos.Count; j++)//positions
        {
            knots[j].Position = pos[j];
            //trunkSpline.Add(test[j]);
        }

        for (int i = 0; i < knots.Length; i++)//rotation
        {

            if (i != knots.Length - 1)
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 nPos = new Vector3(knots[i + 1].Position.x, knots[i + 1].Position.y, knots[i + 1].Position.z);

                Vector3 forward1 = (nPos - cPos).normalized;

                knots[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
            else
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 pPos = new Vector3(knots[i - 1].Position.x, knots[i - 1].Position.y, knots[i - 1].Position.z);

                Vector3 forward1 = (pPos - cPos).normalized;
                forward1 *= -1;

                knots[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
        }

        for (int j = 0; j < knots.Length; j++)//tangents
        {
            if (j == 0)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);

                float forDist = Vector3.Distance(cPos, nPos) * sharp;


                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, -forDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j == knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);

                float backDist = Vector3.Distance(cPos, pPos) * sharp;


                float3 forward1 = new float3(0, 0, backDist);
                float3 backward1 = new float3(0, 0, -backDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j != 0 && j != knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);


                float forDist = Vector3.Distance(cPos, nPos) * sharp;
                float backDist = Vector3.Distance(cPos, pPos) * sharp;

                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, backDist);
                backward1 *= -1;

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }

            spline.Add(knots[j]);

        }

        return spline;

    }


    public Spline randomSharpSpline(List<Vector3> pos, float minSharp, float maxSharp)
    {
        BezierKnot[] knots = new BezierKnot[pos.Count];

        Spline spline = new Spline();
        spline.CopyTo(knots, 0);
        spline.Clear();


        for (int j = 0; j < pos.Count; j++)//positions
        {
            knots[j].Position = pos[j];
            //trunkSpline.Add(test[j]);
        }

        for (int i = 0; i < knots.Length; i++)//rotation
        {

            if (i != knots.Length - 1)
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 nPos = new Vector3(knots[i + 1].Position.x, knots[i + 1].Position.y, knots[i + 1].Position.z);

                Vector3 forward1 = (nPos - cPos).normalized;

                knots[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
            else
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 pPos = new Vector3(knots[i - 1].Position.x, knots[i - 1].Position.y, knots[i - 1].Position.z);

                Vector3 forward1 = (pPos - cPos).normalized;
                forward1 *= -1;

                knots[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
        }

        float sharp = 0;

        for (int j = 0; j < knots.Length; j++)//tangents
        {

            sharp = subRandom(minSharp, maxSharp);

            if (j == 0)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);



                float forDist = Vector3.Distance(cPos, nPos) * sharp;


                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, -forDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j == knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);

                float backDist = Vector3.Distance(cPos, pPos) * sharp;


                float3 forward1 = new float3(0, 0, backDist);
                float3 backward1 = new float3(0, 0, -backDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j != 0 && j != knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);


                float forDist = Vector3.Distance(cPos, nPos) * sharp;
                float backDist = Vector3.Distance(cPos, pPos) * sharp;

                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, backDist);
                backward1 *= -1;

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }

            spline.Add(knots[j]);

        }

        return spline;

    }



    public Spline EasyCurveSpline(List<Vector3> pos, float sharp)
    {
        BezierKnot[] knots = new BezierKnot[pos.Count];

        Spline spline = new Spline();
        spline.CopyTo(knots, 0);
        spline.Clear();


        for (int j = 0; j < pos.Count; j++)//positions
        {
            knots[j].Position = pos[j];
            //trunkSpline.Add(test[j]);
        }

        for (int i = 0; i < knots.Length; i++)//rotation
        {

            if (i != knots.Length - 1)
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 nPos = new Vector3(knots[i + 1].Position.x, knots[i + 1].Position.y, knots[i + 1].Position.z);

                Vector3 forward1 = (nPos - cPos).normalized;

                
                Quaternion firstDir = Quaternion.LookRotation(forward1, transform.up);
                Quaternion dir = firstDir;

                if(i >0 && i < knots.Length - 1)
                {
                    Vector3 backDir = cPos - new Vector3(knots[i - 1].Position.x, knots[i - 1].Position.y, knots[i - 1].Position.z);
                    Quaternion secDir = Quaternion.LookRotation(backDir);

                    dir = Quaternion.Slerp(firstDir, secDir, .5f);
                }

                knots[i].Rotation = dir;
            }
            else
            {

                Vector3 cPos = new Vector3(knots[i].Position.x, knots[i].Position.y, knots[i].Position.z);
                Vector3 pPos = new Vector3(knots[i - 1].Position.x, knots[i - 1].Position.y, knots[i - 1].Position.z);

                Vector3 forward1 = (pPos - cPos).normalized;
                forward1 *= -1;

                knots[i].Rotation = Quaternion.LookRotation(forward1, transform.up);
            }
        }

        for (int j = 0; j < knots.Length; j++)//tangents
        {
            if (j == 0)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);

                float forDist = Vector3.Distance(cPos, nPos) * sharp;


                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, -forDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j == knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);

                float backDist = Vector3.Distance(cPos, pPos) * sharp;


                float3 forward1 = new float3(0, 0, backDist);
                float3 backward1 = new float3(0, 0, -backDist);

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }
            if (j != 0 && j != knots.Length - 1)
            {

                Vector3 cPos = new float3(knots[j].Position.x, knots[j].Position.y, knots[j].Position.z);
                Vector3 nPos = new float3(knots[j + 1].Position.x, knots[j + 1].Position.y, knots[j + 1].Position.z);
                Vector3 pPos = new float3(knots[j - 1].Position.x, knots[j - 1].Position.y, knots[j - 1].Position.z);


                float forDist = Vector3.Distance(cPos, nPos) * sharp;
                float backDist = Vector3.Distance(cPos, pPos) * sharp;

                float3 forward1 = new float3(0, 0, forDist);
                float3 backward1 = new float3(0, 0, backDist);
                backward1 *= -1;

                knots[j].TangentIn = backward1;
                knots[j].TangentOut = forward1;
            }

            spline.Add(knots[j]);

        }

        return spline;

    }


    public Mesh GenerateMeshFromSplineContainerList(List<SplineContainer> containers, int resolution, bool invertTris, bool worldUVs)
    {

        int yRes = containers.Count;
        int xRes = resolution;

        Vector3[] verts = new Vector3[(yRes + 1) * (xRes + 1)];
        int[] triangles = new int[yRes * xRes * 6];
        Vector2[] UVs = new Vector2[verts.Length];

        int i = 0;

        for (int y = 0; y < yRes; y++)
        {
            Spline spline1 = containers[y][0];


            for (int x = 0; x <= xRes; x++)
            {

                float time = (float)x / xRes;

                float3 pos, st, up;

                containers[y].Evaluate(spline1, time, out pos, out st, out up);

                verts[i] = pos;

                //UVs[i] = new Vector2((float)x / xRes, (float)y / yRes);

                UVs[i] = new Vector2((float)y / (yRes-1), (float)x / (xRes));

                

                i++;
            }

        }

        int vert = 0, tris = 0;

        for (int z = 0; z < yRes - 1; z++)
        {
            if (invertTris)
            {
                for (int x = 0; x < xRes; x++)
                {
                    //vert = (z * yRes) + x;

                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 2] = vert + xRes + 1;
                    triangles[tris + 1] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 5] = vert + xRes + 1;
                    triangles[tris + 4] = vert + xRes + 2;


                    vert++;
                    tris += 6;
                }
            }
            if (!invertTris)
            {
                for (int x = 0; x < xRes; x++)
                {
                    //vert = (z * yRes) + x;

                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + xRes + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xRes + 1;
                    triangles[tris + 5] = vert + xRes + 2;

                    vert++;
                    tris += 6;
                }
            }
            

            vert++;
        }

        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = UVs;

        mesh.RecalculateNormals();

        

        if (worldUVs)
        {

            for(int a = 0; a < mesh.vertices.Length;a++)
            {

                Vector3 normDir = mesh.normals[a].normalized;
                normDir = new Vector3(Mathf.Abs(normDir.x), Mathf.Abs(normDir.y), Mathf.Abs(normDir.z));
                Vector3 pos = mesh.vertices[a];

                Vector2 zy = new Vector2(pos.z, pos.y);
                Vector2 xy = new Vector2(pos.x, pos.y);
                Vector2 xz = new Vector2(pos.x, pos.z);

                float xTime = step(normDir.x);
                float yTime = step(normDir.y);

                Vector2 outUV = Vector2.Lerp(xy, xz, yTime);
                outUV = Vector2.Lerp(outUV, zy, xTime);

                //outUV = new Vector2(pos.x, pos.z);

                //Debug.Log("world uv " + outUV);

                UVs[a] = outUV;

            }

        }
        mesh.uv = UVs;

        return mesh;

    }

    //as in the splines are partially measured, not that the list itself is partial
    public Mesh PartialSplineList_MeshGen(List<SplineContainer> containers, int resolution, bool invertTris, bool worldUVs, float minPercentMeasured, float maxPercentMeasured)
    {

        int yRes = containers.Count;
        int xRes = resolution;

        Vector3[] verts = new Vector3[(yRes + 1) * (xRes + 1)];
        int[] triangles = new int[yRes * xRes * 6];
        Vector2[] UVs = new Vector2[verts.Length];

        int i = 0;

        for (int y = 0; y < yRes; y++)
        {
            Spline spline1 = containers[y][0];


            for (int x = 0; x <= xRes; x++)
            {

                float time = (float)x / xRes;

                time = Mathf.Lerp(minPercentMeasured, maxPercentMeasured, time);

                float3 pos, st, up;

                containers[y].Evaluate(spline1, time, out pos, out st, out up);

                verts[i] = pos;

                //UVs[i] = new Vector2((float)x / xRes, (float)y / yRes);

                UVs[i] = new Vector2((float)y / yRes, (float)x / xRes);



                i++;
            }

        }

        int vert = 0, tris = 0;

        for (int z = 0; z < yRes - 1; z++)
        {
            if (invertTris)
            {
                for (int x = 0; x < xRes; x++)
                {
                    //vert = (z * yRes) + x;

                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 2] = vert + xRes + 1;
                    triangles[tris + 1] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 5] = vert + xRes + 1;
                    triangles[tris + 4] = vert + xRes + 2;


                    vert++;
                    tris += 6;
                }
            }
            if (!invertTris)
            {
                for (int x = 0; x < xRes; x++)
                {
                    //vert = (z * yRes) + x;

                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + xRes + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xRes + 1;
                    triangles[tris + 5] = vert + xRes + 2;

                    vert++;
                    tris += 6;
                }
            }


            vert++;
        }

        Mesh mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = UVs;

        mesh.RecalculateNormals();



        if (worldUVs)
        {

            for (int a = 0; a < mesh.vertices.Length; a++)
            {

                Vector3 normDir = mesh.normals[a].normalized;
                normDir = new Vector3(Mathf.Abs(normDir.x), Mathf.Abs(normDir.y), Mathf.Abs(normDir.z));
                Vector3 pos = mesh.vertices[a];

                Vector2 zy = new Vector2(pos.z, pos.y);
                Vector2 xy = new Vector2(pos.x, pos.y);
                Vector2 xz = new Vector2(pos.x, pos.z);

                float xTime = step(normDir.x);
                float yTime = step(normDir.y);

                Vector2 outUV = Vector2.Lerp(xy, xz, yTime);
                outUV = Vector2.Lerp(outUV, zy, xTime);

                //outUV = new Vector2(pos.x, pos.z);

                //Debug.Log("world uv " + outUV);

                UVs[a] = outUV;

            }

        }
        mesh.uv = UVs;

        return mesh;

    }

    float step(float input)
    {
        //float stepFloat = input;
        if (input >= .5f)
        {
            input = 1;
        }
        else
        {
            input = 0;
        }

        return input;

    }


    public Mesh nonTaporStemMesh1(int sideNumber, int lengthCount, float tunnelThickness, bool revTriangles, SplineContainer TrunkSC, Spline trunkSpline, Vector3 scale)
    {

        Vector3[] vertices = new Vector3[(sideNumber + 1) * (lengthCount + 1)];
        Vector3[] normals = new Vector3[vertices.Length];

        float time = 0;
        //float interval = 2f / lengthCount;
        float width = tunnelThickness;
        float3 pos;
        float3 st;
        float3 up;

        var rad = math.radians(360f / sideNumber);

        for (int i = 0, z = 0; z <= lengthCount; z++)
        {
            //time = (float)interval * z;
            time = (float)z/(lengthCount - 1);

            TrunkSC.Evaluate(trunkSpline, time, out pos, out st, out up);
            st = math.normalize(st);

            var rot = quaternion.LookRotationSafe(st, up);

            //for (int x = 0; x <= sideNumber; x++)
            for (int x = sideNumber; x >= 0; x--)
            {
                float3 p = new float3(math.cos(x * rad), math.sin(x * rad), 0f) * width;

                float3 pos1 = pos + math.rotate(rot, p);

                pos1 *= scale;

                vertices[i] = pos1;
                Vector3 normal = pos1 - pos;
                normal = normal.normalized;
                //Debug.Log("center pos " + pos + " new pos " + pos1 + " dir " +normal);

                normals[i] = normal;


                i++;
            }



        }

        int[] triangles = new int[sideNumber * lengthCount * 6];

        int vert = 0, tris = 0;


        if (revTriangles == false)
        {
            for (int z = 0; z < lengthCount; z++)
            {

                for (int x = 0; x < sideNumber; x++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + sideNumber + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + sideNumber + 1;
                    triangles[tris + 5] = vert + sideNumber + 2;

                    vert++;
                    tris += 6;
                }



                vert++;
            }
        }

        if (revTriangles == true)
        {
            for (int x = 0; x < lengthCount; x++)
            {

                for (int z = 0; z < sideNumber; z++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + sideNumber + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + sideNumber + 1;
                    triangles[tris + 5] = vert + sideNumber + 2;

                    vert++;
                    tris += 6;
                }



                vert++;
            }
        }


        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= lengthCount; z++)
        {
            for (int x = 0; x <= sideNumber; x++)
            {
                uvs[i] = new Vector2((float)x / sideNumber, (float)z / lengthCount);

                i++;
            }
        }




        Mesh hMesh = new Mesh();

        hMesh.vertices = vertices;
        hMesh.triangles = triangles;
        hMesh.uv = uvs;
        hMesh.normals = normals;


        return hMesh;
    }



    public Mesh UNWEIGHTEDCombineMeshsAndTextures(List<MeshFilter> meshesToCombine, List<List<string>> mapVarNames, out List<Texture2D> combinedTextureMaps)
    {
        bool CombineTextures = true;

        combinedTextureMaps = new List<Texture2D>();

        if (mapVarNames == null)
        {
            CombineTextures = false;
            //combinedTextureMaps = null;
        }

        //first list corrisponds to materials on the object list
        //sec list corrisponds to the parameter names of every texture map we want combined
        //(like the color map, normal map, and mask mat)

        //List<MeshFilter> meshesToCombine = new List<MeshFilter>();

        List<int> meshToMatIndexes = new List<int>();
        List<Material> matsToCombine = new List<Material>();
        Rect[] rects = new Rect[0];

        if (CombineTextures)
        {

            for (int i = 0; i < meshesToCombine.Count; i++)
            {

                Material mat = meshesToCombine[i].GetComponent<MeshRenderer>().material;

                int MeshMatInd = 0;
                bool hasMat = false;
                foreach (Material compareMat in matsToCombine)
                {
                    if (mat == compareMat)
                    {

                        hasMat = true;
                        break;
                    }

                    MeshMatInd++;
                }

                if (!hasMat)
                {
                    matsToCombine.Add(mat);
                }
                meshToMatIndexes.Add(MeshMatInd);
            }

            rects = new Rect[matsToCombine.Count];

            int maxCount = 0;

            foreach (List<string> sl in mapVarNames)
            {

                if (maxCount < sl.Count)
                {
                    maxCount = sl.Count;
                }

            }

            for (int i = 0; i < maxCount; i++)
            {
                Texture2D packedTexture = new Texture2D(8192, 8192);

                List<Texture2D> localTexs = new List<Texture2D>();

                for (int m = 0; m < matsToCombine.Count; m++)
                {

                    int reverseFindIndex = 0;
                    foreach (int iii in meshToMatIndexes)
                    {
                        if (iii == m)
                        {
                            break;
                        }
                        reverseFindIndex++;
                    }

                    Texture2D tex = (Texture2D)matsToCombine[m].GetTexture(mapVarNames[reverseFindIndex][i]);
                    localTexs.Add(tex);
                    //List<string> a = mapVarNames[reverseFindIndex];

                }

                rects = packedTexture.PackTextures(localTexs.ToArray(), 0, 8192);
                combinedTextureMaps.Add(packedTexture);

            }

        }


        var combine = new CombineInstance[meshesToCombine.Count];
        int vertCount = 0;

        for (int i = 0; i < meshesToCombine.Count; i++)
        {
            vertCount += meshesToCombine[i].mesh.vertices.Length;
            combine[i].mesh = meshesToCombine[i].mesh;
            combine[i].transform = meshesToCombine[i].transform.localToWorldMatrix;

        }



        var mesh = new Mesh();

        if(vertCount > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.CombineMeshes(combine);


        List<Vector2> finalUVs = new List<Vector2>();

        for (int i = 0; i < meshesToCombine.Count; i++)
        {
            //int prevUVCount = 0;
            //for(int p = 0; p < i; p++)
            //{
            //    prevUVCount += combMeshs[p].sharedMesh.uv.Length;
            //}

            if (CombineTextures)
            {
                Rect cRect = rects[meshToMatIndexes[i]];
                Vector2[] UVs = meshesToCombine[i].mesh.uv;



                for (int u = 0; u < UVs.Length; u++)
                {
                    float x = UVs[u].x;
                    float y = UVs[u].y;


                    x *= cRect.width;
                    y *= cRect.height;
                    x += cRect.x;
                    y += cRect.y;

                    finalUVs.Add(new Vector2(x, y));

                }
            }
            



            //combine[i].mesh.uv = UVs;

        }


        //ajust UVs


        return mesh;

    }

    public GameObject GenerateAGuardForABlade(bool hasGuard, bool hasBasket, bool hasD, bool isSym, float mWidth, float mThick, SplineContainer mainCurve, Material blankMetalMat)//don't use this from the cross guard generator script, that would be bad
    {

        GameObject g = Instantiate(crossGuardBuilder, transform);
        CrossGaurdGenerator cgg = g.GetComponent<CrossGaurdGenerator>();

        cgg.seed = seed;
        cgg.importedSwordWidth = mWidth;
        cgg.importedSwordThickness = mThick;

        cgg.hasGuard = hasGuard;
        cgg.hasBasketTypeGuard = hasBasket;
        cgg.dGuard = hasD;
        cgg.symetrical = isSym;

        cgg.mainBladeCurve = mainCurve;
        cgg.BlankMetalMat = blankMetalMat;
        cgg.Begin();
        //g.SetActive(false);
        //Destroy(g, 1);
        return cgg.Final;

    }

    public Spline connectAlternatingSplinesRough(List<SplineContainer> containers, int pointNumber, bool startAlt)
    {

        //Spline sp = new Spline();
        List<Vector3> points = new List<Vector3>();

        
        foreach(SplineContainer container in containers)
        {
            for(int i = 0; i < pointNumber; i++)
            {
                float percent = (float)i / (pointNumber - 1);
                if (!startAlt)
                {
                    percent = 1 - percent;
                }

                float3 pos, st, up;

                container.Evaluate(container[0], percent, out pos, out st, out up);

                points.Add(pos);

                

            }

            startAlt = !startAlt;
        }

        return EasyCurveSpline(points, .5f);

    }


    public List<Vector3> connectPointsWithRandomDetours(Vector3 startPoint, Vector3 endPoint, float minOffset, float maxOffset, Vector3 offsetDir, bool useOffsetDir, int pointCount)
    {
        List<Vector3> points = new List<Vector3>();

        points.Add(startPoint);

        for(int i = 1; i < pointCount-1; i++)
        {
            float percent = i/(pointCount-1f);

            Vector3 pos = Vector3.Lerp(startPoint, endPoint, percent);
            if (useOffsetDir)
            {
                pos += offsetDir * subRandom(minOffset, maxOffset);

            }
            else
            {
                pos += new Vector3(subRandom(minOffset, maxOffset), subRandom(minOffset, maxOffset), subRandom(minOffset, maxOffset));
            }

            points.Add(pos);

        }

        points.Add(endPoint);

        return points;
    }


    public MeshFilter emptyFilt_AAA()
    {
        return Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
    }

    public void MakeMeshShadeFlat(MeshFilter filter)
    {

        //must be done before vertex coloring, or any other operation aside from the ones listed below

        Vector3[] newVerts;
        Vector2[] newUVs;
        int[] newTrigs;

        RemakeMeshToDiscrete(filter.mesh.vertices, filter.mesh.triangles, filter.mesh.uv, out newVerts, out newTrigs, out newUVs);

        filter.mesh.vertices = newVerts;
        filter.mesh.triangles = newTrigs;
        filter.mesh.uv = newUVs;

        filter.mesh.RecalculateNormals();



    }

    void RemakeMeshToDiscrete(Vector3[] vert, int[] trig, Vector2[] UV, out Vector3[] vertDiscrete, out int[] trigDiscrete, out Vector2[] UVDiscrete)
    {
        vertDiscrete = new Vector3[trig.Length];
        UVDiscrete = new Vector2[trig.Length];
        trigDiscrete = new int[trig.Length];


        for (int i = 0; i < trig.Length; i++)
        {
            vertDiscrete[i] = vert[trig[i]];
            UVDiscrete[i] = UV[trig[i]];

            trigDiscrete[i] = i;
        }

    }


    public void InvertXZNormalDirs(MeshFilter filter)
    {

        Vector3[] norms = filter.mesh.normals;
        for (int i = 0; i < norms.Length; i++)
        {
            norms[i] = new Vector3(-norms[i].x, norms[i].y, -norms[i].z);
        }

        filter.mesh.normals = norms;

    }


}
