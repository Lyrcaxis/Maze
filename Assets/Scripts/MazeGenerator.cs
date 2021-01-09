using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	MazeCell[,] cells;
	List<MazeCell> correctPath = new List<MazeCell>();

	// start -- 0,0
	// end   -- X,X

	int safetyCheck = 0;

	public static MazeGenerator instance;

	void Start() {
		instance = this;
		//if (++safetyCheck > 1000) { throw new UnityException("Infinite loop"); }

		// Initialize grid
		cells = new MazeCell[gridSize, gridSize];

		for (int x = 0; x < gridSize; x++) {
			for (int y = 0; y < gridSize; y++) {
				cells[x, y] = new MazeCell(new Vector2Int(x, y));
			}
		}
		foreach (var c in cells) { c.notScannedNeighbors = GetNeighbors(c); }

		// Initialize entry and exit
		MazeCell startCell = cells[0, 0];
		MazeCell endCell = cells[gridSize - 1, gridSize - 1];

		if (Random.Range(0, 2) == 1) { startCell.wallsRemaining &= ~WallsRemaining.Left; }
		else { startCell.wallsRemaining &= ~WallsRemaining.Down; }

		if (Random.Range(0, 2) == 1) { endCell.wallsRemaining &= ~WallsRemaining.Right; }
		else { startCell.wallsRemaining &= ~WallsRemaining.Up; }

		startCell.hasBeenVisited = true;
		endCell.hasBeenVisited = true;

		// Generate correct path
		//List<MazeCell> correctPath = new List<MazeCell>();

		Vector2Int checkPos = startCell.pos;
		correctPath.Add(startCell);

		while (checkPos != endCell.pos) {
			var currentCell = cells[checkPos.x, checkPos.y];

			while (true) {
				if (currentCell.notScannedNeighbors.Count == 0) {
					checkPos = currentCell.parent.pos;
					currentCell.notScannedNeighbors = GetNeighbors(currentCell);
					currentCell.hasBeenVisited = false;
					correctPath.Remove(currentCell);
					break;
				}

				MazeCell nb = currentCell.notScannedNeighbors.GetRandom();
				currentCell.notScannedNeighbors.Remove(nb);

				if (nb.hasBeenVisited) {
					continue;
				}
				nb.hasBeenVisited = true;
				nb.parent = currentCell;
				//nb.KnockdownWall(checkPos);
				//cells[checkPos.x, checkPos.y].KnockdownWall(nb.pos);
				checkPos = nb.pos;
				correctPath.Add(nb);
				break;
			}
		}

		return;

		// Generate Maze
		int visitedCells = correctPath.Count;
		int totalCells = gridSize * gridSize;

		safetyCheck = 0;
		while (visitedCells != totalCells) {

		}
	}

	public List<MazeCell> GetNeighbors(MazeCell cell) {
		var neighborsTemp = new List<MazeCell>(8);

		var pos = cell.pos;

		if (pos.y > 0) { neighborsTemp.Add(cells[pos.x, pos.y - 1]); }
		if (pos.y < gridSize - 1) { neighborsTemp.Add(cells[pos.x, pos.y + 1]); }

		if (pos.x > 0) { neighborsTemp.Add(cells[pos.x - 1, pos.y]); }
		if (pos.x < gridSize - 1) { neighborsTemp.Add(cells[pos.x + 1, pos.y]); }

		return neighborsTemp;
	}

	MazeCell GetCell(int x, int y) => cells[x, y];

	void OnDrawGizmosSelected() {
		if (cells == null) { return; }
		Vector3 size = Vector3.one * 0.25f;

		foreach (var cell in cells) {
			// Draw cell
			Gizmos.DrawWireCube(new Vector3(cell.pos.x, cell.pos.y), size);

			// Draw walls
			Gizmos.color = Color.black;
			if (cell.wallsRemaining.HasFlag(WallsRemaining.Left)) { Gizmos.DrawWireCube(cell.pos + Vector2.left * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(WallsRemaining.Right)) { Gizmos.DrawWireCube(cell.pos + Vector2.right * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(WallsRemaining.Up)) { Gizmos.DrawWireCube(cell.pos + Vector2.up * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(WallsRemaining.Down)) { Gizmos.DrawWireCube(cell.pos + Vector2.down * 0.25f, size); }
		}
		Gizmos.color = Color.green;
		foreach (var cell in correctPath) {
			Gizmos.DrawCube(new Vector3(cell.pos.x, cell.pos.y), Vector3.one * 0.25f);
		}

		Gizmos.color = Color.red;
		for (int i = 1; i < correctPath.Count; i++) {
			Gizmos.DrawLine((Vector2) correctPath[i - 1].pos, (Vector2) correctPath[i].pos);
		}
	}
}
