using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class MazeGenerator : MonoBehaviour {
	[SerializeField] int gridSize = default;
	[SerializeField] GameObject wallPrefab = default;

	public MazeCell[,] cells { get; set; }
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

		// Generate maze
		VisitNeighbors(cells[0, 0]);

		// Create exits
		if (Random.Range(0, 2) == 1) { cells[0, 0].wallsRemaining &= ~GridDir.Left; }
		else { cells[0, 0].wallsRemaining &= ~GridDir.Down; }

		if (Random.Range(0, 2) == 1) { cells[gridSize - 1, gridSize - 1].wallsRemaining &= ~GridDir.Right; }
		else { cells[gridSize - 1, gridSize - 1].wallsRemaining &= ~GridDir.Up; }

		CreatePath();

		void VisitNeighbors(MazeCell cell) {
			cell.hasBeenVisited = true;

			var nbs = GetShuffledNeighbors(cell);

			foreach (var nb in nbs) {
				if (!nb.hasBeenVisited) {
					nb.KnockdownWall(cell.pos);
					cell.KnockdownWall(nb.pos);
					VisitNeighbors(nb);
				}
			}
		}

		List<MazeCell> GetShuffledNeighbors(MazeCell cell) {
			var pos = cell.pos;
			var neighbors = new List<MazeCell>(4);

			if (pos.x > 0) { neighbors.Add(cells[pos.x - 1, pos.y]); }
			if (pos.x < gridSize - 1) { neighbors.Add(cells[pos.x + 1, pos.y]); }
			if (pos.y < gridSize - 1) { neighbors.Add(cells[pos.x, pos.y + 1]); }
			if (pos.y > 0) { neighbors.Add(cells[pos.x, pos.y - 1]); }

			neighbors.Shuffle();
			return neighbors;
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

	void Update() {
		if (Input.GetMouseButtonDown(1)) { Start(); }
	}

}