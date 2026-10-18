using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using EasyWeapon;

public class MeleeParentScript : MetalShapeGen
{

    [SerializeField] string objRename;


    [SerializeField] bool invertEnchantFilterNorms;
    [SerializeField] bool hideEnchants;

    public MagicParameterHolder mparamHolder;
    public Word_Language_Rand wordManager;
    public RandomPatternGen patternGen, magicPatternGen;//remember these work off perpendicular splines to make a grid (better for complicated shapes)
    LetterBuilder letterBuild;

    public MeshFilter outputEnchantmentFilter;

    public bool colorOverride;
    public Color bladeColor, hiltColor, handleColor, enchantmentColor;
    public Material MetalMaterial;
    //something to determin words on sword

    [Range(0f, 1f)]
    public float minBladeSmooth = 0f, maxBladeSmooth = 1f, minHiltSmooth = 0f, maxHiltSmooth = 1f, minHandleSmooth = 0f, maxHandleSmooth = 1f;

    public float3 pos, st, up;
    public List<Texture2D> ts;

    public override void Begin()
    {
        if (!colorOverride)
        {
            RandomizeColors();

        }

        base.Begin();

        //get the letter builder and the pattern builder going here

        if(mparamHolder != null)
        {
            mparamHolder.Begin();

            if (wordManager != null)
            {
                enchantmentSetUp();

            }

        }

         
        if(Final == null)
        {
            Final = Instantiate(emptyMeshPreAAA, transform);
            Final.AddComponent<weaponControllerAA>();//add enchant filters before all merged into one big mesh
            Final.GetComponent<weaponControllerAA>().enchantAlwaysVisable = !hideEnchants;

            if(objRename != null && objRename != "")
            {
                Final.name = objRename;
            }

        }
        else
        {

            if (objRename != null && objRename != "")
            {
                Final.name = objRename;
            }
        }


        if(patternGen != null)
        {
            patternGen.seed = seed;
            patternGen.Begin();

        }

        if(mparamHolder != null)
        {
            enchantmentColor = mparamHolder.spellColor;

        }
        else
        {
            if (!colorOverride)
            {
                enchantmentColor = bladeColor;

            }
        }

    }

    public void RandomizeColors()
    {

        int realSeed = seed;
        seed = colorSeed;
        randomIndex = 0;

        Color[] cols = new Color[3];

        for(int i = 0; i < cols.Length; i++)
        {
            float h = subRandom(0,1f), s = subRandom(0, 1f), v = subRandom(0, 1f);

            cols[i] = Color.HSVToRGB(h, s, v);
            if (decider5050() && i == 0)
            {
                //if(i != 2)
                //{
                //    cols[i].r += 1f;
                //    cols[i].g += 1f;
                //    cols[i].b += 1f;
                //}
                cols[i].r += 1f;
                cols[i].g += 1f;
                cols[i].b += 1f;
            }
        }

        bladeColor = cols[0];
        hiltColor = cols[1];
        handleColor = cols[2];

        bladeColor.a = subRandom(minBladeSmooth, maxBladeSmooth);
        hiltColor.a = subRandom(minHiltSmooth, maxHiltSmooth);
        handleColor.a = subRandom(minHandleSmooth, maxHandleSmooth);

        seed = realSeed;
        randomIndex = 0;
    }

    public void enchantmentSetUp()
    {
        if(mparamHolder.isPatternBased)
        {
            magicPatternGen.seed = mparamHolder.seed;
            magicPatternGen.Begin();
        }
        else
        {
            wordManager.seed = mparamHolder.seed;
            wordManager.Begin();
            letterBuild = wordManager.usedBuilder;
        }



    }

    public void enchantmentPlacement(SplineContainer innerContainer, SplineContainer outerContainer, Transform parent, bool useSplineUP)
    {
        if (mparamHolder != null && wordManager != null)
        {
            if (mparamHolder.isPatternBased)
            {
                magicPatternGen.arrangeAlongTwoSplines(innerContainer, outerContainer, out outputEnchantmentFilter);
                outputEnchantmentFilter.transform.parent = parent;
            }
            else
            {
                string convertedParameters;
                wordManager.CreateMagicWords(mparamHolder.generatedSpellInfo, mparamHolder.seed, out convertedParameters);
                letterBuild.PlaceLettersAlongGuides(convertedParameters, parent, innerContainer, outerContainer, Vector3.up, out outputEnchantmentFilter, useSplineUP);

            }

            Color[] cols = new Color[outputEnchantmentFilter.mesh.vertices.Length];
            for(int i = 0; i < cols.Length; i++)
            {
                cols[i] = enchantmentColor;
            }

            outputEnchantmentFilter.mesh.colors = cols;

            if (invertEnchantFilterNorms)
            {
                InvertXZNormalDirs(outputEnchantmentFilter);
            }

        }


    }
    public void ReColorAMesh(MeshFilter filt, Color col)
    {
        Color[] bladeCols = new Color[filt.mesh.vertices.Length];
        for (int i = 0; i < bladeCols.Length; i++)
        {
            bladeCols[i] = col;
        }
        filt.mesh.colors = bladeCols;

    }



    public Mesh PartialSplineDrivenTapor(int sideNumber, int lengthCount, bool revTriangles, SplineContainer TrunkSC, SplineContainer ThickDeterminer, float maxThick, float maxPercent)
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

    public SplineContainer emptyCont_AAA()
    {
        SplineContainer upB = Instantiate(containerPreAAA, transform);
        return upB;
    }


    public List<float> variedPercentsAAA(int count, bool ended, float minGap)
    {
        List<float> results = new List<float>();
        float total = 0;
        if (!ended)
        {
            total += subRandom(minGap / 2, 1f - (minGap / 2));
        }

        for (int i = 0; i < count; i++)
        {
            results.Add(total);

            if (i < count - 1)
            {
                total += subRandom(minGap / 2, 1f - (minGap / 2));
            }

        }
        if (!ended)
        {
            total += subRandom(minGap / 2, 1f - (minGap / 2));
        }

        for (int i = 0; i < count; i++)
        {
            results[i] /= total;

        }

        return results;
    }

    public void InactivateParaphanalia()
    {
        if (mparamHolder != null)
        {
            mparamHolder.gameObject.SetActive(false);

        }
        if(wordManager != null)
        {
            wordManager.gameObject.SetActive(false);

        }
        patternGen.gameObject.SetActive(false);
        magicPatternGen.gameObject.SetActive(false);
    }

    public void MakeEnchantedDuplicate()
    {

        MeshFilter dup = emptyFilt_AAA();
        dup.mesh = Final.GetComponent<MeshFilter>().mesh;

        Color[] newCols = new Color[dup.mesh.colors.Length];
        int ind = 0;
        foreach (Color col in dup.mesh.colors)
        {
            newCols[ind] = col;
            if (col == enchantmentColor)
            {

                newCols[ind] = bladeColor;
            }
            ind++;
        }

        Final.GetComponent<MeshFilter>().mesh.colors = newCols;
        dup.transform.localScale += new Vector3(.01f, .01f, 0);
        Final.GetComponent<weaponControllerAA>().enchantmentFilters.Add(dup);
        dup.GetComponent<MeshRenderer>().material = MetalMaterial;

        dup.transform.parent = Final.transform;


    }


}
