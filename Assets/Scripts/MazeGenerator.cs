﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	[SerializeField] bool visualize = default;
	MazeCell[,] cells;
	List<MazeCell> correctPath = new List<MazeCell>();

	System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

	void Start() {
		var cam = Camera.main;
		cam.orthographicSize = gridSize / 2f;
		cam.transform.position = new Vector3(gridSize / 2f, gridSize / 2f - 0.5f, -10f);

		foreach (var wall in walls) { GameObject.DestroyImmediate(wall); }

		// Initialize grid
		cells = new MazeCell[gridSize, gridSize];

		for (int x = 0; x < gridSize; x++) {
			for (int y = 0; y < gridSize; y++) {
				cells[x, y] = new MazeCell(new Vector2Int(x, y));
			}
		}

		foreach (var c in cells) {
			c.neighbors = new Vector2Int?[4];
			var pos = c.pos;
			if (pos.x > 0) { c.neighbors[0] = (new Vector2Int(pos.x - 1, pos.y)); }
			if (pos.x < gridSize - 1) { c.neighbors[1] = (new Vector2Int(pos.x + 1, pos.y)); }
			if (pos.y < gridSize - 1) { c.neighbors[2] = (new Vector2Int(pos.x, pos.y + 1)); }
			if (pos.y > 0) { c.neighbors[3] = (new Vector2Int(pos.x, pos.y - 1)); }
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

		List<MazePoint> mazePath = new List<MazePoint>();

		mazePath.Add(new MazePoint(startCell.pos));

		Vector2Int checkPos = mazePath[0].pos;
		MazePoint checkingPoint = mazePath[0];

		while (checkingPoint.pos != endCell.pos) {
			IterateNeighbors(checkingPoint, out var nb);

			if (nb == null) { mazePath.Remove(checkingPoint); }
			else {
				Vector2Int nbPos = nb.pos;
				if (mazePath.Any(x => x.pos == nbPos)) { continue; }
				if (nb.pos.x == gridSize - 1 && nb.pos.y < checkingPoint.pos.y) { continue; }
				if (nb.pos.y == gridSize - 1 && nb.pos.x < checkingPoint.pos.x) { continue; }
				if (nb.pos.x == 0 && nb.pos.y < checkingPoint.pos.y) { continue; }
				if (nb.pos.y == 0 && nb.pos.x < checkingPoint.pos.x) { continue; }
				mazePath.Add(new MazePoint(nbPos));
			}
			checkingPoint = mazePath.Last();

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


		Callback?.Invoke();
	}

	IEnumerator GenerateSecondaryPath(System.Action Callback) {
		var visitedPoints = correctPath.Count;
		var totalPoints = cells.Length;
		var pathCopied = correctPath.Copy();
		List<MazePoint> uncheckedPoints = new List<MazePoint>();

		MazePoint checkingPoint = null;
		while (true) {
			if (pathCopied.Count != 0) {
				var rndStart = pathCopied.GetRandom();
				checkingPoint = new MazePoint(rndStart.pos);
				pathCopied.Remove(rndStart);
			}
			else if (uncheckedPoints.Count != 0) {
				var rndStart = uncheckedPoints.GetRandom();
				checkingPoint = new MazePoint(rndStart.pos);
			}
			else {
				Debug.Log("Reaching final point");
				break;
			}

			var mazeCell = cells[checkingPoint.pos.x, checkingPoint.pos.y];
			mazeCell.hasBeenVisited = true;

			while (true) {
				if (visualize) { yield return null; }

				IterateNeighbors(checkingPoint, out var nb);

				if (nb == null) { break; }
				if (nb.hasBeenVisited) { continue; }

				mazeCell.KnockdownWall(nb.pos);
				nb.KnockdownWall(mazeCell.pos);
				nb.hasBeenVisited = true;

				if (checkingPoint.uncheckedDirs != GridDir.None) {
					if (uncheckedPoints.Contains(checkingPoint)) {
						uncheckedPoints.Add(checkingPoint);
					}
				}
				else { uncheckedPoints.Remove(checkingPoint); }

				checkingPoint = new MazePoint(nb.pos);
				mazeCell = cells[checkingPoint.pos.x, checkingPoint.pos.y];

				if (!visualize && sw.ElapsedMilliseconds > 5000) {
					Debug.Log("Slow loading detected.");
					Debug.Log("Visualization mode enabled. Left click to disable, Right click to try again.");
					sw.Stop();
					visualize = true;
				}
			}
		}

		// Final iteration
		List<MazeCell> unconnectedCells = new List<MazeCell>();
		foreach (var c in cells) { if (!c.hasBeenVisited) { unconnectedCells.Add(c); } }
		foreach (var c in unconnectedCells) {
			checkingPoint = new MazePoint(c.pos);

			IterateNeighbors(checkingPoint, out var nb);
			nb.KnockdownWall(checkingPoint.pos);
			cells[checkingPoint.pos.x, checkingPoint.pos.y].KnockdownWall(nb.pos);
		}


		Callback?.Invoke();
	}

	void IterateNeighbors(MazePoint currentCell, out MazeCell nb) {
		nb = null;
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


		MazeCell GetNeighbor(MazeCell cell, GridDir dir) {
			Vector2Int pos;
			switch (dir) {
				case GridDir.Left:
					if (cell.neighbors[0].HasValue) { pos = cell.neighbors[2].Value; }
					else { return null; }
					break;
				case GridDir.Right:
					if (cell.neighbors[1].HasValue) { pos = cell.neighbors[2].Value; }
					else { return null; }
					break;
				case GridDir.Up:
					if (cell.neighbors[2].HasValue) { pos = cell.neighbors[2].Value; }
					else { return null; }
					break;
				case GridDir.Down:
					if (cell.neighbors[3].HasValue) { pos = cell.neighbors[2].Value; }
					else { return null; }
					break;
				default:
					return null;
			}
			return cells[pos.x, pos.y];
		}
	}

	public GameObject wallPrefab;
	List<GameObject> walls = new List<GameObject>();
	void CreatePath() {
		foreach (var cell in cells) {
			if (cell.wallsRemaining.HasFlag(GridDir.Left)) { CreateCube((Vector2) cell.pos + Vector2.left * 0.5f, 90); }
			if (cell.wallsRemaining.HasFlag(GridDir.Right)) { CreateCube((Vector2) cell.pos + Vector2.right * 0.5f, 90); }
			if (cell.wallsRemaining.HasFlag(GridDir.Up)) { CreateCube((Vector2) cell.pos + Vector2.up * 0.5f, 0); }
			if (cell.wallsRemaining.HasFlag(GridDir.Down)) { CreateCube((Vector2) cell.pos + Vector2.down * 0.5f, 0); }
		}

		void CreateCube(Vector3 pos, float rotZ) {
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