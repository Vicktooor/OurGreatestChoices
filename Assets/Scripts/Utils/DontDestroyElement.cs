using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DontDestoyStruct
{
    public bool state;
    public Quaternion rotation;
    public Vector3 position;

    public DontDestoyStruct(bool pState, Vector3 pPos, Quaternion pRot)
    {
        state = pState;
        rotation = pRot;
        position = pPos;
    }
}

public class DontDestroyElement : MonoBehaviour {

    private List<GameObject> _gos = new List<GameObject>();
    private Dictionary<GameObject, DontDestoyStruct> _states = new Dictionary<GameObject, DontDestoyStruct>();

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        Events.Instance.AddListener<OnGoToMenu>(GoToMenu);
    }

    private void Start()
    {
        _gos = new List<GameObject>();

        _states.Add(gameObject, new DontDestoyStruct(gameObject.activeSelf, gameObject.transform.position, gameObject.transform.rotation));
        _gos.Add(gameObject);
        GetChildStates(transform);
    }

    private void GetChildStates(Transform lTransform)
    {
        if (lTransform.childCount > 0)
        {
            for (int i = 0; i < lTransform.childCount; i++)
            {
                Transform lChild = lTransform.GetChild(i);
                _states.Add(lChild.gameObject, new DontDestoyStruct(lChild.gameObject.activeSelf, lChild.position, lChild.rotation));
                _gos.Add(lChild.gameObject);

                GetChildStates(lChild.transform);
            }
        }
    }

    private void GoToMenu(OnGoToMenu e)
    {
        Reset();
    }

    private void Reset()
    {
        foreach (GameObject go in _gos)
        {
            if (go.GetComponent<DynamicUI>() != null) continue;
            go.SetActive(_states[go].state);
            go.transform.position = _states[go].position;
            go.transform.rotation = _states[go].rotation;
        }
    }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<OnGoToMenu>(GoToMenu);
    }
}
