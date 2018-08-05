using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game.UI
{
	public enum EUISwitchAxis { V, H }

	/// <summary>
	/// 
	/// </summary>
	public class UISwitchElement : MonoBehaviour
	{
		[SerializeField]
		protected List<EUISwitchAxis> switchAxis;
		[SerializeField]
		protected bool keepRotation;
		[SerializeField]
		protected bool keepPosition;

		protected Vector3 _position;
		protected Quaternion _rotation;

		protected virtual void Awake()
		{
			_position = transform.position;
			_rotation = transform.rotation;
		}

		protected virtual void Update()
		{
			if (transform.hasChanged)
			{
				_position = transform.position;
				_rotation = transform.rotation;
			}
		}

		public virtual void SwitchSide()
		{
			if (switchAxis.Contains(EUISwitchAxis.V))
			{
				if (!keepPosition)
				{
					_position.x *= -1;
					transform.position = _position;
				}			
				if (!keepRotation)
				{
					_rotation *= Quaternion.Euler(0f, 180f, 2 * _rotation.eulerAngles.z);
					transform.rotation = _rotation;
				}
			}

			if (switchAxis.Contains(EUISwitchAxis.H))
			{
				if (!keepPosition)
				{
					_position.y *= -1;
					transform.position = _position;
				}		
				if (!keepRotation)
				{
					_rotation *= Quaternion.Euler(180f, 0f, 2 * _rotation.eulerAngles.z);
					transform.rotation = _rotation;
				}
			}
		}
	}
}