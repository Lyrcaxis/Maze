using System.Collections.Generic;

using UnityEngine;

public class MazeCell {
	public bool hasBeenVisited;
	public Vector2Int pos;
	public WallsRemaining wallsRemaining = WallsRemaining.All;
	public MazeCell parent;
	public List<Vector2Int> neighbors = new List<Vector2Int>();

	public MazeCell(Vector2Int pos) {
		this.pos = pos;
	}

	public void KnockdownWall(Vector2Int fromPos) {
		if (fromPos == pos + Vector2Int.right) { wallsRemaining &= ~WallsRemaining.Right; }
		else if (fromPos == pos + Vector2Int.left) { wallsRemaining &= ~WallsRemaining.Left; }
		else if (fromPos == pos + Vector2Int.up) { wallsRemaining &= ~WallsRemaining.Up; }
		else if (fromPos == pos + Vector2Int.down) { wallsRemaining &= ~WallsRemaining.Down; }
		else { throw new UnityException($"Weird wall {fromPos} --> {pos}"); }
	}
}

public struct MazePoint {
	public List<Vector2Int> remainingNeighbors;
	public Vector2Int pos;

	public MazePoint(Vector2Int pos, List<Vector2Int> remainingNeighbors) {
		this.pos = pos;
		this.remainingNeighbors = remainingNeighbors.Copy();
	}
}