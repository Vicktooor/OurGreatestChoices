using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.PNJ
{
	[Serializable]
	public class LocaNPCDialogue
	{
		public SystemLanguage lang;
		public List<string> texts;
	}
}
