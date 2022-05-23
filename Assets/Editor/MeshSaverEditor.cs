using UnityEditor;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class MeshSaverEditor {

	[MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
	public static void SaveMeshInPlace (MenuCommand menuCommand) {
		MeshFilter mf = menuCommand.context as MeshFilter;
		Mesh m = mf.sharedMesh;
		SaveMesh(m,mf.transform, m.name, false, true);
	}

	[MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
	public static void SaveMeshNewInstanceItem (MenuCommand menuCommand) {
		MeshFilter mf = menuCommand.context as MeshFilter;
		Mesh m = mf.sharedMesh;
		SaveMesh(m, mf.transform, m.name, true, true);
	}

	public static void SaveMesh (Mesh mesh,Transform t, string name, bool makeNewInstance, bool optimizeMesh) {
		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if (string.IsNullOrEmpty(path)) return;
        
		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
		Vector3[] vert = meshToSave.vertices;
		Vector3[] norm = meshToSave.normals;
		for (int i = 0; i < meshToSave.vertexCount; ++i)
		{
			vert[i]= t.TransformPoint(vert[i]);
			norm[i]= t.TransformPoint(norm[i]) - t.position;
		}

		meshToSave.vertices = vert;
		meshToSave.normals = norm;
		if (optimizeMesh)
		     MeshUtility.Optimize(meshToSave);
        
		AssetDatabase.CreateAsset(meshToSave, path);
		string fileText = File.ReadAllText(path);
		fileText = fileText.Replace("m_IsReadable: 0", "m_IsReadable: 1");
		File.WriteAllText(path, fileText);
		AssetDatabase.SaveAssets();
	}
	
}
