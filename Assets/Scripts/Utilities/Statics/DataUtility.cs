using UnityEngine;
using System.Collections.Generic;

namespace NaniCore {
	public static class DataUtility {
		public static T PickRandom<T>(this IList<T> list) {
			if(list == null || list.Count <= 0)
				return default;
			int i = Mathf.FloorToInt(Random.Range(0, list.Count));
			return list[i];
		}
	}
}