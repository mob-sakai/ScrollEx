using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Mobcast.Coffee;
using UnityEngine.EventSystems;

public class ScrollTweener : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IScrollHandler
{
	bool m_IsDragging;
	int m_ScrollCount;
	Coroutine m_CoTweening;

	public void OnScroll(PointerEventData eventData)
	{
		StopSnapping();
//		StopTweening ();
		m_ScrollCount = 10;
//		IsDragging = true;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		StopSnapping();
//		StopTweening ();
		m_ScrollCount = 0;
		m_IsDragging = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		StopSnapping();
		m_ScrollCount = 0;
		m_IsDragging = false;
	}

	/// <summary>
	/// The easing type
	/// </summary>
	public enum TweenType
	{
		immediate,
		linear,
		spring,
		easeInQuad,
		easeOutQuad,
		easeInOutQuad,
		easeInCubic,
		easeOutCubic,
		easeInOutCubic,
		easeInQuart,
		easeOutQuart,
		easeInOutQuart,
		easeInQuint,
		easeOutQuint,
		easeInOutQuint,
		easeInSine,
		easeOutSine,
		easeInOutSine,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		easeInCirc,
		easeOutCirc,
		easeInOutCirc,
		easeInBounce,
		easeOutBounce,
		easeInOutBounce,
		easeInBack,
		easeOutBack,
		easeInOutBack,
		easeInElastic,
		easeOutElastic,
		easeInOutElastic
	}

	//	Coroutine m_CoTweening;


	//	public ScrollRect scrollRect
	//	{
	//		get
	//		{
	//			if (!m_ScrollRect)
	//				m_ScrollRect = GetComponent<ScrollRect>();
	//			return m_ScrollRect;
	//		}
	//	}

	ScrollRect m_ScrollRect;

	//	public ScrollRectEx scrollRectEx
	//	{
	//		get
	//		{
	//			if (!m_ScrollRect)
	//				m_ScrollRect = GetComponent<ScrollRectEx>();
	//			return m_ScrollRect;
	//		}
	//	}

	ScrollRectEx m_ScrollRectEx;

	// <summary>
	/// The linear velocity is the velocity on one axis.
	/// The scroller should only be moving one one axix.
	/// </summary>
	public float LinearVelocity
	{
		get { return (m_ScrollRect.vertical ? m_ScrollRect.velocity.y : m_ScrollRect.velocity.x); }
		set
		{
			// set the appropriate component of the velocity
			if (m_ScrollRect.vertical)
			{
				m_ScrollRect.velocity = new Vector2(0, value);
			}
			else
			{
				m_ScrollRect.velocity = new Vector2(value, 0);
			}
		}
	}

	void Awake()
	{
		m_ScrollRect = GetComponent<ScrollRect>();
		m_ScrollRectEx = GetComponent<ScrollRectEx>();
	}

	void Update()
	{
		if (canSnap && Mathf.Abs(m_ScrollRect.vertical ? m_ScrollRect.velocity.y : m_ScrollRect.velocity.x) <= th)
		{
			Snap();
		}
	}


	[SerializeField] float th = 100;
	[SerializeField] bool canSnap = false;

	void Snap()
	{
		// if the speed has dropped below the threshhold velocity
		//				Debug.Log(LinearVelocity + ", " + IsDragging + ", " + scrollCount);
		if (Mathf.Abs(m_ScrollRect.vertical ? m_ScrollRect.velocity.y : m_ScrollRect.velocity.x) <= th && !m_IsDragging)
		{
			// Call the snap function
			Snap();
		}
	}

	void StopSnapping()
	{
		if (m_CoTweening != null)
		{
			StopCoroutine(m_CoTweening);
			m_CoTweening = null;

			// fire the delegate for the tween ending
//			IsTweening = false;
//			if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, false);

//			_snapJumping = false;
			m_ScrollRect.inertia = m_Inertia;
		}
	}

	bool m_Inertia = false;

	public bool isSnapping { get { return m_CoTweening != null; } }

	public void StartSnapping(TweenType tweenType, float time, float startValue, float endValue, Action onComplete, Func<float> onChangedPosition)
	{
		StopSnapping();
		m_CoTweening = StartCoroutine(Co_SnapTo(tweenType, time, startValue, endValue, onComplete, onChangedPosition));
	}

