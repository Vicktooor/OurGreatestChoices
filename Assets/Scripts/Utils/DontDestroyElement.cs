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

    private ObjectArray<GameObject> _gos = new ObjectArray<GameObject>();
    private Dictionary<GameObject, DontDestoyStruct> _states = new Dictionary<GameObject, DontDestoyStruct>();

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        Events.Instance.AddListener<OnGoToMenu>(GoToMenu);
    }

    private void Start()
    {
        _gos = new ObjectArray<GameObject>();

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
        foreach (NamedObject<GameObject> go in _gos.Objs)
        {
            if (go.obj.GetComponent<DynamicUI>() != null) continue;
            go.obj.SetActive(_states[go.obj].state);
            go.obj.transform.position = _states[go.obj].position;
            go.obj.transform.rotation = _states[go.obj].rotation;
        }
    }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<OnGoToMenu>(GoToMenu);
    }
}
