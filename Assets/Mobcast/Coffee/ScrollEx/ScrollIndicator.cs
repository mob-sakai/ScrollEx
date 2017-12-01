using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


//
//public interface IScrollPager
//{
//	int index { get; set; }
//
//	int count { get; }
//}

public class ScrollIndicator : MonoBehaviour
{
//	cla
//	public enum Alignment
//	{
//		TopOrLeft,
//		Center,
//		BottomOrRight,
//	}
//
//
//
//	bool m_IsDragging;
//	int m_ScrollCount;
//	Coroutine m_CoTweening;
//	IScrollPager m_Target;
//	bool m_TriggerSnap;
//
//	public bool m_SnapOnEndDrag = false;
//
//	public Alignment m_Alignment = Alignment.Center;
//
//	public float m_ThresholdVerocity = 200;
//
//	public Method m_Method = Method.EaseOutSine;
//
//	public float m_Duration = 0.5f;

//	/// <summary>
//	/// スクロールのコンテンツオブジェクト.
//	/// </summary>
//	public RectTransform scrollContent { get { return m_Content; } set { m_Content = value; } }
//
//	[SerializeField] RectTransform m_Content = null;


	/// <summary>
	/// インジケータ表示の元となるToggle.
	/// </summary>
//	public Toggle indicatorOriginToggle { get { return m_PagerToggle; } set { m_PagerToggle = value; } }

	[SerializeField] Toggle m_PagerToggle = null;

	List< Toggle> m_List = new List<Toggle>();

//	int m_LastCount;

	int m_Index;
	int m_Count;
	bool m_HasChanged;


	public event UnityAction<int> onIndexChanged;


	public void SetVirticalMode(bool vertical)
	{
		if (!m_LayoutGroup)
			m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();

		if (!m_LayoutGroup || vertical != (m_LayoutGroup is VerticalLayoutGroup))
		{
			var type = vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
			var layout = GetComponent<LayoutGroup>();

			if (!layout)
			{
				m_LayoutGroup = gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
			}
			else if (layout.GetType() == type)
			{
				RectOffset pad = layout ? layout.padding : new RectOffset();
				float space = (layout is HorizontalOrVerticalLayoutGroup) ? (layout as HorizontalOrVerticalLayoutGroup).spacing : 0;
				DestroyImmediate(layout);
				m_LayoutGroup = gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
				m_LayoutGroup.padding = pad;
				m_LayoutGroup.spacing = space;
			}
		}
	}

//	public HorizontalOrVerticalLayoutGroup layoutGroup
//	{
//		get
//		{
//			if (!m_LayoutGroup)
//				m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
//
//			if (!m_LayoutGroup || vertical != (m_LayoutGroup is VerticalLayoutGroup))
//			{
//				UpdateLauyout();
//
////				#if UNITY_EDITOR
////				if (!Application.isPlaying)
////					UnityEditor.EditorApplication.delayCall += UpdateLauyout;
////				else
////				#endif
////					UpdateLauyout();
//			}
//
//			return m_LayoutGroup;
//		}
//	}

	HorizontalOrVerticalLayoutGroup m_LayoutGroup;


//	void UpdateLauyout()
//	{
//		var type = vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
//		var layout = GetComponent<LayoutGroup>();
//
//		if (layout && layout.GetType() == type)
//		{
//		}
//		else if (!layout)
//		{
//			m_LayoutGroup = gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
//		}
//		else if (vertical != (layout is VerticalLayoutGroup))
//		{
//			#if UNITY_EDITOR
//			if (!Application.isPlaying)
//			{
//				ComponentConverter.ConvertTo(layout, type);
//				m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
//			}
//			else
//			#endif
//			{
//				RectOffset pad = layout ? layout.padding : new RectOffset();
//				float space = (layout is HorizontalOrVerticalLayoutGroup) ? (layout as HorizontalOrVerticalLayoutGroup).spacing : 0;
//				DestroyImmediate(layout);
//				m_LayoutGroup = gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
//				m_LayoutGroup.padding = pad;
//				m_LayoutGroup.spacing = space;
//			}
//		}
//	}
//
//	bool m_Vertical = false;
//	public bool vertical
//	{
//		get { return m_Vertical; }
//		set
//		{
//			if (m_Index == value)
//				return;
//
//			m_Index = value;
//			m_HasChanged = true;
//			onIndexChanged.Invoke(value);
//		}
//	}
//

	public int index
	{
		get { return m_Index; }
		set
		{
			if (m_Index == value)
				return;
			
			m_Index = value;
			m_HasChanged = true;

			if(onIndexChanged != null)
				onIndexChanged.Invoke(value);
		}
	}

	public int count
	{
		get { return m_Count; }
		set
		{
			if (m_Count == value)
				return;

			m_Count = value;
			m_HasChanged = true;
		}
	}


	void Start()
	{
		m_PagerToggle.gameObject.SetActive(false);
	}


	public void LateUpdate()
	{
		if (m_HasChanged && m_LayoutGroup)
		{
			m_HasChanged = false;

			// ページャが不足している場合、新しく生成します.
			while (m_List.Count < m_Count)
			{
				int i = m_List.Count;
				Toggle toggle = Object.Instantiate(m_PagerToggle);
				toggle.transform.SetParent(transform);
				toggle.onValueChanged.AddListener(_ => index = i);
				m_List.Add(toggle);
			}

			// 全ページャの表示状態を更新します.
			for (int i = 0; i < m_List.Count; i++)
			{
				m_List[i].gameObject.SetActive(i < m_Count);
				m_List[i].isOn = (i == index);
			}
		}
	}
}
