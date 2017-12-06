using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Mobcast.Coffee.UI.Scrolling
{

//	/// <summary>
//	/// スクロールインジケータハンドラー.
//	/// </summary>
//	public interface IIndicatorHandler
//	{
//		int activeIndex { get; }
//
//		void JumpTo(int index);
//
//		/// <summary>
//		/// 最大ページ数を取得します.
//		/// </summary>
//		int GetPageCount();
//
//		/// <summary>
//		/// 現在のページ数を取得します.
//		/// </summary>
//		int GetPageIndex();
//	}

	/// <summary>
	/// 現在のインデックスをインジケータ表示するモジュールです.
	/// インジケータをクリックすると対象のインデックスに移動できます.
	/// </summary>
	[Serializable]
	public class IndicatorModule
	{
#region Serialize

		[SerializeField] LayoutGroup m_LayoutGroup;
		[SerializeField] public Toggle m_Template = null;
		[SerializeField] int m_Limit = 10;

#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set; }

		public Toggle template { get { return m_Template; } set { m_Template = value; } }

		public int limit { get { return m_Limit; } set { m_Limit = value; } }

		public LayoutGroup layoutGroup { get { return m_LayoutGroup; } set { m_LayoutGroup = value; } }

		public void Update()
		{
			if (!m_LayoutGroup)
				return;

			// 変更なし.
			int index = handler.activeIndex;
			int count = handler.dataCount;
			if (!m_LayoutGroup || (_count == count && _index == index))
				return;

			if (m_Template.gameObject.activeSelf)
			{
				m_Template.gameObject.SetActive(false);
			}

			_count = count;
			_index = index;

			// ページャが不足している場合、新しく生成します.
			int max = Mathf.Min(m_Limit, _count);
			while (_toggles.Count < max)
			{
				int i = _toggles.Count;
				Toggle toggle = UnityEngine.Object.Instantiate(m_Template);
				toggle.name = "PageToggle_" + i;
				toggle.transform.SetParent(m_LayoutGroup.transform);

				// TODO: スナップに変更
				toggle.onValueChanged.AddListener(flag =>
					{
						if (!flag || handler.activeIndex == i)
							return;
						
						handler.JumpTo(i);
						toggle.isOn = false;
					});
				_toggles.Add(toggle);
			}

			// 全ページャの表示状態を更新します.
			for (int i = 0; i < _toggles.Count; i++)
			{
				_toggles[i].gameObject.SetActive(i < max);
				_toggles[i].isOn = (i == _index);
			}
		}

#endregion Public

#region Private

		List< Toggle> _toggles = new List<Toggle>();
		int _index;
		int _count;

#endregion Private
	}
}