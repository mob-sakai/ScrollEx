using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using EnhancedUI;
using UnityEngine.EventSystems;

//using TweenType = ScrollTweener.TweenType;
using TweenMethod = Mobcast.Coffee.Tweening.TweenMethod;
using System.Collections.Generic;

namespace Mobcast.Coffee
{
//	public interface IScrollViewDelegate
//	{
//		int GetDataCount();
//
//		float GetCellViewSize(int dataIndex);
//
//		ScrollCellView GetCellView(int dataIndex);
//	}

	/// <summary>
	/// The ScrollRectEx allows you to easily set up a dynamic scroller that will recycle views for you. This means
	/// that using only a handful of views, you can display thousands of rows. This will save memory and processing
	/// power in your application.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectEx : MonoBehaviour, IScrollSnapHandler, IScrollPagerHandler
	//, IScrollViewDelegate
	{
		public enum Alignment
		{
			TopOrLeft,
			Center,
			BottomOrRight,
		}


//		public IScrollViewDelegate scrollViewDelegate { get { return m_ScrollViewDelegate ?? this; } set { m_ScrollViewDelegate = value; } }
//
//		IScrollViewDelegate m_ScrollViewDelegate;

		public IScrollViewController controller { get; set; }

		public IScrollCellViewPool scrollPool { get; set; }

		public ScrollPager scrollPager { get{ return m_ScrollPager;} }

		public ScrollSnap scrollSnap { get{ return m_ScrollSnap;} }

#region Serialize

		[SerializeField] ScrollPager m_ScrollPager;

		[SerializeField] ScrollSnap m_ScrollSnap;

		[SerializeField] Alignment m_Alignment = Alignment.Center;

		[SerializeField] TweenMethod m_TweenMethod = TweenMethod.EaseOutSine;

		[SerializeField] float m_TweenDuration = 0.5f;

		[SerializeField] bool m_Loop;

#endregion Serialize

#region Public

		public RectTransform content { get { return scrollRect.content; } }

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

		/// <summary>
		/// セルビュー間のスペースを調整します.
		/// </summary>
		public float spacing { get { return layoutGroup.spacing; } set {layoutGroup.spacing = value; _needToReload = true;} }

		/// <summary>
		/// スクロール領域のパディングサイズを調整します.
		/// </summary>
		public RectOffset padding { get { return layoutGroup.padding; }  set {layoutGroup.padding = value; _needToReload = true;} }


		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float scrollPosition
		{
			get { return m_ScrollPosition; }
			set
			{
				// 非ループ時はスクロールのオーバーランを防ぐために、スクロール制限をかけます.
				if (!loop)
				{
					var min = -scrollRectSize;
					var max = GetScrollPositionFromIndex(_cellViewSizeArray.Count - 1) + scrollRectSize;
					if (value < min || max < value)
					{
						value = Mathf.Clamp(value, min, max);
						scrollRect.velocity = Vector2.zero;
					}
				}

				// 座標が変更された時のみ、新しく座標を設定します.
				if (0.01f < Mathf.Abs(m_ScrollPosition - value))
				{
					_needToReflesh = true;
					m_ScrollPosition = value;
					if (_scrollRect.vertical)
						_scrollRect.verticalNormalizedPosition = 1f - (m_ScrollPosition / scrollSize);
					else
						_scrollRect.horizontalNormalizedPosition = (m_ScrollPosition / scrollSize);
				}
			}
		}

		/// <summary>
		/// スクロールがループするかどうかを設定/取得します.
		/// </summary>
		public bool loop
		{
			get { return m_Loop; }
			set
			{
				if (m_Loop == value)
					return;
				
				m_Loop = value;
				var oldPos = m_ScrollPosition;

				// セルサイズ配列を再計算します.
				_Resize(false);

				scrollPosition = m_Loop
					? _loopFirstScrollPosition + oldPos
					: oldPos - _loopFirstScrollPosition;
			}
		}

		/// <summary>
		/// 現在アクティブなセルビュー.
		/// </summary>
		public SmallList<ScrollCellView> activeCellViews { get { return _activeCellViews; } }

		/// <summary>
		/// 現在のインデックス(Alignment基準)を取得します.
		/// </summary>
		public int activeIndex
		{
			get
			{
				if (dataCount <= 0 || _cellViewOffsetArray.Count <= 0)
					return -1;

				var pos = scrollPosition + (scrollRectSize * Mathf.Clamp01((int)m_Alignment * 0.5f));
				var cellViewIndex = _GetIndexFromScrollPosition(pos, 0, _cellViewOffsetArray.Count - 1);
				return cellViewIndex % dataCount;
			}
		}

		/// <summary>
		/// 現在先頭にある、アクティブなセルビューのインデックス.
		/// </summary>
		public int activeStartCellIndex{get;set;}

		/// <summary>
		/// 現在末尾にある、アクティブなセルビューのインデックス.
		/// </summary>
		public int activeEndCellIndex{get;set;}

		/// <summary>
		/// データの要素数を取得します.
		/// </summary>
		public virtual int dataCount { get { return controller.GetDataCount(); } }


		/// <summary>
		/// ScrollRect自体の大きさです.
		/// </summary>
		public float scrollRectSize { get { return scrollRect.vertical ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width; } }


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
			_needToReload = false;

			// アクティブなセルビューを全てプールに返却.
			while (_activeCellViews.Count > 0)
				_PoolCellView(_activeCellViews[0]);
			
			activeStartCellIndex = 0;
			activeEndCellIndex = 0;

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


		/// <summary>
		/// 目的のインデックスに移動します.
		/// </summary>
		public void JumpToDataIndex(int dataIndex, Alignment align, TweenMethod tweenType = TweenMethod.immediate, float tweenTime = 0f)
		{
			float normalizedOffset = (int)align * 0.5f;
			bool useSpacing = align == Alignment.Center;

			var cellOffsetPosition = 0f;

			if (normalizedOffset < 0 || 0 < normalizedOffset)
			{
				// calculate the cell offset position

				// get the cell's size
				var cellSize = controller.GetCellViewSize(dataIndex);

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
				var set1Position = GetScrollPositionFromIndex(dataIndex) + offset;
				var set2Position = GetScrollPositionFromIndex(dataIndex + dataCount) + offset;
				var set3Position = GetScrollPositionFromIndex(dataIndex + dataCount * 2) + offset;

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
				newScrollPosition = GetScrollPositionFromIndex(dataIndex) + offset;
			}


			if (useSpacing)
			{
				newScrollPosition -= spacing;
			}

			if (scrollRect.movementType == ScrollRect.MovementType.Clamped)
			{
				newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionFromIndex(_cellViewSizeArray.Count - 1));
			}

			scrollSnap.StartScrollTween(tweenType, tweenTime, scrollPosition, newScrollPosition);
		}


