using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.PNJ;
using Assets.Scripts.Game.Save;

public enum FtueInputs { NONE, TOUCH, PINCH }

namespace Assets.Scripts.Game.UI.Ftue
{
	[Serializable]
	public class FtueComponent
	{
		public SimpleLocalisationText[] text;
        public UIFtueImageContainer titleImage;
        public UIFtueImageContainer headImage;
        public UIFtueImageContainer ftueSprite;
        public UIFtueImageContainer hiddenUI;
        public UIFtueImageContainer UItarget;
        public FtueInputs input;
        public bool forceAlpha = false;

        public FtueComponent Copy()
		{
			FtueComponent copy = new FtueComponent();
			copy.text = text;
            copy.titleImage = titleImage;
            copy.headImage = headImage;
            copy.ftueSprite = ftueSprite;
            copy.hiddenUI = hiddenUI;
            copy.UItarget = UItarget;
            copy.input = input;
            copy.forceAlpha = forceAlpha;
			return copy;
		}
	}

    /// <summary>
    /// 
    /// </summary>
    public class FtueManager : MonoBehaviour
	{
		public int startStep;
		public int modificationStepTarget;

        bool receiveTouch = false;

        [Header("Configuration")]
		public bool active;
		protected int currentStepIndex = 0;
		public int CurrentStepIndex { get { return currentStepIndex; } }

		public FtueInputs activeInput;

        public UIObjectPointer simpleModel;
        public UIObjectPointIcon iconModel;

		public UIFtueVisual pinchSprite;
		public UIFtueVisual tapSprite;
		public UIFtueText ftueText;

		[Header("Add Step")]
		public FtueComponent stepToAdd;

		[Header("Current FTUE Step")]
		public FtueComponent currentStep;	

