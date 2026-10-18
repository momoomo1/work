using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Animations.Rigging;
using EasyWeapon;

public class bowGenerator : MeleeParentScript
{
    [SerializeField] pinicalGenerator2 leafTipGen;
    [SerializeField] bowWeaponControllerAA stringController;
    [SerializeField] SkinnedMeshRenderer blankCylinder;
    [SerializeField] SkinnedMeshRenderer blankSkinnedMesh;

    [SerializeField] RigBuilder builder;
    [SerializeField] Rig chainHolder;

    [SerializeField] List<Transform> topAjusts, botAjusts;
    [SerializeField] Transform topEnd, botEnd;
    [SerializeField] Transform emptyRotater, emptyRawMeshParent, bowRigParent;

    [SerializeField] float minBowThickPerc, maxBowThickPerc, minLeafRadiusPercent=.5f, maxLeafRadiusPercent=1.5f,
        minBandLengthPercent = .05f, maxBandLengthPercent = .2f, minBandCount = 1, maxBandCount = 5, minBandEdgePercentThick
        , maxBandEdgePercentThick, minNockLengthPercent = .1f, maxNockLengthPercent = .2f, 
        maxBoneOff = .25f, maxBoneRot = 25, minBoneScale = .75f, maxBoneScale = 1.25f;

    [SerializeField] int minNumberOfLeafs, maxNumberOfLeafs, minCylQuarterPointCount = 3, maxCylQuarterPointCount = 6, cylinderResolution,
        leafLengthResolution = 15;

    [SerializeField] bool allowRotation;

    List<MeshFilter> woodParts = new List<MeshFilter>();
    List<MeshFilter> metalParts = new List<MeshFilter>();

    Transform halfParent;

    //Material woodMat, metalMat;

    float bowThickPercent, leafLength;
    float frontTiltDistPercent;
    int bandCount;
    float bandLengthPercent, bandEdgePercentThick;

    int numOfLeafs;
    bool multipleLeafs, irregularLeafShape;

    public override void Begin()
    {
        base.Begin();

        multipleLeafs = decider5050();
        irregularLeafShape = decider5050();

        //Material mCop = inMats[2];

        //woodMat = inMats[5];
        //metalMat = new Material(woodMat);

        //metalMat.SetColor("_Color", mCop.GetColor("_FlatColor"));
        //metalMat.SetColor("_AltColor_1", mCop.GetColor("_OverrideColor"));
        //metalMat.SetTexture("_Texture2D", mCop.GetTexture("_OverrideTexture"));
        //metalMat.SetTexture("_mainNormalMap", mCop.GetTexture("_FlatNormal"));
        //metalMat.SetTexture("_altNormalMap", mCop.GetTexture("_OverrideNormal"));



        if (width > thick)
        {
            thick = width;
        }
        else
        {
            width = thick;
        }

        leafLength = length / 2;

        frontTiltDistPercent = subRandom(0f, 1f);
        bandCount = (int)subRandom(minBandCount, maxBandCount + 1);
        bandLengthPercent = subRandom(minBandLengthPercent, maxBandLengthPercent);
        bandEdgePercentThick = subRandom(minBandEdgePercentThick, maxBandEdgePercentThick);
        createBowLeafs();

        AjustTheRig();

        //stringController.SetStringMaterial(metalMat);

        stringController.stringColor = handleColor;
        stringController.Begin();

        if(allowRotation)
        {
            stringController.rotateToStartPosition();
        }

    }

