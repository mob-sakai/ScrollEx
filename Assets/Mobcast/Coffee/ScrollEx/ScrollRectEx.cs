using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using EnhancedUI;
using UnityEngine.EventSystems;

//using TweenType = ScrollTweener.TweenType;
using Method = Mobcast.Coffee.Tweening.Method;
using System.Collections.Generic;

namespace Mobcast.Coffee
{
	public interface IScrollViewDelegate
	{
		int GetDataCount();

		float GetCellViewSize(int dataIndex);

		ScrollCellView GetCellView(int dataIndex);
	}

	/// <summary>
	/// The ScrollRectEx allows you to easily set up a dynamic scroller that will recycle views for you. This means
	/// that using only a handful of views, you can display thousands of rows. This will save memory and processing
	/// power in your application.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectEx : MonoBehaviour, IScrollSnap, IScrollPager, IScrollPool, IScrollViewDelegate
	{
		#region IScrollViewDelegate implementation

		public int GetDataCount()
		{
			return content.childCount - 2;
		}

		public float GetCellViewSize(int dataIndex)
		{
			var rt = content.GetChild(dataIndex) as RectTransform;
			var layoutElement = rt.GetComponent<LayoutElement>();

			if (layoutElement)
				return scrollRect.vertical ? layoutElement.preferredHeight : layoutElement.preferredWidth;
			else
				return scrollRect.vertical ? rt.rect.height : rt.rect.width;
		}

		public ScrollCellView GetCellView(int dataIndex)
		{
			return null;
		}

		#endregion

		public IScrollViewDelegate scrollViewDelegate { get { return m_ScrollViewDelegate ?? this; } set { m_ScrollViewDelegate = value; } }

		IScrollViewDelegate m_ScrollViewDelegate;


		public IScrollPool scrollPool { get { return m_ScrollPool ?? this; } set { m_ScrollPool = value; } }

		IScrollPool m_ScrollPool;


		[SerializeField]
		public ScrollPager m_ScrollPager;

		[SerializeField]
		public ScrollSnap m_ScrollSnap;

		[SerializeField]
		public ScrollSnap.Alignment m_Alignment = ScrollSnap.Alignment.Center;

		[SerializeField]
		public Method m_Method = Method.EaseOutSine;

		[SerializeField]
		public float m_Duration = 0.5f;

		#region Public

		/// <summary>
		/// The spacing to use between layout elements in the layout group.
		/// </summary>
		public float spacing { get { return layoutGroup.spacing; } }

		/// <summary>
		/// The padding to add around the child layout elements.
		/// </summary>
		public RectOffset padding { get { return layoutGroup.padding; } }

		/// <summary>
		/// Whether the scroller should loop the cell views.
		/// </summary>
		[SerializeField] bool m_Loop;

		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float scrollPosition
		{
			get { return m_ScrollPosition; }
			set
			{
				// 非ループ時はオーバーランを防ぐためにスクロール制限をかけます.
				if (!loop)
				{
					var min = -scrollRectSize;
					var max = GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1) + scrollRectSize;
					if (value < min || max < value)
					{
						value = Mathf.Clamp(value, min, max);
						scrollRect.velocity = Vector2.zero;
					}
				}

				// 座標が変更された時のみ、新しく座標を設定します.
				if (0.01f < Mathf.Abs(m_ScrollPosition - value))
				{
					_needRefleshActive = true;
					m_ScrollPosition = value;
					if (_scrollRect.vertical)
						_scrollRect.verticalNormalizedPosition = 1f - (m_ScrollPosition / scrollSize);
					else
						_scrollRect.horizontalNormalizedPosition = (m_ScrollPosition / scrollSize);
				}
			}
		}


		public bool loop
		{
			get { return m_Loop; }
			set
			{
				if (m_Loop == value)
					return;
				
				m_Loop = value;
				var oldPos = m_ScrollPosition;

				_Resize(false);
				scrollPosition = m_Loop
					? _loopFirstScrollPosition + oldPos
					: oldPos - _loopFirstScrollPosition;
			}
		}

