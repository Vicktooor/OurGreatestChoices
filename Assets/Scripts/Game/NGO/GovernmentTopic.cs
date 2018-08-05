using Assets.Scripts.PNJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Game.NGO
{
	[CreateAssetMenu(fileName = "New Topic", menuName = "Topic/Government")]
	public class GovernmentTopic : ScriptableObject
	{
		public SimpleLocalisationText[] topicTitles;
		public Item targetItem;
		public LocaNPCDialogue[] texts;
		public Sprite icon;
	}
}
