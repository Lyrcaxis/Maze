using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	[SerializeField] bool visualize = default;
	[SerializeField] GameObject wallPrefab = default;

	public MazeCell[,] cells { get; set; }
	List<MazeCell> correctPath = new List<MazeCell>();

	System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

	MazeCell invalidCell = new MazeCell(new Vector2Int(-1, -1));
	List<GameObject> walls = new List<GameObject>();


	void Start() {
		var cam = Camera.main;
		cam.orthographicSize = gridSize / 2f + 0.1f;
		cam.transform.position = new Vector3(gridSize / 2f, gridSize / 2f - 0.5f, -10f);

		foreach (var wall in walls) { GameObject.DestroyImmediate(wall); }

		// Initialize grid
		cells = new MazeCell[gridSize, gridSize];

		for (int x = 0; x < gridSize; x++) {
			for (int y = 0; y < gridSize; y++) {
				cells[x, y] = new MazeCell(new Vector2Int(x, y));
			}
		}

		// Get neighbors
		foreach (var c in cells) {
			c.neighbors = new Vector2Int[4];

			var pos = c.pos;
			if (pos.x > 0) { c.neighbors[0] = (new Vector2Int(pos.x - 1, pos.y)); }
			if (pos.x < gridSize - 1) { c.neighbors[1] = (new Vector2Int(pos.x + 1, pos.y)); }
			if (pos.y < gridSize - 1) { c.neighbors[2] = (new Vector2Int(pos.x, pos.y + 1)); }
			if (pos.y > 0) { c.neighbors[3] = (new Vector2Int(pos.x, pos.y - 1)); }

			// Discard non-existing neighbors
			for (int i = 0; i < c.neighbors.Length; i++) {
				if ((c.neighbors[i] - c.pos).sqrMagnitude != 1) { c.neighbors[i] = invalidCell.pos; }
			}
		}

		// Initialize entry and exit
		MazeCell startCell = cells[0, 0];
		MazeCell endCell = cells[gridSize - 1, gridSize - 1];

		if (Random.Range(0, 2) == 1) { startCell.wallsRemaining &= ~GridDir.Left; }
		else { startCell.wallsRemaining &= ~GridDir.Down; }

		if (Random.Range(0, 2) == 1) { endCell.wallsRemaining &= ~GridDir.Right; }
		else { endCell.wallsRemaining &= ~GridDir.Up; }

		startCell.hasBeenVisited = true;
		endCell.hasBeenVisited = true;

		StartCoroutine(GenerateCorrectPath(startCell, endCell, OnCorrectPathGenerated));


		void OnCorrectPathGenerated() {
			Debug.Log("Generating Secondary path");
			StartCoroutine(GenerateSecondaryPath(OnMazeCompletelyLoaded));
		}
		void OnMazeCompletelyLoaded() {
			Debug.Log("Maze completely generated");
			CreatePath();
		}
	}

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
	IEnumerator GenerateCorrectPath(MazeCell startCell, MazeCell endCell, System.Action Callback) {
		sw.Start();

		List<MazeCell> mazePath = new List<MazeCell>();

		mazePath.Add(startCell.withResettedUncheckedDirs);

		Vector2Int checkPos = mazePath[0].pos;
		MazeCell checkingPoint = mazePath[0];

		while (checkingPoint.pos != endCell.pos) {
			if (visualize) {
				yield return null;

				correctPath = mazePath;
			}
			VisualizationCheck();


			IterateNeighbors(checkingPoint, out var nb);

			if (nb == invalidCell) { continue; }
			if (nb == null) { mazePath.Remove(checkingPoint); }
			else {
				Vector2Int nbPos = nb.pos;
				if (mazePath.Any(x => x.pos == nbPos)) { continue; }
				if (nb.pos.x == gridSize - 1 && nb.pos.y < checkingPoint.pos.y) { continue; }
				if (nb.pos.y == gridSize - 1 && nb.pos.x < checkingPoint.pos.x) { continue; }
				if (nb.pos.x == 0 && nb.pos.y < checkingPoint.pos.y) { continue; }
				if (nb.pos.y == 0 && nb.pos.x < checkingPoint.pos.x) { continue; }
				mazePath.Add(cells[nbPos.x, nbPos.y]);
			}
			checkingPoint = mazePath.Last();
		}

		correctPath = mazePath;
		foreach (var c in correctPath) { c.hasBeenVisited = true; }

		for (int i = 1; i < correctPath.Count; i++) {
			correctPath[i].KnockdownWall(correctPath[i - 1].pos);
			correctPath[i - 1].KnockdownWall(correctPath[i].pos);
		}


		Callback?.Invoke();
	}

	IEnumerator GenerateSecondaryPath(System.Action Callback) {
		var visitedPoints = correctPath.Count;
		var totalPoints = cells.Length;
		var pathCopied = correctPath.Copy();
		List<MazeCell> uncheckedPoints = new List<MazeCell>();

		MazeCell checkingPoint = null;
		while (true) {
			if (pathCopied.Count != 0) {
				var rndStart = pathCopied.GetRandom();
				checkingPoint = rndStart.withResettedUncheckedDirs;
				pathCopied.Remove(rndStart);
			}
			else if (uncheckedPoints.Count != 0) {
				var rndStart = uncheckedPoints.GetRandom();
				checkingPoint = rndStart.withResettedUncheckedDirs;
			}
			else {
				Debug.Log("Reaching final point");
				break;
			}

			checkingPoint.hasBeenVisited = true;

			while (true) {
				if (visualize) { yield return null; }

				IterateNeighbors(checkingPoint, out var nb);

				if (nb == invalidCell) { continue; }
				if (nb == null) { break; }
				if (nb.hasBeenVisited) { continue; }

				checkingPoint.KnockdownWall(nb.pos);
				nb.KnockdownWall(checkingPoint.pos);
				nb.hasBeenVisited = true;

				if (checkingPoint.uncheckedDirs != GridDir.None) {
					if (uncheckedPoints.Contains(checkingPoint)) {
						uncheckedPoints.Add(checkingPoint);
					}
				}
				else { uncheckedPoints.Remove(checkingPoint); }

				checkingPoint = nb.withResettedUncheckedDirs;

				if (!visualize && sw.ElapsedMilliseconds > 5000) {
					Debug.Log("Slow loading detected.");
					Debug.Log("Visualization mode enabled. Left click to disable, Right click to try again.");
					sw.Stop();
					visualize = true;
				}
			}
		}

		// Connect all unconnected cells
		List<MazeCell> unconnectedCells = new List<MazeCell>();
		foreach (var c in cells) { if (!c.hasBeenVisited) { unconnectedCells.Add(c); } }
		foreach (var c in unconnectedCells) {
			checkingPoint = c.withResettedUncheckedDirs;
			c.hasBeenVisited = true;

			while (true) {
				if (visualize) { yield return null; }
				VisualizationCheck();

				IterateNeighbors(checkingPoint, out var nb);

				if (nb == invalidCell) { continue; }
				if (nb == null) { break; }
				int wallsRemaining = Extensions.NumberOfSetBits((int) checkingPoint.wallsRemaining);
				if (Extensions.NumberOfSetBits((int) nb.wallsRemaining) <= 2 && checkingPoint.wallsRemaining != GridDir.All) { continue; }

				nb.KnockdownWall(checkingPoint.pos);
				checkingPoint.KnockdownWall(nb.pos);
				checkingPoint = nb.withResettedUncheckedDirs;
				if (nb.hasBeenVisited) { break; }
				nb.hasBeenVisited = true;
			}
		}

		Callback?.Invoke();
	}

	void IterateNeighbors(MazeCell currentCell, out MazeCell nb) {
		nb = null;
		// really sorry about this.. testing optimizations
		try {
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
					nb = GetNeighbor(cells[currentCell.pos.x, currentCell.pos.y], dir);
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
		}
		catch { Debug.Log(currentCell.pos); }


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

			if (pos.x < 0) { return invalidCell; }
			return cells[pos.x, pos.y];
		}
	}

	void VisualizationCheck() {
		if (!visualize && sw.ElapsedMilliseconds > 5000) {
			Debug.Log("Slow loading detected on neighbor.");
			Debug.Log("Visualization mode enabled. Left click to disable, Right click to try again.");
			sw.Stop();
			visualize = true;
		}
	}

	void CreatePath() {
		foreach (var cell in cells) {
			if (cell.wallsRemaining.HasFlag(GridDir.Left)) { CreateWall(cell.pos + Vector2.left * 0.5f, 90); }
			if (cell.wallsRemaining.HasFlag(GridDir.Right)) { CreateWall(cell.pos + Vector2.right * 0.5f, 90); }
			if (cell.wallsRemaining.HasFlag(GridDir.Up)) { CreateWall(cell.pos + Vector2.up * 0.5f, 0); }
			if (cell.wallsRemaining.HasFlag(GridDir.Down)) { CreateWall(cell.pos + Vector2.down * 0.5f, 0); }
		}

		void CreateWall(Vector3 pos, float rotZ) {
			var newCube = Instantiate(wallPrefab);
			newCube.transform.position = pos;
			newCube.transform.eulerAngles = new Vector3(0, 0, rotZ);
			walls.Add(newCube);
		}
	}

	void OnDrawGizmos() {
		if (cells == null) { return; }
		Vector3 size = Vector3.one * 0.25f;

		foreach (var cell in cells) {
			// Draw cell
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(new Vector3(cell.pos.x, cell.pos.y), size * 1.1f);

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