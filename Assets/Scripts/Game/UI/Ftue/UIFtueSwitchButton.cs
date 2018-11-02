
using Assets.Script;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game.UI.Ftue
{
	public class UIFtueSwitchButton : UIFtueElement
	{
        protected Image img;
        protected Button btn;

        public EPlayer targetPlayer;

        public void Awake()
        {
            img = GetComponent<Image>();
            btn = GetComponent<Button>();
        }

        public void Update()
        {
            if (!btn.interactable) img.raycastTarget = false;
            else img.raycastTarget = true;

            if (PlayerManager.Instance)
            {
                Player player = PlayerManager.Instance.GetPlayerByType(targetPlayer);
                if (player != null) transform.position = Camera.main.WorldToScreenPoint(player.transform.position);
            }
        }
    }
}
