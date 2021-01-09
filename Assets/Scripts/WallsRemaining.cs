public enum WallsRemaining {
	None = 0,
	Left = 1 << 0,
	Right = 1 << 1,
	Up = 1 << 2,
	Down = 1 << 3,

	All = Left | Right | Up | Down
}
