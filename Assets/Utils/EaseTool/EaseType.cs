using System;
using UnityEngine;

namespace Utils.Ease
{
    /// <summary>
    /// 插值类型
    /// In: 从0开始加速;
    /// Out: 减速到0;
    /// lnOut:前半段从0开始加速，后半段减速到0的缓动
    /// 
    /// </summary>
    public enum EaseType
    {
        Unset,
        Linear, //线性匀速
        InSine, //正弦曲线(sin(t)) 
        OutSine,
        InOutSine,
        InQuad, //二次方曲线
        OutQuad,
        InOutQuad,
        InCubic, //三次方曲线
        OutCubic,
        InOutCubic,
        InQuart, //四次方曲线
        OutQuart,
        InOutQuart,
        InQuint, //五次方曲线
        OutQuint,
        InOutQuint,
        InExpo, //指数曲线(2At)
        OutExpo,
        InOutExpo,
        InCirc, //圆形曲线(sqrt(1-t^2))
        OutCirc,
        InOutCirc,
        InElastic, //指数衰减的正弦曲线
        OutElastic,
        InOutElastic,
        InBack, //超过范围的三次方缓动((s+1)tN3 - st^2)
        OutBack,
        InOutBack,
        InBounce, //指数衰减的反弹缓动
        OutBounce,
        InOutBounce,
        Flash, //闪烁效果
        InFlash,
        OutFlash,
        InOutFlash,

        /// <summary>
        /// Don't assign this! It's assigned automatically when creating 0 duration tweens
        /// </summary>
        INTERNAL_Zero,
    }

    public static class EaseExtension
    {
        private const double _PI_Half = 0.5 * Math.PI;
        private const double _2PI = 2 * Math.PI;
        private const double _PI = Math.PI;
        private const float _1 = 1;
        private const float _0_5 = 0.5f;
        private const float _2 = 2;

