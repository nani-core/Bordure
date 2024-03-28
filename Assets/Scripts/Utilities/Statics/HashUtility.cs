using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace NaniCore {
	public static class HashUtility {
		public static uint Hash(params byte[] data) {
			using(var hasher = SHA256.Create()) {
				var hasedData = hasher.ComputeHash(data);

				//https://stackoverflow.com/a/468084/15186859
				const uint p = 16777619;
				uint hash = 2166136261;
				for(int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;
				return hash;
			}
		}

		public static uint Hash(params int[] data) {
			List<byte> bytes = new();
			foreach(var datum in data)
				bytes.AddRange(BitConverter.GetBytes(datum));
			return Hash(bytes.ToArray());
		}
	}
}