		/// <summary>
		/// This is the first cell view index showing in the scroller's visible area
		/// </summary>
		public int StartCellViewIndex { get { return _activeCellViewsStartIndex; } }

		/// <summary>
		/// This is the last cell view index showing in the scroller's visible area
		/// </summary>
		public int EndCellViewIndex { get { return _activeCellViewsEndIndex; } }

		/// <summary>
		/// This is the first data index showing in the scroller's visible area
		/// </summary>
		public int StartDataIndex { get { return _activeCellViewsStartIndex % dataCount; } }

		/// <summary>
		/// This is the last data index showing in the scroller's visible area
		/// </summary>
		public int EndDataIndex { get { return _activeCellViewsEndIndex % dataCount; } }

		/// <summary>
		/// This is the number of cells in the scroller
		/// </summary>
		public virtual int dataCount { get { return scrollViewDelegate.GetDataCount(); } }


		/// <summary>
		/// The size of the visible portion of the scroller
		/// </summary>
		public float scrollRectSize { get { return scrollRect.vertical ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width; } }

		readonly Dictionary<int, Stack<ScrollCellView>> _pool = new Dictionary<int, Stack<ScrollCellView>>();

		public ScrollCellView RentCellView(ScrollCellView template)
		{
			var id = template.GetInstanceID();
			Stack<ScrollCellView> stack;
			if (_pool.TryGetValue(id, out stack) && 0 < stack.Count)
				return stack.Pop();

			var cellView = GameObject.Instantiate(template);
			cellView.templateId = id;
			cellView.transform.SetParent(_recycledCellViewContainer, false);
			cellView.transform.localPosition = Vector3.zero;
			cellView.transform.localRotation = Quaternion.identity;
			cellView.transform.localScale = Vector3.zero;
			return cellView;
		}

		public void ReturnCellView(ScrollCellView obj)
		{
			int id = obj.templateId;
			Stack<ScrollCellView> stack;
			if (!_pool.TryGetValue(id, out stack))
			{
				stack = new Stack<ScrollCellView>();
				_pool.Add(id, stack);
			}

			if (!stack.Contains(obj))
			{
				obj.transform.SetParent(_recycledCellViewContainer, false);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localScale = Vector3.zero;
				stack.Push(obj);
			}
		}


		/// <summary>
		/// セルビューをプールから取得または新規作成します.
		/// </summary>
		public ScrollCellView GetCellView(ScrollCellView template)
		{
			return scrollPool.RentCellView(template);
		}

		/// <summary>
		/// データをリロードし、内部的なキャッシュを全てリフレッシュします.
		/// </summary>
		public void ReloadData(float scrollPositionFactor = 0)
		{
			_reloadData = false;

			// アクティブなセルビューを全てプールに返却.
			while (_activeCellViews.Count > 0)
				_PoolCellView(_activeCellViews[0]);
			
			_activeCellViewsStartIndex = 0;
			_activeCellViewsEndIndex = 0;

			_Resize(false);

			m_ScrollPosition = scrollPositionFactor * scrollSize;
			if (_scrollRect.vertical)
				_scrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
			else
				_scrollRect.horizontalNormalizedPosition = scrollPositionFactor;
		}