        private const float _6_283185 = 6.283185f;
        public static float Evaluate(this EaseType type, float time, float duration, float overshootOrAmplitude,
            float period)
        {
            switch (type)
            {
                case EaseType.Linear:
                    return time / duration;
                case EaseType.InSine:
                    return (float)(-Math.Cos(time / duration * _PI_Half) + _1);
                case EaseType.OutSine:
                    return (float)Math.Sin(time / duration * _PI_Half);
                case EaseType.InOutSine:
                    return (float)(-_0_5 * (Math.Cos(_PI * time / duration) - _1));
                case EaseType.InQuad:
                    return (time /= duration) * time;
                case EaseType.OutQuad:
                    return (float)(-(double)(time /= duration) * (time - _2));
                case EaseType.InOutQuad:
                    return (double)(time /= duration * (float)_0_5) < _1
                        ? (float)_0_5 * time * time
                        : (float)(-_0_5 * ((double)--time * ((double)time - _2) - _1));
                case EaseType.InCubic:
                    return (time /= duration) * time * time;
                case EaseType.OutCubic:
                    return (float)((double)(time = (float)((double)time / (double)duration - _1)) * (double)time *
                        (double)time + _1);
                case EaseType.InOutCubic:
                    return (double)(time /= duration * _0_5) < _1
                        ? _0_5 * time * time * time
                        : (float)(_0_5 * ((double)(time -= (float)_2) * (double)time * (double)time + _2));
                case EaseType.InQuart:
                    return (time /= duration) * time * time * time;
                case EaseType.OutQuart:
                    return (float)-((double)(time = (float)((double)time / (double)duration - _1)) * (double)time *
                        (double)time * (double)time - _1);
                case EaseType.InOutQuart:
                    return (double)(time /= duration * _0_5) < _1
                        ? _0_5 * time * time * time * time
                        : (float)(-_0_5 * ((double)(time -= _2) * (double)time * (double)time * (double)time - _2));
                case EaseType.InQuint:
                    return (time /= duration) * time * time * time * time;
                case EaseType.OutQuint:
                    return (float)((double)(time = (float)((double)time / (double)duration - _1)) * (double)time *
                        (double)time * (double)time * (double)time + _1);
                case EaseType.InOutQuint:
                    return (double)(time /= duration * _0_5) < _1
                        ? _0_5 * time * time * time * time * time
                        : (float)(_0_5 * ((double)(time -= _2) * (double)time * (double)time * (double)time *
                            (double)time + _2));
                case EaseType.InExpo:
                    return (double)time != 0.0
                        ? (float)Math.Pow(_2, 10.0 * ((double)time / (double)duration - _1))
                        : 0.0f;
                case EaseType.OutExpo:
                    return (double)time == (double)duration
                        ? 1f
                        : (float)(-Math.Pow(_2, -10.0 * (double)time / (double)duration) + _1);
                case EaseType.InOutExpo:
                    if ((double)time == 0.0) return 0.0f;
                    if ((double)time == (double)duration) return 1f;
                    return (double)(time /= duration * _0_5) < _1
                        ? _0_5 * (float)Math.Pow(_2, 10.0 * ((double)time - _1))
                        : (float)(_0_5 * (-Math.Pow(_2, -10.0 * (double)--time) + _2));
                case EaseType.InCirc:
                    return (float)-(Math.Sqrt(_1 - (double)(time /= duration) * (double)time) - _1);
                case EaseType.OutCirc:
                    return (float)Math.Sqrt(_1 - (double)(time = (float)((double)time / (double)duration - _1)) *
                        (double)time);
                case EaseType.InOutCirc:
                    return (double)(time /= duration * _0_5) < _1
                        ? (float)(-_0_5 * (Math.Sqrt(_1 - (double)time * (double)time) - _1))
                        : (float)(_0_5 * (Math.Sqrt(_1 - (double)(time -= _2) * (double)time) + _1));
                case EaseType.InElastic:
                    if ((double)time == 0.0) return 0.0f;
                    if ((double)(time /= duration) == _1) return 1f;
                    if ((double)period == 0.0) period = duration * 0.3f;
                    float num1;
                    if ((double)overshootOrAmplitude < _1)
                    {
                        overshootOrAmplitude = 1f;
                        num1 = period / 4f;
                    }
                    else num1 = period / 6.283185f * (float)Math.Asin(_1 / (double)overshootOrAmplitude);

                    return (float)-((double)overshootOrAmplitude * Math.Pow(_2, 10.0 * (double)--time) *
                                    Math.Sin(((double)time * (double)duration - (double)num1) * 6.28318548202515 /
                                             (double)period));
                case EaseType.OutElastic:
                    if ((double)time == 0.0) return 0.0f;
                    if ((double)(time /= duration) == _1) return 1f;
                    if ((double)period == 0.0) period = duration * 0.3f;
                    float num2;
                    if ((double)overshootOrAmplitude < _1)
                    {
                        overshootOrAmplitude = 1f;
                        num2 = period / 4f;
                    }
                    else num2 = period / 6.283185f * (float)Math.Asin(_1 / (double)overshootOrAmplitude);

                    return (float)((double)overshootOrAmplitude * Math.Pow(_2, -10.0 * (double)time) *
                                   Math.Sin(((double)time * (double)duration - (double)num2) * _6_283185 /
                                            (double)period) +
                                   _1);
                case EaseType.InOutElastic:
                    if ((double)time == 0.0) return 0.0f;
                    if ((double)(time /= duration * _0_5) == _2) return 1f;
                    if ((double)period == 0.0) period = duration * 0.45f;
                    float num3;
                    if ((double)overshootOrAmplitude < _1)
                    {
                        overshootOrAmplitude = 1f;
                        num3 = period / 4f;
                    }
                    else num3 = period / 6.283185f * (float)Math.Asin(_1 / (double)overshootOrAmplitude);

                    return (double)time < _1
                        ? (float)(-_0_5 * ((double)overshootOrAmplitude * Math.Pow(_2, 10.0 * (double)--time) *
                                           Math.Sin(((double)time * (double)duration - (double)num3) *
                                               _6_283185 / (double)period)))
                        : (float)((double)overshootOrAmplitude * Math.Pow(_2, -10.0 * (double)--time) *
                            Math.Sin(((double)time * (double)duration - (double)num3) * _6_283185 /
                                     (double)period) * _0_5 + _1);
                case EaseType.InBack:
                    return (float)((double)(time /= duration) * (double)time *
                                   (((double)overshootOrAmplitude + _1) * (double)time - (double)overshootOrAmplitude));
                case EaseType.OutBack:
                    return (float)((double)(time = (float)((double)time / (double)duration - _1)) * (double)time *
                        (((double)overshootOrAmplitude + _1) * (double)time + (double)overshootOrAmplitude) + _1);
                case EaseType.InOutBack:
                    return (double)(time /= duration * _0_5) < _1
                        ? (float)(_0_5 * ((double)time * (double)time *
                                          (((double)(overshootOrAmplitude *= 1.525f) + _1) * (double)time -
                                           (double)overshootOrAmplitude)))
                        : (float)(_0_5 * ((double)(time -= _2) * (double)time *
                            (((double)(overshootOrAmplitude *= 1.525f) + _1) * (double)time +
                             (double)overshootOrAmplitude) + _2));
                case EaseType.InBounce:
                    return Bounce.EaseIn(time, duration, overshootOrAmplitude, period);
                case EaseType.OutBounce:
                    return Bounce.EaseOut(time, duration, overshootOrAmplitude, period);
                case EaseType.InOutBounce:
                    return Bounce.EaseInOut(time, duration, overshootOrAmplitude, period);
                case EaseType.Flash:
                    return Flash.Ease(time, duration, overshootOrAmplitude, period);
                case EaseType.InFlash:
                    return Flash.EaseIn(time, duration, overshootOrAmplitude, period);
                case EaseType.OutFlash:
                    return Flash.EaseOut(time, duration, overshootOrAmplitude, period);
                case EaseType.InOutFlash:
                    return Flash.EaseInOut(time, duration, overshootOrAmplitude, period);
                case EaseType.INTERNAL_Zero:
                    return 1f;
                default:
                    return (float)(-(double)(time /= duration) * ((double)time - _2));
            }
        }

