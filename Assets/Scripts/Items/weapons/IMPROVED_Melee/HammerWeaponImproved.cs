using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class HammerWeaponImproved : MeleeParentScript
{

    [SerializeField] handleGen handleG;

    [SerializeField] MeshFilter face, spike, center, corner, HANDLE, HEAD;

    [SerializeField] SplineContainer circleContainer;
    SplineContainer profileSpline, faceProfile, innerC, outerC;


    [SerializeField] float minFaceWidthPerc = .5f, maxFaceWidthPerc = 1.5f, minFaceProfileWidth = .1f, minSpikeLengthProp = 1f, maxSpikeLengthProp = 5f,
        minCenterBalance = .25f, maxCenterBalance = .75f, minCornOffset = .25f, maxCornOffset = 1.25f, maxCornerThickPerc = .1f, minCornPatSize = .75f, maxCornPatSize = 1.25f,
        minheadCenterBalance = .1f, maxheadCenterBalance = .5f, minMaceHeadPerc = .15f, maxMaceHeadPerc = .33f, handleThickPerc = .4f;
    float transitionBalance, overrallSharpness, centerConnecterBalance;

    [SerializeField] int minSideCount = 4, maxSideCount = 10, minFaceResolution = 6, maxFaceResolution = 12, faceFrofileCount = 3, minMultSpikeCount = 2, maxMultSpikeCount = 4,
        connecterResolution = 8, minCornerPointCount = 5, maxCornerPointCount = 8, minCornFaceCount = 1, maxCornFaceCount = 4;
    int centerSideCount, faceSideCount, faceRes, cornerPointCount, cornFaceCount;

    bool isMaceType, centerOverReach, hasFace;


    public override void Begin()
    {
        base.Begin();

        if (decider5050())
        {
            isMaceType = decider5050();
        }

        centerOverReach = decider5050();
        hasFace = decider5050();

        if(isMaceType)
        {
            hasFace = false;
        }

        centerSideCount = (int)subRandom(minSideCount, maxSideCount+1);
        cornerPointCount = (int)subRandom(minCornerPointCount, maxCornerPointCount+1);
        cornFaceCount = (int)subRandom(minCornFaceCount, maxCornFaceCount+1);

        float half = centerSideCount / 2f;
        if(half > centerSideCount / 2)
        {
            centerSideCount++;//keeps it an even number
        }


        if (decider5050())
        {
            faceSideCount = (int)subRandom(minSideCount, maxSideCount + 1);

        }
        else
        {
            faceSideCount = minSideCount;
        }

        transitionBalance = subRandom(minFaceWidthPerc, maxFaceWidthPerc);
        transitionBalance = subRandom(minFaceWidthPerc, transitionBalance);
        overrallSharpness = subRandom(.01f, .5f);
        centerConnecterBalance = subRandom(minCenterBalance, maxCenterBalance);

        CreateFace();
        CreateSpike();
        CreateCenter();
        CreateCorner();
        CreateHandle();

        FinalAssemble();


        patternGen.gameObject.SetActive(false);
        magicPatternGen.gameObject.SetActive(false);

    }


    #region face
    void CreateFace()
    {

        FaceProfile();

        List<SplineContainer> faceConts = new List<SplineContainer>();
        List<float> timeBalances = new List<float>();

        faceRes = (int)subRandom(minFaceResolution, maxFaceResolution + 1);



        float total = 0;
        for(int i = 0; i < faceRes; i++)
        {
            
            if (i > 0)
            {
                total += subRandom(0f, 1f);
            }

            timeBalances.Add(total);
        }

        for(int i = 0; i < timeBalances.Count; i++)
        {
            timeBalances[i] /= total;
        }

        int innerContainerInd = (int)subRandom(2, faceRes-1);

        for( int i = 0; i < faceRes; i++)
        {

            SplineContainer c = Instantiate(circleContainer, transform);

            float bal = timeBalances[i];

            c.transform.position = Vector3.forward * bal;



            faceProfile.Evaluate(faceProfile[0], bal, out pos, out st, out up);

            c.transform.localScale = Vector3.one * pos.y;

            if(i == 0 || i == faceRes - 1)
            {
                c.transform.localScale = Vector3.zero;
            }

            faceConts.Add(c);

            if(i == innerContainerInd)
            {
                innerC = c;
                outerC = faceConts[i - 1];

                if (decider5050())
                {
                    if (decider5050())
                    {
                        innerC.transform.localScale = Vector3.one;
                        outerC.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        innerC.transform.localScale = Vector3.one * minFaceProfileWidth;
                        outerC.transform.localScale = Vector3.one * minFaceProfileWidth;
                    }
                }

                

            }

            if(i == faceRes - 1 && decider5050())
            {
                c.transform.localPosition = faceConts[i - 1].transform.localPosition;
            }

        }

        face = emptyFilt_AAA();

        face.mesh = GenerateMeshFromSplineContainerList(faceConts, faceSideCount, true, false);
        if(decider5050() || faceSideCount <= minFaceResolution+2)
        {
            MakeMeshShadeFlat(face);

        }
        ReColorAMesh(face, bladeColor);
        

        face.GetComponent<MeshRenderer>().material = MetalMaterial;

        EnchantFace(faceSideCount);

    }

    void FaceProfile()
    {

        faceProfile = Instantiate(containerPreAAA, transform);

        List<float> bala = new List<float>();
        List<float> yOff = new List<float>();
        List<Vector3> points = new List<Vector3>();
        float total = 0;
        for(int i = 0; i < faceFrofileCount; i++)
        {
            if (i > 0)
            {
                total += subRandom(0, 1f);
            }

            yOff.Add(subRandom(minFaceProfileWidth, 1f));

            bala.Add(total);
        }
        for (int i = 0; i < bala.Count; i++)
        {
            bala[i] /= total;

            points.Add(new Vector3(0, yOff[i], bala[i]));

        }

        faceProfile.AddSpline(EasyCurveSpline(points, overrallSharpness));

    }

    void EnchantFace( int sideCount)
    {
        if (decider5050())
        {
            Vector3 ogPos = innerC.transform.localPosition;

            innerC.transform.localPosition = Vector3.zero;

            List<Vector3> newPoints = new List<Vector3>();

            for (int i = 0; i < sideCount; i++)
            {
                float bal = i / (sideCount-0f);

                innerC.Evaluate(innerC[0], bal, out pos, out st, out up);

                newPoints.Add(pos);


            }

            SplineContainer sc = Instantiate(containerPreAAA, transform);

            sc.AddSpline(EasyCurveSpline(newPoints, .01f));
            sc[0].Closed = true;
            innerC = sc;
            innerC.transform.localPosition = ogPos;


            //outerC

            ogPos = outerC.transform.localPosition;
            outerC.transform.localPosition = Vector3.zero;
            newPoints.Clear();

            for (int i = 0; i < sideCount; i++)
            {
                float bal = i / (sideCount - 0f);

                outerC.Evaluate(outerC[0], bal, out pos, out st, out up);

                newPoints.Add(pos);


            }
            SplineContainer sc2 = Instantiate(containerPreAAA, transform);

            sc2.AddSpline(EasyCurveSpline(newPoints, .01f));
            sc2[0].Closed = true;
            outerC = sc2;
            outerC.transform.localPosition = ogPos;


        }




        enchantmentPlacement(innerC, outerC, transform, true);

        List<MeshFilter> filts = new List<MeshFilter>();

        filts.Add(face);
        if(outputEnchantmentFilter != null)
        {
            //ReColorAMesh(outputEnchantmentFilter, enchantmentColor);
            filts.Add(outputEnchantmentFilter);
            

        }

        face.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);

    }

    #endregion

    void CreateSpike()
    {

        float SpikeLengthProp = subRandom(minSpikeLengthProp, maxSpikeLengthProp);

        SplineContainer spikeCurve = Instantiate(containerPreAAA, transform);

        List<Vector3> points = new List<Vector3>();

        bool curvedSpike = decider5050();
        for(int i = 0; i < 3;  i++)
        {

            float zPos = 0f, yPos = 0f;

            if(curvedSpike)
            {
                if(i > 0)
                {
                    yPos = subRandom(0f,1f);
                }
                
            }
            
            if(i == 1)
            {
                zPos = subRandom(0.1f, .9f);
            }
            if(i == 2)
            {
                zPos = 1f;
            }

            zPos *= SpikeLengthProp;

            points.Add(new Vector3(0,yPos,zPos));

            if(i == 0)
            {
                points.Add(Vector3.forward * .001f);
            }

        }

        spikeCurve.AddSpline(EasyCurveSpline(points, overrallSharpness));

        SplineContainer spikeTapor = Instantiate(containerPreAAA, transform);

        float steep = subRandom(.1f, 1f);

        points.Clear();
        points.Add(Vector3.up);
        points.Add((Vector3.up * steep) + (Vector3.forward * .5f));
        points.Add(Vector3.forward);

        spikeTapor.AddSpline(EasyCurveSpline(points, overrallSharpness));

        spike = emptyFilt_AAA();
        spike.mesh = PartialSplineDrivenTapor(6, faceRes, false, spikeCurve, spikeTapor, .5f, 1f);

        int spikeCount = (int)subRandom(minMultSpikeCount, maxMultSpikeCount);

        //if (decider5050())//multipleSpikes
        {

            List<MeshFilter> filts = new List<MeshFilter>();

            //if(decider5050())//rotated around axis
            //{
            //    for(int i = 0; i < spikeCount; i++)
            //    {

            //        float rot = i/(float)spikeCount;
            //        rot *= 360;

            //        Transform t = Instantiate(spike.transform, transform);

            //        t.localEulerAngles = Vector3.forward * rot;

            //        t.transform.localScale = Vector3.one * subRandom(1f, 1.01f);

            //        filts.Add(t.GetComponent<MeshFilter>());

            //    }

            //    spike.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);

            //}
            //else//put into a grid
            {

                int sideLength = Mathf.RoundToInt(Mathf.Sqrt(spikeCount));

                if(sideLength < 2)
                {
                    sideLength = 2;
                }

                //float offset = sideLength / 2f;

                for(int i = 0;i < sideLength; i++)
                {
                    for(int j = 0; j < sideLength; j++)
                    {

                        

                        Transform t = Instantiate(spike.transform, transform);

                        t.transform.localPosition = new Vector3(i, j, 0);


                        filts.Add(t.GetComponent<MeshFilter>());


                    }
                }

                MeshFilter m = emptyFilt_AAA();
                m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);
                m.transform.localScale = Vector3.one * (1f / sideLength);
                m.transform.localPosition = new Vector3(-.25f, -.25f, 0);
                filts.Clear();
                filts.Add(m);

                spike.mesh = UNWEIGHTEDCombineMeshsAndTextures(filts, null, out ts);

            }

        }


        ReColorAMesh(spike, bladeColor);
        spike.GetComponent<MeshRenderer>().material = MetalMaterial;


    }

    #region center

    void CreateCenter()
    {
        MeshFilter cenConnecter = CreateConnecter();

        SplineContainer sidePre = Instantiate(containerPreAAA, transform);

        List<Vector3> points = new List<Vector3>()
        {
            new Vector3(0,-.5f,-.5f),
            new Vector3(0,0,-.5f),
            new Vector3(0,0,.5f),
            new Vector3(0,-.5f,.5f)

        };

        sidePre.AddSpline(EasyCurveSpline(points, .01f));

        List<SplineContainer> segments = new List<SplineContainer>();


        for(int i = 0; i <= centerSideCount; i++)
        {

            float perc = i / (centerSideCount-0f);

            SplineContainer seg = Instantiate(sidePre, transform);


            seg.transform.localEulerAngles = Vector3.forward * perc * 360;

            seg.transform.localPosition = seg.transform.up * .5f;

            segments.Add(seg);

        }

        MeshFilter cc = emptyFilt_AAA();

        cc.mesh = GenerateMeshFromSplineContainerList(segments, 4, false, false);

        if (decider5050())
        {
            MakeMeshShadeFlat(cc);
        }

        ReColorAMesh(cc, bladeColor);
        List<MeshFilter> filters = new List<MeshFilter>();

        if (decider5050())
        {

            foreach(SplineContainer seg in segments)
            {

                MeshFilter edg = emptyFilt_AAA();
                edg.mesh = nonTaporStemMesh1(4, 5, subRandom(.07f, .13f), false, seg, seg[0], Vector3.one);
                ReColorAMesh(edg, hiltColor);
                filters.Add(edg);

            }

            filters.Add(cc);

            cc.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);



        }

        

        filters.Clear();

        filters.Add(cc);

        bool magicAnyway = decider5050();
        if (magicAnyway)
            magicAnyway = decider5050();

        if (hasFace && !magicAnyway)
        {
            //dectrative patterns on every other face

            for(int i = 0; i < segments.Count-1; i += 2)
            {

                SplineContainer c1 = segments[i];
                SplineContainer c2 = segments[i+1];

                MeshFilter decor;

                patternGen.arrangeAlongTwoSplines(c1, c2, out decor);

                decor.transform.parent = transform;

                ReColorAMesh(decor, hiltColor);

                filters.Add(decor);

            }

        }
        if(magicAnyway || !hasFace)
        {
            //magic inscription

            for (int i = 0; i < segments.Count - 1; i += 2)
            {

                innerC = segments[i];
                outerC = segments[i + 1];


                enchantmentPlacement(innerC, outerC, transform, true);

                if(mparamHolder != null)
                {
                    MeshFilter decor = outputEnchantmentFilter;
                    
                    decor.transform.parent = transform;
                    
                    filters.Add(decor);
                }
                

            }
        }

        cc.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        cc.transform.localEulerAngles = Vector3.forward * (-.5f / centerSideCount);

        filters.Clear();
        filters.Add(cc);

        center = emptyFilt_AAA();

        //ADD THE CONNECTERS

        if(!isMaceType)
        {
            cc.transform.localScale = new Vector3(1, 1, centerConnecterBalance);

            float invCenBal = 1f - centerConnecterBalance;
            invCenBal /= 2f;

            cenConnecter.transform.localScale = new Vector3(1, 1, invCenBal);
            MeshFilter invCon = Instantiate(cenConnecter, transform);
            invCon.transform.localScale = new Vector3(1, 1, -invCenBal);

            invCon.transform.localPosition = -Vector3.forward * centerConnecterBalance / 2f;
            cenConnecter.transform.localPosition = Vector3.forward * centerConnecterBalance / 2f;

            filters.Add(cenConnecter); 
        
            filters.Add(invCon);
        }
        

        center.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        center.GetComponent<MeshRenderer>().material = MetalMaterial;
    }

    MeshFilter CreateConnecter()
    {
        MeshFilter con = emptyFilt_AAA();

        SplineContainer conCurve = Instantiate(containerPreAAA, transform);

        List<Vector3> points = new List<Vector3>
        {
            Vector3.zero,
            Vector3.forward * .1f,
            Vector3.forward * subRandom(.2f, .8f),
            Vector3.forward * .9f,
            Vector3.forward,
            Vector3.forward * 1.5f
        };

        if (decider5050())
        {
            points[2] += Vector3.up * subRandom(-.5f, .5f);

        }
        

        conCurve.AddSpline(EasyCurveSpline(points, overrallSharpness));

        points.Clear();
        SplineContainer conProfile = Instantiate(containerPreAAA, transform);

        points.Add(Vector3.up * subRandom(.75f, 1f));
        points.Add((Vector3.forward / 2) + (Vector3.up * subRandom(.2f, 1f)));
        points.Add(Vector3.forward+Vector3.up * subRandom(.75f, 1f));
        points.Add(Vector3.forward * 1.5f);

        conProfile.AddSpline(EasyCurveSpline(points, overrallSharpness));

        con.mesh = PartialSplineDrivenTapor(centerSideCount, connecterResolution, false, conCurve, conProfile, .5f, 1f);

        if (decider5050())
        {
            MakeMeshShadeFlat(con);
        }

        //if (decider5050())
        //{
        //    ReColorAMesh(con, bladeColor);
        //}
        //else
        {
            ReColorAMesh(con, hiltColor);

        }

        return con;
    }

    #endregion

    void CreateCorner()
    {
        SplineContainer cornerProfile = Instantiate(containerPreAAA, transform);

        List<Vector3> cornPoints = new List<Vector3>();

        cornPoints.Add(Vector3.left * .5f);

        for(int i = 0; i < cornerPointCount-2; i++)
        {
            


            float yPos = i/(cornerPointCount-2f);
            yPos -= .5f;
            float zPos = subRandom(minCornOffset, maxCornOffset);

            cornPoints.Add(new Vector3(yPos, 0, zPos));

        }
        cornPoints.Add(Vector3.right * .5f);

        cornerProfile.AddSpline(EasyCurveSpline(cornPoints, overrallSharpness));
        cornerProfile.transform.localEulerAngles = Vector3.forward * 90;

        bool hollowCorners = decider5050();
        hollowCorners = true;

        corner = emptyFilt_AAA();

        List<MeshFilter> filters = new List<MeshFilter>();


        SplineContainer backCon = Instantiate(containerPreAAA, transform);
        List<Vector3> backPoints = new List<Vector3>()
            {
                Vector3.down * .5f,
                Vector3.up * .5f
            };
        backCon.AddSpline(EasyCurveSpline(backPoints, .1f));

        SplineContainer leftBackCon = Instantiate(backCon, transform);
        SplineContainer leftCornCon = Instantiate(cornerProfile, transform);

        cornerProfile.transform.localPosition = Vector3.right * .5f * maxCornerThickPerc;
        leftCornCon.transform.localPosition = Vector3.right * -.5f * maxCornerThickPerc;
        backCon.transform.localPosition = Vector3.right * .5f * maxCornerThickPerc;
        leftBackCon.transform.localPosition = Vector3.right * -.5f * maxCornerThickPerc;


        List<SplineContainer> cornerSplines = new List<SplineContainer>()
            {
                backCon,
                cornerProfile,
                leftCornCon,
                leftBackCon
            };

        corner.mesh = GenerateMeshFromSplineContainerList(cornerSplines, faceRes, true, false);
        ReColorAMesh(corner, bladeColor);

        int a = (int)subRandom(0, 3 + 1);

        Transform m2;
        if (decider5050())
        {
            m2 = Instantiate(patternGen.HollowStar, transform);
        }
        else
        {
            m2 = Instantiate(patternGen.HollowPoly, transform);

        }
        ReColorAMesh(m2.GetComponent<MeshFilter>(), hiltColor);
        m2.localScale = new Vector3(1, 2, 1) * subRandom(minCornPatSize, maxCornPatSize);
        m2.localEulerAngles = Vector3.forward * 90;
        m2.localEulerAngles += Vector3.right * subRandom(0, 360);

        filters.Add(m2.GetComponent<MeshFilter>());

        filters.Add(corner);

        corner.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        corner.GetComponent<MeshRenderer>().material = MetalMaterial;

    }

    void CreateHandle()
    {
        handleG.pillarSeed = seed;
        handleG.Begin();
        List<MeshFilter> filters = new List<MeshFilter>();
        filters.AddRange(handleG.FinalPillar.GetComponentsInChildren<MeshFilter>());

        ReColorAMesh(filters[0], hiltColor);
        ReColorAMesh(filters[1], handleColor);
        ReColorAMesh(filters[2], hiltColor);

        HANDLE = emptyFilt_AAA();
        HANDLE.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        HANDLE.GetComponent<MeshRenderer>().material = MetalMaterial;
    }

    void FinalAssemble()
    {

        HEAD = emptyFilt_AAA();
        List<MeshFilter> filters = new List<MeshFilter>();

        float headCenterBalance = subRandom(minheadCenterBalance, maxheadCenterBalance);

        filters.Add(center);
        int picker = 0;

        if (isMaceType)
        {
            float maceHeadPerc = subRandom(minMaceHeadPerc, maxMaceHeadPerc);

            center.transform.localScale = new Vector3(thick, thick, maceHeadPerc * length);
            center.transform.localEulerAngles += new Vector3(90,0,0);
            corner.transform.localScale = new Vector3(width / 2f, maceHeadPerc * length, width / 2f);

            float addition = .5f / centerSideCount;

            for(int i = 0; i < centerSideCount; i++)
            {

                float perc = i/(centerSideCount-0f);

                MeshFilter m = Instantiate(corner, transform);

                m.transform.localEulerAngles = new Vector3(0, perc * 360f, 0);
                m.transform.localPosition = m.transform.forward * thick / 2;

                filters.Add(m);

            }


        }
        else
        {
            MeshFilter half1;

            if(hasFace)
            {
                half1 = face;
            }
            else
            {
                if (decider5050())//has spikes
                {
                    half1 = spike;
                }
                else//has corners
                {
                    //create corner face
                    half1 = cornerFace();
                }
            }

            MeshFilter half2 = null;

            bool sym = decider5050();

            if (sym)//symetry
            {
                half2 = Instantiate(half1, transform);
            }
            else//Asymetry
            {
                picker = (int)subRandom(0, 1+1);
                if (hasFace)
                {
                    picker = (int)subRandom(0, 2+1);
                }
                if(picker == 0)
                    half2 = Instantiate(spike, transform);
                if (picker == 1)
                    half2 = cornerFace();
                if(picker == 2)
                    half2 = Instantiate(face, transform);
            }

            if(decider5050() && !sym)
            {
                half2.transform.localEulerAngles = new Vector3(180, 0, 0);
            }
            else
            {
                half2.transform.localEulerAngles = new Vector3(0, 180, 0);
            }

            half1.transform.localScale = new Vector3(thick, thick, (1f - headCenterBalance) * (width / 2));
            half2.transform.localScale = new Vector3(thick, thick, (1f - headCenterBalance) * (width / 2));
            center.transform.localScale = new Vector3(thick, thick, (headCenterBalance) * width);

            half1.transform.localPosition = Vector3.forward * ((headCenterBalance/2f) * width);
            half2.transform.localPosition = Vector3.forward * ((-headCenterBalance / 2f) * width);

            filters.Add(half1);
            filters.Add(half2);
            filters.Add(center);



        }


        HEAD.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        HEAD.GetComponent<MeshRenderer>().material = MetalMaterial;

        filters.Clear();

        HANDLE.transform.localScale = new Vector3(handleThickPerc * thick, handleThickPerc * thick, length);

        HANDLE.transform.localEulerAngles = new Vector3(90,0,0);

        HEAD.transform.localPosition = Vector3.up * (length - (thick / 2f));

        GameObject g = Instantiate(emptyGameObjectAAA, transform);
        HEAD.transform.parent = g.transform;
        HANDLE.transform.parent = g.transform;

        g.transform.localEulerAngles = new Vector3(90, 0, 0);

        filters.Add(HEAD);
        filters.Add(HANDLE);

        //Final = Instantiate(emptyMeshPreAAA, transform);
        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        Final.GetComponent<MeshRenderer>().material = MetalMaterial;

        //DupEnchantWork();
        MakeEnchantedDuplicate();
    }


    MeshFilter cornerFace()
    {
        MeshFilter cornFace = emptyFilt_AAA();
        List<MeshFilter> filters = new List<MeshFilter>();

        for(int i = 0; i < cornFaceCount; i++)
        {

            float xPos = i/(cornFaceCount-1f);
            xPos -= .5f;

            MeshFilter m = Instantiate(corner, transform);
            m.transform.localPosition = Vector3.right * xPos;

            filters.Add(m);
        }

        cornFace.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        return cornFace;
    }

}
