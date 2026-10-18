using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PrefabAssetGen))]
public class PreAssetGenEditorController : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PrefabAssetGen prefabAssetGen = (PrefabAssetGen)target;

        if (GUILayout.Button("Create Prefab"))
        {
            prefabAssetGen.CreateAsset();
        }


    }

}