    void createBowLeafs()
    {

        halfParent = Instantiate(emptyGameObjectAAA, emptyRawMeshParent).transform;

        if (multipleLeafs)
        {
            numOfLeafs = (int)subRandom(minNumberOfLeafs, maxNumberOfLeafs+1);
        }
        else
        {
            numOfLeafs = minNumberOfLeafs;
        }

        Transform barrelBase = Instantiate(emptyGameObjectAAA, halfParent).transform;

        barrelBase.position = Vector3.zero;

        bowThickPercent = subRandom(minBowThickPerc, maxBowThickPerc);

        float uBarEnd = subRandom(minBowThickPerc, bowThickPercent) * width;
        uBarEnd /= numOfLeafs;

        bowThickPercent *= width;

        float uBarStart = bowThickPercent / numOfLeafs;

        leafLength = length/2;

        //float remainingPercent = 1f;

        //List<float> lengths = new List<float>();

        int quarterRes = (int)subRandom(minCylQuarterPointCount, maxCylQuarterPointCount + 1);

        leafTipGen.pillarSeed = seed;
        leafTipGen.Begin();

        Transform tipObj = Instantiate(leafTipGen.FinalPillar, halfParent).transform;
        tipObj.localEulerAngles = Vector3.zero;

        float tipLength = subRandom(minNockLengthPercent, maxNockLengthPercent) * length;

        tipObj.localScale = new Vector3(uBarEnd, tipLength, uBarEnd);
        tipObj.localEulerAngles = new Vector3(90,0,0);

        tipObj.localPosition = Vector3.forward * ((length / 2)-tipLength/2f);
        ReColorAMesh(tipObj.GetComponent<MeshFilter>(), hiltColor);
        tipObj.GetComponent<MeshRenderer>().material = MetalMaterial;
        
        leafTipGen.gameObject.SetActive(false);

        if (!irregularLeafShape)
        {
            quarterRes = minCylQuarterPointCount;
        }

        SplineContainer mainbarrelShape = Instantiate(containerPreAAA, transform);

        //SplineContainer quCon = Instantiate(containerPreAAA, transform);

        //List<Vector3> quarterPoints = new List<Vector3>();

        Vector3[] proPoints = new Vector3[(quarterRes * 4)];

        for (int i = 0; i < quarterRes; i++)//base spline must be about a radius of .5 (diameter of 1) so it can be scaled correctly later
        {
            float time = (float)i / (quarterRes);

            float offsetPercent = subRandom(minLeafRadiusPercent, maxLeafRadiusPercent);

            emptyRotater.transform.localEulerAngles = new Vector3(0, 0, -90f * time);

            Vector3 basePos = emptyRotater.up * offsetPercent * .5f;

            //repeating not symetrical

            proPoints[i] = basePos;//top right
            proPoints[i + quarterRes] = new Vector3(basePos.y, -basePos.x, basePos.z);// bottom right

            proPoints[i + (quarterRes * 2)] = new Vector3(-basePos.x, -basePos.y, basePos.z);// bottom left
            proPoints[i + (quarterRes * 3)] = new Vector3(-basePos.y, basePos.x, basePos.z);// top left



        }
        //proPoints[proPoints.Length - 1] = proPoints[0];

        List<Vector3> pointConversion = new List<Vector3>();
        pointConversion.AddRange(proPoints);

        mainbarrelShape.AddSpline(newSpline1(pointConversion, .5f));
        mainbarrelShape[0].Closed = true;

        int lInd = 0;

        for(int i = 0; i < numOfLeafs; i++)
        {
            SplineContainer startContainer = Instantiate(containerPreAAA, transform);

            startContainer.AddSpline(mainbarrelShape[0]);


            SplineContainer endContainer = Instantiate(startContainer, transform);

            endContainer.transform.localPosition = new Vector3(0, 0, leafLength);

            if (numOfLeafs > 1)
            {
                float percentage = (float)lInd / (numOfLeafs);

                emptyRotater.transform.localEulerAngles = new Vector3(0, 0, 360f * percentage);

                startContainer.transform.localPosition = emptyRotater.up * (uBarStart / 2);

            }

            startContainer.transform.localScale = Vector3.one * uBarStart;
            endContainer.transform.localScale = Vector3.one * uBarEnd;

            GameObject g = Instantiate(emptyMeshPreAAA, barrelBase.transform);

            SplineContainer capContainer = Instantiate(startContainer, transform);
            capContainer.transform.localScale = Vector3.zero;

            List<SplineContainer> conts = new List<SplineContainer>();
            conts.Add(capContainer);
            conts.Add(startContainer);

            for(int aaa = 1; aaa < leafLengthResolution-1; aaa++)
            {

                float percent = aaa / (leafLengthResolution - 1f);

                SplineContainer spc = Instantiate(startContainer, transform);
                spc.transform.position = Vector3.Lerp(startContainer.transform.position, endContainer.transform.position, percent);
                spc.transform.localScale = Vector3.Lerp(startContainer.transform.localScale, endContainer.transform.localScale, percent);

                conts.Add(spc);

            }

            conts.Add(endContainer);

            SplineContainer endcapContainer = Instantiate(endContainer, transform);
            endcapContainer.transform.localScale = Vector3.zero;

            conts.Add(endcapContainer);

            g.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(conts, cylinderResolution, false, true);
            ReColorAMesh(g.GetComponent<MeshFilter>(), bladeColor);
            g.GetComponent<MeshRenderer>().material = MetalMaterial;

            woodParts.Add(g.GetComponent<MeshFilter>());

            CreateBarrelBands(g.transform, leafLength, startContainer, uBarStart, uBarEnd, startContainer.transform.localPosition, tipObj.GetComponent<MeshFilter>());


            lInd++;

        }

        Transform a = Instantiate(halfParent, emptyRawMeshParent);
        a.localScale = new Vector3(1, 1, -1);


        MeshFilter wood = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

        List<Texture2D> ts;

        wood.mesh = UNWEIGHTEDCombineMeshsAndTextures(woodParts, null, out ts);

        MeshFilter w2 = Instantiate(wood, transform);
        w2.transform.localScale = new Vector3(1, 1, -1f);

        woodParts.Clear();
        woodParts.Add(wood);
        woodParts.Add(w2);

        wood.mesh = UNWEIGHTEDCombineMeshsAndTextures(woodParts, null, out ts);



        MeshFilter metal = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
        metal.mesh = UNWEIGHTEDCombineMeshsAndTextures(metalParts, null, out ts);

        MeshFilter m2 = Instantiate(metal, transform);
        m2.transform.localScale = new Vector3(1, 1, -1f);
        metalParts.Clear();
        metalParts.Add(metal);
        metalParts.Add(m2);

        metal.mesh = UNWEIGHTEDCombineMeshsAndTextures(metalParts, null, out ts);

        blankCylinder.gameObject.SetActive(true);

        woodParts.Clear();

        woodParts.Add(wood);
        woodParts.Add(metal);

        wood.mesh = UNWEIGHTEDCombineMeshsAndTextures(woodParts, null, out ts);

        AttachMeshToBones(wood, true);
        //AttachMeshToBones(metal, false);

        blankCylinder.gameObject.SetActive(false);
        bowRigParent.localScale = Vector3.one * (1f / 6) * length;
    }


