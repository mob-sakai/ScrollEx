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

		public bool snapOnEndDrag { get{ return m_SnapOnEndDrag;} set{ m_SnapOnEndDrag = value;} }

		public float thresholdVelocity { get{ return m_ThresholdVelocity;} set{ m_ThresholdVelocity = value;} }

		public void OnScroll()
		{
			StopScrollTween();
			_mouseScrollCount = 10;
		}

		public void OnBeginDrag()
		{
			StopScrollTween();
			_isDragging = true;
		}

		public void OnEndDrag()
		{
			StopScrollTween();
			_isDragging = false;

			// スナップをトリガ.
			_triggerSnap = snapOnEndDrag;
		}

		public void Update()
		{
			if (0 < _mouseScrollCount && _mouseScrollCount-- == 0 && snapOnEndDrag)
			{
				_triggerSnap = true;
			}
			if (!_isDragging && _triggerSnap && Mathf.Abs(target.scrollRect.vertical ? target.scrollRect.velocity.x : target.scrollRect.velocity.y) <= thresholdVelocity)
			{
				target.OnTriggerSnap();
			}
		}

		public void StartScrollTween(Method tweenType, float time, float startValue, float endValue)
		{
			StopScrollTween();
			_coTweening = target.scrollRect.StartCoroutine(CoScrollTweening(tweenType, time, startValue, endValue));

//			// Tweenが不要な場合、即終了します.
//			if (tweenType == Method.immediate || time <= 0)
//			{
//				target.OnChangeTweenPosition(endValue, 0 < (endValue - startValue));
//			}
//			else
		}

#endregion Public


#region Private

		bool _isDragging;
		int _mouseScrollCount;
		Coroutine _coTweening;
		bool _triggerSnap;
		bool _inertia = false;
		ScrollRect.MovementType _movementType;

		void StopScrollTween()
		{
			_triggerSnap = false;
			_mouseScrollCount = 0;
			if (_coTweening != null)
			{
				target.scrollRect.StopCoroutine(_coTweening);
				_coTweening = null;
				target.scrollRect.inertia = _inertia;
				target.scrollRect.movementType = _movementType;
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
			_inertia = scroll.inertia;
			_movementType = scroll.movementType;
			scroll.inertia = false;
			scroll.movementType = ScrollRect.MovementType.Unrestricted;
			bool positive = 0 < (endValue - startValue);

			if (tweenType == Method.immediate || time <= 0)
			{
				target.OnChangeTweenPosition(endValue, positive);
				yield return null;
			}
			else
			{
				// Tweenを実行します.
				float unscaleTimer = 0;
				while (unscaleTimer < time)
				{
					target.OnChangeTweenPosition(Tweening.GetTweenValue(tweenType, startValue, endValue, unscaleTimer / time), positive);
					unscaleTimer += Time.unscaledDeltaTime;
					yield return null;
				}
			}

			// Tweenを停止します.
			target.OnChangeTweenPosition(endValue, positive);
			StopScrollTween();
			yield break;
		}
#endregion Private
	}
}