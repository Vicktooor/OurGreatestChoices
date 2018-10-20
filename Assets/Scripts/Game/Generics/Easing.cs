public static class Easing
{
    public delegate TResult EaseFunc<in T, out TResult>(T arg);
    public delegate TResult EaseFuncN<in T, in T2, out TResult>(T arg1, T2 arg2);

    /// <summary>
    /// Easing function range map using Fastest smooth function
    /// </summary>
    /// <param name="x">val</param>
    /// <param name="inStart">In range start</param>
    /// <param name="inEnd">In range end</param>
    /// <param name="outStart">Out range start</param>
    /// <param name="outEnd">out range end</param>
    /// <param name="easingFunc"></param>
    /// <returns></returns>
    public static float RangeMap(float val, float inStart, float inEnd, float outStart, float outEnd, EaseFunc<float, float> easingFunc)
    {
        float res = val - inStart;
        res /= (inEnd - inStart);
        res = easingFunc(res);
        res *= (outEnd - outStart);
        return res - outStart;
    }

    public static float SetInRange(float val, float inStart, float inEnd)
    {
        float res = val - inStart;
        res /= (inEnd - inStart);
        return res;
    }

    public static float SetOutRange(float res, float outStart, float outEnd)
    {
        res *= (outEnd - outStart);
        return res - outStart;
    }

    #region Normalized 1D non linear curve

    /// <summary>
    /// Smooth start transformation
    /// </summary>
    public static float SmoothStart(float x, int n)
    {
        float res = 0;
        for (int i = 0; i < n; i++) res = ((i == 0) ? x : res * x);
        return res;
    }

    /// <summary>
    /// Smooth stop transformation
    /// </summary>
    public static float SmoothStop(float x, int n)
    {
        return Flip(SmoothStart(Flip(x), n));
    }

    /// <summary>
    /// Arch transformation
    /// </summary>
    public static float Arch(float x)
    {
        return (4f * x) - (4f * (x * x));
    }

    /// <summary>
    /// Arch with smooth start
    /// </summary>
    public static float SmoothArchStart(float x, int n = 1)
    {
        if (n < 1) n = 1;
        return SmoothArchStartRatio(n) * ((4f * SmoothStart(x, n + 1)) - (4f * SmoothStart(x, n + 2)));
    }

    private static float SmoothArchStartRatio(int n)
    {
        float x = (n + 1f) / (n + 2f);
        return 1f / ((4f * SmoothStart(x, n + 1)) - (4f * SmoothStart(x, n + 2)));
    }

    /// <summary>
    /// Arch with smooth stop
    /// </summary>
    public static float SmoothArchStop(float x, int n = 1)
    {
        if (n < 1) n = 1;
        return SmoothArchStopRatio(n) * ((4f * x * SmoothStart(1f - x, n)) - (4f * SmoothStart(x, 2) * SmoothStart(1f - x, n)));
    }

    private static float SmoothArchStopRatio(int n)
    {
        float x = 1f - ((n + 1f) / (n + 2f));
        return 1f / ((4f * x * SmoothStart(1f - x, n)) - (4f * SmoothStart(x, 2) * SmoothStart(1f - x, n)));
    }

    /// <summary>
    /// Arch with smooth stop & start
    /// </summary>
    public static float SmoothArchStep(float x, int n)
    {
        if (n < 1) n = 1;
        return SmoothArchStepRatio(n) * (SmoothArchStart(x, n) * SmoothArchStop(x, n));
    }

    private static float SmoothArchStepRatio(int n)
    {
        return 1f / (SmoothArchStart(0.5f, n) * SmoothArchStop(0.5f, n));
    }

    #endregion

    /// <summary>
    /// Flip a normalized float
    /// </summary>
    /// <param name="val">In range [0,1]</param>
    /// <returns></returns>
    public static float Flip(float x) { return 1 - x; }

    public static float Scale(float x, float scale) { return scale * x; }
    public static float Scale(EaseFunc<float, float> func, float x, float scale) { return scale * func(x); }
    public static float Scale(EaseFuncN<float, int, float> func, float x, int n, float scale) { return scale * func(x, n); }

    public static float FlipScale(float x, float scale) { return scale * (1f - x); }
    public static float FlipScale(EaseFunc<float, float> func, float x, float scale) { return (1 - x) * func(x) * scale; }
    public static float FlipScale(EaseFuncN<float, int, float> func, float x, int n, float scale) { return (1 - x) * func(x, n) * scale; }

    /// <summary>
    /// SmoothStart^A.B = Mix(SmoothStartA, SmoothStartA+1, B)
    /// </summary>
    public static float Mix(EaseFunc<float, float> func1, EaseFunc<float, float> func2, float blend, float x)
    {
        return ((1f - blend) * func1(x)) + (blend * func2(x));
    }

    public static float Mix(EaseFunc<float, float> func1, EaseFuncN<float, int, float> funcN, int n, float blend, float x)
    {
        return ((1f - blend) * func1(x)) + (blend * funcN(x, n));
    }

    public static float Mix(EaseFuncN<float, int, float> funcN1, int n1, EaseFuncN<float, int, float> funcN2, int n2, float blend, float x)
    {
        return ((1f - blend) * funcN1(x, n1)) + (blend * funcN2(x, n2));
    }

    public static float Mix(float resFunc1, float resFunc2, float blend, float x)
    {
        return ((1f - blend) * resFunc1) + (blend * resFunc2);
    }

    /// <summary>
    /// To use with SmoothStart2 (exemple) & SmoothStop2 (exemple)
    /// </summary>
    public static float CrossFade(EaseFunc<float, float> func1, EaseFunc<float, float> func2, float x)
    {
        return ((1 - x) * func1(x)) + (x * func2(x));
    }

    public static float CrossFade(EaseFunc<float, float> func1, EaseFuncN<float, int, float> funcN, int n, float x)
    {
        return ((1 - x) * func1(x)) + (x * funcN(x, n));
    }

    public static float CrossFade(EaseFuncN<float, int, float> funcN1, int n1, EaseFuncN<float, int, float> funcN2, int n2, float x)
    {
        return ((1 - x) * funcN1(x, n1)) + (x * funcN2(x, n2));
    }

    public static float CrossFade(float resFunc1, float resFunc2, float blend, float x)
    {
        return ((1 - x) * resFunc1) + (x * resFunc2);
    }
}