		/// <summary>
		/// インデックスからスクロール座標を取得します.
		/// </summary>
		public float GetScrollPositionFromIndex(int cellViewIndex, bool beforeCell = true)
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

#endregion

		#region Private

		ScrollRect _scrollRect;

		RectTransform _scrollRectTransform;

		bool _needToReload = true;

		bool _needToReflesh;

		LayoutElement _firstPadding;

		LayoutElement _lastPadding;

//		RectTransform _cellViewPool;

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
		/// スクロール領域をリサイズします.
		/// 各セルのセルサイズ、オフセットを再計算します.
		/// </summary>
		private void _Resize(bool keepPosition)
		{
			// リサイズ前座標.
			var originalScrollPosition = m_ScrollPosition;

			// セルサイズを再計算します.
			_cellViewSizeArray.Clear();
			var offset = 0f;
			for (var i = 0; i < dataCount; i++)
			{
				_cellViewSizeArray.Add(controller.GetCellViewSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
				offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
			}

			// ループの場合、セルサイズ配列を少なくとも2回複製して結合する必要があります.
			if (loop)
			{
				// スクロール領域に足りない大きさ分は、セルサイズ配列を複製して結合します.
				if (offset < scrollRectSize)
					_DuplicateCellViewSizes(Mathf.CeilToInt(scrollRectSize / offset));

				// ループ開始/終了インデックスを計算します.
				_loopFirstCellIndex = _cellViewSizeArray.Count;
				_loopLastCellIndex = _loopFirstCellIndex + _cellViewSizeArray.Count - 1;

				// セルサイズ配列を2回複製して結合します.
				_DuplicateCellViewSizes(2);
			}

			// セルのオフセットを再計算します.
			_cellViewOffsetArray.Clear();
			offset = 0f;
			for (var i = 0; i < _cellViewSizeArray.Count; i++)
			{
				offset += _cellViewSizeArray[i];
				_cellViewOffsetArray.Add(offset);
			}

			// スクロール領域のサイズを設定します.
			if (_scrollRect.vertical)
				content.sizeDelta = new Vector2(content.sizeDelta.x, _cellViewOffsetArray.Last() + padding.top + padding.bottom);
			else
				content.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, content.sizeDelta.y);

			// ループ開始/終了座標を計算します.
			if (loop)
			{
				_loopFirstScrollPosition = GetScrollPositionFromIndex(_loopFirstCellIndex, beforeCell: true) + (spacing * 0.5f);
				_loopLastScrollPosition = GetScrollPositionFromIndex(_loopLastCellIndex, beforeCell: false) - scrollRectSize + (spacing * 0.5f);

				_loopFirstJumpTrigger = _loopFirstScrollPosition - scrollRectSize;
				_loopLastJumpTrigger = _loopLastScrollPosition + scrollRectSize;
			}

			// 表示すべきセルビューをすべてリセットします.
			_ResetVisibleCellViews();

			// keepPositionを指定した場合、座標を戻します.
			if (keepPosition)
				scrollPosition = originalScrollPosition;
			// スナップ設定されているとき、スナップを実行します.
			else if (scrollSnap.snapOnEndDrag)
				JumpToDataIndex(0, m_Alignment, TweenMethod.immediate, 0);
			// それ以外の場合、初期位置に戻します.
			else
				scrollPosition = loop ? _loopFirstScrollPosition : 0;
		}

//		float _AddCellViewSizes()
//		{
//			var offset = 0f;
//			for (var i = 0; i < dataCount; i++)
//			{
//				_cellViewSizeArray.Add(scrollViewDelegate.GetCellViewSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
//				offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
//			}
//
//			return offset;
//		}


