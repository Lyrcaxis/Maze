using System.Collections.Generic;

using UnityEngine;

static class Extensions {
	public static T GetRandom<T>(this IList<T> list) => list[Random.Range(0, list.Count)];
}