using Assets.Scripts.Game;
using Assets.Scripts.Utils;
using UnityEngine;

public class ObjectArray<T> where T : Object
{
	private T[] _array = new T[0];
	public T[] Objs { get { return _array; } }

	protected int _length;
	public int Length { get { return _length; } }

	public void Add(T pObj)
	{
		if (pObj == null) return;

		T[] newArray;
		if (_array.Length == 0) newArray = new T[1] { pObj };
		else
		{
			newArray = new T[_array.Length + 1];
			for (int i = 0; i < _array.Length; i++)
			{
				newArray[i] = _array[i];
			}
			newArray[newArray.Length - 1] = pObj;
		}
		_array = newArray;
		_length = newArray.Length;
	}

	public void Remove(T pObj)
	{
		if (pObj == null) return;
		if (_array.Length == 0) return;
		T[] newArray = new T[_array.Length - 1];
		int index = -1;
		for (int i = 0; i < _array.Length; i++)
		{
			if (pObj.Equals(_array[i]))
			{
				index = i;
				break;
			}
		}

		if (index < 0) return;

		bool insert = false;
		for (int i = 0; i < newArray.Length; i++)
		{
			if (i == index) insert = true;

			if (!insert) newArray[i] = _array[i];
			else newArray[i] = _array[i + 1];
		}
		_array = newArray;
		_length = newArray.Length;
	}

	public void CheckIt()
	{
		int iteCheck = Mathf.RoundToInt(_array.Length / 2f);
		int[] newArray = new int[_array.Length];
		for (int i = 0; i < iteCheck; i++)
		{
			T SwapObj;
			int rIndex1 = Random.Range(0, _array.Length);
			int rIndex2 = Random.Range(0, _array.Length);

			if (rIndex1 == rIndex2) i--;
			else
			{
				SwapObj = _array[rIndex1];
				_array[rIndex1] = _array[rIndex2];
				_array[rIndex2] = SwapObj;
			}
		}
	}

    public void CleanDoublon()
    {
        ObjectArray<T> tArray = new ObjectArray<T>();

        for (int i = 0; i < _array.Length; i++)
        {
            if (!tArray.Contains(_array[i])) tArray.Add(_array[i]);
        }

        _array = tArray.Objs;
        _length = tArray.Length;
    }

	public bool Contains(T pObj)
	{
		for (int i = 0; i < _array.Length; i++)
		{
			if (_array[i].Equals(pObj)) return true;
		}
		return false;
	}

    public void Clear()
    {
        _array = new T[0];
        _length = 0;
    }
}


/// <summary>
/// 
/// </summary>
public class Culling<T> where T : BaseObject
{
    private Transform _camTransform;
    private GameCamera _gc;

	static Culling<T> instanceInternal = null;
	public static Culling<T> Instance
	{
		get
		{
			if (instanceInternal == null)
			{
				instanceInternal = new Culling<T>();
			}

			return instanceInternal;
		}
	}

	public float nearCullingZoomAngle = 150f;
	public float farCullingZoomAngle = 75f;
	private float cullingMapAngle = 90;

	protected ObjectArray<T> Objects = new ObjectArray<T>();

	public void Update()
	{
        if (!_camTransform)
        {
            _gc = CameraManager.Instance.GameCamera;
            if (_gc) _camTransform = _gc.transform;
            else _camTransform = Camera.main.transform;
            if (!_gc) return;
        }

		T[] objs = Objects.Objs;
		for (int i = 0; i < Objects.Length; i++)
		{
			float lAngle = AngleInDeg(objs[i].Position, _camTransform.forward);

            if (_gc.mode == ECameraTargetType.ZOOM)
            {
                if ((lAngle < farCullingZoomAngle) || (lAngle > nearCullingZoomAngle))
                    objs[i].Cull(false);
                else objs[i].Cull(true);
            }
            else
            {
                if (lAngle < cullingMapAngle)
                    objs[i].Cull(false);
                else objs[i].Cull(true);
            }
        }
	}

	public void Add(T pObj)
	{
		Objects.Add(pObj);
	}

	public void Remove(T pObj)
	{
		Objects.Remove(pObj);
	}

    public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
    {
        float dot = Vector3.Dot(vec1, vec2);
        float sqrVectMagn = (vec1.sqrMagnitude * vec2.sqrMagnitude);
        dot = dot / Mathf.Sqrt(sqrVectMagn);
        float acos = Mathf.Acos(dot);
        return (acos * 180) / Mathf.PI;
    }
}
