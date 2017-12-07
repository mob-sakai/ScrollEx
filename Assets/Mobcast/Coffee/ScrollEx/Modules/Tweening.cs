using UnityEngine;
using System.Collections;


namespace Mobcast.Coffee
{
	public static class Tweening
	{
#region Public

		/// <summary>
		/// The easing type
		/// </summary>
		public enum TweenMethod
		{
			immediate,
			linear,
			spring,
			EaseInQuad,
			EaseOutQuad,
			EaseInOutQuad,
			EaseInCubic,
			EaseOutCubic,
			EaseInOutCubic,
			EaseInQuart,
			EaseOutQuart,
			EaseInOutQuart,
			EaseInQuint,
			EaseOutQuint,
			EaseInOutQuint,
			EaseInSine,
			EaseOutSine,
			EaseInOutSine,
			EaseInExpo,
			EaseOutExpo,
			EaseInOutExpo,
			EaseInCirc,
			EaseOutCirc,
			EaseInOutCirc,
			EaseInBounce,
			EaseOutBounce,
			EaseInOutBounce,
			EaseInBack,
			EaseOutBack,
			EaseInOutBack,
			EaseInElastic,
			EaseOutElastic,
			EaseInOutElastic
		}

		public static float GetTweenValue(TweenMethod tweenType, float startValue, float endValue, float val)
		{
			switch (tweenType)
			{
				case TweenMethod.linear:
					return _Linear(startValue, endValue, val);
				case TweenMethod.spring:
					return _Spring(startValue, endValue, val);
				case TweenMethod.EaseInQuad:
					return _EaseInQuad(startValue, endValue, val);
				case TweenMethod.EaseOutQuad:
					return _EaseOutQuad(startValue, endValue, val);
				case TweenMethod.EaseInOutQuad:
					return _EaseInOutQuad(startValue, endValue, val);
				case TweenMethod.EaseInCubic:
					return _EaseInCubic(startValue, endValue, val);
				case TweenMethod.EaseOutCubic:
					return _EaseOutCubic(startValue, endValue, val);
				case TweenMethod.EaseInOutCubic:
					return _EaseInOutCubic(startValue, endValue, val);
				case TweenMethod.EaseInQuart:
					return _EaseInQuart(startValue, endValue, val);
				case TweenMethod.EaseOutQuart:
					return _EaseOutQuart(startValue, endValue, val);
				case TweenMethod.EaseInOutQuart:
					return _EaseInOutQuart(startValue, endValue, val);
				case TweenMethod.EaseInQuint:
					return _EaseInQuint(startValue, endValue, val);
				case TweenMethod.EaseOutQuint:
					return _EaseOutQuint(startValue, endValue, val);
				case TweenMethod.EaseInOutQuint:
					return _EaseInOutQuint(startValue, endValue, val);
				case TweenMethod.EaseInSine:
					return _EaseInSine(startValue, endValue, val);
				case TweenMethod.EaseOutSine:
					return _EaseOutSine(startValue, endValue, val);
				case TweenMethod.EaseInOutSine:
					return _EaseInOutSine(startValue, endValue, val);
				case TweenMethod.EaseInExpo:
					return _EaseInExpo(startValue, endValue, val);
				case TweenMethod.EaseOutExpo:
					return _EaseOutExpo(startValue, endValue, val);
				case TweenMethod.EaseInOutExpo:
					return _EaseInOutExpo(startValue, endValue, val);
				case TweenMethod.EaseInCirc:
					return _EaseInCirc(startValue, endValue, val);
				case TweenMethod.EaseOutCirc:
					return _EaseOutCirc(startValue, endValue, val);
				case TweenMethod.EaseInOutCirc:
					return _EaseInOutCirc(startValue, endValue, val);
				case TweenMethod.EaseInBounce:
					return _EaseInBounce(startValue, endValue, val);
				case TweenMethod.EaseOutBounce:
					return _EaseOutBounce(startValue, endValue, val);
				case TweenMethod.EaseInOutBounce:
					return _EaseInOutBounce(startValue, endValue, val);
				case TweenMethod.EaseInBack:
					return _EaseInBack(startValue, endValue, val);
				case TweenMethod.EaseOutBack:
					return _EaseOutBack(startValue, endValue, val);
				case TweenMethod.EaseInOutBack:
					return _EaseInOutBack(startValue, endValue, val);
				case TweenMethod.EaseInElastic:
					return _EaseInElastic(startValue, endValue, val);
				case TweenMethod.EaseOutElastic:
					return _EaseOutElastic(startValue, endValue, val);
				case TweenMethod.EaseInOutElastic:
					return _EaseInOutElastic(startValue, endValue, val);
				default:
					return endValue;
			}
		}

#endregion Public

#region Private

		static float _Linear(float start, float end, float val)
		{
			return Mathf.Lerp(start, end, val);
		}

