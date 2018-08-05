using UnityEngine;
using System;
using Assets.Scripts.Game.UI.Ftue;

namespace Assets.Scripts.Game
{

	/// <summary>
	/// 
	/// </summary>
	public class TimeManager : MonoBehaviour
	{
		protected static int INITIAL_YEAR = 2018;
		protected float _currentYearTime;
		protected float _currentMonthTime;
		protected float _currentWeekTime;
		protected float _currentDeforestationTime;

		[SerializeField]
		protected float _yearTime;
		public float YearTime { get { return _yearTime; } }

		protected int _actualYear;
		protected int _actualMonth;
		protected int _actualWeek;

		protected float _monthTime;
		public float MonthTime { get { return _monthTime; } }

		protected float _weekTime;
		public float WeekTime { get { return _weekTime; } }

		protected float _dayTime;
		public float DayTime { get { return _dayTime; } }

        public float NormalizeYearTime { get { return _currentYearTime / YearTime; } }

        public bool canActive = false;

        public float deforestationTime;

        public float skySpeed = 5f;
        private float SkyboxSpeed { get { return Mathf.Clamp(skySpeed, 0.1f, 10f); } }

        #region Instance
        private static TimeManager _instance;

		/// <summary>
		/// instance unique de la classe     
		/// </summary>
		public static TimeManager instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de TimeManager alors que c'est un singleton.");
			}
			_instance = this;
			_monthTime = YearTime / 12f;
			_weekTime = YearTime / 51f;
			_dayTime = YearTime / 365f;
			RestartTime();
		}

		protected void RestartTime()
		{
			_currentYearTime = 0;
			_currentMonthTime = 0;
			_currentWeekTime = 0;
			_currentDeforestationTime = 0;
			_actualYear = 0;
			_actualMonth = 0;
			_actualWeek = 0;
		}

        private float _skyRot = 0;
		protected void Update()
		{
            if (FtueManager.instance)
            {
                skySpeed = SkyboxSpeed;
                if (FtueManager.instance.active && GameManager.PARTY_TYPE != EPartyType.NONE)
                {
                    _skyRot = (_skyRot + (Time.deltaTime * (SkyboxSpeed / 10f))) % 360f;
                    RenderSettings.skybox.SetFloat("_Rotation", _skyRot);
                }
            }

            if (!canActive) return;

            _skyRot = (_skyRot + (Time.deltaTime * (SkyboxSpeed / 10f))) % 360f;
            RenderSettings.skybox.SetFloat("_Rotation", _skyRot);

            _currentYearTime += Time.deltaTime;
			_currentMonthTime += Time.deltaTime;
			_currentWeekTime += Time.deltaTime;
            _currentDeforestationTime += Time.deltaTime;

            if (_currentWeekTime >= _weekTime)
			{
				_currentWeekTime = 0;
				++_actualWeek;
				Events.Instance.Raise(new OnNewWeek(_actualWeek));
			}

			if (_currentMonthTime >= _monthTime)
			{
				_currentMonthTime = 0;
				++_actualMonth;
				Events.Instance.Raise(new OnNewMonth(_actualMonth));
			}

			if (_currentYearTime >= _yearTime)
			{
				_currentYearTime = 0;
				++_actualYear;
				Events.Instance.Raise(new OnNewYear(_actualYear + INITIAL_YEAR));
			}

            if (_currentDeforestationTime >= deforestationTime)
            {
                _currentDeforestationTime = 0;
                Events.Instance.Raise(new OnUpdateForest());
            }
		}

        public void Active()
        {
            canActive = true;
        }

        public void Desactive()
        {
            canActive = false;
        }

        public void Stop()
        {
            RestartTime();
            canActive = false;
        }

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}