		/// <summary>
		/// アクティブなセルを全てリフレッシュします.
		/// </summary>
		public void RefreshActiveCellViews()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				_activeCellViews[i].RefreshCellView();
			}
		}

		//		public void ClearAll()
		//		{
		//			ClearActive();
		//			ClearRecycled();
		//		}
		//
		//		public void ClearActive()
		//		{
		//			for (var i = 0; i < _activeCellViews.Count; i++)
		//			{
		//				DestroyImmediate(_activeCellViews[i].gameObject);
		//			}
		//			_activeCellViews.Clear();
		//		}
		//
		//		public void ClearRecycled()
		//		{
		//			for (var i = 0; i < _recycledCellViews.Count; i++)
		//			{
		//				DestroyImmediate(_recycledCellViews[i].gameObject);
		//			}
		//			_recycledCellViews.Clear();
		//		}

		public void JumpToDataIndex(int dataIndex,
		                            ScrollSnap.Alignment align,
		                            Method tweenType = Method.immediate,
		                            float tweenTime = 0f
		)
		{

			float normalizedOffset = (int)align * 0.5f;
			bool useSpacing = align == ScrollSnap.Alignment.Center;

			var cellOffsetPosition = 0f;

			if (normalizedOffset < 0 || 0 < normalizedOffset)
			{
				// calculate the cell offset position

				// get the cell's size
				var cellSize = scrollViewDelegate.GetCellViewSize(dataIndex);

				if (useSpacing)
				{
					// if using spacing add spacing from one side
					cellSize += spacing;

					// if this is not a bounday cell, then add spacing from the other side
					if (dataIndex > 0 && dataIndex < (dataCount - 1))
						cellSize += spacing;
				}

				// calculate the position based on the size of the cell and the offset within that cell
				cellOffsetPosition = cellSize * normalizedOffset;
			}

			var newScrollPosition = 0f;

			// cache the offset for quicker calculation
			var offset = -(normalizedOffset * scrollRectSize) + cellOffsetPosition;

			if (loop)
			{
				// if looping, then we need to determine the closest jump position.
				// we do that by checking all three sets of data locations, and returning the closest one

				// get the scroll positions for each data set.
				// Note: we are calculating the position based on the cell view index, not the data index here
				var set1Position = GetScrollPositionForCellViewIndex(dataIndex) + offset;
				var set2Position = GetScrollPositionForCellViewIndex(dataIndex + dataCount) + offset;
				var set3Position = GetScrollPositionForCellViewIndex(dataIndex + dataCount * 2) + offset;

				// get the offsets of each scroll position from the current scroll position
				var set1Diff = (Mathf.Abs(m_ScrollPosition - set1Position));
				var set2Diff = (Mathf.Abs(m_ScrollPosition - set2Position));
				var set3Diff = (Mathf.Abs(m_ScrollPosition - set3Position));

				// choose the smallest offset from the current position (the closest position)
				newScrollPosition = set1Diff < set2Diff
					? set1Diff < set3Diff ? set1Position : set3Position
					: set2Diff < set3Diff ? set2Position : set3Position;
			}
			else
			{
				newScrollPosition = GetScrollPositionForDataIndex(dataIndex) + offset;
			}


			if (useSpacing)
			{
				newScrollPosition -= spacing;
			}

			if (scrollRect.movementType == ScrollRect.MovementType.Clamped)
			{
				newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1));
			}

			m_ScrollSnap.StartScrollTween(tweenType, tweenTime, scrollPosition, newScrollPosition);
		}

		public int currentIndex
		{
			get
			{
				if (dataCount <= 0)
					return -1;
				var pos = scrollPosition + (scrollRectSize * Mathf.Clamp01((int)m_Alignment * 0.5f));
				var cellViewIndex = _GetCellViewIndexAtPosition(pos);
				return cellViewIndex % dataCount;
			}
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
		/// </summary>
		/// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForCellViewIndex(int cellViewIndex, bool beforeCell = true)
		{
			// 要素がない場合、0を返します.
			if (dataCount == 0)
				return 0;

			// 先頭の要素を指定した場合、paddingサイズを返します.
			if (cellViewIndex == 0 && beforeCell)
				return _scrollRect.vertical ? padding.top : padding.left;

			// 要素数を超えていた場合、最後のセルの位置を返します.
			if (_cellViewOffsetArray.Count <= cellViewIndex)
				return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];

			if (beforeCell)
				return _cellViewOffsetArray[cellViewIndex - 1] + spacing + (_scrollRect.vertical ? padding.top : padding.left);
			else
				return _cellViewOffsetArray[cellViewIndex] + (_scrollRect.vertical ? padding.top : padding.left);
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
		/// </summary>
		/// <param name="dataIndex">The data index to look for</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForDataIndex(int dataIndex, bool beforeCell = true)
		{
			return GetScrollPositionForCellViewIndex(loop ? dataCount + dataIndex : dataIndex, beforeCell);
		}

		/// <summary>
		/// Gets the index of a cell view at a given position
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <returns></returns>
		int _GetCellViewIndexAtPosition(float position)
		{
			return _GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
		}

		#endregion

		#region Private

		/// <summary>
		/// Cached reference to the scrollRect
		/// </summary>
		private ScrollRect _scrollRect;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		private RectTransform _scrollRectTransform;

		/// <summary>
		/// Flag to tell the scroller to reload the data
		/// </summary>
		private bool _reloadData = true;

		/// <summary>
		/// Flag to tell the scroller to refresh the active list of cell views
		/// </summary>
		private bool _needRefleshActive;

		/// <summary>
		/// List of views that have been recycled
		/// </summary>
		//		private SmallList<ScrollCellView> _recycledCellViews = new SmallList<ScrollCellView>();

		/// <summary>
		/// Cached reference to the element used to offset the first visible cell view
		/// </summary>
		private LayoutElement _firstPadder;

		/// <summary>
		/// Cached reference to the element used to keep the cell views at the correct size
		/// </summary>
		private LayoutElement _lastPadder;

		/// <summary>
		/// Cached reference to the container that holds the recycled cell views
		/// </summary>
		private RectTransform _recycledCellViewContainer;

		/// <summary>
		/// Internal list of cell view sizes. This is created when the data is reloaded 
		/// to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewSizeArray = new SmallList<float>();

		/// <summary>
		/// Internal list of cell view offsets. Each cell view offset is an accumulation 
		/// of the offsets previous to it.
		/// This is created when the data is reloaded to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

		/// <summary>
		/// The scrollers position
		/// </summary>
		private float m_ScrollPosition;

		/// <summary>
		/// The list of cell views that are currently being displayed
		/// </summary>
		private SmallList<ScrollCellView> _activeCellViews = new SmallList<ScrollCellView>();

		/// <summary>
		/// The index of the first cell view that is being displayed
		/// </summary>
		private int _activeCellViewsStartIndex;

		/// <summary>
		/// The index of the last cell view that is being displayed
		/// </summary>
		private int _activeCellViewsEndIndex;

		/// <summary>
		/// The index of the first element of the middle section of cell view sizes.
		/// Used only when looping
		/// </summary>
		private int _loopFirstCellIndex;

		/// <summary>
		/// The index of the last element of the middle seciton of cell view sizes.
		/// used only when looping
		/// </summary>
		private int _loopLastCellIndex;

		/// <summary>
		/// The scroll position of the first element of the middle seciotn of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopFirstScrollPosition;

		/// <summary>
		/// The scroll position of the last element of the middle section of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopLastScrollPosition;

		/// <summary>
		/// The position that triggers the scroller to jump to the end of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopFirstJumpTrigger;

		/// <summary>
		/// The position that triggers the scroller to jump to the start of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopLastJumpTrigger;

		/// <summary>
		/// The cached value of the last scroll rect size. This is checked every frame to see
		/// if the scroll rect has resized. If so, it will refresh.
		/// </summary>
		private float _lastScrollRectSize;

		/// <summary>
		/// The cached value of the last loop setting. This is checked every frame to see
		/// if looping was toggled. If so, it will refresh.
		/// </summary>
		private bool _lastLoop;

		/// <summary>
		/// The cell view index we are snapping to
		/// </summary>
		//		private int _snapCellViewIndex;

		private float scrollSize { get { return contentSize - scrollRectSize; } }

		private float contentSize { get { return (scrollRect.vertical ? content.rect.height : content.rect.width); } }


		/// <summary>
		/// This function will create an internal list of sizes and offsets to be used in all calculations.
		/// It also sets up the loop triggers and positions and initializes the cell views.
		/// </summary>
		/// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
		private void _Resize(bool keepPosition)
		{
			// cache the original position
			var originalScrollPosition = m_ScrollPosition;

			// clear out the list of cell view sizes and create a new list
			_cellViewSizeArray.Clear();
			var offset = _AddCellViewSizes();

			// if looping, we need to create three sets of size data
			if (loop)
			{
				// if the cells don't entirely fill up the scroll area, 
				// make some more size entries to fill it up
				if (offset < scrollRectSize)
				{
					int additionalRounds = Mathf.CeilToInt(scrollRectSize / offset);
					_DuplicateCellViewSizes(additionalRounds, _cellViewSizeArray.Count);
				}

				// set up the loop indices
				_loopFirstCellIndex = _cellViewSizeArray.Count;
				_loopLastCellIndex = _loopFirstCellIndex + _cellViewSizeArray.Count - 1;

				// create two more copies of the cell sizes
				_DuplicateCellViewSizes(2, _cellViewSizeArray.Count);
			}

			// calculate the offsets of each cell view
			_CalculateCellViewOffsets();

			// set the size of the active cell view container based on the number of cell views there are and each of their sizes
			if (_scrollRect.vertical)
				content.sizeDelta = new Vector2(content.sizeDelta.x, _cellViewOffsetArray.Last() + padding.top + padding.bottom);
			else
				content.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, content.sizeDelta.y);

			// if looping, set up the loop positions and triggers
			if (loop)
			{
				_loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, beforeCell: true) + (spacing * 0.5f);
				_loopLastScrollPosition = GetScrollPositionForCellViewIndex(_loopLastCellIndex, beforeCell: false) - scrollRectSize + (spacing * 0.5f);

				_loopFirstJumpTrigger = _loopFirstScrollPosition - scrollRectSize;
				_loopLastJumpTrigger = _loopLastScrollPosition + scrollRectSize;
			}

			// create the visibile cells
			_ResetVisibleCellViews();

			// if we need to maintain our original position
			if (keepPosition)
				scrollPosition = originalScrollPosition;
			else if (m_ScrollSnap.snapOnEndDrag)
				JumpToDataIndex(0, m_Alignment, Method.immediate, 0);
			else
				scrollPosition = loop ? _loopFirstScrollPosition : 0;
		}

		float _AddCellViewSizes()
		{
			var offset = 0f;
			for (var i = 0; i < dataCount; i++)
			{
				_cellViewSizeArray.Add(scrollViewDelegate.GetCellViewSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
				offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
			}

			return offset;
		}

		void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			for (var i = 0; i < numberOfTimes; i++)
			{
				for (var j = 0; j < cellCount; j++)
				{
					_cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? layoutGroup.spacing : 0));
				}
			}
		}

		void _CalculateCellViewOffsets()
		{
			_cellViewOffsetArray.Clear();
			var offset = 0f;
			for (var i = 0; i < _cellViewSizeArray.Count; i++)
			{
				offset += _cellViewSizeArray[i];
				_cellViewOffsetArray.Add(offset);
			}
		}

		//		/// <summary>
		//		/// Get a recycled cell with a given identifier if available
		//		/// </summary>
		//		/// <param name="cellPrefab">The prefab to check for</param>
		//		/// <returns></returns>
		//		private ScrollCellView _GetRecycledCellView(ScrollCellView cellPrefab)
		//		{
		//			for (var i = 0; i < _recycledCellViews.Count; i++)
		//			{
		//				if (_recycledCellViews[i].cellIdentifier == cellPrefab.cellIdentifier)
		//				{
		//					// the cell view was found, so we use this recycled one.
		//					// we also remove it from the recycled list
		//					var cellView = _recycledCellViews.RemoveAt(i);
		//					return cellView;
		//				}
		//			}
		//
		//			return null;
		//		}

		/// <summary>
		/// 表示すべきセルビューをリセットします.
		/// </summary>
		private void _ResetVisibleCellViews()
		{
			int startIndex;
			int endIndex;

			// 現在見えているセルビューの範囲を取得します.
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// 不要なセルビューをプールします.
			var i = 0;
			SmallList<int> remainingCellIndices = new SmallList<int>();
			while (i < _activeCellViews.Count)
			{
				if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
				{
					_PoolCellView(_activeCellViews[i]);
				}
				else
				{
					// このセルインデックスは新しい描画範囲でも利用可能です.
					remainingCellIndices.Add(_activeCellViews[i].cellIndex);
					i++;
				}
			}

			if (remainingCellIndices.Count == 0)
			{
				for (i = startIndex; i <= endIndex; i++)
					_AddCellView(i, false);
			}
			else
			{
				// セルインデックスを再利用します.
				for (i = endIndex; i >= startIndex; i--)
				{
					if (i < remainingCellIndices.First())
						_AddCellView(i, true);
				}

				for (i = startIndex; i <= endIndex; i++)
				{
					if (i > remainingCellIndices.Last())
						_AddCellView(i, false);
				}
			}

			_activeCellViewsStartIndex = startIndex;
			_activeCellViewsEndIndex = endIndex;

			// パディングサイズを調整します.
			_AdjustPadding();
		}

		//		/// <summary>
		//		/// セルビューをスクロールビュープールに返却します.
		//		/// Recycles all the active cells
		//		/// </summary>
		//		private void _PoolAllCells()
		//		{
		//			while (_activeCellViews.Count > 0)
		//				_PoolCellView(_activeCellViews[0]);
		//			_activeCellViewsStartIndex = 0;
		//			_activeCellViewsEndIndex = 0;
		//		}

		/// <summary>
		/// セルビューをスクロールビュープールに返却します.
		/// </summary>
		private void _PoolCellView(ScrollCellView cellView)
		{
			cellView.OnWillRecycleCellView();

			_activeCellViews.Remove(cellView);

			scrollPool.ReturnCellView(cellView);

			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active = false;

			cellView.OnChangedCellViewVisibility();
		}

		/// <summary>
		/// セルビューを追加します.
		/// 可能であれば、プールからセルビューを再利用します.
		/// </summary>
		private void _AddCellView(int cellIndex, bool atStart)
		{
			if (dataCount == 0 || scrollViewDelegate == this)
				return;

			// 新しく追加されるセルビューを取得します.
			var dataIndex = cellIndex % dataCount;
			var cellView = scrollViewDelegate.GetCellView(dataIndex);

			cellView.cellIndex = cellIndex;
			cellView.dataIndex = dataIndex;
			cellView.active = true;

			// スクロールにセルビューを追加します.
			cellView.transform.SetParent(content, false);
			cellView.transform.localScale = Vector3.one;

			LayoutElement layoutElement = cellView.GetComponent<LayoutElement>()
			                              ?? cellView.gameObject.AddComponent<LayoutElement>();

			// セルビューのサイズを設定します.
			if (_scrollRect.vertical)
				layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
			else
				layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);

			if (atStart)
				_activeCellViews.AddStart(cellView);
			else
				_activeCellViews.Add(cellView);

			// ヒエラルキー順を調整します.
			cellView.transform.SetSiblingIndex(atStart ? 1 : content.childCount - 2);

			cellView.OnChangedCellViewVisibility();
		}

		/// <summary>
		/// 上下のパディングサイズを調整します.
		/// </summary>
		private void _AdjustPadding()
		{
			if (dataCount == 0 || scrollViewDelegate == this)
				return;

			var firstSize = _cellViewOffsetArray[_activeCellViewsStartIndex] - _cellViewSizeArray[_activeCellViewsStartIndex];
			var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[_activeCellViewsEndIndex];

			_SetPadding(_firstPadder, firstSize);
			_SetPadding(_lastPadder, lastSize);
//			if (_scrollRect.vertical)
//			{
//				// set the first padder and toggle its visibility
//				_firstPadder.minHeight = firstSize;
//				_firstPadder.gameObject.SetActive(_firstPadder.minHeight > 0);
//
//				// set the last padder and toggle its visibility
//				_lastPadder.minHeight = lastSize;
//				_lastPadder.gameObject.SetActive(_lastPadder.minHeight > 0);
//			}
//			else
//			{
//				// set the first padder and toggle its visibility
//				_firstPadder.minWidth = firstSize;
//				_firstPadder.gameObject.SetActive(_firstPadder.minWidth > 0);
//
//				// set the last padder and toggle its visibility
//				_lastPadder.minWidth = lastSize;
//				_lastPadder.gameObject.SetActive(_lastPadder.minWidth > 0);
//			}
		}

		private void _SetPadding(LayoutElement padder, float size)
		{
			if (_scrollRect.vertical)
				padder.minHeight = size;
			else
				padder.minWidth = size;

			padder.gameObject.SetActive(size > 0);
		}

		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		private void _RefreshActive()
		{
			_needRefleshActive = false;

			int startIndex;
			int endIndex;
			var nextVelocity = Vector2.zero;

			// if looping, check to see if we scrolled past a trigger
			if (loop)
			{
				if (m_ScrollPosition < _loopFirstJumpTrigger)
				{
					nextVelocity = _scrollRect.velocity;
					scrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - m_ScrollPosition);
					_scrollRect.velocity = nextVelocity;
				}
				else if (m_ScrollPosition > _loopLastJumpTrigger)
				{
					nextVelocity = _scrollRect.velocity;
					scrollPosition = _loopFirstScrollPosition + (m_ScrollPosition - _loopLastJumpTrigger);
					_scrollRect.velocity = nextVelocity;
				}
			}

			// get the range of visibile cells
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == _activeCellViewsStartIndex && endIndex == _activeCellViewsEndIndex)
				return;

			// recreate the visibile cells
			_ResetVisibleCellViews();
		}

		/// <summary>
		/// Determines which cells can be seen
		/// </summary>
		/// <param name="startIndex">The index of the first cell visible</param>
		/// <param name="endIndex">The index of the last cell visible</param>
		private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			startIndex = 0;
			endIndex = 0;

			// get the positions of the scroller
			var startPosition = m_ScrollPosition;
			var endPosition = m_ScrollPosition + scrollRectSize;//(m_ScrollRect.vertical ? m_ScrollRectTransform.rect.height : m_ScrollRectTransform.rect.width);

			// calculate each index based on the positions
			startIndex = _GetCellViewIndexAtPosition(startPosition);
			endIndex = _GetCellViewIndexAtPosition(endPosition);
		}

		/// <summary>
		/// Gets the index of a cell at a given position based on a subset range.
		/// This function uses a recursive binary sort to find the index faster.
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <param name="startIndex">The first index of the range</param>
		/// <param name="endIndex">The last index of the rnage</param>
		/// <returns></returns>
		private int _GetCellIndexAtPosition(float position, int startIndex, int endIndex)
		{
			// if the range is invalid, then we found our index, return the start index
			if (startIndex >= endIndex)
				return startIndex;

			// determine the middle point of our binary search
			var middleIndex = (startIndex + endIndex) / 2;

			// if the middle index is greater than the position, then search the last
			// half of the binary tree, else search the first half
			if ((_cellViewOffsetArray[middleIndex] + (_scrollRect.vertical ? padding.top : padding.left)) >= position)
				return _GetCellIndexAtPosition(position, startIndex, middleIndex);
			else
				return _GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
		}

		public RectTransform content { get { return scrollRect.content; } }

		//		RectTransform m_Content;


		public ScrollRect scrollRect
		{
			get
			{
				if (!_scrollRect)
					_scrollRect = GetComponent<ScrollRect>();
				return _scrollRect;
			}
		}

		public HorizontalOrVerticalLayoutGroup layoutGroup
		{
			get
			{
				if (!m_LayoutGroup)
					m_LayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
				return m_LayoutGroup;
			}
		}

		HorizontalOrVerticalLayoutGroup m_LayoutGroup;


		//		public Scrollbar scrollbar { get { return scrollRect.vertical ? scrollRect.verticalScrollbar : scrollRect.horizontalScrollbar; } }

		//==== v MonoBehavior Callbacks v ====
		/// <summary>
		/// Caches and initializes the scroller
		/// </summary>
		protected virtual void Awake()
		{
			// cache some components
//			m_ScrollRect = this.GetComponent<ScrollRect>();
			_scrollRectTransform = scrollRect.transform as RectTransform;
			m_ScrollSnap.target = this;
			m_ScrollPager.target = this;

			if (!scrollRect.vertical && !scrollRect.horizontal)
			{
				//無効なスクロール
				enabled = false;
				return;
			}

			// force the scroller to scroll in the direction we want
			// set the containers anchor and pivot
			RectTransform c = content;
			HorizontalOrVerticalLayoutGroup lg = layoutGroup;
			if (scrollRect.vertical)
			{
				scrollRect.horizontal = false;
				c.anchorMin = new Vector2(0, 1);
				c.anchorMax = Vector2.one;
				c.pivot = new Vector2(0.5f, 1f);
			}
			else
			{
				scrollRect.vertical = false;
				c.anchorMin = Vector2.zero;
				c.anchorMax = new Vector2(0, 1f);
				c.pivot = new Vector2(0, 0.5f);
			}

			c.offsetMax = Vector2.zero;
			c.offsetMin = Vector2.zero;
			c.localPosition = Vector3.zero;
			c.localRotation = Quaternion.identity;
			c.localScale = Vector3.one;

			lg.childAlignment = TextAnchor.UpperLeft;
			lg.childForceExpandHeight = true;
			lg.childForceExpandWidth = true;

			// create the padder objects
			GameObject go = new GameObject("FirstPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(c, false);
			go.SetActive(false);
			_firstPadder = go.GetComponent<LayoutElement>();

			go = new GameObject("LastPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(c, false);
			go.SetActive(false);
			_lastPadder = go.GetComponent<LayoutElement>();

			// create the recycled cell view container
			go = new GameObject("CellViewPool", typeof(RectTransform));
			go.transform.SetParent(_scrollRect.transform, false);
			go.SetActive(false);
			_recycledCellViewContainer = go.GetComponent<RectTransform>();

			// set up the last values for updates
			_lastScrollRectSize = scrollRectSize;
			_lastLoop = loop;

			_Resize(false);

			// スクロール値が変化した時、コールバックを受け取ります.
			scrollRect.onValueChanged.AddListener(val =>
				{
					scrollPosition = scrollRect.vertical
						? (1f - val.y) * scrollSize
						: val.x * scrollSize;
					_RefreshActive();
				});

//			m_ScrollSnap.OnEndDrag();
		}

		/// <summary>
		/// LateUpdate is called every frame, if the Behaviour is enabled.
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (_reloadData)
			{
				// if the reload flag is true, then reload the data
				ReloadData();
			}

			m_ScrollPager.Update();
			m_ScrollSnap.Update();

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if ((loop && _lastScrollRectSize != scrollRectSize) || (loop != _lastLoop))
			{
				_Resize(true);
				_lastScrollRectSize = scrollRectSize;

				_lastLoop = loop;
			}

			if (_needRefleshActive)
			{
				_RefreshActive();
			}
		}
		//==== ^ MonoBehavior Callbacks ^ ====

		//#### v IScrollSnap Implementation v ####
		/// <summary>
		/// Called by the EventSystem when a Scroll event occurs.
		/// </summary>
		public virtual void OnScroll(PointerEventData eventData)
		{
			m_ScrollSnap.OnScroll();
		}

		/// <summary>
		/// Called before a drag is started.
		/// </summary>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			m_ScrollSnap.OnBeginDrag();
		}

		/// <summary>
		/// Called by the EventSystem once dragging ends.
		/// </summary>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			m_ScrollSnap.OnEndDrag();
		}

		public void OnTriggerSnap()
		{
			if (dataCount == 0)
				return;

			// スナップするインデックスを計算します.
			var snapPosition = scrollPosition + (scrollRectSize * Mathf.Clamp01((int)m_Alignment * 0.5f));
			var snapCellViewIndex = _GetCellViewIndexAtPosition(snapPosition);
			var snapDataIndex = snapCellViewIndex % dataCount;

			JumpToDataIndex(snapDataIndex, m_Alignment, m_Method, m_Duration);
		}

		public void OnChangeTweenPosition(float value, bool positive)
		{
			// ループ時はループポイント跨ぎを考慮します.
			if (loop)
			{
				if (positive && value > _loopLastJumpTrigger)
					value = _loopFirstScrollPosition + (value - _loopLastJumpTrigger);
				else if (!positive && value < _loopFirstJumpTrigger)
					value = _loopLastScrollPosition - (_loopFirstJumpTrigger - value);
			}
			scrollPosition = value;
		}
		//#### ^ IScrollSnap Implementation ^ ####

		#endregion
	}
}