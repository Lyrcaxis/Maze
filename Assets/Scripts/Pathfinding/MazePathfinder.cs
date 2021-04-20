using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazePathfinder : MonoBehaviour {

	static MazePathfinder _instance;
	public static MazePathfinder instance => _instance ? _instance : (_instance = FindObjectOfType<MazePathfinder>());

	void Awake() {
		MazeGenerator.OnMazeGenerated += CacheNewCells;

		void CacheNewCells() {
			var cells = GetComponent<MazeGenerator>().cells;
			int gridSize = cells.GetLength(0);

			foreach (var cell in cells) {
				cell.walkableNeighbors = new Dictionary<GridDir, MazeCell>(4);
				cell.dirsToCheckForPathfinding = GridDir.None;

				if ((cell.wallsRemaining & ~GridDir.Left) == cell.wallsRemaining) { AddNB(-1, 0); }
				if ((cell.wallsRemaining & ~GridDir.Right) == cell.wallsRemaining) { AddNB(1, 0); }
				if ((cell.wallsRemaining & ~GridDir.Up) == cell.wallsRemaining) { AddNB(0, 1); }
				if ((cell.wallsRemaining & ~GridDir.Down) == cell.wallsRemaining) { AddNB(0, -1); }

				void AddNB(int x, int y) {
					x += cell.pos.x;
					y += cell.pos.y;

					if (x < 0 || y < 0 || x >= gridSize || y >= gridSize) { return; }
					cell.walkableNeighbors[cell.DirFromPos(new Vector2Int(x, y))] = cells[x, y];
					cell.dirsToCheckForPathfinding |= cell.DirFromPos(new Vector2Int(x, y));
				}
			}
		}
	}

	public List<MazeCell> GetMazePath(Vector2Int startPos, Vector2Int targetPos) {
		var mazeGen = GetComponent<MazeGenerator>();
		if (mazeGen.visualize) { return new List<MazeCell>(); }

		var cells = mazeGen.cells;
		int gridSize = cells.GetLength(0);
		int gridLength = cells.Length;

		if (startPos.x < 0 || startPos.y < 0 || startPos.x >= gridSize || startPos.y >= gridSize) { startPos = new Vector2Int(0,0); }
		if (targetPos.x < 0 || targetPos.y < 0 || targetPos.x >= gridSize || targetPos.y >= gridSize) { targetPos = new Vector2Int(gridSize - 1, gridSize - 1); }
		
		int attempt = 0;

		var startCell = cells[startPos.x, startPos.y];
		var endCell = cells[targetPos.x, targetPos.y];

		foreach (var cell in cells) { cell.uncheckedDirsTemp = cell.dirsToCheckForPathfinding; }

		List<MazeCell> path = new List<MazeCell>();
		FollowPathRecursively(startCell, GridDir.None);
		return path;

		bool FollowPathRecursively(MazeCell cell, GridDir dir) {
			var nbs = cell.walkableNeighbors;
			if (attempt++ > 1000000) { throw new UnityException("stackoverflow"); }

			foreach (var nb in nbs) {
				var nbValue = nb.Value;
				var dirToNeighbor = cell.DirFromPos(nbValue.pos);
				var dirFromCurrent = nbValue.DirFromPos(cell.pos);
				if ((cell.uncheckedDirsTemp & ~dirToNeighbor) == cell.uncheckedDirsTemp) { continue; }
				if ((nbValue.uncheckedDirsTemp & ~dirFromCurrent) == nbValue.uncheckedDirsTemp) { continue; }

				cell.uncheckedDirsTemp &= ~cell.DirFromPos(nbValue.pos);
				nbValue.uncheckedDirsTemp &= ~nbValue.DirFromPos(cell.pos);

				if (nbValue == endCell || FollowPathRecursively(nbValue, nbValue.DirFromPos(cell.pos))) {
					path.Add(nbValue);
					return true;
				}
			}

			return false;
		}
	}
}
