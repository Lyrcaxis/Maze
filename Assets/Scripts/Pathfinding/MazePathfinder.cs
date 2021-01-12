using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePathfinder {
	public List<Vector2Int> GetMazePath(Vector2Int startPos, Vector2Int targetPos) {
		var cells = Object.FindObjectOfType<MazeGenerator>().cells;

		var startCell = cells[startPos.x, startPos.y];
		var endCell = cells[targetPos.x, targetPos.y];

		var checkingCell = startCell;
		while (checkingCell != endCell) {

		}

		List<Vector2Int> path = new List<Vector2Int>();

		return path;
	}

	//static List<Vector2Int> 

	static Vector2Int GetPointAtDir(MazeCell c, GridDir dir) {
		switch (dir) {
			case GridDir.Left: { return c.pos + Vector2Int.left; }
			case GridDir.Right: { return c.pos + Vector2Int.right; }
			case GridDir.Up: { return c.pos + Vector2Int.up; }
			case GridDir.Down: { return c.pos + Vector2Int.down; }
			default:
				throw new UnityException("Wrong direction");
		}
	}
}
