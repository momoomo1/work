using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CrossGaurdGenerator : MetalShapeGen
{

    public float importedSwordWidth;
    public float importedSwordThickness;

    public bool hasGuard;
    public bool symetrical;
    public bool dGuard;
    public bool basketGaurd;
    public Material BlankMetalMat;

    public SplineContainer mainBladeCurve;

    bool straightGaurd;
    //bool hasBasket;//this means its ma
    
    bool dWire;
    bool sideLoops;
    bool wire;//this means the basket stuff is made from a wire mesh instead of a solid mesh
    //bool sharpSplines;
    bool hasAlts;
    bool isTapored;
    bool n_cleanerLook;

    public GameObject gaurdEndsObj;

    [SerializeField] BladeBaseGen betterBBase;

    [SerializeField] Transform mainGuardSplineParent;
    [SerializeField] SplineContainer mainGuardSpline;

    //    [SerializeField] SplineContainer profileContainerPre;
    //[SerializeField] SplineContainer altProfileContainerPre;
    [SerializeField] SplineContainer n_offsetContainerPre;

    [SerializeField] Transform QuadPre;

    //[SerializeField] int minAltsBees, maxAltsBees;

    //[SerializeField] float minGuardOffset, maxGuardOffset;
    [SerializeField] float maxCurvedGuardOffset;
    [SerializeField] float minBasketProfileOffset, maxBasketProfileOffset;

    [SerializeField] int minGuardPointCount, maxGuardPointCount, minBasektPointCount, maxBasektPointCount;
    [SerializeField] int minGuardRes,maxGuardRes, minBasektRes, maxBasektRes;
    [SerializeField] float minGuardLength_Percent, maxGuardLength_Percent;
    [SerializeField] int minProfileCount, maxProfileCount;
    [SerializeField] int minProfilePointCount, maxProfilePointCount;

    [SerializeField] float minGuardThick_Percent, maxGuardThick_Percent;
    [SerializeField] float n_minGuardWidth_Perc, n_maxGuardWidth_Perc;
    //[SerializeField] float minGaurdWidth, maxGaurdWidth;
    [SerializeField] float minWireThick, maxWireThick;
    [SerializeField] float minBasketWidth, maxBasketWidth;

    [SerializeField] float minSplineSharpness, maxSplineSharpness;

    float gaurdThick;
    float gaurdWidth;
    float wireThick;
    float basketWidth;

    [SerializeField] handleGen pillGen;
    [SerializeField] GameObject emptyWithMeshFilterPre;
    //[SerializeField] pinicalGenerator pinicGenerator;

    List<MeshFilter> pinnicalTypes = new List<MeshFilter>();

    [SerializeField] GameObject FinalPinicalTypes;

    [SerializeField] float n_minOffPercent, n_maxOffPercent;
    [SerializeField] Transform n_QuadEndPre;
    Vector3[] n_DirOffsets;
    [SerializeField] List<SplineContainer> n_containersToMesh = new List<SplineContainer>();


    //[SerializeField] SwordBladeGen swordBlade;
    public override void Begin()
    {
        base.Begin();

        gaurdThick = subRandom(minGuardThick_Percent, minGuardThick_Percent)* importedSwordWidth;
        gaurdWidth = subRandom(n_minGuardWidth_Perc, n_maxGuardWidth_Perc) * importedSwordWidth;

        //gaurdThick /= 2;
        //gaurdWidth /= 2;

        n_DirOffsets = new Vector3[4];
        n_DirOffsets[0] = new Vector3(1 * gaurdThick, 0, 1 * gaurdWidth);//front right
        n_DirOffsets[1] = new Vector3(-1 * gaurdThick, 0, 1 * gaurdWidth);//front left
        n_DirOffsets[2] = new Vector3(-1 * gaurdThick, 0, -1 * gaurdWidth);//back left
        n_DirOffsets[3] = new Vector3(1 * gaurdThick, 0, -1 * gaurdWidth);//back right

        //importedSwordWidth = swordBlade.width;
        //importedSwordThickness = swordBlade.thick;

        //hasGuard = decider5050();
        //hasGuard = true;


        //symetrical = decider5050();
        //straightGaurd = decider5050();

        if (subRandom(-4f,1f) > 0)
        {
            straightGaurd = true;
        }

        //curvedGaurd = decider5050();
        dGuard = decider5050();
        dWire = decider5050();
        wire = decider5050();

        //hasBasket = decider5050();

        basketGaurd = decider5050();
        sideLoops = decider5050();
        //sharpSplines = decider5050();
        hasAlts = decider5050();
        isTapored = decider5050();

        n_cleanerLook = decider5050();

        //isTapored = true;


        //gaurdWidth = subRandom(minGaurdWidth, maxGaurdWidth);
        //gaurdThick = subRandom(minGuardThick_Percent, maxGuardThick_Percent);
        wireThick = subRandom(minWireThick, maxWireThick);
        basketWidth = subRandom(minBasketWidth, maxBasketWidth);

        if(dGuard && basketGaurd)
        {
            if (dWire)
            {
                wire = true;
            }


        }

        CreateBladeBase();


        if (hasGuard)
        {
            CreateHilt();

        }

        

        List<Texture2D> ts = new List<Texture2D>();

        //Debug.Log("pinical types length " + pinnicalTypes.Count);

        FinalPinicalTypes.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(pinnicalTypes, null, out ts);

        FinalPinicalTypes.GetComponent<MeshRenderer>().material = BlankMetalMat;

    }


    void CreateBladeBase()
    {
        //pinicGenerator.pillarSeed = seed;
        //pinicGenerator.Begin();
        //    public SplineContainer bladeCurve;
        //public float BladeWidth;
        //public float BladeThick;
        //public bool bladeCurveSymetry = false;

        betterBBase.seed = seed;
        betterBBase.BladeWidth = importedSwordWidth*2;
        betterBBase.BladeThick = importedSwordThickness*.60f;
        betterBBase.bladeCurveSymetry = symetrical;
        betterBBase.bladeCurve = mainBladeCurve;
        betterBBase.Begin();


        GameObject bBase = Instantiate(betterBBase.Final, transform);

        //bBase.transform.localRotation = Quaternion.identity;
        //bBase.transform.localScale = new Vector3(importedSwordThickness*2f, importedSwordWidth*2, ((importedSwordThickness * 2)+(importedSwordWidth * 2))/2);//here

        pinnicalTypes.Add(bBase.GetComponentInChildren<MeshFilter>());
    }

    void CreateHilt()
    {
        List<Texture2D> emptys = new List<Texture2D>();

        if (straightGaurd)
        {
            //just use a pillar generator
            pillGen.pillarSeed = seed;
            pillGen.Begin();
            Transform T = Instantiate(pillGen.FinalPillar, transform).transform;
            float GHeight = importedSwordWidth * subRandom(minGuardLength_Percent, maxGuardLength_Percent);
            float GWidth = importedSwordThickness * subRandom(minGuardThick_Percent, maxGuardThick_Percent);

            T.localScale = new Vector3(GWidth, GHeight, GWidth);

            T.localPosition = -Vector3.up * (GHeight / 2);

            //T.localPosition = new Vector3()

            List<MeshFilter> filters = new List<MeshFilter>();

            filters.AddRange(T.GetComponentsInChildren<MeshFilter>());

            Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out emptys);
            Final.GetComponent<MeshRenderer>().material = BlankMetalMat;
        }
        else
        {
            List<Vector3> guardCurvePoints = new List<Vector3>();
            int curveCount = (int)subRandom(minGuardPointCount, maxGuardPointCount+1);
            float guardLength = importedSwordWidth * subRandom(minGuardLength_Percent, maxGuardLength_Percent);
            if (symetrical)
            {
                curveCount = (int)subRandom(minGuardPointCount, maxGuardPointCount-1);

                for (int i = 0; i < curveCount; i++)
                {
                    //height is y

                    //offset is z

                    float yPos = ((float)i / (curveCount - 1));
                    yPos *= guardLength/2;

                    float offset = subRandom(-maxCurvedGuardOffset, maxCurvedGuardOffset);

                    guardCurvePoints.Add(new Vector3(0,yPos, offset));
                }
            }
            else
            {
                float startDiff = subRandom(.5f,.5f);
                //float startDiff = .5f;

                for (int i = 0; i < curveCount; i++)
                {
                    //height is y

                    //offset is z

                    float yPos = ((float)i / (curveCount - 1))-startDiff;
                    yPos *= guardLength;

                    float offset = subRandom(-maxCurvedGuardOffset, maxCurvedGuardOffset);

                    guardCurvePoints.Add(new Vector3(0, yPos, offset));
                }



            }

            float sharpness = splineSharp;

            //if (sharpSplines)
            //{
            //    sharpness = subRandom(minSplineSharpness, maxSplineSharpness);
            //}

            mainGuardSpline.AddSpline(newSpline1(guardCurvePoints, sharpness));

            //once you make the half mesh for the symetrical, be sure to mirror and combine

            #region new Work

            //NOTE we will use double the number of points in main spline

            int n_curveCount = curveCount * 2;

            if (symetrical)
            {
                //do only the first half // reverse it // then copy that to all the other splines

                List<float> offPercents = new List<float>();

                for (int i = 0; i < n_curveCount/2; i++)
                {
                    offPercents.Add(subRandom(n_minOffPercent, n_maxOffPercent));
                }
                for (int i = 0; i < n_curveCount / 2; i++)
                {
                    offPercents.Add(offPercents[(n_curveCount / 2)-i-1]);
                }

                foreach (Vector3 v in n_DirOffsets)
                {
                    List<Vector3> vs = new List<Vector3>();
                    for (int j = 0; j < n_curveCount; j++)
                    {
                        float percent = (float)j / (offPercents.Count - 1);

                        float3 pos2, st2, up2;

                        mainGuardSpline.Evaluate(mainGuardSpline[0], percent, out pos2, out st2, out up2);

                        pos2 += (float3)(v * offPercents[j]);

                        vs.Add(pos2);

                    }

                    SplineContainer spl = Instantiate(n_offsetContainerPre, transform);
                    spl.AddSpline(randomSharpSpline(vs, minSplineSharpness, maxSplineSharpness));

                    n_containersToMesh.Add(spl);

                }


            }
            else
            {


                if (n_cleanerLook)
                {
                    List<float> offPercents = new List<float>();
                    
                    for(int i =0; i < n_curveCount; i++)
                    {
                        offPercents.Add(subRandom(n_minOffPercent, n_maxOffPercent));
                    }
                    foreach(Vector3 v in n_DirOffsets)
                    {
                        List<Vector3> vs = new List<Vector3>();
                        for(int j =0; j < n_curveCount; j++)
                        {
                            float percent = (float)j/(offPercents.Count-1);


                            float3 pos2, st2, up2;

                            mainGuardSpline.Evaluate(mainGuardSpline[0], percent, out pos2, out st2, out up2);

                            pos2 += (float3)(v * offPercents[j]);

                            vs.Add(pos2);

                        }

                        SplineContainer spl = Instantiate(n_offsetContainerPre, transform);
                        spl.AddSpline(randomSharpSpline(vs, minSplineSharpness, maxSplineSharpness));

                        n_containersToMesh.Add(spl);

                    }


                }
                else
                {
                    foreach (Vector3 v in n_DirOffsets)
                    {
                        List<Vector3> vs = new List<Vector3>();
                        for (int j = 0; j < n_curveCount; j++)
                        {
                            float percent = (float)j / (n_curveCount - 1);


                            float3 pos2, st2, up2;

                            mainGuardSpline.Evaluate(mainGuardSpline[0], percent, out pos2, out st2, out up2);

                            pos2 += (float3)(v * subRandom(n_minOffPercent, n_maxOffPercent));

                            vs.Add(pos2);

                        }

                        SplineContainer spl = Instantiate(n_offsetContainerPre, transform);
                        spl.AddSpline(randomSharpSpline(vs, minSplineSharpness, maxSplineSharpness));

                        n_containersToMesh.Add(spl);

                    }
                }
            }

            n_containersToMesh.Add(n_containersToMesh[0]);

            #endregion

            CreateQuadCaps();


            GameObject g = Instantiate(emptyWithMeshFilterPre, transform);

            int hiltRes = (int)subRandom(minGuardRes, maxGuardRes+1);

            Debug.Log("splinecount " + n_containersToMesh.Count);

            Final.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(n_containersToMesh, hiltRes, true, false);
            Final.GetComponent<MeshRenderer>().material = BlankMetalMat;

        }

    }

    void CreateQuadCaps()
    {

        float3 pos, st, up;

        Vector3[] startVerts = new Vector3[4];
        Vector3[] endVerts = new Vector3[4];
        Vector2[] UVs = new Vector2[4];
        //Vector2[] eUVs = new Vector2[4];

        UVs[0] = new Vector2(0, 1);
        UVs[1] = new Vector2(1, 1);
        UVs[2] = new Vector2(0, 0);
        UVs[3] = new Vector2(1, 0);

        int[] sTris = new int[6];
        int[] eTris = new int[6];

        sTris[0] = 0;
        sTris[1] = 1;
        sTris[2] = 2;
        sTris[3] = 0;
        sTris[4] = 2;
        sTris[5] = 3;


        eTris[0] = 0;
        eTris[2] = 1;
        eTris[1] = 2;
        eTris[3] = 0;
        eTris[5] = 2;
        eTris[4] = 3;



        n_containersToMesh[0].Evaluate(n_containersToMesh[0][0], 0f, out pos, out st, out up);

        startVerts[0] = pos;
        n_containersToMesh[1].Evaluate(n_containersToMesh[1][0], 0f, out pos, out st, out up);

        startVerts[1] = pos;
        n_containersToMesh[2].Evaluate(n_containersToMesh[2][0], 0f, out pos, out st, out up);

        startVerts[2] = pos;
        n_containersToMesh[3].Evaluate(n_containersToMesh[3][0], 0f, out pos, out st, out up);

        startVerts[3] = pos;



        n_containersToMesh[0].Evaluate(n_containersToMesh[0][0], 1f, out pos, out st, out up);

        endVerts[0] = pos;
        n_containersToMesh[1].Evaluate(n_containersToMesh[1][0], 1f, out pos, out st, out up);

        endVerts[1] = pos;
        n_containersToMesh[2].Evaluate(n_containersToMesh[2][0], 1f, out pos, out st, out up);

        endVerts[2] = pos;
        n_containersToMesh[3].Evaluate(n_containersToMesh[3][0], 1f, out pos, out st, out up);

        endVerts[3] = pos;

        GameObject start = Instantiate(emptyWithMeshFilterPre, Final.transform);
        GameObject end = Instantiate(emptyWithMeshFilterPre, Final.transform);

        Mesh sMesh = new Mesh();
        sMesh.vertices = startVerts;
        sMesh.uv = UVs;
        sMesh.triangles = sTris;
        sMesh.RecalculateNormals();

        Mesh eMesh = new Mesh();
        eMesh.vertices = endVerts;
        eMesh.uv = UVs;
        eMesh.triangles = eTris;
        eMesh.RecalculateNormals();

        start.GetComponent<MeshFilter>().mesh = sMesh;

        end.GetComponent<MeshFilter>().mesh = eMesh;

        start.GetComponent<MeshRenderer>().material = BlankMetalMat;
        end.GetComponent<MeshRenderer>().material = BlankMetalMat;

    }

    void CreateWiresFromSplineList()
    {

    }

    void CreateMeshFromSplineList()
    {

    }

}
