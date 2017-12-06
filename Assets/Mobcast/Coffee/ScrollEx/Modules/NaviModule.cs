using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;


namespace Mobcast.Coffee.UI.ScrollModule
{
//	/// <summary>
//	/// スクロールビューコントローラインターフェース.
//	/// </summary>
//	public interface IScrollNavigation : IBeginDragHandler, IEndDragHandler
//	{
//		ScrollRect scrollRect { get;}
//
//		int activeIndex { get;}
//
//		bool CanJumpTo(int index);
//		void JumpTo(int index);
//	}

	/// <summary>
	/// 前後のセルビューにジャンプするためのモジュールです.
	/// オートローテーションを有効にすると、自動的にスクロールが進行します.
	/// </summary>
	[Serializable]
	public class NaviModule
	{
#region Serialize

		[SerializeField] bool m_JumpOnSwipe = false;
		[SerializeField][Range(20,500)]  float m_SwipeThreshold = 200;
		[SerializeField] Button m_PreviousButton;
		[SerializeField] Button m_NextButton;

		[SerializeField] bool m_NextAutomatically = false;
		[SerializeField][Range(3f,10f)] float m_Delay = 6;
		[SerializeField][Range(1f,5f)] float m_Interval = 2;
#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		public bool jumpOnSwipe { get{ return m_JumpOnSwipe;} set{ m_JumpOnSwipe = value;} }
		public float swipeThreshold { get{ return m_SwipeThreshold;} set{ m_SwipeThreshold = value;} }
		public Button previousButton { get{ return m_PreviousButton;} set{ m_PreviousButton = value; _changedButton = true;} }
		public Button nextButton { get{ return m_NextButton;} set{ m_NextButton = value; _changedButton = true;} }

		public bool autoJumpToNext { get{ return m_NextAutomatically;} set{ m_NextAutomatically = value;} }
		public float interval { get{ return m_Interval;} set{ m_Interval = value;} }
		public float delay { get{ return m_Delay;} set{ m_Delay = value;} }


		public void OnBeginDrag(PointerEventData eventData)
		{
			_isDragging = true;
			_dragStartIndex = handler.activeIndex;
			_dragStartPosition = handler.scrollRect.vertical ? eventData.position.y : eventData.position.x;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_autoJumpTimer = delay;
			_isDragging = false;
			if (!m_JumpOnSwipe)
				return;

			int index = handler.activeIndex;
			var scroll = handler.scrollRect;
			float velocity = (scroll.vertical ? scroll.velocity.y : scroll.velocity.x);

			if (index != _dragStartIndex || 100 < velocity)
				return;
						float diff = (scroll.vertical ? eventData.position.y : eventData.position.x) - _dragStartPosition;
			if (m_SwipeThreshold < diff&& handler.CanJumpTo(index + 1))
			{
				JumpToNext();
			}
			else if (diff < -m_SwipeThreshold && handler.CanJumpTo(index - 1))
			{
				JumpToPrevious();
			}
		}


		public void Update()
		{
			if (_changedButton)
			{
				_changedButton = false;
				if (previousButton)
				{
					previousButton.onClick.RemoveListener(JumpToPrevious);
					previousButton.onClick.AddListener(JumpToPrevious);
				}
				if (nextButton)
				{
					nextButton.onClick.RemoveListener(JumpToNext);
					nextButton.onClick.AddListener(JumpToNext);
				}
			}

			int index = handler.activeIndex;
			if (previousButton)
			{
				previousButton.interactable = handler.CanJumpTo(index - 1);
			}

			if (nextButton)
			{
				nextButton.interactable = handler.CanJumpTo(index + 1);
			}

			// 自動送り
			if (autoJumpToNext && _coAutoJump == null)
			{
				// 他のコントロールでスクロールした場合、再ディレイさせます.
				if (_lastPosition != handler.scrollPosition || _autoJumpTimer <= 0)
				{
					_autoJumpTimer = Mathf.Max(_autoJumpTimer, delay);
				}
				// 非ドラッグ状態で、オートローテーションタイマが有効な場合、タイマを進め、トリガします.
				else if (!_isDragging && 0 < _autoJumpTimer && (_autoJumpTimer -= Time.unscaledDeltaTime) <= 0)
				{
					_coAutoJump = handler.StartCoroutine(CoAutoJump());
				}
			}
			_lastPosition = handler.scrollPosition;
		}

		IEnumerator CoAutoJump()
		{
			_autoJumpTimer = interval;
			if (handler.loop || handler.CanJumpTo(handler.activeIndex + 1))
				JumpToNext();
			else
				handler.JumpTo(0);
			
			yield return new WaitForSeconds(handler.tweenDuration + 0.1f);
			_coAutoJump = null;
		}


#endregion Public

#region Private

		int _dragStartIndex;
		float _dragStartPosition;
		bool _changedButton = true;
		Coroutine _coAutoJump;
		float _autoJumpTimer = 0;
		bool _isDragging;
		float _lastPosition = 0;

		void JumpToPrevious()
		{
			handler.JumpTo(handler.activeIndex - 1);
		}

		void JumpToNext()
		{
			handler.JumpTo(handler.activeIndex + 1);
		}

#endregion Private
	}
}
