using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UI;
using Mobcast.Coffee;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Mobcast.Coffee
{
	/// <summary>
	/// スクロールスナップハンドラー.
	/// </summary>
	public interface IScrollSnapHandler : IScrollHandler, IBeginDragHandler, IEndDragHandler
	{
		ScrollRect scrollRect { get;}

		/// <summary>
		/// このメソッドは、スナップがトリガされたときにコールされます.
		/// </summary>
		void OnTriggerSnap();

		/// <summary>
		/// このメソッドは、Tweenによるスクロール座標の変化があった時にコールされます.
		/// </summary>
		void OnChangeTweenPosition(float pos, bool dir);
	}

	[Serializable]
	public class ScrollSnap
	{
#region Serialize

		[SerializeField] bool m_SnapOnEndDrag = false;
		[SerializeField] float m_ThresholdVelocity = 200;

#endregion Serialize

#region Public

		public IScrollSnapHandler handler { get; set;}

		/// <summary>
		/// ドラッグが終了した際、スナップをトリガします.
		/// </summary>
		public bool snapOnEndDrag { get{ return m_SnapOnEndDrag;} set{ m_SnapOnEndDrag = value;} }

		/// <summary>
		/// スクロール速度が値以下になったとき、Tweenを実行します.
		/// </summary>
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
			if (!_isDragging && _triggerSnap && Mathf.Abs(handler.scrollRect.vertical ? handler.scrollRect.velocity.x : handler.scrollRect.velocity.y) <= thresholdVelocity)
			{
				handler.OnTriggerSnap();
			}
		}

		public void StartScrollTween(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
		{
			StopScrollTween();
			_coTweening = handler.scrollRect.StartCoroutine(CoScrollTweening(tweenType, time, startValue, endValue));
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
				handler.scrollRect.StopCoroutine(_coTweening);
				_coTweening = null;
				handler.scrollRect.inertia = _inertia;
				handler.scrollRect.movementType = _movementType;
			}
		}

		/// <summary>
		/// Tweenコルーチン.
		/// </summary>
		IEnumerator CoScrollTweening(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
		{
			// Tween中はScrollRect自体の動作(inertia/movementType)を制限します.
			handler.scrollRect.velocity = Vector2.zero;
			_inertia = handler.scrollRect.inertia;
			_movementType = handler.scrollRect.movementType;
			handler.scrollRect.inertia = false;
			handler.scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			bool positive = 0 < (endValue - startValue);

			if (tweenType == Tweening.TweenMethod.immediate || time <= 0)
			{
				handler.OnChangeTweenPosition(endValue, positive);
				yield return null;
			}
			else
			{
				// Tweenを実行します.
				float unscaleTimer = 0;
				while (unscaleTimer < time)
				{
					handler.OnChangeTweenPosition(Tweening.GetTweenValue(tweenType, startValue, endValue, unscaleTimer / time), positive);
					unscaleTimer += Time.unscaledDeltaTime;
					yield return null;
				}
			}

			// Tweenを停止します.
			handler.OnChangeTweenPosition(endValue, positive);
			StopScrollTween();
			yield break;
		}
#endregion Private
	}
}