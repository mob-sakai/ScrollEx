using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UI;
using Mobcast.Coffee;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Method = Mobcast.Coffee.Tweening.Method;

namespace Mobcast.Coffee
{
	public interface IScrollSnap : IScrollHandler, IBeginDragHandler, IEndDragHandler
	{
		void Snap();

		void SetPositionWithDir(float pos, float dir);

		ScrollRect scrollRect { get; }
	}

	[Serializable]
	public class ScrollSnap
	{
		public enum Alignment
		{
			TopOrLeft,
			Center,
			BottomOrRight,
		}



		bool m_IsDragging;
		int m_ScrollCount;
		Coroutine m_CoTweening;
		IScrollSnap m_Target;
		bool m_TriggerSnap;

		public bool m_SnapOnEndDrag = false;

		public Alignment m_Alignment = Alignment.Center;

		public float m_ThresholdVerocity = 200;

		public Method m_Method = Method.EaseOutSine;

		public float m_Duration = 0.5f;


		public void Initialize(IScrollSnap target)
		{
			m_Target = target;
		}

		public void OnScroll()
		{
			StopSnapping();
			m_ScrollCount = 10;
		}

		public void OnBeginDrag()
		{
			StopSnapping();
			m_ScrollCount = 0;
			m_IsDragging = true;
		}

		public void OnEndDrag()
		{
			StopSnapping();
			m_ScrollCount = 0;
			m_IsDragging = false;

			// スナップをトリガ.
			if (m_SnapOnEndDrag)
			{
				m_TriggerSnap = true;
			}
		}

		public void Update()
		{
			if (0 < m_ScrollCount && m_ScrollCount-- == 0 && m_SnapOnEndDrag)
			{
				m_TriggerSnap = true;
			}
			if (!m_IsDragging && m_TriggerSnap && Mathf.Abs(m_Target.scrollRect.vertical ? m_Target.scrollRect.velocity.y : m_Target.scrollRect.velocity.x) <= m_ThresholdVerocity)
			{
				m_TriggerSnap = false;
				m_ScrollCount = 0;
				m_Target.Snap();
			}
		}

		void StopSnapping()
		{
			if (m_CoTweening != null)
			{
				(m_Target as MonoBehaviour).StopCoroutine(m_CoTweening);
				m_CoTweening = null;
				m_Target.scrollRect.inertia = m_OriginInertia;
				m_Target.scrollRect.movementType = m_OriginMovementType;
			}
		}

		bool m_OriginInertia = false;
		ScrollRect.MovementType m_OriginMovementType;

		public bool running { get { return m_CoTweening != null; } }

		public void StartSnapping(Method tweenType, float time, float startValue, float endValue)
		{
			StopSnapping();
			m_CoTweening = (m_Target as MonoBehaviour).StartCoroutine(Co_Snap(tweenType, time, startValue, endValue));
		}

		IEnumerator Co_Snap(Method tweenType, float time, float startValue, float endValue)
		{
			ScrollRect scrollRect = m_Target.scrollRect;
			float upDir = endValue - startValue;
			m_OriginInertia = scrollRect.inertia;
			m_OriginMovementType = scrollRect.movementType;
			scrollRect.inertia = false;
			scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			if (tweenType == Method.immediate || time <= 0)
			{
				m_Target.SetPositionWithDir(endValue, upDir);
			}
			else
			{
				scrollRect.velocity = Vector2.zero;

				float unscaleTimer = 0;

				// while the tween has time left, use an easing function
				while (unscaleTimer < time)
				{
					m_Target.SetPositionWithDir(Tweening.GetTweenValue(tweenType, startValue, endValue, unscaleTimer / time), upDir);
					unscaleTimer += Time.unscaledDeltaTime;
					yield return null;
				}

				// the time has expired, so we make sure the final scroll position
				// is the actual end position.
				m_Target.SetPositionWithDir(endValue, upDir);
			}

			// the tween jump is complete, so we fire the delegate
//		if (onComplete != null)
//			onComplete();
			StopSnapping();
		}
	}
}