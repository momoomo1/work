using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class spearGen_Improved : MeleeParentScript
{

    [SerializeField] float minHandleBalance = .8f, maxHandleBalance = .9f, maxBendVariationPerc = 2f, minWidthPercent = .2f, maxWidthPercent = 1.2f,
        minCurvePointDistPerc = .2f, handleWidthPercent = .2f, HiltCenterPerc = 1.5f, minBarbPartVari = .5f, maxBarbPartVari = 1.5f,
        barbPartRotRange = 60, minPartVari = 2f, maxPartVari = 2.5f, lanceBarbSizePercent = .5f, spearBarbSizePercent = 1.2f;

    [SerializeField] int minAxisOfSymCount = 2, maxAxisOfSymCount = 6, minSideRes = 3, maxSideRes = 6, lowerMeshRes = 10, upperMeshRes = 20,
        minHandlePointCount = 4, maxHandlePointCount = 5, minBladePointCount = 4, maxBladePointCount = 6, minCenterCount = 0, maxCenterCount = 3,
        minPartCount = 1, maxPartCount = 4, minBarbPartCount = 1, maxBarbPartCount = 3, minBladeBarbCount = 1, maxBladeBarbCount = 6;

    [SerializeField] bool useRandomSharp = true;

    MeshFilter bladeFilter, handleFilter;

    [SerializeField] SplineContainer handleContainer, bladeContainer, fontContainer;

    [SerializeField] Transform barbPre;

    float handleLength, bladeLength, overallSharpness, barbSizePerc;

    int symAxisCount, bladePointCount, bladeSideCount, barbPerBladeCount;

    bool isLance, isSym, isCurved, symBarbs;

    [SerializeField] bool overrideBool, hasPommel, hasHilt;
    bool hasBarbs;
    List<Vector3> points = new List<Vector3>();

    List<Transform> allowedBarbs = new List<Transform>();

    [SerializeField] MeshFilter hiltPre;

    Transform emptyT, emptyRoter;

    public override void Begin()
    {
        base.Begin();
        if (decider5050())
        {
            isLance = decider5050();

        }

        //isLance = true;

        emptyT = Instantiate(emptyGameObjectAAA, transform).transform;
        emptyRoter = Instantiate(emptyGameObjectAAA, emptyT).transform;
        isSym = decider5050();
        isCurved = decider5050();
        if(isCurved)
        {
            isCurved = decider5050();
            if(isLance)
            {
                isCurved = decider5050();
            }
        }
        hasBarbs = decider5050();
        symBarbs = decider5050();

        if (isCurved && !isSym)
            hasBarbs = false;

        if (!overrideBool)
        {
            hasPommel = decider5050();
            hasHilt = decider5050();
        }

        barbSizePerc = spearBarbSizePercent;
        if (isLance)
        {
            width *=1.5f;
            thick *= 1.5f;
            
            if (isSym)
            {
                maxBendVariationPerc *= 1.25f;
                minWidthPercent *= 1.25f;
                maxWidthPercent *= 1.25f;
            }
            else
            {
                minWidthPercent *= 1.6f;
                maxWidthPercent *= 1.6f;
            }
            barbSizePerc = lanceBarbSizePercent;
        }

        bladePointCount = (int)subRandom(minBladePointCount, maxBladePointCount + 1);
        bladeSideCount = (int)subRandom(minSideRes, maxSideRes + 1);
        overallSharpness = subRandom(.01f, .5f);

        float handleBalance = subRandom(minHandleBalance, maxHandleBalance);

        if(isLance )
        {
            handleLength = 1f- handleBalance;
        }
        else
        {
            handleLength = handleBalance;
        }

        bladeLength = 1f - handleLength;

        if(hasBarbs)
        {
            barbPerBladeCount = (int)Mathf.Lerp(minBarbPartCount, maxBarbPartCount + 1, bladeLength);

        }
        else
        {
            barbPerBladeCount = 0;
        }

        handleLength *= length;
        bladeLength *= length;



        CreateBarbPre();
        CreateHandle();
        CreateBlade();
        ArrangeBarbs();
        CreateHiltAndPommel();


        FinalAssemble();
    }

    void CreateBarbPre()
    {
        //allowedBarbs.Add(patternGen.PillarPre);
        allowedBarbs.Add(patternGen.starArmPre);
        allowedBarbs.Add(patternGen.hollowStarArmPre);

        int partCount = (int)subRandom(minBarbPartCount, maxBarbPartCount + 1);
        List<MeshFilter> filters = new List<MeshFilter>();
        for(int i = 0; i < partCount; i++)
        {

            Transform t = Instantiate(allowedBarbs[(int)subRandom(0, allowedBarbs.Count)], transform);

            t.localEulerAngles = Vector3.up * subRandom(-barbPartRotRange / 2, barbPartRotRange / 2);

            if (decider5050())
            {
                t.localScale = new Vector3(-1, 1, 1);
            }
            t.localScale *= subRandom(minBarbPartVari, maxBarbPartVari);

            filters.Add(t.GetComponent<MeshFilter>());
        }

        MeshFilter m = emptyFilt_AAA();

        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        ReColorAMesh(m, hiltColor);

        barbPre = m.transform;

    }

    void CreateHandle()
    {
        handleContainer = Instantiate(containerPreAAA, transform);

        int handlePointCount = (int)subRandom(minHandlePointCount, maxHandlePointCount + 1);

        points.Clear();


        if(isCurved && !isLance)
        {

            Vector2 averageV2 = Vector2.zero;

            points.Add(Vector3.zero);

            int i;
            float perc;
            for (i = 1; i < handlePointCount-2; i++)
            {
                perc = i / (handlePointCount - 1f);

                float xOff = subRandom(-maxBendVariationPerc, maxBendVariationPerc) * width;
                float yOff = subRandom(-maxBendVariationPerc, maxBendVariationPerc) * width;

                Vector3 v = new Vector3(xOff, yOff, perc * handleLength);

                points.Add(v);
                averageV2 += (Vector2)v;

            }
            averageV2 /= (handlePointCount - 3);
            averageV2 *= -1;

            perc = i / (handlePointCount - 1f);

            points.Add(new Vector3(averageV2.x, averageV2.y, perc * handleLength));

            points.Add(Vector3.forward * handleLength);

        }
        else
        {
            points.Add(Vector3.zero);
            points.Add(Vector3.forward * handleLength);
        }

        handleContainer.AddSpline(EasyCurveSpline(points, subRandom(.01f, .5f)));

        float handleWidth = handleWidthPercent * width;

        handleFilter = emptyFilt_AAA();
        if (isLance)
        {
            handleFilter.mesh = nonTaporStemMesh1(4, lowerMeshRes, handleWidth, false, handleContainer, handleContainer[0], Vector3.one);

        }
        else
        {
            handleFilter.mesh = nonTaporStemMesh1(4, upperMeshRes, handleWidth, false, handleContainer, handleContainer[0], Vector3.one);

        }
        ReColorAMesh(handleFilter, handleColor);
    }

    void CreateBlade()
    {
        CreateFont();
        
        points.Clear();

        List<float> forwardBalances = new List<float>();

        float total = 0;
        forwardBalances.Add(0f);
        for(int i = 1; i < bladePointCount; i++)
        {

            total += subRandom(minCurvePointDistPerc, 1f);
            forwardBalances.Add(total);
        }
        for(int i =0; i < bladePointCount;i++)
        {
            forwardBalances[i] /= total;
        }

        if(isCurved)
        {
            Vector2 averageV2 = Vector2.zero;

            points.Add(Vector3.zero);
            points.Add(Vector3.forward * .01f);
            int i;
            float perc;
            for (i = 1; i < bladePointCount-1; i++)
            {
                perc = forwardBalances[i];

                float xOff = subRandom(-maxBendVariationPerc, maxBendVariationPerc) * width;
                float yOff = subRandom(-maxBendVariationPerc, maxBendVariationPerc) * width;

                Vector3 v = new Vector3(xOff, yOff, perc * bladeLength);

                points.Add(v);
                averageV2 += (Vector2)v;

            }
            averageV2 /= (bladePointCount - 2f);
            averageV2 *= -1;

            points.Add(new Vector3(averageV2.x, averageV2.y, bladeLength));


        }
        else
        {
            points.Add(Vector3.zero);
            points.Add(Vector3.forward * bladeLength);
        }

        bladeContainer = Instantiate(containerPreAAA, transform);

        if (useRandomSharp)
        {
            bladeContainer.AddSpline(randomSharpSpline(points, .01f, overallSharpness));
        }
        else
        {
            bladeContainer.AddSpline(EasyCurveSpline(points, overallSharpness));

        }

        bladeFilter = emptyFilt_AAA();

        if (isLance)
        {
            bladeFilter.mesh = PartialSplineDrivenTapor(bladeSideCount, upperMeshRes, false, bladeContainer, fontContainer, thick * maxWidthPercent, 1);

        }
        else
        {
            bladeFilter.mesh = PartialSplineDrivenTapor(bladeSideCount, lowerMeshRes, false, bladeContainer, fontContainer, thick * maxWidthPercent, 1);

        }

        if(mparamHolder != null)
        {
            //magic spell stuff here
        }



        ReColorAMesh(bladeFilter, bladeColor);

    }

    void CreateFont()
    {
        points.Clear();

        //points.Add(Vector3.zero);

        for(int i = 0; i < bladePointCount; i++)
        {

            float yPos = subRandom(minWidthPercent, maxWidthPercent);
            if(i == bladePointCount - 1)
            {
                yPos = 0;
            }
            points.Add(new Vector3(0, yPos, i));

        }

        fontContainer = Instantiate(containerPreAAA, transform);

        if (useRandomSharp)
        {
            fontContainer.AddSpline(randomSharpSpline(points, .01f, overallSharpness));
        }
        else
        {
            fontContainer.AddSpline(EasyCurveSpline(points, overallSharpness));

        }


    }

    void CreateHiltAndPommel()
    {
        List<Transform> allowedCenters = new List<Transform>();

        allowedCenters.Add(patternGen.StarPre);
        allowedCenters.Add(patternGen.PolyPre);
        allowedCenters.Add(patternGen.HollowStar);
        allowedCenters.Add(patternGen.HollowPoly);


        List<Vector3> allowedRots = new List<Vector3>()
        {
            Vector3.right*90,
            Vector3.up * 90,
            Vector3.forward * 90
        };
        
        List<MeshFilter> filters = new List<MeshFilter>();

        if (hasHilt)
        {

            MeshFilter hPre;

            GenHiltOrPom(allowedCenters, allowedRots, out hPre);

            hPre.transform.localPosition = Vector3.forward * handleLength;
            filters.Add(hPre);
        }

        if(hasPommel)
        {
            MeshFilter pomPre;
            GenHiltOrPom(allowedCenters, allowedRots, out pomPre);
            pomPre.transform.localScale /= 2;
            filters.Add(pomPre);

        }

        hiltPre = emptyFilt_AAA();
        hiltPre.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        ReColorAMesh(hiltPre, hiltColor);
    }

    void GenHiltOrPom(List<Transform> allowedCenters, List<Vector3> allowedRots, out MeshFilter HorP)
    {
        HorP = emptyFilt_AAA();


        List<MeshFilter> filters = new List<MeshFilter>();

        int centerCount = (int)subRandom(minCenterCount, maxCenterCount + 1);

        int dir = 1, ind = (int)subRandom(0, 2 + 1);
        if (decider5050())
            dir = -1;


        for (int i = 0; i < centerCount; i++)
        {
            Transform t = Instantiate(allowedCenters[(int)subRandom(0, allowedCenters.Count - 1)], transform);

            //t.localEulerAngles = Vector3.up * 90;
            t.localEulerAngles = allowedRots[ind];



            ind += dir;
            if (ind >= allowedRots.Count)
            {
                ind = 0;
            }
            if (ind < 0)
            {
                ind = allowedRots.Count - 1;
            }

            t.localScale = Vector3.one * width * HiltCenterPerc;

            filters.Add(t.GetComponent<MeshFilter>());

        }

        int partCount = (int)subRandom(minPartCount, maxPartCount + 1);


        float rotToBe = subRandom(0f, 360f);

        if(centerCount < maxCenterCount)
        {

            float scaleToBe = subRandom(minPartVari, maxPartVari);

            emptyT.localPosition = Vector3.zero;
            emptyT.localRotation = Quaternion.identity;
            emptyT.localScale = Vector3.one;

            for (int i = 0; i < partCount; i++)
            {

                float perc = i/(partCount - 0f);

                Transform t = Instantiate(barbPre, emptyT);
                //t.localEulerAngles = usedRot + (altRot * Mathf.Lerp(0,4f, perc));
                emptyT.localEulerAngles = Vector3.forward * Mathf.Lerp(0, 360, perc);
                t.localEulerAngles = Vector3.up * rotToBe;

                t.localScale = Vector3.one * scaleToBe * width;

                Quaternion q = t.rotation;

                t.parent = transform;
                t.rotation = q;
                filters.Add(t.GetComponent<MeshFilter>());

            }
        }


        HorP.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

    }


    void ArrangeBarbs()
    {

        //bladeContainer.transform.position += Vector3.forward * handleLength;
        List<MeshFilter> filters = new List<MeshFilter>();

        for(int i = 1; i < barbPerBladeCount+1; i++)
        {
            float alongPercent = i/(barbPerBladeCount+1f);


            bladeContainer.Evaluate(bladeContainer[0], alongPercent, out pos, out st, out up);

            //Debug.Log("arranging barbs" + pos);


            emptyT.position = pos;
            emptyT.rotation = Quaternion.LookRotation(st, up);

            fontContainer.Evaluate(fontContainer[0], alongPercent, out pos, out st, out up);

            float offset = pos.y * thick;


            int symAxisCount = 1;

            if (symBarbs)
            {
                symAxisCount = bladeSideCount;
            }

            for(int a = 0; a < symAxisCount; a++)
            {

                float rotPer = a/(symAxisCount-0f);

                emptyRoter.localEulerAngles = Vector3.forward * ((rotPer * 360) + 90f) ;

                Transform t = Instantiate(barbPre, transform);

                t.position = emptyRoter.position + (emptyRoter.up * offset * 1f);
                t.rotation = Quaternion.LookRotation(emptyRoter.up, emptyRoter.right);

                t.localScale = Vector3.one * offset * barbSizePerc;

                

                filters.Add(t.GetComponent<MeshFilter>());
            }


        }

        filters.Add(bladeFilter);
        bladeFilter.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        ReColorAMesh(bladeFilter, bladeColor);

    }

    void FinalAssemble()
    {

        //if symetrical, duplicate the generated handles and blades, then wrap them around based on axis of sym count

        MeshFilter f = Final.GetComponent<MeshFilter>();

        List<MeshFilter> filters = new List<MeshFilter>();
        bladeFilter.transform.localPosition = Vector3.forward * handleLength;
        filters.Add(bladeFilter);
        filters.Add(handleFilter);
        f.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        filters.Clear();
        filters.Add(f);
        if (isSym && isCurved)
        {


            int symAxisCount = (int)subRandom(minAxisOfSymCount, maxAxisOfSymCount + 1);

            for(int i = 1; i < symAxisCount; i++)
            {
                float rot = (float)i / (symAxisCount);
                rot *= 360f;
                MeshFilter mir = Instantiate(f, transform);
                mir.transform.localEulerAngles = Vector3.forward * rot;
                mir.transform.localScale = Vector3.one * (1+(.01f*i));
                filters.Add(mir);
            }

        }
        //hiltPre.transform.localScale = new Vector3(.5f, .5f, 1f);
        filters.Add(hiltPre);
        f.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        f.GetComponent<MeshRenderer>().material = MetalMaterial;


        if(isLance && isCurved)
        {
            f.transform.localScale = new Vector3(.5f, .5f, 1f);
            filters.Clear();
            filters.Add(f);
            f.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
            f.transform.localScale = Vector3.one;
        }


        //Final = f.gameObject;

    }



    Mesh PartialSplineDrivenTapor(int sideNumber, int lengthCount, bool revTriangles, SplineContainer TrunkSC, SplineContainer ThickDeterminer, float maxThick, float maxPercent)
    {

        Spline trunkSpline = TrunkSC[0];

        Vector3[] vertices = new Vector3[(sideNumber + 1) * (lengthCount + 1)];
        Vector3[] normals = new Vector3[vertices.Length];

        float time = 0;
        //float interval = 2f / lengthCount;
        float width = 0;

        var rad = math.radians(360f / sideNumber);

        for (int i = 0, z = 0; z <= lengthCount; z++)
        {
            //time = (float)interval * z;
            time = (float)z / (lengthCount - 1f);

            ThickDeterminer.Evaluate(ThickDeterminer[0], time, out pos, out st, out up);
            width = pos.y * maxThick;

            TrunkSC.Evaluate(trunkSpline, Mathf.Lerp(0, maxPercent, time), out pos, out st, out up);

            st = math.normalize(st);

            var rot = quaternion.LookRotationSafe(st, up);

            //for (int x = 0; x <= sideNumber; x++)
            for (int x = sideNumber; x >= 0; x--)
            {
                float3 p = new float3(math.cos(x * rad), math.sin(x * rad), 0f) * width;

                float3 pos1 = pos + math.rotate(rot, p);

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
        //hMesh.normals = normals;
        hMesh.RecalculateNormals();


        return hMesh;
    }


}
