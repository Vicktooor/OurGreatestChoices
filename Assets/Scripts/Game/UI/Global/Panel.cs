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
            ControllerInput.OpenScreens.Add(transform);
        }

        public void OnDisable()
        {
            ControllerInput.OpenScreens.Remove(transform);
        }
    }
}