using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;


namespace Mobcast.Coffee.UI.ScrollModule
{
	/// <summary>
	/// レイアウトモジュール
	/// </summary>
	[Serializable]
	public class LayoutModule
	{
#region Serialize

#endregion Serialize

#region Public

		public ScrollRectEx handler { get; set; }

		public int firstPaddingSiblingIndex { get { return _firstPadding.transform.GetSiblingIndex(); } }

		public int lastPaddingSiblingIndex { get { return _lastPadding.transform.GetSiblingIndex(); } }

		/// <summary>
		/// セルビュー間のスペースを調整します.
		/// </summary>
		public float spacing
		{
			get { return layoutGroup.spacing; }
			set
			{
				layoutGroup.spacing = value;
				//_needToReload = true;
			}
		}

		/// <summary>
		/// スクロール領域のパディングサイズを調整します.
		/// </summary>
		public RectOffset padding
		{
			get { return layoutGroup.padding; }
			set
			{
				layoutGroup.padding = value;
				//_needToReload = true;
			}
		}

		ContentSizeFitter _contentSizeFitter;
		public ContentSizeFitter contentSizeFitter
		{
			get
			{
				if (!_contentSizeFitter)
				{
					_contentSizeFitter = layoutGroup.GetComponent<ContentSizeFitter>() ?? layoutGroup.gameObject.AddComponent<ContentSizeFitter>();
				}
				return _contentSizeFitter;
			}
		}


		HorizontalOrVerticalLayoutGroup _layoutGroup;
		public HorizontalOrVerticalLayoutGroup layoutGroup
		{
			get
			{
				if (!_layoutGroup)
				{
					RectTransform content = handler.content;
					if (!content)
						return null;

					_layoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
					if (!_layoutGroup)
					{
						var lg = content.GetComponent<LayoutGroup>();
						if (lg)
							GameObject.DestroyImmediate(lg);
						if (handler.scrollRect.vertical)
							_layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
						else
							_layoutGroup = content.gameObject.AddComponent<HorizontalLayoutGroup>();
					}
				}

				bool isVertical = handler.scrollRect.vertical;
				if (isVertical != (_layoutGroup is VerticalLayoutGroup))
				{
					RectTransform content = handler.content;
					float s = _layoutGroup.spacing;
					RectOffset p = _layoutGroup.padding;
					GameObject.DestroyImmediate(_layoutGroup);
					if (isVertical)
						_layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
					else
						_layoutGroup = content.gameObject.AddComponent<HorizontalLayoutGroup>();
					_layoutGroup.spacing = s;
					_layoutGroup.padding = p;
				}
				return _layoutGroup;
			}
		}

		public void Start()
		{
			// create the padder objects
			GameObject go = new GameObject("___FirstPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(handler.content, false);
			go.SetActive(false);
			_firstPadding = go.GetComponent<LayoutElement>();

			go = new GameObject("___LastPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(handler.content, false);
			go.SetActive(false);
			_lastPadding = go.GetComponent<LayoutElement>();
		}

		public void Update()
		{
		}

#endregion Public

#region Private

		LayoutElement _firstPadding;

		LayoutElement _lastPadding;


		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		void _AdjustPaddingSize(int startIndex, int endIndex, List<float> sizeArray, List<float> offsetArray)
		{
			if (handler.dataCount == 0 || handler.controller is DefaultScrollViewController)
				return;

			var firstSize = offsetArray[startIndex] - sizeArray[startIndex];
			var lastSize = offsetArray[offsetArray.Count - 1] - offsetArray[endIndex];

			_AdjustPaddingSize(_firstPadding, firstSize);
			_AdjustPaddingSize(_lastPadding, lastSize);
		}

		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		void _AdjustPaddingSize(LayoutElement padder, float size)
		{
			if (handler.scrollRect.vertical)
				padder.minHeight = size;
			else
				padder.minWidth = size;

			padder.gameObject.SetActive(size > 0);
		}

#endregion Private
	}
}
