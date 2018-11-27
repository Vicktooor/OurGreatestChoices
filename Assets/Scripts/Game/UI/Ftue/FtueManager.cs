using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.Game.Save;
using Assets.Script;
using UnityEngine.UI;

public enum FtueInputs { NONE, TOUCH, PINCH }

namespace Assets.Scripts.Game.UI.Ftue
{
    [Serializable]
    public class FTUEPoint
    {
        public Vector3 pos;
        public Sprite icon;
        public bool withArrow = true;
    }

    [Serializable]
    public class PointerFtueMove
    {
        public bool active = false;
        public RectTransform startTrf;
    }

	[Serializable]
	public class FtueComponent
	{
		public string text;
        public UIFtueImageContainer titleImage;
        public UIFtueImageContainer ftueSprite;
        public UIFtueImageContainer hiddenUI;
        public UIFtueImageContainer UItarget;
        public Transform waitOpeningPanel;
        public bool targetSmiley = false;
        public bool targetNPCIcon = false;
        public bool pointNPCIcon = false;
        public int targetTopic = -1;
        public FtueInputs input;
        public bool forceAlpha = false;
        public bool waitDezoom = false;
        public UIFtueSwitchButton playerBtn;
        public Vector2 tapPosition = Vector2.zero;
        public int scrollerIndex = -1;
        public EItemType inventoryTarget = EItemType.None;
        public int getNbTarget = 0;
        private int actualGetNb = 0;
        public EItemType popTarget = EItemType.None;
        public int popNbTarget = 0;
        private int actualPopNb = 0;
        public bool waitArrowPos = false;
        public float pointRotation = 0f;
        public bool playerFree = false;
        public RectTransform toPointUI;
        public EPlayer transformTarget;
        public FTUEPoint specificPoint;
        public PointerFtueMove drag;
        public int targetBudget;
        private int actualGetBudget = 0;
        public Transform toDetach;

        public void PickUp()
        {
            actualGetNb++;
        }

        public void GetBudget()
        {
            actualGetBudget++;
        }

        public bool HaveBudget()
        {
            return actualGetBudget >= targetBudget;
        }

        public void PopOne()
        {
            actualPopNb++;
            if (!CanPop() && popNbTarget != 0 && getNbTarget <= 0) FtueManager.instance.ValidStep();
        }

        public bool CanPop()
        {
            return actualPopNb < popNbTarget;
        }

        public bool HaveMaxItem()
        {
            return actualGetNb >= getNbTarget;
        }

        public FtueComponent Copy()
		{
			FtueComponent copy = new FtueComponent();
			copy.text = text;
            copy.titleImage = titleImage;
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
        public bool playerFree = false;

        public UIObjectPointer simpleModel;
        public UIObjectPointer pointTap;
        public UIObjectPointIcon iconModel;

		public UIFtueVisual pinchSprite;
		public UIFtueVisual tapSprite;
		public UIFtueText ftueText;
        public Transform clickTarget;
        public Transform dragTarget;

        [Header("Add Step")]
		public FtueComponent stepToAdd;

		[Header("Current FTUE Step")]
		public FtueComponent currentStep;	

		[Header("FTUE Steps")]
		public List<FtueComponent> steps;

        private Transform _clickableTransform;
        public Transform ClickableTransform { get { return _clickableTransform; } }

		protected void Awake()
		{
			if (_instance != null)
			{
				throw new Exception("Tentative de création d'une autre instance de FtueManager alors que c'est un singleton.");
			}
			_instance = this;
		}

        private bool _finish = false;
        public bool Finish { get { return _finish; } }

		public void Launch()
		{
            if (!_finish && !active)
            {
                active = true;
                Events.Instance.AddListener<OnStartFtue>(StartFTUE);
            }
		}

        public void StartFTUE(OnStartFtue e)
        {
            Events.Instance.RemoveListener<OnStartFtue>(StartFTUE);

            currentStepIndex = startStep;
            currentStep = steps[startStep];

            ArrowDisplayer.Instances("FtueArrow").SetContainer(transform as RectTransform);
            ArrowDisplayer.Instances("FtueArrow").SetModels(simpleModel, iconModel);

            ArrowDisplayer.Instances("FtueTap").SetContainer(transform as RectTransform);
            ArrowDisplayer.Instances("FtueTap").SetModels(pointTap, null);

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].ftueSprite != null) SetFtueUIImageContainer(steps[i].ftueSprite, false);
                if (steps[i].hiddenUI != null) SetFtueUIImageContainer(steps[i].hiddenUI, false);
                if (steps[i].titleImage != null) SetFtueUIImageContainer(steps[i].titleImage, false);
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            RunCurrentStep();
        }

        private void SetFtueUIImageContainer(UIFtueImageContainer comp, bool state, bool forceAlpha = false)
        {
            if (forceAlpha) comp.Alpha = 1;
            else comp.Alpha = 0;
            comp.SetState(state);
        }

