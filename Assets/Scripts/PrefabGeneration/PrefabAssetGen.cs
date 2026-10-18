using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PrefabAssetGen : MonoBehaviour
{

    public List<GameObject> GO_ToPre;

    string objectName = "SavedObj";

    public void CreateAsset()
    {
        foreach (GameObject objectToPre in GO_ToPre)
        {

            if (objectToPre != null)
            {

                objectName = objectToPre.name;

                MeshFilter[] filts = objectToPre.GetComponentsInChildren<MeshFilter>();
                SkinnedMeshRenderer[] skins = objectToPre.GetComponentsInChildren<SkinnedMeshRenderer>();

                Debug.Log("number of filts = " + filts.Length);

                List<Mesh> meshes = new List<Mesh>();
                foreach (MeshFilter filter in filts)
                {
                    filter.gameObject.SetActive(true);
                    meshes.Add(filter.mesh);
                }
                foreach (SkinnedMeshRenderer sk in skins)
                {
                    sk.gameObject.SetActive(true);

                    meshes.Add(sk.sharedMesh);
                }

                string filePath = EditorUtility.SaveFilePanelInProject("Save " + objectName, objectName, "asset", "");
                if (filePath == "") { return; }

                string minus1Path = "";

                string[] splitPath = filePath.Split(char.Parse("/"));

                for (int i = 0; i < splitPath.Length - 1; i++)
                {
                    minus1Path += splitPath[i];
                    minus1Path += "/";
                }

                minus1Path += objectName;

                int indexA = 1;
                foreach (Mesh mesh in meshes)
                {
                    
                    string usedPath = minus1Path;


                    usedPath += indexA + "_Mesh.asset";

                    AssetDatabase.CreateAsset(mesh, usedPath);


                    indexA++;
                }

                string newPath = minus1Path + ".prefab";

                bool prefabSuccess;
                PrefabUtility.SaveAsPrefabAssetAndConnect(objectToPre, newPath, InteractionMode.UserAction, out prefabSuccess);

                PrefabUtility.ApplyPrefabInstance(objectToPre, InteractionMode.UserAction);

            }
        }




    }


}
