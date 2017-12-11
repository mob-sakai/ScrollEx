using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UI;
using Mobcast.Coffee;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI.ScrollModule
{
	/// <summary>
	/// スクロールスナッパー.
	/// スクロール領域をオブジェクトにスナップさせるモジュールです.
	/// ドラッグ終了後にスナップをトリガー出来ます.
	/// </summary>
	[Serializable]
	public class SnapModule
	{
		#region Serialize

		[SerializeField] bool m_SnapOnEndDrag = false;
		[SerializeField][Range(10, 1000)] float m_VelocityThreshold = 200;
		[SerializeField][Range(20, 500)]  float m_SwipeThreshold = 40;

		#endregion Serialize

		#region Public

		public ScrollRectEx handler { get; set; }

		/// <summary>
		/// ドラッグが終了した際、スナップをトリガします.
		/// </summary>
		public bool snapOnEndDrag { get { return m_SnapOnEndDrag; } set { m_SnapOnEndDrag = value; } }

		/// <summary>
		/// スクロール速度が値以下になったとき、Tweenを実行します.
		/// </summary>
		public float velocityThreshold { get { return m_VelocityThreshold; } set { m_VelocityThreshold = value; } }

		public event Action onEndNextTween;

		public void OnScroll(PointerEventData eventData)
		{
			_StopScrollTween();
			_mouseScrollCount = 10;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_StopScrollTween();
			_isDragging = true;
			_dragStartIndex = handler.activeIndex;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
			if (_coTweening != null)
				return;

			// インデックスに変化がある、またはスクロール速度が早い場合ドラッグと判定します.
			// ドラッグ完了後は、スナップをトリガします.
			int index = handler.activeIndex;
			_oldVelocity = velocity;
			if (index != _dragStartIndex || m_VelocityThreshold < _oldVelocity)
			{
				_triggerSnap = snapOnEndDrag;
				return;
			}

			// スワイプ量がしきい値以上の場合、前後のインデックスへスナップします.
			float diff = handler.scrollRect.vertical
					? eventData.position.y - eventData.pressPosition.y
					: -eventData.position.x + eventData.pressPosition.x;
			int newIndex = 0 < diff ? index + 1 : index - 1;
			if (m_SwipeThreshold < Mathf.Abs(diff) && handler.CanJumpTo(newIndex))
			{
				handler.JumpTo(newIndex);
			}
			else
			{
				_triggerSnap = snapOnEndDrag;
			}
		}

		float velocity { get { return handler.scrollRect.vertical ? handler.scrollRect.velocity.y : handler.scrollRect.velocity.x; } }

		public void Update()
		{
			// スクロール後、しばらくしたらスナップをトリガします.
			if (0 < _mouseScrollCount && _mouseScrollCount-- == 0 && (_coTweening == null) && snapOnEndDrag)
			{
				_triggerSnap = true;
			}

			// スナップがトリガされている場合、直近2フレームの平均速度がしきい値以下になったときに、最も近いインデックスへジャンプします.
			if (!_isDragging && _triggerSnap)
			{
				float v = velocity;
				float average = (v + _oldVelocity) / 2;

				if (Mathf.Abs(average) <= velocityThreshold)
				{
					_triggerSnap = false;
					handler.JumpTo(handler.activeIndex);
				}
				// スクロール速度が急に0になることがあるので対策します.
				else if (handler.scrollRect.inertia && v == 0f)
				{
					v = average;
					handler.scrollRect.velocity = handler.scrollRect.vertical
						? new Vector2(0, v)
						: new Vector2(v, 0); 
				}
				_oldVelocity = v;
			}
		}

		/// <summary>
		/// Tweenを実行します.
		/// </summary>
		public void StartScrollTween(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
		{
			_StopScrollTween();
			_coTweening = handler.scrollRect.StartCoroutine(_CoScrollTweening(tweenType, time, startValue, endValue));
		}

		#endregion Public


		#region Private

		bool _isDragging;
		int _mouseScrollCount;
		Coroutine _coTweening;
		bool _triggerSnap;
		bool _inertia = false;
		ScrollRect.MovementType _movementType;
		float _oldVelocity = 0;
		int _dragStartIndex;

		void _JumpToPrev()
		{
			handler.JumpTo(handler.activeIndex - 1);
		}

		void _JumpToNext()
		{
			handler.JumpTo(handler.activeIndex + 1);
		}

		void _StopScrollTween()
		{
			_triggerSnap = false;
			_mouseScrollCount = 0;
			if (_coTweening != null)
			{
				handler.scrollRect.StopCoroutine(_coTweening);
				_coTweening = null;
				handler.scrollRect.inertia = _inertia;
				handler.scrollRect.movementType = _movementType;
				handler.scrollRect.velocity = Vector2.zero;
				if (onEndNextTween != null)
					onEndNextTween.Invoke();

				onEndNextTween = null;
			}
		}

		/// <summary>
		/// Tweenコルーチン.
		/// </summary>
		IEnumerator _CoScrollTweening(Tweening.TweenMethod tweenType, float time, float startValue, float endValue)
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
			_StopScrollTween();
			yield break;
		}

		#endregion Private
	}
}