		public void ValidStep()
		{
            StopAllCoroutines();
            playerFree = false;
            GetComponent<Image>().raycastTarget = true;
            Attach();
            if (_uitargetContainer != null)
            {
                if (currentStep.UItarget != null)
                {
                    currentStep.UItarget.transform.SetParent(_uitargetContainer);
                    currentStep.UItarget.transform.SetSiblingIndex(_childIndex);
                }
                else if (currentStep.playerBtn != null)
                {
                    currentStep.playerBtn.transform.SetParent(_uitargetContainer);
                    currentStep.playerBtn.transform.SetSiblingIndex(_childIndex);
                }
                _uitargetContainer = null;
            }

            if (currentStep.targetTopic != -1)
            {
                NotePad.Instance.Topics[currentStep.targetTopic].gameObject.transform.SetParent(NotePad.Instance.gameObject.transform);
                NotePad.Instance.Topics[currentStep.targetTopic].SetColor(Color.white);
            }

            receiveTouch = false;
			activeInput = FtueInputs.NONE;
			currentStepIndex++;
			if (currentStepIndex >= steps.Count) Clear();
    		else
			{
                ftueText.gameObject.SetActive(false);
                ArrowDisplayer.Instances("FtueArrow").CleanArrows();
                ArrowDisplayer.Instances("FtueTap").CleanArrows();
                _clickableTransform = null;
                
                currentStep = steps[currentStepIndex];
				Events.Instance.Raise(new OnFtueNextStep());
				if (currentStep.targetTopic != -1 && !NotePad.Instance.Open) Events.Instance.AddListener<OnFTUEOpenDialogue>(OpenDialogue);
                else RunCurrentStep();
            }
		}

        public void Clear()
        {
            _finish = true;
            active = false;
            StopAllCoroutines();
            _clickableTransform = null;
            gameObject.SetActive(false);
            receiveTouch = false;
            currentStep = null;
            currentStepIndex = 0;
            activeInput = FtueInputs.NONE;
            tapSprite.gameObject.SetActive(false);
            pinchSprite.gameObject.SetActive(false);
            ftueText.gameObject.SetActive(false);
            GameManager.Instance.EndOfFTUE();
        }

        public void ForceClear()
        {
            _finish = true;
            active = false;
            StopAllCoroutines();
            _clickableTransform = null;
            gameObject.SetActive(false);
            receiveTouch = false;
            currentStep = null;
            currentStepIndex = 0;
            activeInput = FtueInputs.NONE;
            tapSprite.gameObject.SetActive(false);
            pinchSprite.gameObject.SetActive(false);
            ftueText.gameObject.SetActive(false);
            Display(false);
            ActiveHidden();
        }

        public void ActiveHidden()
        {
            for (int i = 0; i < steps.Count; i++) if (steps[i].hiddenUI != null) SetFtueUIImageContainer(steps[i].hiddenUI, true, true);
        }

