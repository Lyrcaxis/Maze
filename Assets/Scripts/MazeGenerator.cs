using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	[SerializeField] bool visualize = default;
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

		if (Random.Range(0, 2) == 1) { startCell.wallsRemaining &= ~GridDir.Left; }
		else { startCell.wallsRemaining &= ~GridDir.Down; }

		if (Random.Range(0, 2) == 1) { endCell.wallsRemaining &= ~GridDir.Right; }
		else { endCell.wallsRemaining &= ~GridDir.Up; }

		startCell.hasBeenVisited = true;
		endCell.hasBeenVisited = true;

		// Generate correct path
		StartCoroutine(GenerateCorrectPath(startCell, endCell));
	}

	System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

	void Update() {
		if (Input.GetMouseButtonDown(1)) {
			StopAllCoroutines();
			Start();
		}
		if (Input.GetMouseButtonDown(0)) {
			visualize = false;
			sw.Restart();
		}
	}
	IEnumerator GenerateCorrectPath(MazeCell startCell, MazeCell endCell) {
		sw.Start();

		List<MazePoint> mazePath = new List<MazePoint>();

		mazePath.Add(new MazePoint(startCell.pos));

		Vector2Int checkPos = mazePath[0].pos;
		var currentCell = mazePath[0];

		while (currentCell.pos != endCell.pos) {
			MazeCell nb = null;
			// really sorry about this.. testing optimizations
			switch (currentCell.uncheckedDirs) {
				case GridDir.None:
					break;
				case GridDir.Left:
				case GridDir.Right:
				case GridDir.Up:
				case GridDir.Down:
					nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], currentCell.uncheckedDirs);
					currentCell.uncheckedDirs = GridDir.None;
					break;
				case GridDir.All:
					var dir = (GridDir) (1 << Random.Range(0, 4));
					nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y],dir);
					currentCell.uncheckedDirs &= ~dir;
					break;
				case GridDir.Down | GridDir.Left:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
					}
					break;
				case GridDir.Up | GridDir.Left:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				case GridDir.Down | GridDir.Right:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Down;
							break;
					}
					break;
				case GridDir.Up | GridDir.Right:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				case GridDir.Left | GridDir.Right:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
					}
					break;
				case GridDir.Up | GridDir.Down:
					switch (Random.Range(0, 2)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Down;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				case GridDir.Left | GridDir.Up | GridDir.Right:
					switch (Random.Range(0, 3)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						case 1:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				case GridDir.Left | GridDir.Down | GridDir.Right:
					switch (Random.Range(0, 3)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						case 1:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Down;
							break;
					}
					break;
				case GridDir.Left | GridDir.Up | GridDir.Down:
					switch (Random.Range(0, 3)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Left);
							currentCell.uncheckedDirs &= ~GridDir.Left;
							break;
						case 1:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Down;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				case GridDir.Down | GridDir.Up | GridDir.Right:
					switch (Random.Range(0, 3)) {
						case 0:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Down);
							currentCell.uncheckedDirs &= ~GridDir.Down;
							break;
						case 1:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Right);
							currentCell.uncheckedDirs &= ~GridDir.Right;
							break;
						default:
							nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], GridDir.Up);
							currentCell.uncheckedDirs &= ~GridDir.Up;
							break;
					}
					break;
				default:
					Debug.Log("FAILED AT:" + currentCell.uncheckedDirs);
					break;
			}

			if (nb == null) {
				mazePath.Remove(currentCell);
			}
			else {
				Vector2Int nbPos = nb.pos;
				if (mazePath.Any(x => x.pos == nbPos)) { continue; }
				if (nb.pos.x == gridSize - 1 && nb.pos.y < currentCell.pos.y) { continue; }
				if (nb.pos.y == gridSize - 1 && nb.pos.x < currentCell.pos.x) { continue; }
				if (nb.pos.x == 0 && nb.pos.y < currentCell.pos.y) { continue; }
				if (nb.pos.y == 0 && nb.pos.x < currentCell.pos.x) { continue; }
				mazePath.Add(new MazePoint(nbPos));
			}
			currentCell = mazePath.Last();

			if (!visualize && sw.ElapsedMilliseconds > 5000) {
				Debug.Log("Slow loading detected.");
				Debug.Log("Visualization mode enabled. Left click to disable, Right click to try again.");
				sw.Stop();
				visualize = true;
			}

			if (visualize) {
				yield return null;

				correctPath.Clear();
				foreach (var c in mazePath) { correctPath.Add(cells[c.pos.x, c.pos.y]); }
			}
		}

		correctPath.Clear();
		foreach (var c in mazePath) {
			var cell = cells[c.pos.x, c.pos.y];
			correctPath.Add(cell);
			cell.hasBeenVisited = true;
		}
		for (int i = 1; i < correctPath.Count; i++) {
			correctPath[i].KnockdownWall(correctPath[i - 1].pos);
			correctPath[i - 1].KnockdownWall(correctPath[i].pos);
		}

		yield return null;
	}

	MazeCell GetNeighbor(MazeCell cell, GridDir dir) {
		Vector2Int pos;
		switch (dir) {
			case GridDir.Left:
				pos = cell.neighbors[0];
				break;
			case GridDir.Right:
				pos = cell.neighbors[1];
				break;
			case GridDir.Up:
				pos = cell.neighbors[2];
				break;
			case GridDir.Down:
				pos = cell.neighbors[3];
				break;
			default:
				return null;
		}
		return cells[pos.x, pos.y];
	}

	Vector2Int[] GetNeighbors(MazeCell cell) {
		var pos = cell.pos;

		var neighborsTemp = new Vector2Int[4];
		if (pos.x > 0) { neighborsTemp[0] = (new Vector2Int(pos.x - 1, pos.y)); }
		if (pos.x < gridSize - 1) { neighborsTemp[1] = (new Vector2Int(pos.x + 1, pos.y)); }
		if (pos.y < gridSize - 1) { neighborsTemp[2] = (new Vector2Int(pos.x, pos.y + 1)); }
		if (pos.y > 0) { neighborsTemp[3] = (new Vector2Int(pos.x, pos.y - 1)); }
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
			if (cell.wallsRemaining.HasFlag(GridDir.Left)) { Gizmos.DrawWireCube(cell.pos + Vector2.left * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(GridDir.Right)) { Gizmos.DrawWireCube(cell.pos + Vector2.right * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(GridDir.Up)) { Gizmos.DrawWireCube(cell.pos + Vector2.up * 0.25f, size); }
			if (cell.wallsRemaining.HasFlag(GridDir.Down)) { Gizmos.DrawWireCube(cell.pos + Vector2.down * 0.25f, size); }
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