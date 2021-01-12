using System.Collections.Generic;

using UnityEngine;

public class MazeCell {
	public bool hasBeenVisited;
	public Vector2Int pos;
	public GridDir wallsRemaining = GridDir.All;
	public List<MazeCell> neighbors;

	public MazeCell(Vector2Int pos) {
		this.pos = pos;
	}

	public bool KnockdownWall(Vector2Int fromPos) {
		var prevWalls = wallsRemaining;
		wallsRemaining &= ~DirFromPos(fromPos);
		return prevWalls == wallsRemaining;
	}

	public GridDir DirFromPos(Vector2Int fromPos) {
		if (fromPos == pos + Vector2Int.right) { return GridDir.Right; }
		else if (fromPos == pos + Vector2Int.left) { return GridDir.Left; }
		else if (fromPos == pos + Vector2Int.up) { return GridDir.Up; }
		else if (fromPos == pos + Vector2Int.down) { return GridDir.Down; }
		else { Debug.LogWarning($"Weird wall connection {fromPos} --> {pos}"); }
		return GridDir.None;
	}
}
