using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	MazeCell[,] cells;
	List<MazeCell> correctPath = new List<MazeCell>();

	// start -- 0,0
	// end   -- X,X
	//if (++safetyCheck > 100000) { throw new UnityException("Infinite loop on neighbors"); }

	void Start() {
		
		// Initialize grid
		cells = new MazeCell[gridSize, gridSize];

		for (int x = 0; x < gridSize; x++) {
			for (int y = 0; y < gridSize; y++) {
				cells[x, y] = new MazeCell(new Vector2Int(x, y));
			}
		}

		foreach (var c in cells) { c.neighbors = GetNeighbors(c); }

		// Initialize entry and exit
		MazeCell startCell = cells[0, 0];
		MazeCell endCell = cells[gridSize - 1, gridSize - 1];

		if (Random.Range(0, 2) == 1) { startCell.wallsRemaining &= ~WallsRemaining.Left; }
		else { startCell.wallsRemaining &= ~WallsRemaining.Down; }

		if (Random.Range(0, 2) == 1) { endCell.wallsRemaining &= ~WallsRemaining.Right; }
		else { endCell.wallsRemaining &= ~WallsRemaining.Up; }

		startCell.hasBeenVisited = true;
		endCell.hasBeenVisited = true;

		// Generate correct path
		StartCoroutine(GenerateCorrectPath(startCell, endCell));





		return;
		// Generate Maze
		int visitedCells = correctPath.Count;
		int totalCells = gridSize * gridSize;

		while (visitedCells != totalCells) {

		}
	}

	void Update() {
		if (Input.GetMouseButtonDown(1)) {
			StopAllCoroutines();
			Start();
		}
	}

	IEnumerator GenerateCorrectPath(MazeCell startCell, MazeCell endCell) {
		List<MazePoint> mazePath = new List<MazePoint>();

		mazePath.Add(new MazePoint(startCell.pos, startCell.neighbors));

		Vector2Int checkPos = mazePath[0].pos;
		var currentCell = mazePath[0];

		while (currentCell.pos != endCell.pos) {

			if (currentCell.remainingNeighbors.Count != 0) {
				var nbPos = currentCell.remainingNeighbors.GetRandom();
				currentCell.remainingNeighbors.Remove(nbPos);

				if (mazePath.Any(x => x.pos == nbPos)) { continue; }

				mazePath.Add(new MazePoint(nbPos, cells[nbPos.x, nbPos.y].neighbors));
			}
			else { mazePath.Remove(currentCell); }

			currentCell = mazePath.Last();
		}

		foreach (var c in mazePath) { correctPath.Add(cells[c.pos.x, c.pos.y]); }
		yield return null;
		for (int i = 1; i < correctPath.Count; i++) {
			correctPath[i].KnockdownWall(correctPath[i - 1].pos);
			correctPath[i - 1].KnockdownWall(correctPath[i].pos);
		}
		MazeCell GetCell(Vector2Int pos) => cells[pos.x, pos.y];
	}


	List<Vector2Int> GetNeighbors(MazeCell cell) {
		var pos = cell.pos;

		var neighborsTemp = new List<Vector2Int>(4);
		if (pos.y > 0) { neighborsTemp.Add(new Vector2Int(pos.x, pos.y - 1)); }
		if (pos.x > 0) { neighborsTemp.Add(new Vector2Int(pos.x - 1, pos.y)); }
		if (pos.y < gridSize - 1) { neighborsTemp.Add(new Vector2Int(pos.x, pos.y + 1)); }
		if (pos.x < gridSize - 1) { neighborsTemp.Add(new Vector2Int(pos.x + 1, pos.y)); }
		return neighborsTemp;
	}

	void OnDrawGizmos() {
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