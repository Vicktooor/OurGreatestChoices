using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

// AUTHOR - Victor

public class MeshCombiner : ScriptableObject
{
    [MenuItem("Custom/Combine/Combine all a planet grounds")]
    static void CombineAllGrounds()
    {
        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length > 2)
        {
            EditorUtility.DisplayDialog("Error", "You have to select the planet you want to combine first, then the target container", "ok");
        }
        else
        {
            MeshFilter[] meshFilters;
            MeshFilter targetContainer;
            List<CombineInstance> combine = new List<CombineInstance>();

            if (selection[0].tag == "Planet")
            {
                meshFilters = selection[0].GetComponentsInChildren<MeshFilter>();
                targetContainer = selection[1].GetComponent<MeshFilter>();
            }
            else
            {
                meshFilters = selection[1].GetComponentsInChildren<MeshFilter>();
                targetContainer = selection[0].GetComponent<MeshFilter>();
            }

            int i = 0;
            foreach (MeshFilter groundMeshFilter in meshFilters)
            {
                if (groundMeshFilter.name != "Planet")
                {
                    CombineInstance lGroundMesh = new CombineInstance();
                    lGroundMesh.mesh = groundMeshFilter.mesh;
                    lGroundMesh.transform = groundMeshFilter.transform.localToWorldMatrix;
                    combine.Add(lGroundMesh);
                    i++;
                }
            }

            var mf = targetContainer.GetComponent<MeshFilter>();
            if (mf)
            {
                var savePath = "Assets/ExportedObj/" + targetContainer.name + ".asset";
                mf.sharedMesh = new Mesh();
                mf.mesh.CombineMeshes(combine.ToArray());
                AssetDatabase.CreateAsset(mf.mesh, savePath);             

                EditorUtility.DisplayDialog("Mesh generated", "Mesh have been generated in the root folder", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Target container have no mesh filter", "ok");
            }
        } 
    }

    [MenuItem("Custom/Combine/[HOW USE] Combine all a planet")]
    static void Help()
    {
        EditorUtility.DisplayDialog("Combine help", "Generate your mesh, click on the generated mesh game object [need to have Planet as tag] then on the futur mesh game object, and finally combine your mesh", "Thx");
    }
}