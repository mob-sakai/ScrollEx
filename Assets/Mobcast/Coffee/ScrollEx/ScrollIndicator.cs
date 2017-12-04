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
		int index { get; }

		int count { get; }
	}


	[Serializable]
	public class ScrollIndicator
	{
		[SerializeField] public Toggle m_PagerToggle = null;

		List< Toggle> m_List = new List<Toggle>();
		[SerializeField] int m_Limit = 10;

		int m_Index;
		int m_Count;

		public ScrollRectEx m_Target { get; set; }

		[SerializeField] LayoutGroup m_LayoutGroup;

		public void Update()
		{
			// 変更なし.
			if (!m_Target || !m_LayoutGroup || (m_Count == m_Target.cellCount && m_Index == m_Target.StartDataIndex))
				return;

			if (m_PagerToggle.gameObject.activeSelf)
			{
				m_PagerToggle.gameObject.SetActive(false);
			}

			m_Count = m_Target.cellCount;
			m_Index = m_Target.StartDataIndex;

			// ページャが不足している場合、新しく生成します.
			int max = Mathf.Min(m_Limit, m_Count);
			while (m_List.Count < max)
			{
				int i = m_List.Count;
				Toggle toggle = UnityEngine.Object.Instantiate(m_PagerToggle);
				toggle.transform.SetParent(m_LayoutGroup.transform);
//				m_Target.m_ScrollSnap.running
				// TODO: スナップに変更
				toggle.onValueChanged.AddListener(flag =>
					{
//						if(flag)
//						{
//							m_Target.
//							m_Index = i;
//						}
					});
				m_List.Add(toggle);
			}

			// 全ページャの表示状態を更新します.
			for (int i = 0; i < m_List.Count; i++)
			{
				m_List[i].gameObject.SetActive(i < max);
				m_List[i].isOn = (i == m_Index);
			}
		}
	}
}