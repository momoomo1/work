using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialRandomizer : MonoBehaviour
{    

    [SerializeField] List<Material> materials = new List<Material>();
    //[SerializeField] Material handleMat;
    [SerializeField] List<Texture2D> OverrideTextures, PossibleNormals, PossibleSmoothMaps;


    [SerializeField] bool alteredColor = false;
    //[SerializeField] Color minColor, maxColor;
    [SerializeField] Gradient minToMaxColor;
    [SerializeField] Gradient minToMaxHandleColor;

    float minHue = 0, maxHue = 1f, minSat=0f, maxSat=1f, minVal = 0f, maxVal =1f;



    [SerializeField] bool monoChrome = false;
    [SerializeField] bool changedMon = false;

    [SerializeField] float maxColorOffset = .2f;
    [SerializeField] float maxAnagHueSpread = .5f;
    [SerializeField] float minGlow=2, maxGlow=10;
    [SerializeField] float pureWhiteCutOff = .1f;
    [SerializeField] float minGlowSat = .9f;

    [SerializeField] float minSmoothTiling = .5f, maxSmoothTiling = 2f;
    [SerializeField] float maxSmoothDenom = 4;
    int usedSeed;
    int randomIndex;

    //Material swordM;      0
    //Material flatFaceM;   1
    //Material roundFaceM;  2//blank
    //Material spikeM;      3
    //Material axeM;        4
    //handel material       5
    //alt blank metal       6

    public void MakeMaterialTypes(out List<Material> matTypes, int InputSeed)
    {
        randomIndex = 0;
        usedSeed = InputSeed;
        usedSeed = (int)subrandom(0, 999999999);

        if (!changedMon)
        {
            float decider = subrandom(0f, 1f);
            if (decider >= .75f)
            {
                monoChrome = true;
            }
        }

        //if (alteredColor)
        //{
        //    Color.RGBToHSV(minColor, out minHue, out minSat, out minVal);
        //    Color.RGBToHSV(maxColor, out maxHue, out maxSat, out maxVal);
        //}
        //else
        //{
        //    minHue = 0; maxHue = 1; minSat = 0; maxSat = 1; minVal = 0;maxVal = 1;
        //}
        

        matTypes = new List<Material>();

        float[] parameters = new float[11];
        for(int i = 0; i < 7; i++)
        {
            parameters[i] = subrandom(0f, 1f);
        }

        Color[] colors1;
        DecideBatchColors(out colors1);

        int[] texInds = new int[8];

        for(int i = 0; i < texInds.Length; i++)
        {
            int inde = (int)subrandom(0, PossibleNormals.Count);

            if (i == 0 || i == 4)
            {
                inde = (int)subrandom(0, OverrideTextures.Count);
            }
            if(i == 5)
            {
                inde = (int)subrandom(0, PossibleSmoothMaps.Count);
            }
            texInds[i] = inde;
        }
        parameters[6] = subrandom(minSmoothTiling,maxSmoothTiling);
        //m.SetFloat("_smoothOffset", subrandom(-maxSmoothTiling, maxSmoothTiling));
        parameters[7] = subrandom(-maxSmoothTiling, maxSmoothTiling);
        //m.SetFloat("_smoothDom", subrandom(0f, 1f));
        //m.SetFloat("_smoothCrisp", subrandom(0f, 1f));
        //m.SetFloat("_smoothDenom", subrandom(1f, maxSmoothDenom));
        parameters[10] = subrandom(1f, maxSmoothDenom);
        int metalInd = 0;
        foreach (Material mat in materials)
        {
            if(metalInd <= 4)
            {
                Material m = new Material(mat);

                m.SetTexture("_OverrideTexture", OverrideTextures[texInds[0]]);
                m.SetTexture("_EdgeNormal", PossibleNormals[texInds[1]]);
                m.SetTexture("_FlatNormal", PossibleNormals[texInds[2]]);
                m.SetTexture("_OverrideNormal", PossibleNormals[texInds[3]]);

                



                m.SetColor("_FlatColor", colors1[0]);
                m.SetColor("_EdgeColor", colors1[1]);
                m.SetColor("_OverrideColor", colors1[2]);

                //if (metalInd == 2)
                //{
                //    m.SetColor("_FlatColor", colors1[1]);
                //    m.SetColor("_EdgeColor", colors1[0]);
                //}


                m.SetFloat("_edgeDom", parameters[0]);
                m.SetFloat("_edgeCrisp", parameters[1]);
                m.SetFloat("_overrideDom", parameters[2]);
                m.SetFloat("_overrideCrisp", parameters[3]);
                m.SetFloat("_edgeSmooth", parameters[4]);
                m.SetFloat("_flatSmooth", parameters[5]);
                m.SetFloat("_overrideSmooth", parameters[6]);

                m.SetTexture("_SmoothMod", PossibleSmoothMaps[texInds[5]]);
             
                m.SetFloat("_smoothTiling", parameters[6]);
                m.SetFloat("_smoothOffset", parameters[7]);
                m.SetFloat("_smoothDom", parameters[8]);
                m.SetFloat("_smoothCrisp", parameters[9]);
                m.SetFloat("_smoothDenom", parameters[10]);

                matTypes.Add(m);
            }
            
            metalInd++;
        }

        Material handleMat = new Material(materials[5]);

        handleMat.SetColor("Color", colors1[3]);
        handleMat.SetColor("_AltColor_1", colors1[2]);

        handleMat.SetFloat("_smoothCrisp", subrandom(0f,1f));
        handleMat.SetFloat("_smoothDom", subrandom(0f, 1f));
        handleMat.SetFloat("_minSmooth", subrandom(0f, 1f));
        handleMat.SetFloat("_maxSmooth", subrandom(0f, 1f));
        handleMat.SetFloat("_metallic", subrandom(0f, 1f));
        handleMat.SetFloat("_xScale", 1);
        handleMat.SetFloat("_yScale", subrandom(1f, 10f));

        handleMat.SetTexture("_Texture2D", OverrideTextures[texInds[4]]);
        handleMat.SetTexture("_mainNormalMap", PossibleNormals[texInds[6]]);
        handleMat.SetTexture("_altNormalMap", PossibleNormals[texInds[7]]);

        matTypes.Add(handleMat);


        Material altBlank = new Material(matTypes[2]);


        altBlank.SetColor("_FlatColor", colors1[1]);
        altBlank.SetColor("_EdgeColor", colors1[0]);

        matTypes.Add(altBlank);


    }



    void DecideBatchColors(out Color[] cols)
    {
        //edge Color
        //flat color
        //override color

        float hueDiff = maxHue - minHue;
        //float satDiff = maxSat - minSat;
        //float valDiff = maxVal - minVal;

        bool isContrast = false;
        float decider = subrandom(0f, 1f);
        if (decider > .5f)
        {
            isContrast = true;
        }

        cols = new Color[4];


        float diff = 1 / cols.Length;
        float startHue = subrandom(minHue, maxHue);

        if (!isContrast)
        {

            float spread = subrandom(0, maxAnagHueSpread);
            diff = spread / cols.Length;
        }

        for (int i = 0; i < cols.Length; i++)
        {
            float currentHue = startHue + (diff * i);
            currentHue += subrandom(0, 1f);

            if(currentHue > maxHue)
            {
                currentHue -= hueDiff;
            }
            if(currentHue < minHue)
            {
                currentHue += hueDiff;
            }

            if (monoChrome)
            {
                currentHue = startHue;
            }

            float sat = subrandom(0f, 1f);
            float val = subrandom(0f, 1f);

            if (alteredColor)
            {

                float h, s, v;

                Color.RGBToHSV(minToMaxColor.Evaluate(currentHue), out h, out s, out v);

                currentHue = h;

                Color.RGBToHSV(minToMaxColor.Evaluate(sat), out h, out s, out v);

                sat = s;

                Color.RGBToHSV(minToMaxColor.Evaluate(val), out h, out s, out v);

                val = v;

                if (i == 3)
                {
                    //sample from handle colors

                    Color.RGBToHSV(minToMaxHandleColor.Evaluate(currentHue), out h, out s, out v);

                    currentHue = h;

                    Color.RGBToHSV(minToMaxHandleColor.Evaluate(sat), out h, out s, out v);

                    sat = s;

                    Color.RGBToHSV(minToMaxHandleColor.Evaluate(val), out h, out s, out v);

                    val = v;

                }

            }

            Color col = Color.HSVToRGB(currentHue, sat, val);

            

            float decider2 = subrandom(0, 1f);
            if (decider2>.5f && i ==2)
            {
                float h, s, v;

                Color.RGBToHSV(col,out h, out s, out v);
                if (s > pureWhiteCutOff)
                {
                    s = subrandom(minGlowSat, 1f);
                }
                else
                {
                    s = 0f;
                }

                col = Color.HSVToRGB(h, s, v);

                col *= subrandom(minGlow, maxGlow);
            }

            cols[i] = col;



        }


    }


    float subrandom(float min, float max)
    {
        Random.InitState(usedSeed - randomIndex);
        float outFloat = Random.Range(min, max);
        randomIndex++;
        return outFloat;
    }


}