		static float _Spring(float start, float end, float val)
		{
			val = Mathf.Clamp01(val);
			val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
			return start + (end - start) * val;
		}

		static float _EaseInQuad(float start, float end, float val)
		{
			end -= start;
			return end * val * val + start;
		}

		static float _EaseOutQuad(float start, float end, float val)
		{
			end -= start;
			return -end * val * (val - 2) + start;
		}

		static float _EaseInOutQuad(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val + start;
			val--;
			return -end / 2 * (val * (val - 2) - 1) + start;
		}

		static float _EaseInCubic(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val + start;
		}

		static float _EaseOutCubic(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * (val * val * val + 1) + start;
		}

		static float _EaseInOutCubic(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val + start;
			val -= 2;
			return end / 2 * (val * val * val + 2) + start;
		}

		static float _EaseInQuart(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val + start;
		}

		static float _EaseOutQuart(float start, float end, float val)
		{
			val--;
			end -= start;
			return -end * (val * val * val * val - 1) + start;
		}

		static float _EaseInOutQuart(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val * val + start;
			val -= 2;
			return -end / 2 * (val * val * val * val - 2) + start;
		}

		static float _EaseInQuint(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val * val + start;
		}

		static float _EaseOutQuint(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * (val * val * val * val * val + 1) + start;
		}

		static float _EaseInOutQuint(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val * val * val + start;
			val -= 2;
			return end / 2 * (val * val * val * val * val + 2) + start;
		}

		static float _EaseInSine(float start, float end, float val)
		{
			end -= start;
			return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
		}

		static float _EaseOutSine(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
		}

		static float _EaseInOutSine(float start, float end, float val)
		{
			end -= start;
			return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
		}

		static float _EaseInExpo(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
		}

		static float _EaseOutExpo(float start, float end, float val)
		{
			end -= start;
			return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
		}

		static float _EaseInOutExpo(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
			val--;
			return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
		}

		static float _EaseInCirc(float start, float end, float val)
		{
			end -= start;
			return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
		}

		static float _EaseOutCirc(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * Mathf.Sqrt(1 - val * val) + start;
		}

		static float _EaseInOutCirc(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
			val -= 2;
			return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
		}

		static float _EaseInBounce(float start, float end, float val)
		{
			end -= start;
			float d = 1f;
			return end - _EaseOutBounce(0, end, d - val) + start;
		}

		static float _EaseOutBounce(float start, float end, float val)
		{
			val /= 1f;
			end -= start;
			if (val < (1 / 2.75f))
			{
				return end * (7.5625f * val * val) + start;
			}
			else if (val < (2 / 2.75f))
			{
				val -= (1.5f / 2.75f);
				return end * (7.5625f * (val) * val + .75f) + start;
			}
			else if (val < (2.5 / 2.75))
			{
				val -= (2.25f / 2.75f);
				return end * (7.5625f * (val) * val + .9375f) + start;
			}
			else
			{
				val -= (2.625f / 2.75f);
				return end * (7.5625f * (val) * val + .984375f) + start;
			}
		}

		static float _EaseInOutBounce(float start, float end, float val)
		{
			end -= start;
			float d = 1f;
			if (val < d / 2)
				return _EaseInBounce(0, end, val * 2) * 0.5f + start;
			else
				return _EaseOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
		}

		static float _EaseInBack(float start, float end, float val)
		{
			end -= start;
			val /= 1;
			float s = 1.70158f;
			return end * (val) * val * ((s + 1) * val - s) + start;
		}

		static float _EaseOutBack(float start, float end, float val)
		{
			float s = 1.70158f;
			end -= start;
			val = (val / 1) - 1;
			return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
		}

		static float _EaseInOutBack(float start, float end, float val)
		{
			float s = 1.70158f;
			end -= start;
			val /= .5f;
			if ((val) < 1)
			{
				s *= (1.525f);
				return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
			}
			val -= 2;
			s *= (1.525f);
			return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
		}

		static float _EaseInElastic(float start, float end, float val)
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (val == 0)
				return start;
			val = val / d;
			if (val == 1)
				return start + end;

			if (a == 0f || a < Mathf.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			val = val - 1;
			return -(a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
		}

		static float _EaseOutElastic(float start, float end, float val)
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (val == 0)
				return start;

			val = val / d;
			if (val == 1)
				return start + end;

			if (a == 0f || a < Mathf.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return (a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) + end + start);
		}

		static float _EaseInOutElastic(float start, float end, float val)
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (val == 0)
				return start;

			val = val / (d / 2);
			if (val == 2)
				return start + end;

			if (a == 0f || a < Mathf.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			if (val < 1)
			{
				val = val - 1;
				return -0.5f * (a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
			}
			val = val - 1;
			return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
		}

#endregion Private
	}
}