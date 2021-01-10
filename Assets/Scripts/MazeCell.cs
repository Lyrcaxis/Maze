using System.Collections.Generic;

using UnityEngine;

public class MazeCell {
	public bool hasBeenVisited;
	public Vector2Int pos;
	public Vector2Int[] neighbors;
	public GridDir wallsRemaining = GridDir.All;
	public GridDir uncheckedDirs;

	public MazeCell(Vector2Int pos) {
		this.pos = pos;
		this.uncheckedDirs = GridDir.All;
	}

	public void KnockdownWall(Vector2Int fromPos) {
		if (fromPos == pos + Vector2Int.right) { wallsRemaining &= ~GridDir.Right; }
		else if (fromPos == pos + Vector2Int.left) { wallsRemaining &= ~GridDir.Left; }
		else if (fromPos == pos + Vector2Int.up) { wallsRemaining &= ~GridDir.Up; }
		else if (fromPos == pos + Vector2Int.down) { wallsRemaining &= ~GridDir.Down; }
		else { Debug.LogWarning($"Weird wall {fromPos} --> {pos}"); }
	}
}

public class MazePoint {
	public Vector2Int pos;
	public GridDir uncheckedDirs;

	public MazePoint(Vector2Int pos) {
		this.pos = pos;
		this.uncheckedDirs = GridDir.All;
	}
}