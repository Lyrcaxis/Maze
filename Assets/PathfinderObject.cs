using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderObject : MonoBehaviour {
	MazePathfinder pathfinder => MazePathfinder.instance;
	List<MazeCell> path;

	void Awake() {
		MazeGenerator.OnGenerationRequested += () => path = null;
		MazeGenerator.OnMazeGenerated += () => {
			transform.position = Vector2.zero;
			path = null;
		};
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			var mousePos = Input.mousePosition;
			var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -10));
			path = pathfinder.GetMazePath(Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(worldPos));

			if (path.Count != 0) {
				path.Add(new MazeCell(Vector2Int.RoundToInt(transform.position)));
				transform.position = (Vector2) path[0].pos;
			}
		}
	}
	void OnDrawGizmos() {
		if (path == null || path.Count == 0) { return; }

		var startColor = Color.red;
		var midColor = Color.yellow;
		var endColor = Color.blue;

		for (int i = 1; i < path.Count; i++) {
			Gizmos.color = GetColor(i);
			Gizmos.DrawLine((Vector2) path[i].pos, (Vector2) path[i - 1].pos);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere((Vector2) path[0].pos, 0.25f);

		Color GetColor(int i) {
			var t = 1 - (i / (float) path.Count);

			if (t < 0.5f) { return Color.Lerp(startColor, midColor, 2 * t); }
			else { return Color.Lerp(midColor, endColor, 2 * (t - 0.5f)); }
		}
	}
}
