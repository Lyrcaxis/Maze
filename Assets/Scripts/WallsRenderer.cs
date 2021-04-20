using System.Collections.Generic;

using UnityEngine;

public class WallsRenderer : MonoBehaviour {
	public Mesh wallMesh;
	public Material wallMat;
	static List<Matrix4x4[]> mArrays = new List<Matrix4x4[]>();

	void Update() {
		foreach (var mArray in mArrays) { Graphics.DrawMeshInstanced(wallMesh, 0, wallMat, mArray); }
	}

	public static void Init(List<Matrix4x4> matrices) {
		List<List<Matrix4x4>> mLists = new List<List<Matrix4x4>>();

		int mListsAmount = matrices.Count / 1023; // 1023 is the max amount of matrices supported by DrawMeshInstanced
		for (int i = 0; i <= mListsAmount; i++) { mLists.Add(new List<Matrix4x4>()); }
		for (int i = 0; i < matrices.Count; i++) { mLists[i / 1023].Add(matrices[i]); }

		foreach (var mList in mLists) { mArrays.Add(mList.ToArray()); }
	}
	public static void Clear() => mArrays.Clear();
}