using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flatFaceGen : MetalShapeGen
{

    [SerializeField] pinicalGenerator2 HeadGen;
    [SerializeField] GameObject blankMeshObj;
    bool symetrical = false;

    public override void Begin()
    {


        base.Begin();

        symetrical = decider5050();

        HeadGen.pillarSeed = seed;
        HeadGen.Begin();

        GameObject firstObj = Instantiate(blankMeshObj,transform);
        firstObj.GetComponent<MeshFilter>().mesh = HeadGen.FinalPillar.GetComponentInChildren<MeshFilter>().mesh;

        firstObj.transform.localPosition = new Vector3(0, 0, .5f);
        firstObj.transform.localEulerAngles = new Vector3(-90, 0, 0);

        List<MeshFilter> meshes = new List<MeshFilter>();
        meshes.Add(firstObj.GetComponent<MeshFilter>());

        Final = Instantiate(blankMeshObj, transform);

        //Final = HeadGen.FinalPillar;
        if (symetrical)
        {
            GameObject secObj = Instantiate(firstObj, transform);

            Transform inverse = secObj.transform;
            inverse.localPosition = new Vector3(0, 0, -.5f);
            inverse.localScale = -Vector3.one;
            
            meshes.Add(inverse.GetComponent<MeshFilter>());



            //Destroy(inverse.gameObject);
        }
        List<Texture2D> nullTex = null;

        Final.GetComponent<MeshFilter>().mesh = UNWEIGHTEDCombineMeshsAndTextures(meshes, null, out nullTex);


        Final.transform.localScale = new Vector3(thick, width, length);

    }
}
