using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazePathfinder : MonoBehaviour {
	[SerializeField] Transform pathfinderObj = default;
	MazeCell[,] cells;
	int gridSize;
	public List<MazeCell> GetMazePath(Vector2Int startPos, Vector2Int targetPos) {
		cells = Object.FindObjectOfType<MazeGenerator>().cells;
		gridSize = cells.GetLength(0);

		int attempt = 0;

		var startCell = cells[startPos.x, startPos.y];
		var endCell = cells[targetPos.x, targetPos.y];

		List<MazeCell> path = new List<MazeCell>();
		FollowPath(startCell, GridDir.None);
		return path;

		bool FollowPath(MazeCell cell, GridDir dir) {
			var nbs = GetNextCells(cell, dir);
			if (attempt++ >= 10000000) { throw new UnityException("stackoverflow"); }

			foreach (var nb in nbs) {
				if (nb == endCell || FollowPath(nb, nb.DirFromPos(cell.pos))) {
					path.Add(nb);
					return true;
				}
			}

			return false;
		}


		List<MazeCell> GetNextCells(MazeCell cell, GridDir fromDir) {
			var neighbors = new List<MazeCell>(4);

			if (fromDir != GridDir.Left && (cell.wallsRemaining & ~GridDir.Left) == cell.wallsRemaining) { AddNB(-1, 0); }
			if (fromDir != GridDir.Right && (cell.wallsRemaining & ~GridDir.Right) == cell.wallsRemaining) { AddNB(1, 0); }
			if (fromDir != GridDir.Up && (cell.wallsRemaining & ~GridDir.Up) == cell.wallsRemaining) { AddNB(0, 1); }
			if (fromDir != GridDir.Down && (cell.wallsRemaining & ~GridDir.Down) == cell.wallsRemaining) { AddNB(0, -1); }

			return neighbors;

			void AddNB(int x, int y) {
				x += cell.pos.x;
				y += cell.pos.y;
				if (x < 0 || y < 0 || x >= gridSize || y >= gridSize) { return; }
				neighbors.Add(cells[x, y]);
			}
		}
	}

	List<MazeCell> path;
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			var mousePos = Input.mousePosition;
			var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -10));
			path = GetMazePath(Vector2Int.FloorToInt(pathfinderObj.position), Vector2Int.RoundToInt(worldPos));
			if (path.Count != 0) {
				path.Add(cells[(int) pathfinderObj.position.x, (int) pathfinderObj.position.y]);
				pathfinderObj.position = (Vector2) path[0].pos;
			}
		}
	}
	void OnDrawGizmos() {
		var mousePos = Input.mousePosition;
		var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -10));
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere((Vector2) worldPos, 0.25f);

		if (path == null) { return; }
		Gizmos.color = Color.green;

		for (int i = path.Count - 1; i > 0; i--) {
			Gizmos.DrawLine((Vector2) path[i].pos, (Vector2) path[i - 1].pos);
		}

		Gizmos.DrawWireSphere((Vector2) path[0].pos, 0.25f);
	}
}
