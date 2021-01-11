
using UnityEngine;

public class MazePoint {
	public Vector2Int pos;
	public GridDir uncheckedDirs;

	public MazePoint(Vector2Int pos) {
		this.pos = pos;
		this.uncheckedDirs = GridDir.All;
	}
}