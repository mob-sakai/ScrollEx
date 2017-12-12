using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using UnityEngine.Events;


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

		[SerializeField] Button m_FirstButton;

		[SerializeField] Button m_LastButton;

#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		public Button previousButton { get{ return m_PreviousButton;} set{ m_PreviousButton = value; _changedButton = true;} }
		public Button nextButton { get{ return m_NextButton;} set{ m_NextButton = value; _changedButton = true;} }

		public Button firstButton { get{ return m_FirstButton;} set{ m_FirstButton = value; _changedButton = true;} }
		public Button lastButton { get{ return m_LastButton;} set{ m_LastButton = value; _changedButton = true;} }

		public void Update()
		{
			if (_changedButton)
			{
				_changedButton = false;

				// コールバックを再設定.
				_SetButtonCallback(previousButton, ()=>handler.JumpTo(handler.activeIndex - 1));
				_SetButtonCallback(nextButton, ()=>handler.JumpTo(handler.activeIndex + 1));
				_SetButtonCallback(firstButton, ()=>handler.JumpTo(0));
				_SetButtonCallback(lastButton, ()=>handler.JumpTo(handler.dataCount - 1));
			}

			// 指定されたインデックスに移動可能であれば、ボタンを有効化.
			_activeIndex = handler.activeIndex;
			_SetButtonInteractable(previousButton, _activeIndex - 1);
			_SetButtonInteractable(nextButton, _activeIndex + 1);
			_SetButtonInteractable(firstButton, 0);
			_SetButtonInteractable(lastButton, handler.dataCount - 1);
		}


#endregion Public

#region Private

		bool _changedButton = true;

		int _activeIndex;

		/// <summary>
		/// ボタンにコールバックを再設定.
		/// </summary>
		void _SetButtonCallback(Button b, UnityAction callback)
		{
			if (b)
			{
				b.onClick.RemoveListener(callback);
				b.onClick.AddListener(callback);
			}
		}

		/// <summary>
		/// 指定されたインデックスに移動可能であれば、ボタンを有効化.
		/// </summary>
		void _SetButtonInteractable(Button b, int index)
		{
			if (b)
			{
				b.interactable = _activeIndex != index && handler.CanJumpTo(index);
			}
		}

#endregion Private
	}
}
