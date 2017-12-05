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
		ScrollRect scrollRect { get;}

		void OnTriggerSnap();

		void OnChangeTweenPosition(float pos, bool dir);
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

#region Serialize

		[SerializeField] bool m_SnapOnEndDrag = false;
		[SerializeField] float m_ThresholdVelocity = 200;

#endregion Serialize

#region Public

		public IScrollSnap target { get; set; }


		public void OnScroll()
		{
			StopScrollTween();
			m_ScrollCount = 10;
		}

		public void OnBeginDrag()
		{
			StopScrollTween();
			m_IsDragging = true;
		}

		public void OnEndDrag()
		{
			StopScrollTween();
			m_IsDragging = false;

			// スナップをトリガ.
			m_TriggerSnap = m_SnapOnEndDrag;
		}

		public void Update()
		{
			if (0 < m_ScrollCount && m_ScrollCount-- == 0 && m_SnapOnEndDrag)
			{
				m_TriggerSnap = true;
			}
			if (!m_IsDragging && m_TriggerSnap && Mathf.Abs(target.scrollRect.vertical ? target.scrollRect.velocity.x : target.scrollRect.velocity.y) <= m_ThresholdVelocity)
			{
				target.OnTriggerSnap();
			}
		}

		public void StartScrollTween(Method tweenType, float time, float startValue, float endValue)
		{
			StopScrollTween();

			// Tweenが不要な場合、即終了します.
			if (tweenType == Method.immediate || time <= 0)
				target.OnChangeTweenPosition(endValue, 0 < (endValue - startValue));
			else
				m_CoTweening = target.scrollRect.StartCoroutine(CoScrollTweening(tweenType, time, startValue, endValue));
		}

#endregion Public


#region Private

		bool m_IsDragging;
		int m_ScrollCount;
		Coroutine m_CoTweening;
		bool m_TriggerSnap;
		bool m_OriginInertia = false;
		ScrollRect.MovementType m_OriginMovementType;

		void StopScrollTween()
		{
			m_TriggerSnap = false;
			m_ScrollCount = 0;
			if (m_CoTweening != null)
			{
				target.scrollRect.StopCoroutine(m_CoTweening);
				m_CoTweening = null;
				target.scrollRect.inertia = m_OriginInertia;
				target.scrollRect.movementType = m_OriginMovementType;
			}
		}

		/// <summary>
		/// Tweenコルーチン.
		/// </summary>
		IEnumerator CoScrollTweening(Method tweenType, float time, float startValue, float endValue)
		{
			// Tween中はScrollRect自体の動作(inertia/movementType)を制限します.
			var scroll = target.scrollRect;
			scroll.velocity = Vector2.zero;
			m_OriginInertia = scroll.inertia;
			m_OriginMovementType = scroll.movementType;
			scroll.inertia = false;
			scroll.movementType = ScrollRect.MovementType.Unrestricted;

			// Tweenを実行します.
			bool positive = 0 < (endValue - startValue);
			float unscaleTimer = 0;
			while (unscaleTimer < time)
			{
				target.OnChangeTweenPosition(Tweening.GetTweenValue(tweenType, startValue, endValue, unscaleTimer / time), positive);
				unscaleTimer += Time.unscaledDeltaTime;
				yield return null;
			}

			// Tweenを停止します.
			target.OnChangeTweenPosition(endValue, positive);
			StopScrollTween();
			yield break;
		}
#endregion Private
	}
}