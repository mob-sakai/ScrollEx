using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;


namespace Mobcast.Coffee.UI.ScrollModule
{
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
#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		public bool jumpOnSwipe { get{ return m_JumpOnSwipe;} set{ m_JumpOnSwipe = value;} }
		public float swipeThreshold { get{ return m_SwipeThreshold;} set{ m_SwipeThreshold = value;} }
		public Button previousButton { get{ return m_PreviousButton;} set{ m_PreviousButton = value; _changedButton = true;} }
		public Button nextButton { get{ return m_NextButton;} set{ m_NextButton = value; _changedButton = true;} }

		public void OnBeginDrag(PointerEventData eventData)
		{
			_dragStartIndex = handler.activeIndex;
			_dragStartPosition = handler.scrollRect.vertical ? eventData.position.y : eventData.position.x;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
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
				_JumpToNext();
			}
			else if (diff < -m_SwipeThreshold && handler.CanJumpTo(index - 1))
			{
				_JumpToPrevious();
			}
		}


		public void Update()
		{
			if (_changedButton)
			{
				_changedButton = false;
				if (previousButton)
				{
					previousButton.onClick.RemoveListener(_JumpToPrevious);
					previousButton.onClick.AddListener(_JumpToPrevious);
				}
				if (nextButton)
				{
					nextButton.onClick.RemoveListener(_JumpToNext);
					nextButton.onClick.AddListener(_JumpToNext);
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
		}


#endregion Public

#region Private

		int _dragStartIndex;
		float _dragStartPosition;
		bool _changedButton = true;

		void _JumpToPrevious()
		{
			handler.JumpTo(handler.activeIndex - 1);
		}

		void _JumpToNext()
		{
			handler.JumpTo(handler.activeIndex + 1);
		}

#endregion Private
	}
}
