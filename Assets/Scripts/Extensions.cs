using System.Collections.Generic;

using UnityEngine;

static class Extensions {
	public static T GetRandom<T>(this IList<T> list) => list[Random.Range(0, list.Count)];
	public static List<T> WithoutElement<T>(this List<T> list, T removedElement) {
		list.Remove(removedElement);
		return list;
	}
	public static List<T> Copy<T>(this List<T> list) {
		var newList = new List<T>(list.Count);
		foreach (var item in list) { newList.Add(item); }
		return newList;
	}
}