        public static class Flash
        {
            public static float Ease(float time, float duration, float overshootOrAmplitude, float period)
            {
                int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
                float stepDuration = duration / overshootOrAmplitude;
                time -= stepDuration * (float)(stepIndex - _1);
                float dir = stepIndex % 2 != 0 ? _1 : -_1;
                if (dir < 0) time -= stepDuration;
                float res = time * dir / stepDuration;
                return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
            }

            public static float EaseIn(float time, float duration, float overshootOrAmplitude, float period)
            {
                int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
                float stepDuration = duration / overshootOrAmplitude;
                time -= stepDuration * (float)(stepIndex - 1);
                float dir = stepIndex % 2 != 0 ? _1 : -_1;
                if (dir < 0) time -= stepDuration;
                time *= dir;
                float res = (time /= stepDuration) * time;
                return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
            }

            public static float EaseOut(float time, float duration, float overshootOrAmplitude, float period)
            {
                int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
                float stepDuration = duration / overshootOrAmplitude;
                time -= stepDuration * (float)(stepIndex - 1);
                float dir = stepIndex % 2 != 0 ? _1 : -_1;
                if (dir < 0) time -= stepDuration;
                time *= dir;
                float res = (float)(-(double)(time /= stepDuration) * ((double)time - _2));
                return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
            }

            public static float EaseInOut(float time, float duration, float overshootOrAmplitude, float period)
            {
                int stepIndex = Mathf.CeilToInt(time / duration * overshootOrAmplitude);
                float stepDuration = duration / overshootOrAmplitude;
                time -= stepDuration * (float)(stepIndex - 1);
                float dir = stepIndex % 2 != 0 ? _1 : -_1;
                if (dir < 0) time -= stepDuration;
                time *= dir;
                float res = (double)(time /= stepDuration * _0_5) < _1
                    ? _0_5 * time * time
                    : (float)(-_0_5 * ((double)--time * ((double)time - _2) - _1));
                return Flash.WeightedEase(overshootOrAmplitude, period, stepIndex, stepDuration, dir, res);
            }

