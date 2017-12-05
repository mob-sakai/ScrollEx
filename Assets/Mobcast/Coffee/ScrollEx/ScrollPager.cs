using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Mobcast.Coffee
{

	public interface IScrollPager
	{
		int currentIndex { get;}
		int dataCount { get;}
	}

	[Serializable]
	public class ScrollPager
	{
#region Serialize
		[SerializeField] public Toggle m_Template = null;
		[SerializeField] int m_Limit = 10;
		[SerializeField] LayoutGroup m_LayoutGroup;





#endregion Serialize

#region Public
		public IScrollPager target { get; set; }

		public Toggle template { get{ return m_Template;} set{ m_Template = value;} }
		public int limit { get{ return m_Limit;} set{ m_Limit = value;} }
		public LayoutGroup layoutGroup { get{ return m_LayoutGroup;} set{ m_LayoutGroup = value;} }


		public void Update()
		{
			// 変更なし.
			if (!m_LayoutGroup || (_count == target.dataCount && _index == target.currentIndex))
				return;

			if (m_Template.gameObject.activeSelf)
			{
				m_Template.gameObject.SetActive(false);
			}

			_count = target.dataCount;
			_index = target.currentIndex;

			// ページャが不足している場合、新しく生成します.
			int max = Mathf.Min(m_Limit, _count);
			while (_toggles.Count < max)
			{
				//				int i = m_List.Count;
				Toggle toggle = UnityEngine.Object.Instantiate(m_Template);
				toggle.transform.SetParent(m_LayoutGroup.transform);
				// TODO: スナップに変更
				//				toggle.onValueChanged.AddListener(flag =>
				//					{
				//						if(flag)
				//						{
				//							m_Target.
				//							m_Index = i;
				//						}
				//					});
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