		/// <summary>
		/// セルサイズ配列を複製し、末尾に追加します.
		/// </summary>
		void _DuplicateCellViewSizes(int numberOfTimes)
		{
			int cellCount = _cellViewSizeArray.Count;
			for (var i = 0; i < numberOfTimes; i++)
				for (var j = 0; j < cellCount; j++)
					_cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? layoutGroup.spacing : 0));
		}

//		void _CalculateCellViewOffsets()
//		{
//			_cellViewOffsetArray.Clear();
//			var offset = 0f;
//			for (var i = 0; i < _cellViewSizeArray.Count; i++)
//			{
//				offset += _cellViewSizeArray[i];
//				_cellViewOffsetArray.Add(offset);
//			}
//		}

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

			// 現在アクティブな開始/終了インデックスを更新.
			activeStartCellIndex = startIndex;
			activeEndCellIndex = endIndex;

			// パディングサイズを調整します.
			_AdjustPaddingSize(startIndex, endIndex);
		}

		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		private void _AdjustPaddingSize(int startIndex, int endIndex)
		{
			if (dataCount == 0 || controller is DefaultScrollViewController)
				return;

			var firstSize = _cellViewOffsetArray[startIndex] - _cellViewSizeArray[startIndex];
			var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[endIndex];

			_AdjustPaddingSize(_firstPadding, firstSize);
			_AdjustPaddingSize(_lastPadding, lastSize);
		}

		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		private void _AdjustPaddingSize(LayoutElement padder, float size)
		{
			if (_scrollRect.vertical)
				padder.minHeight = size;
			else
				padder.minWidth = size;

			padder.gameObject.SetActive(size > 0);
		}


		/// <summary>
		/// セルビューをスクロールビュープールに返却します.
		/// </summary>
		private void _PoolCellView(ScrollCellView cellView)
		{
			cellView.OnBeforePool();

			_activeCellViews.Remove(cellView);

			scrollPool.ReturnCellView(cellView);

			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active = false;

			cellView.OnChangedVisibility(false);
		}

		/// <summary>
		/// セルビューを追加します.
		/// 可能であれば、プールからセルビューを再利用します.
		/// </summary>
		private void _AddCellView(int cellIndex, bool atStart)
		{
			if (dataCount == 0 || controller is DefaultScrollViewController)
				return;

			// 新しく追加されるセルビューを取得します.
			var dataIndex = cellIndex % dataCount;
			var cellView = controller.GetCellView(dataIndex);

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

			cellView.OnChangedVisibility(true);
		}


		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		private void _RefreshActive()
		{
			_needToReflesh = false;

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
			// 現在見えているセルビューの範囲を取得します.
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == activeStartCellIndex && endIndex == activeEndCellIndex)
				return;

			// recreate the visibile cells
			_ResetVisibleCellViews();
		}

		/// <summary>
		/// 現在見えているセルビューの範囲を取得します.
		/// </summary>
		private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			// スクロール座標からインデックスを取得します.
			int lastIndex =  _cellViewOffsetArray.Count - 1;
			startIndex = _GetIndexFromScrollPosition(m_ScrollPosition, 0, lastIndex);
			endIndex = _GetIndexFromScrollPosition(m_ScrollPosition + scrollRectSize, 0, lastIndex);
		}

		/// <summary>
		/// 座標からインデックスを取得します.
		/// 各セルのオフセットはリサイズ時に予め保存されています.
		/// 再帰的な二分探索により高速に探索されます.
		/// </summary>
		private int _GetIndexFromScrollPosition(float position, int startIndex, int endIndex)
		{
			// 二分探索終了.
			if (startIndex >= endIndex)
				return startIndex;

			// 中間点を比較し、前半部分または後半部分を再帰的に二分検索.
			var middleIndex = (startIndex + endIndex) / 2;
			return position <= (_cellViewOffsetArray[middleIndex] + (_scrollRect.vertical ? padding.top : padding.left))
					? _GetIndexFromScrollPosition(position, startIndex, middleIndex)
					: _GetIndexFromScrollPosition(position, middleIndex + 1, endIndex);
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
			_firstPadding = go.GetComponent<LayoutElement>();

			go = new GameObject("LastPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(c, false);
			go.SetActive(false);
			_lastPadding = go.GetComponent<LayoutElement>();

			// スナップにハンドラーを設定
			scrollSnap.handler = this;

			// ページャハンドラーを設定
			scrollPager.handler = this;

			// デフォルトのセルビュープールを生成.
			if (scrollPool == null)
			{
				go = new GameObject("CellViewPool", typeof(RectTransform));
				go.transform.SetParent(_scrollRect.transform, false);
				go.SetActive(false);
//				_cellViewPool = go.GetComponent<RectTransform>();
				scrollPool = new ScrollCellViewPool(go.GetComponent<RectTransform>());
			}

			// デフォルトのスクロールビューコントローラ生成.
			if (controller == null)
			{
				controller = new DefaultScrollViewController(scrollRect);
			}

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
			if (_needToReload)
			{
				// if the reload flag is true, then reload the data
				ReloadData();
			}

			scrollPager.Update();
			scrollSnap.Update();

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if ((loop && _lastScrollRectSize != scrollRectSize) || (loop != _lastLoop))
			{
				_Resize(true);
				_lastScrollRectSize = scrollRectSize;

				_lastLoop = loop;
			}

			if (_needToReflesh)
			{
				_RefreshActive();
			}
		}
		//==== ^ MonoBehavior Callbacks ^ ====
		#endregion



#region IScrollSnap implementation
		/// <summary>
		/// Called by the EventSystem when a Scroll event occurs.
		/// </summary>
		public virtual void OnScroll(PointerEventData eventData)
		{
			scrollSnap.OnScroll();
		}

		/// <summary>
		/// Called before a drag is started.
		/// </summary>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			scrollSnap.OnBeginDrag();
		}

		/// <summary>
		/// Called by the EventSystem once dragging ends.
		/// </summary>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			scrollSnap.OnEndDrag();
		}

		public void OnTriggerSnap()
		{
			JumpToDataIndex(activeIndex, m_Alignment, m_TweenMethod, m_TweenDuration);
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
#endregion IScrollSnap implementation

#region IScrollPagerHandler implementation

		/// <summary>
		/// 最大ページ数を取得します.
		/// </summary>
		public int GetPageCount()
		{
			return controller.GetDataCount();
		}

		/// <summary>
		/// 現在のページ数を取得します.
		/// </summary>
		public int GetPageIndex()
		{
			return activeIndex;
		}

#endregion IScrollPagerHandler implementation






//#region Default ICellViewPool implementation
//
//		readonly Dictionary<int, Stack<ScrollCellView>> _pool = new Dictionary<int, Stack<ScrollCellView>>();
//
//		/// <summary>
//		/// セルビューをプールから取得または新規作成します.
//		/// </summary>
//		public ScrollCellView RentCellView(ScrollCellView template)
//		{
//			var id = template.GetInstanceID();
//			Stack<ScrollCellView> stack;
//			if (_pool.TryGetValue(id, out stack) && 0 < stack.Count)
//				return stack.Pop();
//
//			var cellView = GameObject.Instantiate(template);
//			cellView.templateId = id;
//			cellView.transform.SetParent(_cellViewPool, false);
//			cellView.transform.localPosition = Vector3.zero;
//			cellView.transform.localRotation = Quaternion.identity;
//			cellView.transform.localScale = Vector3.zero;
//			return cellView;
//		}
//
//		/// <summary>
//		/// セルビューをプールに返却します.
//		/// </summary>
//		public void ReturnCellView(ScrollCellView obj)
//		{
//			int id = obj.templateId;
//			Stack<ScrollCellView> stack;
//			if (!_pool.TryGetValue(id, out stack))
//			{
//				stack = new Stack<ScrollCellView>();
//				_pool.Add(id, stack);
//			}
//
//			if (!stack.Contains(obj))
//			{
//				obj.transform.SetParent(_cellViewPool, false);
//				obj.transform.localPosition = Vector3.zero;
//				obj.transform.localRotation = Quaternion.identity;
//				obj.transform.localScale = Vector3.zero;
//				stack.Push(obj);
//			}
//		}
//
//#endregion Default ICellViewPool implementation

//#region Default IScrollViewDelegate implementation
//
//		public int GetDataCount()
//		{
//			return content.childCount - 2;
//		}
//
//		public float GetCellViewSize(int dataIndex)
//		{
//			var rt = content.GetChild(dataIndex) as RectTransform;
//			var layoutElement = rt.GetComponent<LayoutElement>();
//
//			if (layoutElement)
//				return scrollRect.vertical ? layoutElement.preferredHeight : layoutElement.preferredWidth;
//			else
//				return scrollRect.vertical ? rt.rect.height : rt.rect.width;
//		}
//
//		public ScrollCellView GetCellView(int dataIndex)
//		{
//			return null;
//		}
//
//#endregion Default IScrollViewDelegate implementation
	}
}