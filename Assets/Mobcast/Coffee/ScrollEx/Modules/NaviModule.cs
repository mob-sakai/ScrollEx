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

		[SerializeField] Button m_PreviousButton;
		[SerializeField] Button m_NextButton;
#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		public Button previousButton { get{ return m_PreviousButton;} set{ m_PreviousButton = value; _changedButton = true;} }
		public Button nextButton { get{ return m_NextButton;} set{ m_NextButton = value; _changedButton = true;} }

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
