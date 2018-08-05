using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Planet
{
	[Serializable]
	public class BiomeColor
	{
		public CellState type;
		public List<Color> colors;
		public Color PolutedColor;
	}
}
