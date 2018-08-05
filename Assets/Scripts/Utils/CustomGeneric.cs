using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	public static class CustomGeneric
	{
		/// <summary>
		/// Return the index of the value, return -1 if !exist
		/// </summary>
		/// <param name="array">Array to check on</param>
		/// <param name="value">Value</param>
		/// <returns></returns>
		public static int ArrayContain(Vector3[] array, Vector3 value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == value) return i;
			}
			return -1;
		}

		/// <summary>
		/// Return the index of the value, return -1 if !exist
		/// </summary>
		/// <param name="array">Array to check on</param>
		/// <param name="value">Value</param>
		/// <returns></returns>
		public static int IntArrayContain(int[] array, int value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == value) return i;
			}
			return -1;
		}


		/// <summary>
		/// Object has property
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static bool HasProperty(this System.Type obj, string propertyName)
		{
			return obj.GetProperty(propertyName) != null;
		}
	}
}
