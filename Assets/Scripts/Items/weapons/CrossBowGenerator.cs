using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossBowGenerator : MeleeParentScript
{

    [SerializeField] bowGenerator bowGen;
    [SerializeField] gunGenerator gunGen;

    //[SerializeField] Transform QuadPlane;

    Transform bow;
    Transform gun;

    [SerializeField] List<MeshFilter> meshFilters;

    public override void Begin()
    {
        
        base.Begin();

        gunGen.MetalMaterial = MetalMaterial;
        bowGen.MetalMaterial = MetalMaterial;

        if(colorOverride)
        {
            gunGen.colorOverride = true;
            bowGen.colorOverride = true;

            gunGen.bladeColor = bladeColor;
            gunGen.hiltColor = hiltColor;
            gunGen.handleColor = handleColor;
            gunGen.enchantmentColor = enchantmentColor;

            bowGen.bladeColor = bladeColor;
            bowGen.hiltColor = hiltColor;
            bowGen.handleColor = handleColor;
            bowGen.enchantmentColor = enchantmentColor;
        }

        gunGen.seed = seed;
        gunGen.colorSeed = colorSeed;
        gunGen.alotedMass = alotedMass;

        gunGen.Begin();

        bowGen.seed = seed;
        bowGen.colorSeed = colorSeed;
        bowGen.alotedMass = alotedMass;

        bowGen.Begin();


        bow = bowGen.Final.transform;
        gun = gunGen.Final.transform;

        bow.parent = Final.transform;
        gun.parent = Final.transform;

        bow.localPosition = Vector3.forward * bowGen.length/2;

        bow.localEulerAngles = new Vector3(0, 90, 90);

        bowGen.gameObject.SetActive(false );



    }
}