	void OnChangedPosition(float value, float dir)
	{
		if (m_ScrollRectEx)
		{
			m_ScrollRectEx.SetPosition(value, dir);
		}
		else if (m_ScrollRect.vertical)
		{
			m_ScrollRect.verticalNormalizedPosition = 1f - (value / m_ScrollRect.content.rect.height - (m_ScrollRect.transform as RectTransform).rect.height);
		}
		else
		{
			m_ScrollRect.horizontalNormalizedPosition = 1f - (value / m_ScrollRect.content.rect.width - (m_ScrollRect.transform as RectTransform).rect.width);
		}
	}

	void OnStartSnapAuto()
	{
		if (m_ScrollRectEx)
		{
			m_ScrollRectEx.Snap();
		}
	}

	/// <summary>
	/// Moves the scroll position over time between two points given an easing function. When the
	/// tween is complete it will fire the jumpComplete delegate.
	/// </summary>
	/// <param name="tweenType">The type of easing to use</param>
	/// <param name="time">The amount of time to interpolate</param>
	/// <param name="start">The starting scroll position</param>
	/// <param name="end">The ending scroll position</param>
	/// <param name="jumpComplete">The action to fire when the tween is complete</param>
	/// <returns></returns>
	IEnumerator Co_SnapTo(TweenType tweenType, float time, float startValue, float endValue, Action onComplete, Func<float> onChangedPosition)
	{
		float upDir = endValue - startValue;
		m_Inertia = m_ScrollRect.inertia;
		if (tweenType == TweenType.immediate || time <= 0)
		{
			OnChangedPosition(endValue, upDir);
		}
		else
		{
			// zero out the velocity
			m_ScrollRect.velocity = Vector2.zero;

			// fire the delegate for the tween start
//			IsTweening = true;
//			if (scrollerTweeningChanged != null)
//				scrollerTweeningChanged(this, true);

			float unscaleTimer = 0;
			var newPosition = 0f;

			// while the tween has time left, use an easing function
			while (unscaleTimer < time)
			{
				switch (tweenType)
				{
					case TweenType.linear:
						newPosition = linear(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.spring:
						newPosition = spring(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInQuad:
						newPosition = easeInQuad(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutQuad:
						newPosition = easeOutQuad(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutQuad:
						newPosition = easeInOutQuad(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInCubic:
						newPosition = easeInCubic(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutCubic:
						newPosition = easeOutCubic(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutCubic:
						newPosition = easeInOutCubic(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInQuart:
						newPosition = easeInQuart(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutQuart:
						newPosition = easeOutQuart(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutQuart:
						newPosition = easeInOutQuart(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInQuint:
						newPosition = easeInQuint(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutQuint:
						newPosition = easeOutQuint(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutQuint:
						newPosition = easeInOutQuint(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInSine:
						newPosition = easeInSine(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutSine:
						newPosition = easeOutSine(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutSine:
						newPosition = easeInOutSine(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInExpo:
						newPosition = easeInExpo(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutExpo:
						newPosition = easeOutExpo(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutExpo:
						newPosition = easeInOutExpo(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInCirc:
						newPosition = easeInCirc(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutCirc:
						newPosition = easeOutCirc(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutCirc:
						newPosition = easeInOutCirc(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInBounce:
						newPosition = easeInBounce(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutBounce:
						newPosition = easeOutBounce(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutBounce:
						newPosition = easeInOutBounce(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInBack:
						newPosition = easeInBack(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutBack:
						newPosition = easeOutBack(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutBack:
						newPosition = easeInOutBack(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInElastic:
						newPosition = easeInElastic(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeOutElastic:
						newPosition = easeOutElastic(startValue, endValue, (unscaleTimer / time));
						break;
					case TweenType.easeInOutElastic:
						newPosition = easeInOutElastic(startValue, endValue, (unscaleTimer / time));
						break;
				}


				OnChangedPosition(newPosition, upDir);

//				if (loop)
//				{
//					// if we are looping, we need to make sure the new position isn't past the jump trigger.
//					// if it is we need to reset back to the jump position on the other side of the area.
//
//					if (end > start && newPosition > _loopLastJumpTrigger)
//					{
//						//Debug.Log("name: " + name + " went past the last jump trigger, looping back around");
//						newPosition = _loopFirstScrollPosition + (newPosition - _loopLastJumpTrigger);
//					}
//					else if (start > end && newPosition < _loopFirstJumpTrigger)
//					{
//						//Debug.Log("name: " + name + " went past the first jump trigger, looping back around");
//						newPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - newPosition);
//					}
//				}
//
//				// set the scroll position to the tweened position
//				ScrollPosition = newPosition;

				// increase the time elapsed
				unscaleTimer += Time.unscaledDeltaTime;

				yield return null;
			}

			// the time has expired, so we make sure the final scroll position
			// is the actual end position.
//			ScrollPosition = end;
			OnChangedPosition(endValue, upDir);
		}

		// the tween jump is complete, so we fire the delegate
		if (onComplete != null)
			onComplete();

		//			// fire the delegate for the tween ending
		//			IsTweening = false;
		//			if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, false);

		StopSnapping();
	}

	private float linear(float start, float end, float val)
	{
		return Mathf.Lerp(start, end, val);
	}

	private static float spring(float start, float end, float val)
	{
		val = Mathf.Clamp01(val);
		val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
		return start + (end - start) * val;
	}

	private static float easeInQuad(float start, float end, float val)
	{
		end -= start;
		return end * val * val + start;
	}

	private static float easeOutQuad(float start, float end, float val)
	{
		end -= start;
		return -end * val * (val - 2) + start;
	}

	private static float easeInOutQuad(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return end / 2 * val * val + start;
		val--;
		return -end / 2 * (val * (val - 2) - 1) + start;
	}

	private static float easeInCubic(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val + start;
	}

	private static float easeOutCubic(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * (val * val * val + 1) + start;
	}

	private static float easeInOutCubic(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return end / 2 * val * val * val + start;
		val -= 2;
		return end / 2 * (val * val * val + 2) + start;
	}

	private static float easeInQuart(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val * val + start;
	}

	private static float easeOutQuart(float start, float end, float val)
	{
		val--;
		end -= start;
		return -end * (val * val * val * val - 1) + start;
	}

	private static float easeInOutQuart(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return end / 2 * val * val * val * val + start;
		val -= 2;
		return -end / 2 * (val * val * val * val - 2) + start;
	}

	private static float easeInQuint(float start, float end, float val)
	{
		end -= start;
		return end * val * val * val * val * val + start;
	}

	private static float easeOutQuint(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * (val * val * val * val * val + 1) + start;
	}

	private static float easeInOutQuint(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return end / 2 * val * val * val * val * val + start;
		val -= 2;
		return end / 2 * (val * val * val * val * val + 2) + start;
	}

	private static float easeInSine(float start, float end, float val)
	{
		end -= start;
		return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
	}

	private static float easeOutSine(float start, float end, float val)
	{
		end -= start;
		return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
	}

	private static float easeInOutSine(float start, float end, float val)
	{
		end -= start;
		return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
	}

	private static float easeInExpo(float start, float end, float val)
	{
		end -= start;
		return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
	}

	private static float easeOutExpo(float start, float end, float val)
	{
		end -= start;
		return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
	}

	private static float easeInOutExpo(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
		val--;
		return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
	}

	private static float easeInCirc(float start, float end, float val)
	{
		end -= start;
		return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
	}

	private static float easeOutCirc(float start, float end, float val)
	{
		val--;
		end -= start;
		return end * Mathf.Sqrt(1 - val * val) + start;
	}

	private static float easeInOutCirc(float start, float end, float val)
	{
		val /= .5f;
		end -= start;
		if (val < 1)
			return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
		val -= 2;
		return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
	}

	private static float easeInBounce(float start, float end, float val)
	{
		end -= start;
		float d = 1f;
		return end - easeOutBounce(0, end, d - val) + start;
	}

	private static float easeOutBounce(float start, float end, float val)
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

	private static float easeInOutBounce(float start, float end, float val)
	{
		end -= start;
		float d = 1f;
		if (val < d / 2)
			return easeInBounce(0, end, val * 2) * 0.5f + start;
		else
			return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
	}

	private static float easeInBack(float start, float end, float val)
	{
		end -= start;
		val /= 1;
		float s = 1.70158f;
		return end * (val) * val * ((s + 1) * val - s) + start;
	}

	private static float easeOutBack(float start, float end, float val)
	{
		float s = 1.70158f;
		end -= start;
		val = (val / 1) - 1;
		return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
	}

	private static float easeInOutBack(float start, float end, float val)
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

	private static float easeInElastic(float start, float end, float val)
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

	private static float easeOutElastic(float start, float end, float val)
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

	private static float easeInOutElastic(float start, float end, float val)
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
