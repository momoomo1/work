using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class LetterBuilder : BaseMono
{
    [SerializeField] Material letterMaterial;

    [SerializeField] SplineContainer fontContainer;
    [SerializeField] Transform EmptyT, roundSplinePre, StraightSplinePre, ChevSplinePre;

    public List<string> letterStrings = new List<string>();
    [SerializeField] List<LetterDefinitions> letterDefs = new List<LetterDefinitions>();

    [SerializeField] float minFontThickPercent = .15f, LetterMaxThick = .18f, minScaleBonus = 1f, maxScaleBonus = 2f;

    [SerializeField]
    int minRoundCounts = 0, maxRoundCounts = 7, minStraightCounts = 0, maxStraightCounts = 7,
        minChevCounts = 0, maxChevCounts = 7, minFontPoints = 3, maxFontPoints = 7, lengthMeshRes = 14,
        maxLetterCount = 100;

    float roundBias, straighBias, chevBias, scaleBonus;

    int realMaxRound, realMaxStraight, realMaxChev;

    bool symetricalFont;

    public string testMessage = "AVERAGE BALSAMIC SALAMANDER";
    public SplineContainer innerCon111, outerCon111;


    float3 pos, up, st;
    List<Texture2D> ts;

    
    public override void Begin()
    {


        base.Begin();

        roundBias = subrandom(0f, 1f);
        straighBias = subrandom(0f, 1f);
        chevBias = subrandom(0f, 1f);

        float totalBiasSTR = roundBias + straighBias + chevBias;

        roundBias /= totalBiasSTR;
        straighBias /= totalBiasSTR;
        chevBias /= totalBiasSTR;

        realMaxRound = Mathf.RoundToInt(Mathf.Lerp(minRoundCounts, maxRoundCounts, roundBias));
        realMaxStraight = Mathf.RoundToInt(Mathf.Lerp(minStraightCounts, maxStraightCounts, straighBias));
        realMaxChev = Mathf.RoundToInt(Mathf.Lerp(minChevCounts, maxChevCounts, chevBias));

        scaleBonus = subrandom(minScaleBonus, maxScaleBonus);

        if (letterDefs.Count == 0)
        {
            CreateRandomAlphabet();
        }

        CreateAllLetters();


    }

    public void PlaceLettersAlongGuides(string message, Transform parent, SplineContainer innerCon, SplineContainer outerCon, Vector3 upDir, out MeshFilter mf, bool useUP)
    {
        mf = Instantiate(EmptyMeshPreAAA, parent).GetComponent<MeshFilter>();

        List<Transform> letterPresInOrder = new List<Transform>();

        int fInd = 0;

        for (int i = 0; i < message.Length; i++)
        {
            for (int j = 0; j < letterStrings.Count; j++)
            {
                fInd = message.IndexOf(letterStrings[j], i);

                if (fInd == i)
                {

                    Transform LetMesh = Instantiate(letterDefs[j].LetterMesh, parent).transform;
                    letterPresInOrder.Add(LetMesh);

                }
            }
            //for (int j = 0; j < letterDefs.Count; j++)
            //{
            //    fInd = message.IndexOf(letterDefs[j].name, i);

            //    if (fInd == i)
            //    {

            //        Transform LetMesh = Instantiate(letterDefs[j].LetterMesh, parent).transform;
            //        letterPresInOrder.Add(LetMesh);

            //    }
            //}
        }

        float innerConLength = determineSplineLength(innerCon);

        float totalPercentUsed = 0f;

        float3 pos1;

        int currentLetterInd = 0, saftyCheck = 0;
        List<MeshFilter> filts = new List<MeshFilter>();

        while (totalPercentUsed < 1f && saftyCheck < maxLetterCount)
        {
            innerCon.Evaluate(innerCon[0], totalPercentUsed, out pos, out st, out up);
            outerCon.Evaluate(outerCon[0], totalPercentUsed, out pos1, out st, out up);

            float dist = Vector3.Distance(pos, pos1);

            Vector3 posToBe = (pos + pos1) / 2;
            Vector3 dirToFace = (pos - pos1);
            dirToFace.Normalize();



            Transform lT = Instantiate(letterPresInOrder[currentLetterInd], transform);

            lT.localPosition = posToBe;
            if(useUP)
            {
                lT.rotation = Quaternion.LookRotation(dirToFace, up);

            }
            else
            {
                lT.rotation = Quaternion.LookRotation(dirToFace, upDir);

            }
            //lT.localScale = Vector3.one * dist;
            lT.localScale = Vector3.one * dist*scaleBonus;

            filts.Add(lT.GetComponent<MeshFilter>());

            currentLetterInd++;
            saftyCheck++;
            if(currentLetterInd >= letterPresInOrder.Count)
            {
                currentLetterInd = 0;
            }

            float usedPerc = dist / innerConLength;
            //usedPerc /= 2f;

            if(usedPerc < 1f/maxLetterCount)
            {
                usedPerc = 1f/maxLetterCount;
            }

            totalPercentUsed += usedPerc;
            
        }

        mf.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);

        for(int i = letterPresInOrder.Count - 1; i >= 0; i--)
        {
            Destroy(letterPresInOrder[i].gameObject);
        }

    }


    void CreateRandomAlphabet()
    {

        for(int i = 0; i < letterStrings.Count-1; i++)
        {

            LetterDefinitions d = new LetterDefinitions();
            d.name = letterStrings[i];

            int roundCount = (int)subrandom(minRoundCounts, realMaxRound + 1);
            d.roundPercents = new float[roundCount];
            for (int j = 0; j < roundCount; j++)
            {
                d.roundPercents[j] = subrandom(.25f,1f);
            }
            d.straightCount = (int)subrandom(minStraightCounts, realMaxStraight + 1);
            d.chevCount = (int)subrandom(minChevCounts, realMaxChev + 1);

            if (d.roundPercents.Length == 0 && d.straightCount == 0 && d.chevCount == 0)
            {
                d.straightCount = 1;
            }

            letterDefs.Add(d);

        }

        LetterDefinitions d1 = new LetterDefinitions();
        d1.name = letterStrings[letterStrings.Count-1];

        int roundCount1 = 0;
        d1.roundPercents = new float[roundCount1];
        for (int j = 0; j < roundCount1; j++)
        {
            d1.roundPercents[j] = 0;
        }
        d1.straightCount = 0;
        d1.chevCount = 0;

        letterDefs.Add(d1);



    }


    void CreateAllLetters()
    {

        DecideFont();

        for (int j = 0; j < letterDefs.Count; j++)
        {
            CreateIndividualLetter(letterDefs[j], j);
        }
    }

    void DecideFont()
    {
        symetricalFont = decider5050();
        int fontPointCount = (int)subrandom(minFontPoints, maxFontPoints+1);

        List<float> offsets = new List<float>();

        List<Vector3> points = new List<Vector3>();

        points.Add(new Vector3(0, 0, -.1f));
        offsets.Add(0);
        for (int j = 0; j < fontPointCount; j++)
        {
            float zOff = j;
            float yOff = subrandom(minFontThickPercent, 1f);
            if (j == 0)
            {
                yOff /= 2;
            }
            if (!symetricalFont && j == fontPointCount - 1)
            {
                yOff /= 2;
            }
            points.Add(new Vector3(0, yOff, zOff));
            offsets.Add(yOff);

            if (!symetricalFont && j == fontPointCount - 1)
            {
                points.Add(new Vector3(0, 0, zOff + .1f));
                offsets.Add(0);
            }

        }


        if (symetricalFont)
        {
            int j2 = fontPointCount - 1;
            for(int i = points.Count - 1; i >= 0; i--) 
            {
                float zOff = j2;
                float yOff = offsets[i];

                points.Add(new Vector3(0, yOff, zOff));
                j2++;
                //offsets.Add(xOff);
            }
        }
        Vector3 v = points[points.Count - 1];
        v.y = points[0].y;
        points[points.Count - 1] = v;

        fontContainer.AddSpline(EasyCurveSpline(points, subrandom(0f, .5f)));
    }

    void CreateIndividualLetter(LetterDefinitions def, int ind)
    {
        List<MeshFilter> filtersToCombine = new List<MeshFilter>();

        Transform t = Instantiate(EmptyT, transform);

        Transform roundParent = Instantiate(EmptyT, t);
        Transform straightParent = Instantiate(EmptyT, t);
        Transform chevParent = Instantiate(EmptyT, t);

        float RoundPercents = .5f;

        if (decider5050() && decider5050())
        {
            RoundPercents = 1f;
            if (decider5050())
            {
                RoundPercents = subrandom(.1f, 1f);

            }
        }

        for(int i = 0; i < def.roundPercents.Length; i++)
        {
            PiecePosAndRot(roundSplinePre, roundParent);

            SplineContainer[] cons = roundParent.GetComponentsInChildren<SplineContainer>();

            foreach(SplineContainer c in cons)
            {
                MeshFilter m = Instantiate(EmptyMeshPreAAA, roundParent).GetComponent<MeshFilter>();

                m.mesh = PartialSplineDrivenTapor(4, lengthMeshRes, false, c, fontContainer, LetterMaxThick, RoundPercents);

                filtersToCombine.Add(m);

            }

        }
        for(int i = 0; i < def.straightCount; i++)
        {
            PiecePosAndRot(StraightSplinePre, straightParent);

            SplineContainer[] cons = straightParent.GetComponentsInChildren<SplineContainer>();

            foreach (SplineContainer c in cons)
            {
                MeshFilter m = Instantiate(EmptyMeshPreAAA, straightParent).GetComponent<MeshFilter>();

                m.mesh = PartialSplineDrivenTapor(4, lengthMeshRes, false, c, fontContainer, LetterMaxThick, 1f);

                filtersToCombine.Add(m);

            }
        }
        for (int i = 0;i < def.chevCount; i++)
        {
            PiecePosAndRot(ChevSplinePre, chevParent);
            SplineContainer[] cons = chevParent.GetComponentsInChildren<SplineContainer>();

            foreach (SplineContainer c in cons)
            {
                MeshFilter m = Instantiate(EmptyMeshPreAAA, chevParent).GetComponent<MeshFilter>();

                m.mesh = PartialSplineDrivenTapor(4, lengthMeshRes, false, c, fontContainer, LetterMaxThick, 1f);

                filtersToCombine.Add(m);

            }
        }

        MeshFilter letterMesh = Instantiate(EmptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        letterMesh.mesh = UNWEIGHTEDCombineMeshsAndTextures(filtersToCombine, null, out ts);

        letterMesh.GetComponent<MeshRenderer>().material = letterMaterial;

        def.LetterMesh = letterMesh.gameObject;


        int xPos = ind / 8;
        int zPos = ind - (xPos*8);

        //letterMesh.transform.localPosition = new Vector3(xPos, 0, zPos) * 1.5f;
        //t.localPosition = Vector3.right * ind;

        //def.LetterMesh = t.gameObject;
        roundParent.gameObject.SetActive(false);
        straightParent.gameObject.SetActive(false);
        chevParent.gameObject.SetActive(false);

    }

    void PiecePosAndRot(Transform pre, Transform parent)
    {

        int xPos = (int)subrandom(-1, 1 + 1);
        int zPos = (int)subrandom(-1, 1 + 1);

        float rot = (int)subrandom(1, 8 + 1);
        rot *= 45f;

        Transform t = Instantiate(pre, parent);

        Vector3 pos = new Vector3(xPos,0,zPos);
        pos *= .25f * 1.5f;

        Vector3 scale = Vector3.one;

        if(xPos ==0 && zPos == 0)
        {
            if (decider5050())
            {
                scale.x *= 2;
            }
            if (decider5050())
            {
                scale.z *= 2;
            }
        }
        scale *= .25f * 1.5f;

        t.localPosition = pos;
        t.localScale = scale;
        t.localEulerAngles = Vector3.up * rot;


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

                if (i > 0 && i < knots.Length - 1)
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

    Mesh UNWEIGHTEDCombineMeshsAndTextures(List<MeshFilter> meshesToCombine, List<List<string>> mapVarNames, out List<Texture2D> combinedTextureMaps)
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

        if (vertCount > 65535)
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


    float determineSplineLength(SplineContainer spl)
    {
        float length = 0;
        int measureRes = 100;

        Vector3 prevPos = Vector3.zero;
        for(int i = 0; i < measureRes; i++)
        {
            spl.Evaluate(spl[0], i/(measureRes - 1f), out pos, out st, out up);

            if(i > 0)
            {
                length += Vector3.Distance(prevPos, pos);
            }
            prevPos = pos;
        }


        return length;
    }
}




[System.Serializable]
public class LetterDefinitions
{
    public string name;
    public float[] roundPercents;
    public int straightCount;
    public int chevCount;
    public GameObject LetterMesh;
}