            private static float WeightedEase(float overshootOrAmplitude, float period, int stepIndex,
                float stepDuration, float dir, float res)
            {
                float num1 = 0;
                float num2 = 0;
                if ((double)dir > 0 && (int)overshootOrAmplitude % 2 == 0)
                    ++stepIndex;
                else if ((double)dir < 0 && (int)overshootOrAmplitude % 2 != 0)
                    ++stepIndex;
                if ((double)period > 0)
                {
                    float num3 = (float)Math.Truncate((double)overshootOrAmplitude);
                    float num4 = overshootOrAmplitude - num3;
                    if ((double)num3 % 2 > 0) num4 = 1f - num4;
                    num2 = num4 * (float)stepIndex / overshootOrAmplitude;
                    num1 = res * (overshootOrAmplitude - (float)stepIndex) / overshootOrAmplitude;
                }
                else if ((double)period < 0)
                {
                    period = -period;
                    num1 = res * (float)stepIndex / overshootOrAmplitude;
                }

                float num5 = num1 - res;
                res += num5 * period + num2;
                if ((double)res > 1) res = _1;
                return res;
            }
        }

        /// <summary>
        /// This class contains a C# port of the easing equations created by Robert Penner (http://robertpenner.com/easing).
        /// </summary>
        public static class Bounce
        {
            private const float Bounce_0 = 0.363636f;
            private const float Bounce_1 = 0.727272f;
            private const float Bounce_2 = 0.909090f;

            private const float Bounce_3 = 121f / 16f;
            private const float Bounce_4 = 0.545455f;
            private const float Bounce_5 = 0.818182f;

            private const float Bounce_6 = 0.75f;

            private const float Bounce_7 = 15f / 16f;
            private const float Bounce_8 = 0.954545f;
            private const float Bounce_9 = 63f / 64f;


            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
            /// 弹跳(指数衰减抛物线弹跳)的减速方程函数:从零速度加速。
            /// </summary>
            /// <param name="time">Current time (in frames or seconds).</param>
            /// <param name="duration"> Expected easing duration (in frames or seconds). </param>
            /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
            /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
            /// <returns>The eased value.</returns>
            public static float EaseIn(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod)
            {
                return _1 - Bounce.EaseOut(duration - time, duration, -_1, -_1);
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
            /// 弹跳(指数衰减抛物线弹跳)的缓和方程函数缓和:从零速度减速。
            /// 斐波那契
            /// </summary>
            /// <param name="time">Current time (in frames or seconds).</param>
            /// <param name="duration"> Expected easing duration (in frames or seconds). </param>
            /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
            /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
            /// <returns>The eased value.</returns>
            public static float EaseOut(float time, float duration, float unusedOvershootOrAmplitude,
                float unusedPeriod)
            {
                if ((double)(time /= duration) < Bounce_0) return Bounce_3 * time * time;
                if ((double)time < Bounce_1)
                    return (float)(Bounce_3 * (double)(time -= Bounce_4) * (double)time + Bounce_6);
                return (double)time < Bounce_2
                    ? (float)(Bounce_3 * (double)(time -= Bounce_5) * (double)time + Bounce_7)
                    : (float)(Bounce_3 * (double)(time -= Bounce_8) * (double)time + Bounce_9);
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
            /// 弹跳(指数衰减抛物线弹跳)的缓和方程函数:加速到中途，然后减速。
            /// 斐波那契
            /// </summary>
            /// <param name="time">Current time (in frames or seconds).</param>
            /// <param name="duration">
            /// Expected easing duration (in frames or seconds).
            /// </param>
            /// <param name="unusedOvershootOrAmplitude">Unused: here to keep same delegate for all ease types.</param>
            /// <param name="unusedPeriod">Unused: here to keep same delegate for all ease types.</param>
            /// <returns>The eased value.</returns>
            public static float EaseInOut(float time, float duration, float unusedOvershootOrAmplitude,
                float unusedPeriod)
            {
                return (double)time < (double)duration * _0_5
                    ? Bounce.EaseIn(time * 2, duration, -1, -1) * _0_5
                    : (float)((double)Bounce.EaseOut(time * 2 - duration, duration, -1, -1) * _0_5 + _0_5);
            }
        }

    }
}
