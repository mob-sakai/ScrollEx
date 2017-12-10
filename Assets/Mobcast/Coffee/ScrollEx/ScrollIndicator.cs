using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// 現在のインデックスをインジケータ表示するモジュールです.
	/// インジケータをクリックすると対象のインデックスに移動できます.
	/// </summary>
	public class ScrollIndicator : MonoBehaviour
	{
		#region Serialize

		[SerializeField] public Toggle m_Template = null;
		[SerializeField] int m_Limit = 30;

		#endregion Serialize

		#region Public

		public event Action<int> onChangedIndex;

		public Toggle template
		{
			get { return m_Template; }
			set
			{
				if (m_Template == value)
					return;
				
				m_Template = value;
				m_Template.gameObject.SetActive(false);

				_changed = true;
			}
		}

		public int limit
		{
			get { return m_Limit; }
			set
			{
				if (m_Limit == value)
					return;
				
				m_Limit = value;
				_changed = true;
			}
		}

		public int index
		{
			get { return _index; }
			set
			{
				if (_index == value)
					return;
				
				_index = value;
				_changed = true;
			}
		}

		public int count
		{
			get { return _count; }
			set
			{
				if (_count == value)
					return;

				_count = value;
				_changed = true;
			}
		}

		public LayoutGroup layoutGroup
		{
			get
			{
				if (!_layoutGroup)
					_layoutGroup = GetComponent<LayoutGroup>();
				return _layoutGroup;
			}
		}

		#endregion Public

		readonly List< Toggle> _toggles = new List<Toggle>();
		int _index;
		int _count;

		bool _changed;
		LayoutGroup _layoutGroup;

		void Start()
		{
			if (m_Template)
				m_Template.gameObject.SetActive(false);
		}

		void LateUpdate()
		{
			if (!_changed || !layoutGroup || !m_Template)
				return;

			_changed = false;

			// 不足している場合、新しく生成します.
			int max = Mathf.Min(m_Limit, _count);
			while (_toggles.Count < max)
			{
				int i = _toggles.Count;
				Toggle toggle = UnityEngine.Object.Instantiate(m_Template);
				toggle.name = "PageToggle_" + i;
				toggle.transform.SetParent(layoutGroup.transform, false);

				toggle.onValueChanged.AddListener(flag =>
					{
						if (!flag || index == i)
							return;

						if(onChangedIndex != null)
							onChangedIndex(i);
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
	}
}
