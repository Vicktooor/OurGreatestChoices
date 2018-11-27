using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Game.UI.Global
{
	/// <summary>
	/// 
	/// </summary>
	public class Panel : MonoBehaviour
	{
        public void OnEnable()
        {
            ControllerInput.AddScreen(transform);
        }

        public void OnDisable()
        {
            ControllerInput.RemoveScreen(transform);
        }
    }
}