        public void Display(bool state)
        {
            tapSprite.gameObject.SetActive(state);
            pinchSprite.gameObject.SetActive(state);
            ftueText.gameObject.SetActive(state);
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].ftueSprite != null) SetFtueUIImageContainer(steps[i].ftueSprite, state);
                if (steps[i].titleImage != null) SetFtueUIImageContainer(steps[i].titleImage, state);
                if (steps[i].hiddenUI != null) SetFtueUIImageContainer(steps[i].hiddenUI, state);
            }
        }

        public int GetChildIndex(Transform child, Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform c = parent.GetChild(i);
                if (c.Equals(child)) return i;
            }
            return -1;
        }

        private Transform _uitargetContainer;
        private Transform _detachContainer;
        private int _childIndex = 0;
        private int _childDetachIndex = 0;
        public void RunCurrentStep()
        {
            activeInput = currentStep.input;
            if (currentStep.text != null && currentStep.text != string.Empty)
            {
                ftueText.gameObject.SetActive(true);
                StartCoroutine(DisplayFtueStep(currentStep.text));
            }
            else if (currentStep.input == FtueInputs.TOUCH)
            {
                StartCoroutine(DisplayFtueStep(string.Empty));
            }

            if (currentStep.toDetach != null)
            {
                _detachContainer = currentStep.toDetach.parent;
                _childDetachIndex = GetChildIndex(currentStep.toDetach, _detachContainer);
                currentStep.toDetach.SetParent(transform);
            }

            if (currentStep.UItarget != null)
            {
                _uitargetContainer = currentStep.UItarget.transform.parent;
                _childIndex = GetChildIndex(currentStep.UItarget.transform, _uitargetContainer);
                currentStep.UItarget.transform.SetParent(transform);
                currentStep.UItarget.SetState(true);
                ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(50f, currentStep.pointRotation, false, currentStep.UItarget.transform as RectTransform, "FtueTap");
            }

            if (currentStep.toPointUI != null)
            {
                ArrowDisplayer.Instances("FtueArrow").UseArrow<UIObjectPointer>(50f, currentStep.pointRotation, false, currentStep.toPointUI, "FtueArrow");
            }

            if (currentStep.specificPoint.pos != Vector3.zero)
            {
                if (currentStep.specificPoint.withArrow)
                {
                    if (currentStep.specificPoint.icon != null)
                        NotePad.Instance.PointTarget(currentStep.specificPoint.pos, currentStep.specificPoint.icon);
                    else NotePad.Instance.PointTarget(currentStep.specificPoint.pos);
                }
                else PlayerManager.Instance.player.SetPosCallBack(currentStep.specificPoint.pos, ValidStep);
            }

            playerFree = currentStep.playerFree;
            if (playerFree) GetComponent<Image>().raycastTarget = false;

            if (currentStep.ftueSprite != null && currentStep.ftueSprite.Alpha < 1f)
                SetFtueUIImageContainer(currentStep.ftueSprite, true, true);

            if (currentStep.tapPosition != Vector2.zero)
            {
                clickTarget.gameObject.transform.localPosition = new Vector3(Screen.width * currentStep.tapPosition.x, Screen.height * currentStep.tapPosition.y, 0f);
                ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(50f, 95f, false, clickTarget as RectTransform, "FtueTap", false);               
            }

            if (currentStep.targetSmiley)
                ArrowDisplayer.Instances("FtueArrow").UseArrow<UIObjectPointer>(75f, currentStep.pointRotation, false, UIManager.instance.PNJState.smileyRenderer.transform as RectTransform, "Ftue");

            if (currentStep.targetNPCIcon)
            {
                Transform nearestIconTrf = PlayerManager.Instance.GetNearestNPCIcon();
                _clickableTransform = nearestIconTrf;
                ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(50f, currentStep.pointRotation, false, nearestIconTrf.position, "FtueTap", false);
            }

            if (currentStep.pointNPCIcon)
            {
                Transform nearestIconTrf = PlayerManager.Instance.GetNearestNPCIcon();
                ArrowDisplayer.Instances("FtueArrow").UseArrow<UIObjectPointer>(50f, currentStep.pointRotation, false, nearestIconTrf.position, "FtueArrow", false);
            }

            if (_clickableTransform != null)
            {
                clickTarget.gameObject.SetActive(true);
                clickTarget.position = Camera.main.WorldToScreenPoint(_clickableTransform.position);
            }

            if (currentStep.targetTopic != -1)
            {
                RectTransform topicTrf = NotePad.Instance.Topics[currentStep.targetTopic].GetComponent<RectTransform>();
                NotePad.Instance.Topics[currentStep.targetTopic].gameObject.transform.SetParent(transform);
                NotePad.Instance.Topics[currentStep.targetTopic].SetColor(Color.green);
                ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(50f, -90f, false, topicTrf, "FtueTap", false);
                Events.Instance.AddListener<OnActiveSelectTopic>(OnSelectTopic);
            }

            if (currentStep.playerBtn != null)
            {
                _uitargetContainer = currentStep.playerBtn.transform.parent;
                _childIndex = GetChildIndex(currentStep.playerBtn.transform, _uitargetContainer);
                currentStep.playerBtn.transform.SetParent(transform);
                ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(25f, currentStep.pointRotation, false, currentStep.playerBtn.transform as RectTransform, "FtueTap");
                Events.Instance.AddListener<SelectPlayer>(OnSelectPlayer);
            }

            if (currentStep.input == FtueInputs.PINCH)
            {
                pinchSprite.gameObject.SetActive(true);
                pinchSprite.GetComponent<Animator>().SetBool("pinch", true);
                Events.Instance.AddListener<OnPinchEnd>(ReceivePinch);
                Events.Instance.AddListener<OnEndFtuePinch>(ReceivePinch);
                Events.Instance.AddListener<OnInputFtuePinch>(DisablePinchSprite);
            }
            
            if (currentStep.drag.active)
            {
                StartCoroutine(DisplayDrag(currentStep.drag));
            }

            if (currentStep.waitDezoom)
            {
                Events.Instance.AddListener<OnEndPanelZoom>(ReceiveZoomValidation);
            }
        }

        public void AttachTarget(Transform trf)
        {
            trf.SetParent(_uitargetContainer);
            trf.SetSiblingIndex(_childIndex);
            _uitargetContainer = null;
        }

        public void DetachTarget(Transform trf)
        {
            _uitargetContainer = trf.parent;
            _childIndex = GetChildIndex(trf, _uitargetContainer);
            trf.SetParent(transform);
        }

        public void Attach()
        {
            if (_detachContainer != null)
            {
                currentStep.toDetach.SetParent(_detachContainer);
                currentStep.toDetach.SetSiblingIndex(_childDetachIndex);
                _detachContainer = null;
            }
        }

        protected void OnSelectPlayer(SelectPlayer e)
        {
            Events.Instance.RemoveListener<SelectPlayer>(OnSelectPlayer);
            ValidStep();
        }

        protected void ReceiveZoomValidation(OnEndPanelZoom e)
        {
            Events.Instance.RemoveListener<OnEndPanelZoom>(ReceiveZoomValidation);
            ValidStep();
        }

        public void OnSelectTopic(OnActiveSelectTopic e)
        {
            Events.Instance.RemoveListener<OnActiveSelectTopic>(OnSelectTopic);
            ValidStep();
        }

        public void OpenDialogue(OnFTUEOpenDialogue e)
        {
            if (active) RunCurrentStep();
        }

        protected void ReceivePinch(OnPinchEnd e)
		{           
            Events.Instance.RemoveListener<OnPinchEnd>(ReceivePinch);
            ftueText.gameObject.SetActive(false);
            ArrowDisplayer.Instances("FtueArrow").CleanArrows();
            ArrowDisplayer.Instances("FtueTap").CleanArrows();
        }

        public void ReceiveClickOnTarget()
        {
            clickTarget.gameObject.SetActive(false);
            if (_clickableTransform != null)
            {
                if (currentStep.targetNPCIcon)
                {
                    InteractablePNJ npc = _clickableTransform.GetComponent<BillboardHelp>().pnj;
                    PlayerManager.Instance.player.LaunchCoroutineTapNPC(new OnTapNPC(npc));
                }
            }
            if (currentStep.waitOpeningPanel == null) ValidStep();
        }

        public void OnEndTransition()
        {
            if (active) ValidStep();
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

        protected IEnumerator WaitForTouch(bool isValidationStep = false, float xPct = 0, float yPct = 0)
		{
			yield return null;
			yield return null;

            if (isValidationStep)
            {
                if (currentStep.tapPosition != Vector2.zero) tapSprite.gameObject.SetActive(false);
                else tapSprite.gameObject.SetActive(true);
            }
            else tapSprite.gameObject.SetActive(false);

            Vector3 baseTapPos = tapSprite.transform.localPosition;
            if (xPct != 0 || yPct != 0) tapSprite.transform.localPosition = new Vector3(Screen.width * xPct, Screen.height * yPct, 0f);

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

                    if (currentStep.ftueSprite != null)
                        SetFtueUIImageContainer(currentStep.ftueSprite, false);
                }

                tapSprite.gameObject.SetActive(false);
                tapSprite.transform.localPosition = baseTapPos;
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

                if (currentStep.hiddenUI != null)
                {
                    SetFtueUIImageContainer(currentStep.hiddenUI, true, currentStep.forceAlpha);
                    if (!currentStep.forceAlpha) currentStep.hiddenUI.Alpha = charCounter / 15f;
                }

                yield return new WaitForSeconds(0.025f);
            }

            if (currentStep.titleImage != null) SetFtueUIImageContainer(currentStep.titleImage, true, true);
            if (currentStep.hiddenUI != null) SetFtueUIImageContainer(currentStep.hiddenUI, true, true);

            ftueText.SetText(txt);
            receiveTouch = false;

            if (activeInput == FtueInputs.TOUCH)
            {
                if (currentStep.tapPosition != Vector2.zero) StartCoroutine(WaitForTouch(true, currentStep.tapPosition.x, currentStep.tapPosition.y));
                else StartCoroutine(WaitForTouch(true));
            }
        }

        private IEnumerator DisplayDrag(PointerFtueMove moveData)
        {
            int stepI = currentStepIndex;
            Vector3 pnjPos = Camera.main.WorldToScreenPoint(PlayerManager.Instance.GetNearestNPC().position);
            dragTarget.position = pnjPos;
            UIObjectPointer arrow = ArrowDisplayer.Instances("FtueTap").UseArrow<UIObjectPointer>(100f, -currentStep.pointRotation, false, clickTarget.transform as RectTransform, "FtueTap", false);
            arrow.SetAnimProperties(0f, 0f);
            float x = 0f;
            float t = 0f;
            while (currentStepIndex == stepI)
            {
                t = Mathf.Clamp(t + (Time.deltaTime * (1f / 1.2f)), 0f, 1f);
                x = Easing.CrossFade(Easing.SmoothStart, 3, Easing.SmoothStop, 3, t);
                clickTarget.transform.position = Vector3.Lerp(moveData.startTrf.position, dragTarget.position, x);
                if (t >= 1f) t = 0f;
                yield return null;
            }
            ArrowDisplayer.Instances("FtueTap").DestroyArrow(arrow);
        }

        public void Restart()
        {
            _finish = false;
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