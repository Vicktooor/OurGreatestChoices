using Assets.Scripts.Game;
using Assets.Scripts.Utils;
using UnityEngine;

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

		/*T[] objs = Objects.Objs;
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
        }*/
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
