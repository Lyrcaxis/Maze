using System.Collections.Generic;

using UnityEngine;

static class Extensions {
	public static T GetRandom<T>(this IList<T> list) => list[Random.Range(0, list.Count)];
	public static List<T> Copy<T>(this List<T> list) {
		var newList = new List<T>(list.Count);
		foreach (var item in list) { newList.Add(item); }
		return newList;
	}
	public static int NumberOfSetBits(int i) {
		i = i - ((i >> 1) & 0x55555555);
		i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
		return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
	}
}
