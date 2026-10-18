using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenSpreader : MetalShapeGen
{
    //[SerializeField] MaterialRandomizer matRand;
    //[SerializeField] metalTexController texController;
    //[SerializeField] Material MatToCompair;
    [SerializeField] int length1 = 10;
    [SerializeField] int width1 = 10;
    [SerializeField] int seedOffset=0;
    [SerializeField] weaponGenerator gener;

    List<weaponGenerator> generators = new List<weaponGenerator>();

    //[SerializeField] List<MeshFilter> filters = new List<MeshFilter>();
    //[SerializeField] List<int> filterEdgeTypes = new List<int>();

    // Start is called before the first frame update
    public override void Start()
    {
        
        seed = seedOffset;

        for(int x = 0; x < length1; x++)
        {
            for(var z = 0; z < width1; z++)
            {

                weaponGenerator gen = Instantiate(gener);

                //gen.inputCompareMat = MatToCompair;

                List<Material> mats;

                //matRand.MakeMaterialTypes(out mats, seed);

                //gen.matsForMetal = mats;

                gen.seed = seed;

                gen.Begin();

                //GameObject g = Instantiate(gen.finalObj);
                GameObject g = gen.finalObj;
                g.transform.parent = null;
                g.transform.position = new Vector3(x, 0, z);

                generators.Add(gen);

                seed = (int)subRandom(0,10000000);

            }
        }

        //for(int i = generators.Count - 1; i >= 0; i--)
        //{
        //    Destroy(generators[i].gameObject);
        //}

        
    }


}
