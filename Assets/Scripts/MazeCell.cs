using System.Collections.Generic;

using UnityEngine;

public class MazeCell {
	public bool hasBeenVisited;
	public Vector2Int pos;
	public Vector2Int[] neighbors;
	public GridDir wallsRemaining = GridDir.All;
	public GridDir uncheckedDirsTemp;
	public GridDir dirsToCheckForPathfinding;

	public Dictionary<GridDir, MazeCell> walkableNeighbors;

	public MazeCell(Vector2Int pos) {
		this.pos = pos;
		uncheckedDirsTemp = GridDir.All;
	}

	public MazeCell withResettedUncheckedDirs {
		get {
			uncheckedDirsTemp = GridDir.All;
			return this;
		}
	}

	public void KnockdownWall(Vector2Int fromPos) => wallsRemaining &= ~DirFromPos(fromPos);

	public GridDir DirFromPos(Vector2Int fromPos) {
		switch (fromPos) {
			case Vector2Int _ when fromPos == pos + Vector2Int.right: { return GridDir.Right; }
			case Vector2Int _ when fromPos == pos + Vector2Int.left: { return GridDir.Left; }
			case Vector2Int _ when fromPos == pos + Vector2Int.up: { return GridDir.Up; }
			case Vector2Int _ when fromPos == pos + Vector2Int.down: { return GridDir.Down; }
			default: {
				Debug.LogWarning($"Weird wall connection {fromPos} --> {pos}");
				return GridDir.None;
			}
		}
	}
}