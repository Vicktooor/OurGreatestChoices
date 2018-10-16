using UnityEngine;

// AUTHOR - Victor

[System.Serializable]
public struct Matrix3x3
{
    public float m00;
    public float m01;
    public float m02;

    public float m10;
    public float m11;
    public float m12;

    public float m20;
    public float m21;
    public float m22;
}

/// <summary>
/// Need to be initialise in degree
/// </summary>
public struct CoupleAngleValue
{
    public float positiveAngle;
    public float negativeAngle;   

    public CoupleAngleValue(float cAngle)
    {
        positiveAngle = cAngle;
        negativeAngle = 360 - cAngle;
    }

    public void ToRadian()
    {
        positiveAngle = positiveAngle * Mathf.Deg2Rad;
        negativeAngle = negativeAngle * Mathf.Deg2Rad;   
    }

    public void ToDegree()
    {
        positiveAngle = negativeAngle * Mathf.Rad2Deg;
        negativeAngle = negativeAngle * Mathf.Rad2Deg;
    }
}

public class MathCustom
{
    /// <summary>
    /// Calcul the a,b,c & d values of the plan equation
    /// </summary>
    public static Vector4 GetPlaneValues(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 AB = B - A;
        Vector3 AC = C - A;

        float x = (AB.y * AC.z) - (AC.y * AB.z);
        float y = (AB.z * AC.x) - (AC.z * AB.x);
        float z = (AB.x * AC.y) - (AC.x * AB.y);
        Vector3 n = new Vector3(x, y, z);

        float d = (A.x * n.x) + (A.y * n.y) + (A.z * n.z);

        return new Vector4(x, y, z, d);
    }

	/// <summary>
	/// Calcul the position of the intersection between plan & vector
	/// </summary>
	/// <param name="A">Vector origin</param>
	/// <param name="B">Vector direction</param>
	/// <param name="planValues">Values a,b,c & d of plan equation</param>
	public static Vector3 LineCutPlaneCoordinates(Vector3 A, Vector3 B, Vector4 planValues)
    {
        Vector3 AB = B - A;

        float numerator = (planValues.x * A.x) + (planValues.y * A.y) + (planValues.z * A.z) - planValues.w;
        float denominator = - (planValues.x * AB.x) - (planValues.y * AB.y) - (planValues.z * AB.z);
        float t = numerator / denominator;
        
        return new Vector3(A.x + (AB.x * t), A.y + (AB.y * t), A.z + (AB.z * t));
    }

    /// <summary>
    /// Calcul the distance between point & plan
    /// </summary>
    /// <param name="getAbs">Want you to get absolute value ?</param>
    /// <returns></returns>
    public static float GetDistanceToPlane(Vector3 point, Vector4 planValues, bool getAbs = false)
    {
        float numerator;
        float denominator;

        if (!getAbs) numerator = (planValues.x * point.x) + (planValues.y * point.y) + (planValues.z * point.z) - planValues.w;
        else numerator = Mathf.Abs((planValues.x * point.x) + (planValues.y * point.y) + (planValues.z * point.z) - planValues.w);
        denominator = Mathf.Sqrt(Mathf.Pow(planValues.x, 2) + Mathf.Pow(planValues.y, 2) + Mathf.Pow(planValues.z, 2));

        return numerator / denominator;
    }

    public static Vector3 RotateDirectionAround(Vector3 origin, float theta, Vector3 axis)
    {
        float c = Mathf.Cos(theta);
        float s = Mathf.Sin(theta);

        Matrix3x3 transformMatrix = new Matrix3x3();

        transformMatrix.m00 = Mathf.Pow(axis.x, 2) + (1 - Mathf.Pow(axis.x, 2)) * c;
        transformMatrix.m01 = (axis.x * axis.y * (1 - c)) - (axis.z * s);
        transformMatrix.m02 = (axis.x * axis.z * (1 - c)) + (axis.y * s);

        transformMatrix.m10 = (axis.x * axis.y * (1 - c)) + (axis.z * s);
        transformMatrix.m11 = Mathf.Pow(axis.y, 2) + (1 - Mathf.Pow(axis.y, 2)) * c;
        transformMatrix.m12 = (axis.y * axis.z * (1 - c)) - (axis.x * s);

        transformMatrix.m20 = (axis.x * axis.z * (1 - c)) - (axis.y * s);
        transformMatrix.m21 = (axis.y * axis.z * (1 - c)) + (axis.x * s);
        transformMatrix.m22 = Mathf.Pow(axis.z, 2) + (1 - Mathf.Pow(axis.z, 2)) * c;

        return Vector3TransformFromMatrix(origin, transformMatrix);
    }

    private static Vector3 Vector3TransformFromMatrix(Vector3 v, Matrix3x3 m)
    {
        Vector3 result = Vector3.zero;

        result.x = (v.x * m.m00) + (v.y * m.m10) + (v.z * m.m20);
        result.y = (v.x * m.m01) + (v.y * m.m11) + (v.z * m.m21);
        result.z = (v.x * m.m02) + (v.y * m.m12) + (v.z * m.m22);

        return result;
    }

    public static Vector3 GetBarycenter(Vector3[] pointsCloud)
	{
		float numX = 0;
		float numY = 0;
		float numZ = 0;
		float den = 0;

		foreach (Vector3 V in pointsCloud)
		{
			numX += V.x * V.magnitude;
			numY += V.y * V.magnitude;
			numZ += V.z * V.magnitude;
			den += V.magnitude;
		}

		return new Vector3(numX / den, numY / den, numZ / den);
	}

	public static void SphericalToCartesian(float radius, float polar, float elevation, out Vector3 outCart)
	{
		float a = radius * Mathf.Cos(elevation);
		outCart.x = a * Mathf.Cos(polar);
		outCart.y = radius * Mathf.Sin(elevation);
		outCart.z = a * Mathf.Sin(polar);
	}

	public static void CartesianToSpherical(Vector3 cartCoords, out float outRadius, out float outPolar, out float outElevation)
	{
		if (cartCoords.x == 0) cartCoords.x = Mathf.Epsilon;
		outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x) + (cartCoords.y * cartCoords.y) + (cartCoords.z * cartCoords.z));
		outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
		if (cartCoords.x < 0) outPolar += Mathf.PI;
		outElevation = Mathf.Asin(cartCoords.y / outRadius);
	}

	/// <summary>
	/// Calcul normal vector of 3 points forming face
	/// </summary>
	public static Vector3 GetFaceNormalVector(Vector3 A, Vector3 B, Vector3 C)
	{
		Vector3 AB = A + B;
		Vector3 AC = A + C;

		return Vector3.Cross(AB, AC);
	}

	public static bool RandomBool() {
        return Random.value > 0.5;
    }

    /// <summary>
    /// Boucing lerp with elasticity
    /// </summary>
    /// <param name="bounce">Elasticity</param>
    /// <returns></returns>
    public static float Berp(float start, float end, float bounce, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + bounce * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }


    /// <summary>
    /// Circular lerp, avoid 0°/360° lerp problem (euler angles for ex)
    /// </summary>
    /// <returns></returns>
    public static float Clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);
        float retval = 0.0f;
        float diff = 0.0f;

        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;

        Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
        return retval;
    }

    /// <summary>
    /// Normalize val from [inStart, inEnd] to [outStart, outEnd]
    /// </summary>
    /// <returns></returns>
    public static float NormalizeRange(float val, float inStart, float inEnd, float outStart, float outEnd)
    {
        float res = val - inStart;
        res /= (inEnd - inStart);
        res *= (outEnd - outStart);
        return res - outStart;
    }
}