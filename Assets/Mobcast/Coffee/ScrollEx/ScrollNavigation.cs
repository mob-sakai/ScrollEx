using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;


namespace Mobcast.Coffee.UI.Scrolling
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

	[Serializable]
	public class NaviModule
	{
#region Serialize

		[SerializeField] bool m_JumpOnSwipe = false;
		[SerializeField] float m_SwipeThreshold = 200;
		[SerializeField] Button m_PreviousButton;
		[SerializeField] Button m_NextButton;

#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}
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
		}


#endregion Public

#region Private

		int _dragStartIndex;
		float _dragStartPosition;
		bool _changedButton = true;

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
