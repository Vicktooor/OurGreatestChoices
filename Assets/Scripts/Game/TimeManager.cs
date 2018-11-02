using UnityEngine;
using System;
using Assets.Scripts.Game.UI.Ftue;
using Assets.Scripts.Game.Save;

[Serializable]
public struct TimeSave
{
    public int actualYear;
    public int actualMonth;
    public int actualWeek;
    public float yearTime;
    public float monthTime;
    public float weekTime;
}

namespace Assets.Scripts.Game
{
	/// <summary>
	/// 
	/// </summary>
	public class TimeManager : MonoSingleton<TimeManager>
	{
		public static int INITIAL_YEAR = 2018;
		protected float _currentYearTime;
		protected float _currentMonthTime;
		protected float _currentWeekTime;
		protected float _currentDeforestationTime;
		protected float _currentPolutionTime;

		[SerializeField]
		protected float _yearTime;
		public float YearTime { get { return _yearTime; } }

		protected int _actualYear;
        public int ActualYear { get { return _actualYear; } }
		protected int _actualMonth;
        public int ActualMonth { get { return _actualMonth; } }
        protected int _actualWeek;

        protected float _monthTime;
		public float MonthTime { get { return _monthTime; } }

		protected float _weekTime;
		public float WeekTime { get { return _weekTime; } }

		protected float _dayTime;
		public float DayTime { get { return _dayTime; } }

        public float NormalizeYearTime { get { return _currentYearTime / YearTime; } }

        public bool canActive = false;

        public float skySpeed = 5f;
        private float SkyboxSpeed { get { return Mathf.Clamp(skySpeed, 0.1f, 10f); } }

		override protected void Awake()
		{
            base.Awake();
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
                Events.Instance.Raise(new OnUpdateForest());
                Events.Instance.Raise(new OnUpdateGround());
            }

			if (_currentYearTime >= _yearTime)
			{
				_currentYearTime = 0;
				++_actualYear;
				Events.Instance.Raise(new OnNewYear(_actualYear + INITIAL_YEAR));
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

        public void LoadSave()
        {
            _currentWeekTime = PlanetSave.GameStateSave.SavedTime.weekTime;
            _currentMonthTime = PlanetSave.GameStateSave.SavedTime.monthTime;
            _currentYearTime = PlanetSave.GameStateSave.SavedTime.yearTime;
            _actualWeek = PlanetSave.GameStateSave.SavedTime.actualWeek;
            _actualMonth = PlanetSave.GameStateSave.SavedTime.actualMonth;
            _actualYear = PlanetSave.GameStateSave.SavedTime.actualYear;
        }

        public TimeSave GenerateSave()
        {
            TimeSave newSave = new TimeSave();
            newSave.actualWeek = _actualWeek;
            newSave.actualMonth = _actualMonth;
            newSave.actualYear = _actualYear;
            newSave.weekTime = _currentWeekTime;
            newSave.monthTime = _currentMonthTime;
            newSave.yearTime = _currentYearTime;
            return newSave;
        }

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}