    void CreateBarrelBands(Transform finalParent, float currentLength, SplineContainer crossSec, float startThick, float endThick, Vector3 startPos, MeshFilter tipObj)
    {
        //create base circle or loop or half

        List<MeshFilter> BandMeshToCombine = new List<MeshFilter>();

        

        float scaleToBe = Mathf.Lerp(1, 1.41421f, frontTiltDistPercent);
        scaleToBe += .01f;//just so there isn't any clipping issues
        float xRot = Mathf.Lerp(0f, 45f, frontTiltDistPercent);

        for (int i = 0; i < bandCount; i++)
        {


            float Percent = ((float)i) / (bandCount);

            Transform baseT = Instantiate(emptyMeshPreAAA, transform).transform;


            Transform a = Instantiate(crossSec.transform, baseT);
            Transform b = Instantiate(crossSec.transform, baseT);
            a.localEulerAngles = new Vector3(xRot, 0, 0);
            b.localEulerAngles = new Vector3(xRot, 0, 0);
            float perPartStartScale = scaleToBe;
            float perPartStartXScale = 1.01f;

            perPartStartScale *= Mathf.Lerp(startThick, endThick, Percent - (bandLengthPercent / 2));
            perPartStartXScale *= Mathf.Lerp(startThick, endThick, Percent - (bandLengthPercent / 2));

            float perPartEndScale = scaleToBe * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 2));
            float perPartEndXScale = 1.01f * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 2));

            a.localScale = new Vector3(perPartStartXScale, perPartStartScale, perPartStartScale);
            b.localScale = new Vector3(perPartEndXScale, perPartEndScale, perPartEndScale);

            a.localPosition = new Vector3(0, 0, -bandLengthPercent * (currentLength) / 2f);
            b.localPosition = new Vector3(0, 0, bandLengthPercent * (currentLength) / 2f);

            //Debug.Log("length " + currentLength);

            MeshFilter bandEdgeA = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();
            MeshFilter bandEdgeB = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();



            List<SplineContainer> containers = new List<SplineContainer>();
            containers.Add(a.GetComponent<SplineContainer>());
            containers.Add(b.GetComponent<SplineContainer>());

            baseT.GetComponent<MeshFilter>().mesh = GenerateMeshFromSplineContainerList(containers, cylinderResolution, false, false);

            //baseT.localPosition = Vector3.forward * currentLength * Percent;
            //startPos
            baseT.localPosition = Vector3.Lerp(startPos, Vector3.forward * currentLength, Percent);


            bandEdgeA.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartStartScale * bandEdgePercentThick, false,
                a.GetComponent<SplineContainer>(), a.GetComponent<SplineContainer>()[0], Vector3.one);
            bandEdgeB.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartEndScale * bandEdgePercentThick, false,
                b.GetComponent<SplineContainer>(), b.GetComponent<SplineContainer>()[0], Vector3.one);

            //if (barrelBandsStickUp)
            //{
            //    //create smaller copy of the edgebands, and place it right between the other 2 bands
            //    //then make the smaller one move up, so that it looks like a sight(keep the forward rot)

            //    SplineContainer stickUpContainer = Instantiate(a, baseT).GetComponent<SplineContainer>();
            //    stickUpContainer.transform.localScale /= 2f;
            //    //stickUpContainer.transform.localPosition = Vector3.Lerp(a.localPosition, b.localPosition, stickUpPos);


            //    float3 pos, st, up;

            //    b.GetComponent<SplineContainer>().Evaluate(b.GetComponent<SplineContainer>()[0], .5f, out pos, out st, out up);

            //    pos.z = Mathf.Lerp(a.position.z, b.position.z, stickUpPos);

            //    pos.y -= stickUpContainer.transform.localScale.y / 2;

            //    stickUpContainer.transform.position = pos;

            //    //stickUpContainer.transform.localPosition += Vector3.up * Mathf.Lerp(startThick, endThick, Percent + (bandLengthPercent / 5));
            //    MeshFilter stickUpBand = Instantiate(emptyMeshPreAAA, transform).GetComponent<MeshFilter>();

            //    stickUpBand.mesh = nonTaporStemMesh1(6, cylinderResolution, perPartEndScale * bandEdgePercentThick * .5f, false,
            //    stickUpContainer, stickUpContainer[0], Vector3.one);
            //    BandMeshToCombine.Add(stickUpBand);
            //}



            BandMeshToCombine.Add(baseT.GetComponent<MeshFilter>());
            BandMeshToCombine.Add(bandEdgeA);
            BandMeshToCombine.Add(bandEdgeB);

            //BandMeshToCombine.Add(//and also the stick up band);!!!!!!!


        }

        BandMeshToCombine.Add(tipObj);

        MeshFilter combineAllTheseMeshes = Instantiate(emptyMeshPreAAA, halfParent).GetComponent<MeshFilter>();

        List<Texture2D> ts = null;

        combineAllTheseMeshes.mesh = UNWEIGHTEDCombineMeshsAndTextures(BandMeshToCombine, null, out ts);

        combineAllTheseMeshes.transform.parent = halfParent;

        combineAllTheseMeshes.transform.localPosition += new Vector3(0, 0, bandLengthPercent * (currentLength) / 2f);


        ReColorAMesh(combineAllTheseMeshes, hiltColor);


        combineAllTheseMeshes.GetComponent<MeshRenderer>().material = MetalMaterial;

        metalParts.Add(combineAllTheseMeshes);

        for (int i = BandMeshToCombine.Count - 1; i >= 0; i--)
        {
            Destroy(BandMeshToCombine[i].gameObject);
        }

    }


    void AttachMeshToBones(MeshFilter meshToAttach, bool isWood)
    {

        //bowRigParent.localScale = Vector3.one * (length/6);
        meshToAttach.transform.localScale *= 6 * (1/length);
        //meshToAttach.transform.localEulerAngles = new Vector3(90, 0, 0);

        //blankCylinder.transform.localScale = new Vector3(1, 1, length/6);

        SkinnedMeshRenderer finalSkinnedMesh = Instantiate(blankSkinnedMesh, Final.transform);

        //finalSkinnedMesh.rootBone = blankCylinder.rootBone;

        Mesh finalMesh = meshToAttach.mesh;



        //boneweights //per vertex, copy from closes vertex

        //finalMesh = meshToAttach.mesh;

        BoneWeight[] weights = new BoneWeight[meshToAttach.mesh.vertices.Length];

        Vector3[] inputMeshVerts = meshToAttach.mesh.vertices;
        Vector3[] blankMeshVerts = blankCylinder.sharedMesh.vertices;

        Vector3[] ajustedSizeVerts = new Vector3[inputMeshVerts.Length];

        for (int i = 0; i < meshToAttach.mesh.vertices.Length; i++)
        {



            int closestVertInd = 0;

            //Vector3 inMeshV = meshToAttach.transform.TransformPoint(inputMeshVerts[i]);
            Vector3 inMeshV = meshToAttach.transform.TransformPoint(inputMeshVerts[i]);

            ajustedSizeVerts[i] = inMeshV;

            //Vector3 inMeshV = inputMeshVerts[i];

            Vector3 blankC = blankCylinder.transform.TransformPoint(blankMeshVerts[0]);

            Vector2 normInXY = inMeshV, 
                normBlankXY = blankC;

            normInXY.Normalize();
            normBlankXY.Normalize();

            Vector3 currentVert = new Vector3(normInXY.x, normInXY.y, inMeshV.z);

            float closestDist = Vector3.Distance(currentVert, new Vector3(normBlankXY.x,normBlankXY.y, blankC.z));

            int index = 0;
            foreach(Vector3 blankPos in blankMeshVerts)
            {

                Vector3 blPos = blankCylinder.transform.TransformPoint(blankPos);

                normBlankXY = blPos;
                normBlankXY.Normalize();

                Vector3 checkPos = new Vector3(normBlankXY.x, normBlankXY.y, blPos.z);

                float dist = Vector3.Distance(currentVert, checkPos);

                if(dist < closestDist)
                {
                    closestVertInd = index;
                    closestDist = dist;
                }
                index++;
            }

            //Debug.Log("inMeshV " + inMeshV);

            weights[i] = blankCylinder.sharedMesh.boneWeights[closestVertInd];


        }

        finalMesh.vertices = ajustedSizeVerts;//THIS IS THE TRICK TO MAKING IT ALL WORK

        finalMesh.boneWeights = weights;

        //bind poses //draw directly from blank cylinder

        finalMesh.bindposes = blankCylinder.sharedMesh.bindposes;

        // ^^^ are under  mesh
        //mesh //the new skinned mesh

        finalSkinnedMesh.sharedMesh = finalMesh;

        //bones//the blank mesh bones

        finalSkinnedMesh.bones = blankCylinder.bones;

        //if (isWood)
        //{
        //    finalSkinnedMesh.material = woodMat;
        //}
        //else
        //{
        //    finalSkinnedMesh.material = metalMat;
        //}

        finalSkinnedMesh.material = MetalMaterial;

        //finalSkinnedMesh.bounds = blankCylinder.bounds;
        //finalSkinnedMesh.sharedMesh.RecalculateBounds();
        finalSkinnedMesh.localBounds = new Bounds(Vector3.zero, Vector3.one * length);
        //finalSkinnedMesh.sharedMesh.bounds.extents = new Vector3(1, .5f, .5f);
        meshToAttach.gameObject.SetActive(false);

    }
    

    void AjustTheRig()
    {
        //Vector3[] posOffs = new Vector3[3];
        //Vector3[] eulOffs = new Vector3[3];
        //Vector3[] scaleOffs = new Vector3[3];

        for(int i = 0; i < 3; i++)
        {
            Vector3 pos = Vector3.forward * subRandom(-maxBoneOff, maxBoneOff);
            Vector3 eul = Vector3.right * subRandom(-maxBoneRot, maxBoneRot);
            Vector3 sca = new Vector3(subRandom(minBoneScale, maxBoneScale), 1, subRandom(minBoneScale, maxBoneScale));

            topAjusts[i].localPosition = pos;
            topAjusts[i].localEulerAngles = eul;
            topAjusts[i].localScale = sca;

            botAjusts[i].localPosition = pos;
            botAjusts[i].localEulerAngles = eul;
            botAjusts[i].localScale = sca;

        }
        builder.layers.Clear();
        builder.layers.Add(new RigLayer(chainHolder));
        builder.Build();


    }

}

