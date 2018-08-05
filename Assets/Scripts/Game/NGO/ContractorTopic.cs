using Assets.Scripts.PNJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Game.NGO
{
	[CreateAssetMenu(fileName = "New Topic", menuName = "Topic/Contractor")]
	public class ContractorTopic : ScriptableObject
	{
		public SimpleLocalisationText[] topicTitles;
		public LocaNPCDialogue[] texts;
		public Sprite icon;
        public Sprite arrowIcon;

		[Header("Remplir ce champ")]
		public Vector3 targetWorldPos;
	}
}
