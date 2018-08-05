using UnityEngine;
using Assets.Scripts.Utils;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class CameraOrbital : CameraCC
{
    public Transform target;

    public float distance = 17.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = 5.5f;
    public float distanceMax = 20f;

    float x = 0.0f;
    float y = 0.0f;

    protected void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;	
	}

	protected override void OnInputKey(OnEdtionInputKeyEvent e)
	{
		if (e.key == KeyCode.S &&Input.GetMouseButtonDown(0)) CameraManager.Instance.ActiveCamera(ECamera.CIRCULAR);
	}

	protected override void OnScroll(OnEditionMouseScroll e)
	{
		distance -= e.value * (distance / 3f);
		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
	}

	protected override void OnRightClick(OnEditionRightClick e)
	{
		x += e.posX * xSpeed * distance * 0.02f;
		y -= e.posY * ySpeed * 0.02f;

		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}

	protected void LateUpdate()
	{
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		Vector3 position = rotation * negDistance + target.position;

		transform.rotation = rotation;
		transform.position = position;
	}

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}