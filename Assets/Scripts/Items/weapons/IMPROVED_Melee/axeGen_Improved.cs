using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class axeGen_Improved : MeleeParentScript
{
    [SerializeField] handleGen handleG;

    [SerializeField] MeshFilter head, handle;

    [SerializeField] Transform headHandleParent;

    [SerializeField] List<SplineContainer> splines = new List<SplineContainer>();

    [SerializeField] float minBladeOffset = .33f, maxBladeOffset = 1f, minBladeBalance = .25f, maxBladeBalance=.75f, minConnectionWidth = .1f, maxConnectionWidth = .3f,
        connecterCenterVariance = .4f, minInnerScale = .6f, maxInnerScale = .8f, minBetaBladeSizePerc = .25f, maxBetaBladeSizePerc = .5f, minConnecterVariance = .5f, 
        maxConnecterVariance = 1.5f, minConnecterDom = .25f, maxConnecterDom = .5f, minTeethVariance = 1f, maxTeethVariance = 3f, minHandleThickVari = .75f, 
        maxHandleThickVari = 1.5f, maxHandleOffsetPerc = .25f, handleThickPerc = 2f;
    float overallSharpness, centerOffset;

    [SerializeField] int minBeardPointCount = 3, maxBeardPointCount = 6, meshRes = 12, minTeethCount = 1, maxTeethCount = 4, minHandleProfilePointCount = 3, 
        maxHandleProfilePointCount = 5, minHandlePointCount = 3, maxhandlePointCount = 5, handleMeshRes = 24, handleSideCount = 6;

    bool symetrical, hollowBlade, hasTeeth, hasPatterns;

    //List<MeshFilter> dupEnchantFilters = new List<MeshFilter>();

    public override void Begin()
    {
        base.Begin();

        symetrical = decider5050();

        //symetrical = false;

        hollowBlade = decider5050();
        if(decider5050()&&decider5050())
            hasTeeth = true;

        hollowBlade = false;


        overallSharpness = subRandom(.01f, .5f);
        if (decider5050())
        {
            overallSharpness = .5f;
        }
        centerOffset = subRandom(- connecterCenterVariance, connecterCenterVariance);

        CreateHead();
        CreateHandle();
        FinalAssemble();

        InactivateParaphanalia();
    }

    #region head

    void CreateHead()
    {
        MeshFilter blade1 = blade();
        MeshFilter blade2 = null;
        MeshFilter Connecter = ConnecterMeshGen();

        float betaBladeSize = subRandom(minBetaBladeSizePerc, maxBetaBladeSizePerc);

        List<MeshFilter> filters = new List<MeshFilter>();

        if (symetrical)
        {
            blade2 = Instantiate(blade1, transform);
            betaBladeSize = 1f;
        }
        else
        {

            blade2 = blade();
        }

        blade2.transform.localScale = new Vector3(1, 1, -1) * betaBladeSize;

        filters.Add(blade1);
        filters.Add(blade2);
        float connectDom = subRandom(minConnecterDom, maxConnecterDom);
        Connecter.transform.localScale = new Vector3(connectDom*width, thick * 25, connectDom*width);
        Connecter.GetComponent<MeshRenderer>().material = MetalMaterial;
        filters.Add(Connecter);

        head = emptyFilt_AAA();
        head.transform.parent = headHandleParent;

        head.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        head.GetComponent<MeshRenderer>().material = MetalMaterial;
    }


    MeshFilter ConnecterMeshGen()
    {
        MeshFilter solid, hollow;
        bool type22 = decider5050();

        solid = Instantiate(patternGen.StarPre, transform).GetComponent<MeshFilter>();

        //if (decider5050())
        //{
        //    solid = Instantiate(patternGen.StarPre, transform).GetComponent<MeshFilter>();
        //}
        //else
        //{
        //    solid = Instantiate(patternGen.PolyPre, transform).GetComponent<MeshFilter>();
        //}

        if (decider5050())
        {
            hollow = Instantiate(patternGen.HollowStar, transform).GetComponent<MeshFilter>();
        }
        else
        {
            hollow = Instantiate(patternGen.HollowPoly, transform).GetComponent<MeshFilter>();
        }
        float yy = subRandom(minConnecterVariance, maxConnecterVariance);
        hollow.transform.localScale = new Vector3(1, 1, 1) * yy;
        
        solid.transform.localScale = new Vector3(1,1.1f,1)*subRandom(minConnecterVariance, yy);

        ReColorAMesh(hollow, hiltColor);
        ReColorAMesh(solid, bladeColor);

        if(hasPatterns)
        {
            ReColorAMesh(hollow, bladeColor);
            ReColorAMesh(solid, bladeColor);
        }
        else
        {
            ReColorAMesh(hollow, hiltColor);
            ReColorAMesh(solid, hiltColor);
        }

        List<MeshFilter> filters = new List<MeshFilter>() { solid, hollow };

        if (hollowBlade)
        {
            filters.Add(hollow);
        }
        else
        {
            if (decider5050()&&decider5050())
            {
                filters.Add(solid);
            }
            else
            {

                filters.Add(hollow);
                filters.Add(solid);
            }
        }
        MeshFilter f = emptyFilt_AAA();
        f.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        return f;
    }

    MeshFilter blade()
    {
        Transform SplinePar = Instantiate(emptyGameObjectAAA, transform).transform;

        SplineContainer beard1 = Beard();
        SplineContainer blade = Blade();

        

        beard1.transform.parent = SplinePar;
        blade.transform.parent = SplinePar;

        float bladeBal = subRandom(minBladeBalance, maxBladeBalance);

        blade.transform.localScale =new Vector3(1,1,bladeBal);

        blade.transform.localPosition = new Vector3(0,centerOffset, bladeBal);



        SplineContainer beard2 = null;
        if (decider5050())
        {
            beard2 = Instantiate(beard1, transform);

        }
        else
        {
            beard2 = Beard();
        }
        beard2.transform.parent = SplinePar;

        float connecterThick = subRandom(minConnectionWidth, maxConnectionWidth);


        beard1.transform.localPosition = Vector3.up * connecterThick/2;
        beard2.transform.localPosition = Vector3.down * connecterThick/2;
        beard2.transform.localEulerAngles += Vector3.forward * 180;

        blade.Evaluate(blade[0], 1f, out pos, out st, out up);
        float3 pos2;
        blade.Evaluate(blade[0], 0f, out pos2, out st, out up);

        SplineContainer innerBlade = Instantiate(blade, SplinePar);
        Vector3 v = innerBlade.transform.localScale;
        innerBlade.transform.localScale = new Vector3(v.x, v.y, 0);


        beard1.transform.localScale = new Vector3(pos.y - (connecterThick/2),1, pos.z);//remember x is actually up for these splines
        beard2.transform.localScale = new Vector3(-pos2.y - (connecterThick / 2), 1, pos2.z);

        SplinePar.transform.localScale = Vector3.one * .5f;


        float scaleAmount = subRandom(minInnerScale, maxInnerScale);

        Transform shrunk = Instantiate(SplinePar, transform);
        v = shrunk.transform.localScale * scaleAmount;
        shrunk.transform.localScale = v;
        shrunk.transform.localPosition = Vector3.forward * v.z/4;

        SplineContainer[] cons = shrunk.GetComponentsInChildren<SplineContainer>();
        splines.AddRange(cons);

        //hollowBlade = true;

        shrunk.parent = SplinePar;

        v = SplinePar.transform.localScale * width;
        SplinePar.transform.localScale = v;
        SplinePar.transform.localEulerAngles = Vector3.forward * 90;

        List<MeshFilter> filters = new List<MeshFilter>();

        MeshFilter halfAA = magicAndPatterns(cons, blade, beard1, beard2, innerBlade);

        filters.Add(halfAA);


        MeshFilter bladeEdge = emptyFilt_AAA();

        float yscale = .5f;
        if (decider5050())
            yscale = 1f;

        bladeEdge.mesh = nonTaporStemMesh1(4, meshRes, thick, false, blade, blade[0], new Vector3(1, yscale, 1));

        MakeMeshShadeFlat(bladeEdge);

        ReColorAMesh(bladeEdge, bladeColor);

        filters.Add(bladeEdge);

        hasTeeth = false;

        if (hasTeeth)
        {
            int dec = (int)subRandom(0, 2 + 1);
            if(dec == 0)
            {
                filters.Add(TeethMeshGen(cons[1], blade));
            }
            if (dec == 1)
            {
                filters.Add(TeethMeshGen(cons[0], beard1));
            }
            if (dec == 2)
            {
                filters.Add(TeethMeshGen(cons[2], beard2));
                if(decider5050())
                    filters.Add(TeethMeshGen(cons[1], blade));
            }

            

        }

        if (!hollowBlade)
        {
            halfAA.transform.localPosition = Vector3.up * thick / 2f;
            MeshFilter otherH = Instantiate(halfAA, transform);
            
            otherH.transform.localPosition = Vector3.down * thick / 2f;

            filters.Add(otherH);

            SplinePar.localPosition = Vector3.up * thick / 2f;
            Transform opHalf = Instantiate(SplinePar, transform);

            opHalf.localPosition = Vector3.down * thick / 2;

            cons = opHalf.GetComponentsInChildren<SplineContainer>();


            splines.Clear();
            splines.AddRange(cons);

            filters.Add(Faces(cons, blade, innerBlade, beard1, beard2));

        }


        MeshFilter bladeFinal = emptyFilt_AAA();
        bladeFinal.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        //bladeFinal.transform.localEulerAngles = Vector3.forward * -90;
        bladeFinal.GetComponent<MeshRenderer>().material = MetalMaterial;
        return bladeFinal;

    }
    MeshFilter TeethMeshGen(SplineContainer innerCon, SplineContainer outerCon)
    {

        Transform tToUse = null;
        if (decider5050())
        {
            tToUse = patternGen.starArmPre;
        }
        else
        {
            tToUse = patternGen.hollowStarArmPre;

        }

        int teethCount = (int)subRandom(minTeethCount, maxTeethCount + 1);

        List<float> percents = new List<float>();
        float total = 0;

        for(int i = 0; i < teethCount; i++)
        {
            total += subRandom(.1f, .9f);
            percents.Add(total);
        }
        total += subRandom(.1f, .9f);

        for (int i = 0; i < teethCount; i++)
        {
            percents[i] /= total;
        }

        List<MeshFilter> filters = new List<MeshFilter>();

        for (int i = 0; i < teethCount; i++)
        {

            Transform t = Instantiate(tToUse, transform);

            float perc = percents[i];

            innerCon.Evaluate(innerCon[0], perc, out pos, out st, out up);
            float3 pos2;
            outerCon.Evaluate(outerCon[0], perc, out pos2, out st, out up);

            Vector3 lookDir = (pos2 - pos);
            lookDir.Normalize();

            float scaleToBe = Vector3.Distance(pos, pos2);

            t.localPosition = pos;
            t.rotation = Quaternion.LookRotation(lookDir);
            t.localEulerAngles += Vector3.up * -90;
            Vector3 scaleF = Vector3.one * scaleToBe * subRandom(minTeethVariance, maxTeethVariance);
            scaleF.y = thick * 20;
            t.localScale = scaleF;

            filters.Add(t.GetComponent<MeshFilter>());

        }
        MeshFilter m = emptyFilt_AAA();
        m.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
        ReColorAMesh(m, bladeColor);
        return m;

    }
    MeshFilter Faces(SplineContainer[] opHalfCons, SplineContainer blade, SplineContainer innerBlade, SplineContainer upperBeard, SplineContainer lowerBeard)
    {



        MeshFilter f = emptyFilt_AAA();

        List<SplineContainer> containers = new List<SplineContainer>()
        {
            opHalfCons[0],
            upperBeard,
            lowerBeard,
            opHalfCons[2],
            opHalfCons[0]
        };

        f.mesh = GenerateMeshFromSplineContainerList(containers, meshRes, true, true);

        containers.Clear();
        containers.Add(opHalfCons[1]);
        containers.Add(opHalfCons[3]);
        containers.Add(innerBlade);
        containers.Add(blade);
        containers.Add(containers[1]);

        MeshFilter f2 = emptyFilt_AAA();
        f2.mesh = GenerateMeshFromSplineContainerList(containers, meshRes, true, true);

        List<MeshFilter> filters = new List<MeshFilter>();
        filters.Add(f);
        filters.Add(f2);
        f.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        MakeMeshShadeFlat(f);

        ReColorAMesh(f, bladeColor);


        return f;
    }

    MeshFilter magicAndPatterns(SplineContainer[] cons, SplineContainer blade, SplineContainer beard1, SplineContainer beard2, SplineContainer innerBlade)
    {
        List<MeshFilter> filters = new List<MeshFilter>();
        MeshFilter half = emptyFilt_AAA();
        MeshFilter decor;


        //enchantment / magic
        if (decider5050())
        {
            filters.Clear();

            enchantmentPlacement(cons[1], blade, transform, true);
            if(outputEnchantmentFilter != null)
            {
                decor = outputEnchantmentFilter;

                filters.Add(decor);

            }
            else
            {
                decor = emptyFilt_AAA();
            }

            filters.Add(half);

            half.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        }

        //enchantmentPlacement(cons[0], beard1, transform, true);

        //decor = outputEnchantmentFilter;

        //filters.Clear();
        //filters.Add(half);
        //filters.Add(decor);
        filters.Clear();

        enchantmentPlacement(cons[0], beard1, transform, true);
        if (outputEnchantmentFilter != null)
        {
            decor = outputEnchantmentFilter;
            
            filters.Add(decor);

        }
        else
        {
            decor = emptyFilt_AAA();
        }

        filters.Add(half);
        half.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        //enchantmentPlacement(cons[2], beard2, transform, true);

        //decor = outputEnchantmentFilter;

        //filters.Clear();
        //filters.Add(half);
        //filters.Add(decor);
        filters.Clear();

        enchantmentPlacement(cons[2], beard2, transform, true);
        if (outputEnchantmentFilter != null)
        {
            decor = outputEnchantmentFilter;

            filters.Add(decor);

        }
        else
        {
            decor = emptyFilt_AAA();
        }

        filters.Add(half);
        half.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        //ReColorAMesh(half, bladeColor);

        //MeshFilter m = Instantiate(half, Final.transform);
        //ReColorAMesh(m, enchantmentColor);
        //dupEnchantFilters.Add(m);

        //regula patterns

        bool hasCenterPattern = true;
        if (!hollowBlade)
        {
            hasCenterPattern = decider5050();
            if (hasCenterPattern)
                hasCenterPattern = decider5050();
        }

        hasPatterns = hasCenterPattern;

        if (hasCenterPattern)
        {
            if(!hollowBlade && decider5050())
            {
                patternGen.arrangeAlongTwoSplines(cons[0], cons[2], out decor);

                ReColorAMesh(decor, hiltColor);

                filters.Clear();
                filters.Add(decor);
                filters.Add(half);

                half.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

            }


            patternGen.arrangeAlongTwoSplines(cons[1], cons[3], out decor);

            ReColorAMesh(decor, hiltColor);

            filters.Clear();
            filters.Add(decor);
            filters.Add(half);

            half.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);


        }

        return half;
    }

    SplineContainer Beard()
    {
        SplineContainer upB = emptyCont_AAA();

        List<Vector3> points = new List<Vector3>();

        int beardPointCount = (int)subRandom(minBeardPointCount, maxBeardPointCount+1);

        float total = 0;
        List<float> percents = new List<float>();
        percents.Add(0f);//placeholders
        for (int i = 1; i < beardPointCount - 1; i++)
        {
            total += subRandom(0f, 1f);
            percents.Add(total);
        }
        total += subRandom(0f, 1f);
        percents.Add(0f);//placeholders
        for (int i = 1; i < beardPointCount - 1; i++)
        {
            percents[i] /= total;
        }

        points.Add(Vector3.zero);
        for(int i = 1; i < beardPointCount-1; i++)
        {

            float perc = percents[i];
            //float zOff = subRandom(0f, 1f);
            float zOff = subRandom(percents[i], 1f);

            points.Add(new Vector3(perc, 0, zOff));
        }
        points.Add(new Vector3(1, 0, 1));

        upB.AddSpline(EasyCurveSpline(points, overallSharpness));

        upB.transform.localEulerAngles = new Vector3(0, 0, 90);

        return upB;
    }
    
    SplineContainer Blade()
    {
        SplineContainer bla = emptyCont_AAA();
        List<Vector3> points = new List<Vector3>();

        points.Add(Vector3.right*-1);
        points.Add(new Vector3(-1,0,.01f));
        

        points.Add(new Vector3(subRandom(-.9f, .9f), 0, subRandom(minBladeOffset, maxBladeOffset)));

        points.Add(new Vector3(1, 0, .01f));
        points.Add(Vector3.right);

        bla.AddSpline(EasyCurveSpline(points, overallSharpness));
        bla.transform.localEulerAngles = new Vector3(0, 0, 90);
        return bla;
    }

    #endregion

    #region handle

    void CreateHandle()
    {
        //straight or curved Handle

        handle = emptyFilt_AAA();
        handle.transform.parent = headHandleParent;

        //cube.transform.localScale = new Vector3(thick, length, thick);

        if (symetrical||decider5050())
        {
            handleG.pillarSeed = seed;
            handleG.Begin();
            List<MeshFilter> filters = new List<MeshFilter>();
            filters.AddRange(handleG.FinalPillar.GetComponentsInChildren<MeshFilter>());

            ReColorAMesh(filters[0], hiltColor);
            ReColorAMesh(filters[1], handleColor);
            ReColorAMesh(filters[2], hiltColor);


            handle.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);
            handle.GetComponent<MeshRenderer>().material = MetalMaterial;

            handle.transform.localScale = new Vector3(thick* handleThickPerc, thick* handleThickPerc, length);
            handle.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
        else
        {
            List<MeshFilter> filters = new List<MeshFilter>();
            Vector3 outPos;
            filters.Add(ASymHandle(out outPos));

            //if (decider5050())
            //{
            //    Transform end = null;

            //    if (decider5050())
            //    {
            //        end = Instantiate(patternGen.StarPre);
            //    }
            //    else
            //    {
            //        end = Instantiate(patternGen.PolyPre);
            //    }

            //    end.localPosition = outPos;
            //    end.transform.localScale = new Vector3(thick * 5, thick * 35, thick * 5);

            //    ReColorAMesh(end.GetComponent<MeshFilter>(), hiltColor);

            //    filters.Add(end.GetComponent<MeshFilter>());



            //}
            

            handle.mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);


        }
        handle.GetComponent<MeshRenderer>().material = MetalMaterial;
    }


    MeshFilter ASymHandle(out Vector3 endPos)
    {
        int handlePointCount = (int)subRandom(minHandlePointCount, maxHandleProfilePointCount + 1);

        List<float> pointPerc = variedPercentsAAA(handlePointCount, true, .1f);

        List<Vector3> points = new List<Vector3>();

        for(int i = 0; i < handlePointCount; i++)
        {

            points.Add(new Vector3(-pointPerc[i] * length, 0, subRandom(-maxHandleOffsetPerc, maxHandleOffsetPerc) * width));

            //maxHandleOffsetPerc
        }
        points[0] = Vector3.zero;

        SplineContainer con1 = emptyCont_AAA();
        con1.AddSpline(EasyCurveSpline(points, overallSharpness));

        MeshFilter f = emptyFilt_AAA();

        f.mesh = PartialSplineDrivenTapor(handleSideCount, handleMeshRes, false, con1, handleThickProfile(), thick * handleThickPerc/2f, 1f);

        ReColorAMesh(f, handleColor);

        con1.Evaluate(con1[0], 1f, out pos, out st, out up);

        endPos = pos;

        return f;
    }

    SplineContainer handleThickProfile()
    {

        int pointCount = (int)subRandom(minHandleProfilePointCount, maxHandleProfilePointCount+1);

        List<float> ProfilePercents = variedPercentsAAA(pointCount, true, .1f);

        List<Vector3> profilePoints = new List<Vector3>();

        //profilePoints.Add(Vector3.zero);
        for(int i = 0; i < pointCount; i++)
        {
            profilePoints.Add(new Vector3(0, subRandom(minHandleThickVari, maxHandleThickVari), ProfilePercents[i] * 10f));
        }
        //profilePoints.Add(Vector3.forward * 10);
        SplineContainer con = emptyCont_AAA();

        con.AddSpline(EasyCurveSpline(profilePoints, overallSharpness));

        return con;

    }

    #endregion

    void FinalAssemble()
    {

        headHandleParent.localEulerAngles = new Vector3(90, -90, 0);
        headHandleParent.localPosition = Vector3.forward * length * .75f;
        List<MeshFilter> filters = new List<MeshFilter>() { head, handle };

        //DupEnchantWork();

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(filters, null, out ts);

        Final.GetComponent<MeshRenderer>().material = MetalMaterial;

        MakeEnchantedDuplicate();

    }

    //void DupEnchantWork()
    //{
    //    int count = dupEnchantFilters.Count;

    //    for(int i = 0; i < count; i++)
    //    {
    //        dupEnchantFilters[i].GetComponent<MeshRenderer>().material = MetalMaterial;
    //        dupEnchantFilters[i].transform.localScale = new Vector3(1, 1.01f, 1);
    //        dupEnchantFilters[i].transform.localPosition = Vector3.forward * length * .75f;
    //        dupEnchantFilters[i].transform.localPosition += Vector3.right * thick / 2;
    //        dupEnchantFilters[i].transform.localEulerAngles = new Vector3(90, -90, 0);

    //        dupEnchantFilters[i].transform.parent = Final.transform;

    //        ReColorAMesh(dupEnchantFilters[i], enchantmentColor);

    //        MeshFilter inver = Instantiate(dupEnchantFilters[i], Final.transform);
    //        inver.transform.localPosition += -Vector3.right * thick ;//not divided by 2 since it starts from og pos
    //        //inver.transform.localScale = new Vector3(1, -1.01f, 1);

    //        dupEnchantFilters.Add(inver);

    //        if (symetrical)
    //        {
    //            MeshFilter xOGInv = Instantiate(dupEnchantFilters[i],Final.transform);
    //            xOGInv.transform.localScale = new Vector3(1,1.01f, -1);

    //            MeshFilter xInvInv = Instantiate(inver, Final.transform);
    //            xInvInv.transform.localScale = new Vector3(1, 1.01f, -1);

    //            dupEnchantFilters.Add(xOGInv);
    //            dupEnchantFilters.Add(xInvInv);


    //        }

    //    }

    //    Final.GetComponent<weaponControllerAA>().enchantmentFilters.AddRange(dupEnchantFilters);

    //}


}
