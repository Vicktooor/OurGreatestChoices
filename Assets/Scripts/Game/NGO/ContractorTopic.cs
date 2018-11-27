using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class NPCDialogue
{
    public string playerText;
    public string NPCText;
    public bool getInfo = true;
    public List<NPCDialogue> nextTexts;
    public Sprite smileySprite;
    public Sprite iconSprite;
}

namespace Assets.Scripts.Game.NGO
{
	[CreateAssetMenu(fileName = "New Topic", menuName = "Topic/Contractor")]
	public class ContractorTopic : ScriptableObject
	{
        public int id;
        public NPCDialogue text;
		public Sprite icon;
        public Sprite arrowIcon;
		[Header("Remplir ce champ")]
		public Vector3 targetWorldPos;
	}
}
