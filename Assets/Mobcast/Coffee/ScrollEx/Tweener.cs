using UnityEngine;
using System.Collections;


namespace Mobcast.Coffee
{
	public static class Tweening
	{
		/// <summary>
		/// The easing type
		/// </summary>
		public enum Method
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

		public static float GetTweenValue(Method tweenType, float startValue, float endValue, float val)
		{
			switch (tweenType)
			{
				case Method.linear:
					return Linear(startValue, endValue, val);
				case Method.spring:
					return Spring(startValue, endValue, val);
				case Method.EaseInQuad:
					return EaseInQuad(startValue, endValue, val);
				case Method.EaseOutQuad:
					return EaseOutQuad(startValue, endValue, val);
				case Method.EaseInOutQuad:
					return EaseInOutQuad(startValue, endValue, val);
				case Method.EaseInCubic:
					return EaseInCubic(startValue, endValue, val);
				case Method.EaseOutCubic:
					return EaseOutCubic(startValue, endValue, val);
				case Method.EaseInOutCubic:
					return EaseInOutCubic(startValue, endValue, val);
				case Method.EaseInQuart:
					return EaseInQuart(startValue, endValue, val);
				case Method.EaseOutQuart:
					return EaseOutQuart(startValue, endValue, val);
				case Method.EaseInOutQuart:
					return EaseInOutQuart(startValue, endValue, val);
				case Method.EaseInQuint:
					return EaseInQuint(startValue, endValue, val);
				case Method.EaseOutQuint:
					return EaseOutQuint(startValue, endValue, val);
				case Method.EaseInOutQuint:
					return EaseInOutQuint(startValue, endValue, val);
				case Method.EaseInSine:
					return EaseInSine(startValue, endValue, val);
				case Method.EaseOutSine:
					return EaseOutSine(startValue, endValue, val);
				case Method.EaseInOutSine:
					return EaseInOutSine(startValue, endValue, val);
				case Method.EaseInExpo:
					return EaseInExpo(startValue, endValue, val);
				case Method.EaseOutExpo:
					return EaseOutExpo(startValue, endValue, val);
				case Method.EaseInOutExpo:
					return EaseInOutExpo(startValue, endValue, val);
				case Method.EaseInCirc:
					return EaseInCirc(startValue, endValue, val);
				case Method.EaseOutCirc:
					return EaseOutCirc(startValue, endValue, val);
				case Method.EaseInOutCirc:
					return EaseInOutCirc(startValue, endValue, val);
				case Method.EaseInBounce:
					return EaseInBounce(startValue, endValue, val);
				case Method.EaseOutBounce:
					return EaseOutBounce(startValue, endValue, val);
				case Method.EaseInOutBounce:
					return EaseInOutBounce(startValue, endValue, val);
				case Method.EaseInBack:
					return EaseInBack(startValue, endValue, val);
				case Method.EaseOutBack:
					return EaseOutBack(startValue, endValue, val);
				case Method.EaseInOutBack:
					return EaseInOutBack(startValue, endValue, val);
				case Method.EaseInElastic:
					return EaseInElastic(startValue, endValue, val);
				case Method.EaseOutElastic:
					return EaseOutElastic(startValue, endValue, val);
				case Method.EaseInOutElastic:
					return EaseInOutElastic(startValue, endValue, val);
				default:
					return endValue;
			}
		}


		static float Linear(float start, float end, float val)
		{
			return Mathf.Lerp(start, end, val);
		}

		static float Spring(float start, float end, float val)
		{
			val = Mathf.Clamp01(val);
			val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
			return start + (end - start) * val;
		}

		static float EaseInQuad(float start, float end, float val)
		{
			end -= start;
			return end * val * val + start;
		}

		static float EaseOutQuad(float start, float end, float val)
		{
			end -= start;
			return -end * val * (val - 2) + start;
		}

		static float EaseInOutQuad(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val + start;
			val--;
			return -end / 2 * (val * (val - 2) - 1) + start;
		}

		static float EaseInCubic(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val + start;
		}

		static float EaseOutCubic(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * (val * val * val + 1) + start;
		}

		static float EaseInOutCubic(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val + start;
			val -= 2;
			return end / 2 * (val * val * val + 2) + start;
		}

		static float EaseInQuart(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val + start;
		}

		static float EaseOutQuart(float start, float end, float val)
		{
			val--;
			end -= start;
			return -end * (val * val * val * val - 1) + start;
		}

		static float EaseInOutQuart(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val * val + start;
			val -= 2;
			return -end / 2 * (val * val * val * val - 2) + start;
		}

		static float EaseInQuint(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val * val + start;
		}

		static float EaseOutQuint(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * (val * val * val * val * val + 1) + start;
		}

		static float EaseInOutQuint(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * val * val * val * val * val + start;
			val -= 2;
			return end / 2 * (val * val * val * val * val + 2) + start;
		}

		static float EaseInSine(float start, float end, float val)
		{
			end -= start;
			return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
		}

		static float EaseOutSine(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
		}

		static float EaseInOutSine(float start, float end, float val)
		{
			end -= start;
			return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
		}

		static float EaseInExpo(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
		}

		static float EaseOutExpo(float start, float end, float val)
		{
			end -= start;
			return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
		}

		static float EaseInOutExpo(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
			val--;
			return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
		}

		static float EaseInCirc(float start, float end, float val)
		{
			end -= start;
			return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
		}

		static float EaseOutCirc(float start, float end, float val)
		{
			val--;
			end -= start;
			return end * Mathf.Sqrt(1 - val * val) + start;
		}

		static float EaseInOutCirc(float start, float end, float val)
		{
			val /= .5f;
			end -= start;
			if (val < 1)
				return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
			val -= 2;
			return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
		}

		static float EaseInBounce(float start, float end, float val)
		{
			end -= start;
			float d = 1f;
			return end - EaseOutBounce(0, end, d - val) + start;
		}

		static float EaseOutBounce(float start, float end, float val)
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

		static float EaseInOutBounce(float start, float end, float val)
		{
			end -= start;
			float d = 1f;
			if (val < d / 2)
				return EaseInBounce(0, end, val * 2) * 0.5f + start;
			else
				return EaseOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
		}

		static float EaseInBack(float start, float end, float val)
		{
			end -= start;
			val /= 1;
			float s = 1.70158f;
			return end * (val) * val * ((s + 1) * val - s) + start;
		}

		static float EaseOutBack(float start, float end, float val)
		{
			float s = 1.70158f;
			end -= start;
			val = (val / 1) - 1;
			return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
		}

		static float EaseInOutBack(float start, float end, float val)
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

		static float EaseInElastic(float start, float end, float val)
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

		static float EaseOutElastic(float start, float end, float val)
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

		static float EaseInOutElastic(float start, float end, float val)
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
	}
}