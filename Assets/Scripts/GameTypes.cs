using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
	public static class GameTypes
	{
		public static List<Type> PNJ_TYPES = new List<Type>() {
			typeof(InteractablePNJ),
			typeof(InteractablePNJ_CarsCompany),
			typeof(InteractablePNJ_CERN),
			typeof(InteractablePNJ_CoalPower),
			typeof(InteractablePNJ_TownHall),
			typeof(InteractablePNJ_OilPlant)
		};
	}
}