		[Header("FTUE Steps")]
		public List<FtueComponent> steps;

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de FtueManager alors que c'est un singleton.");
			}
			_instance = this;
		}

		public void Launch()
		{
            active = true;
            if (GameManager.PARTY_TYPE == EPartyType.NEW)
            {
                currentStepIndex = startStep;
                currentStep = steps[startStep];

                ArrowDisplayer.Instances("Ftue").SetContainer(transform as RectTransform);
                ArrowDisplayer.Instances("Ftue").SetModels(simpleModel, iconModel);

                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i].ftueSprite != null) SetFtueUIImageContainer(steps[i].ftueSprite, false);
                    if (steps[i].headImage != null) SetFtueUIImageContainer(steps[i].headImage, false);
                    if (steps[i].hiddenUI != null) SetFtueUIImageContainer(steps[i].hiddenUI, false);
                    if (steps[i].titleImage != null) SetFtueUIImageContainer(steps[i].titleImage, false);
                }

                if (!gameObject.activeSelf) gameObject.SetActive(true);
                RunCurrentStep();
            }
            else Clear();
		}

        private void SetFtueUIImageContainer(UIFtueImageContainer comp, bool state, bool forceAlpha = false)
        {
            if (forceAlpha) comp.Alpha = 1;
            else comp.Alpha = 0;
            comp.SetState(state);
        }

		public void ValidStep()
		{
            receiveTouch = false;
			activeInput = FtueInputs.NONE;
			currentStepIndex++;
			if (currentStepIndex >= steps.Count) Clear();
    		else
			{
				currentStep = steps[currentStepIndex];
				Events.Instance.Raise(new OnFtueNextStep());
				RunCurrentStep();
			}
		}

        public void Clear()
        {
            active = false;
            StopAllCoroutines();
            gameObject.SetActive(false);
            receiveTouch = false;
            currentStep = null;
            currentStepIndex = 0;
            activeInput = FtueInputs.NONE;
            tapSprite.gameObject.SetActive(false);
            pinchSprite.gameObject.SetActive(false);
            ftueText.gameObject.SetActive(false);
            TimeManager.instance.canActive = true;
            if (GameManager.PARTY_TYPE == EPartyType.NEW) PlanetSave.SaveParty();
        }

        public void Display(bool state)
        {
            tapSprite.gameObject.SetActive(state);
            pinchSprite.gameObject.SetActive(state);
            ftueText.gameObject.SetActive(state);
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].ftueSprite != null) SetFtueUIImageContainer(steps[i].ftueSprite, state);
                if (steps[i].headImage != null) SetFtueUIImageContainer(steps[i].headImage, state);
                if (steps[i].titleImage != null) SetFtueUIImageContainer(steps[i].titleImage, state);
            }
            gameObject.SetActive(state);
        }

        public void RunCurrentStep()
        {
            ftueText.gameObject.SetActive(true);
            ArrowDisplayer.Instances("Ftue").CleanArrows();
            ArrowDisplayer.Instances("NotePad").CleanArrows();

            if (currentStep.UItarget != null)
                ArrowDisplayer.Instances("Ftue").UseArrow<UIObjectPointer>(150f, 80f, false, currentStep.UItarget.transform as RectTransform, "Ftue");

            if (currentStep.ftueSprite != null && currentStep.ftueSprite.Alpha < 1f)
                SetFtueUIImageContainer(currentStep.ftueSprite, true, true);

            activeInput = currentStep.input;
            if (currentStep.text != null)
            {
                for (int i = 0; i < currentStep.text.Length; i++)
                {
                    if (currentStep.text[i].lang == GameManager.LANGUAGE)
                    {
                        StartCoroutine(DisplayFtueStep(currentStep.text[i].text));
                        break;
                    }
                }
            }
            

            if (currentStep.input == FtueInputs.PINCH)
            {
                pinchSprite.gameObject.SetActive(true);
                pinchSprite.GetComponent<Animator>().SetBool("pinch", true);
                Events.Instance.AddListener<PanelLerpEnd>(ReceivePinch);
                Events.Instance.AddListener<OnEndFtuePinch>(ReceivePinch);
                Events.Instance.AddListener<OnInputFtuePinch>(DisablePinchSprite);
            }
        }

        protected void ReceivePinch(PanelLerpEnd e)
		{           
            Events.Instance.RemoveListener<PanelLerpEnd>(ReceivePinch);
			ValidStep();
		}

        protected void ReceivePinch(OnEndFtuePinch e)
        {
            Events.Instance.RemoveListener<OnEndFtuePinch>(ReceivePinch);
            ftueText.gameObject.SetActive(false);
        }

        protected void DisablePinchSprite(OnInputFtuePinch e)
		{
            Events.Instance.RemoveListener<OnInputFtuePinch>(DisablePinchSprite);
            pinchSprite.gameObject.SetActive(false);
		}

        protected IEnumerator WaitForTouch(bool isValidationStep = false)
		{
			yield return null;
			yield return null;

            if (isValidationStep) tapSprite.gameObject.SetActive(true);
            else tapSprite.gameObject.SetActive(false);

            while (!receiveTouch)
			{
				if (Input.GetMouseButtonDown(0)) receiveTouch = true;
				if (Input.touchCount > 1) receiveTouch = true;
				yield return null;
			}
            
            if (isValidationStep)
            {

                Events.Instance.Raise(new OnTapStepFTUE());

                if (currentStepIndex + 1 < steps.Count)
                {
                    if (steps[currentStepIndex + 1].titleImage == null && currentStep.titleImage != null)
                        SetFtueUIImageContainer(currentStep.titleImage, false);

                    if ((steps[currentStepIndex + 1].headImage == null || steps[currentStepIndex + 1].headImage != currentStep.headImage) && currentStep.headImage != null)
                        SetFtueUIImageContainer(currentStep.headImage, false);

                    if (currentStep.ftueSprite != null)
                        SetFtueUIImageContainer(currentStep.ftueSprite, false);

                }
                ValidStep();
            }
        }

        private IEnumerator DisplayFtueStep(string txt)
        {
            StartCoroutine(WaitForTouch());

            int charCounter = 0;
            string createdText = string.Empty;
            while (!receiveTouch && createdText != txt)
            {
                if (charCounter >= txt.Length)
                {
                    createdText = txt;
                    ftueText.SetText(createdText);
                }
                else
                {
                    createdText = createdText + txt[charCounter];
                    ftueText.SetText(createdText);
                    charCounter++;
                }

                if (currentStep.titleImage != null && currentStep.titleImage.Alpha < 1f)
                {
                    SetFtueUIImageContainer(currentStep.titleImage, true, currentStep.forceAlpha);
                    if (!currentStep.forceAlpha) currentStep.titleImage.Alpha = charCounter / 15f;
                }

                if (currentStep.headImage != null)
                {
                    SetFtueUIImageContainer(currentStep.headImage, true, currentStep.forceAlpha);
                    if (!currentStep.forceAlpha) currentStep.headImage.Alpha = charCounter / 15f;
                }

                if (currentStep.hiddenUI != null)
                {
                    SetFtueUIImageContainer(currentStep.hiddenUI, true, currentStep.forceAlpha);
                    if (!currentStep.forceAlpha) currentStep.hiddenUI.Alpha = charCounter / 15f;
                }

                yield return new WaitForSeconds(0.025f);
            }

            if (currentStep.titleImage != null && currentStep.titleImage.Alpha < 1f) currentStep.titleImage.Alpha = 1;
            if (currentStep.headImage != null) currentStep.headImage.Alpha = 1;
            if (currentStep.hiddenUI != null) currentStep.hiddenUI.Alpha = 1;

            ftueText.SetText(txt);
            receiveTouch = false;

            if (activeInput == FtueInputs.TOUCH) StartCoroutine(WaitForTouch(true));
        }

        #region Instance
        private static FtueManager _instance;

		/// <summary>
		/// instance unique de la classe     
		/// </summary>
		public static FtueManager instance
		{
			get
			{
				return _instance;
			}
		}
		#endregion

		protected void OnDestroy()
		{
			_instance = null;
		}
	}
}