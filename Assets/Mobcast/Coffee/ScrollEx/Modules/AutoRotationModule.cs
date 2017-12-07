using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;


namespace Mobcast.Coffee.UI.ScrollModule
{
	/// <summary>
	/// 自動的にスクロールが進行するモジュールです.
	/// </summary>
	[Serializable]
	public class AutoRotationModule
	{
#region Serialize
		[SerializeField] bool m_AutoJumpToNext = false;
		[SerializeField][Range(3f,10f)] float m_Delay = 6;
		[SerializeField][Range(1f,5f)] float m_Interval = 2;
#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set;}

		public bool autoJumpToNext { get{ return m_AutoJumpToNext;} set{ m_AutoJumpToNext = value;} }
		public float interval { get{ return m_Interval;} set{ m_Interval = value;} }
		public float delay { get{ return m_Delay;} set{ m_Delay = value;} }

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isDragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_autoJumpTimer = delay;
			_isDragging = false;
		}

		public void Update()
		{
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
					_coAutoJump = handler.StartCoroutine(_CoAutoJump());
				}
			}
			_lastPosition = handler.scrollPosition;
		}

#endregion Public

#region Private

		Coroutine _coAutoJump;
		float _autoJumpTimer = 0;
		bool _isDragging;
		float _lastPosition = 0;

		IEnumerator _CoAutoJump()
		{
			_autoJumpTimer = interval;
			int index = handler.activeIndex + 1;
			if (handler.loop || handler.CanJumpTo(index))
				handler.JumpTo(index);
			else
				handler.JumpTo(0);

			yield return new WaitForSeconds(handler.tweenDuration + 0.1f);
			_coAutoJump = null;
		}

#endregion Private
	}
}
