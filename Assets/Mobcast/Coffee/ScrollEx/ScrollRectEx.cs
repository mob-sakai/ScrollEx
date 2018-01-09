using System;
using System.Collections.Generic;
using Mobcast.Coffee.UI.ScrollModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TweenMethod = Mobcast.Coffee.Tweening.TweenMethod;
using UnityEngine.Events;

namespace Mobcast.Coffee.UI
{
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectEx : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
	{
		public enum Alignment
		{
			TopOrLeft,
			Center,
			BottomOrRight,
		}
		IScrollViewController _controller;
		public IScrollViewController controller
		{
			get
			{
				return _controller;
			}
			set
			{
				if (_controller == value)
					return;

				_controller = value;

				// デフォルトコントローラ以外の場合、ContentSizeFitterを削除削除する必要がある
				if (!(_controller is DefaultScrollViewController))
				{
					var sizeFitter = content.GetComponent<ContentSizeFitter>();
					if (sizeFitter)
						GameObject.Destroy(sizeFitter);
				}
			}
		}

		public ICellViewPool scrollPool { get; set; }

		public ScrollIndicator indicator { get { return m_Indicator; }
			set
			{
				if (m_Indicator == value)
					return;

				if (Application.isPlaying && m_Indicator)
					indicator.onChangedIndex -= JumpTo;

				m_Indicator = value;
				if (Application.isPlaying && m_Indicator)
					indicator.onChangedIndex += JumpTo;
			}
		}

		public SnapModule snapModule { get { return m_SnapModule; } }

		public NaviModule naviModule { get { return m_NaviModule; } }

		public AutoRotationModule autoRotationModule { get { return m_AutoRotationModule; } }

		public float tweenDuration { get { return m_TweenDuration; } set { m_TweenDuration = value; } }

		public TweenMethod tweenMethod { get { return m_TweenMethod; } set { m_TweenMethod = value; } }

#region Serialize

		[SerializeField] SnapModule m_SnapModule;

		[SerializeField] NaviModule m_NaviModule = new NaviModule();

		[SerializeField] AutoRotationModule m_AutoRotationModule;

		[SerializeField] Alignment m_Alignment = Alignment.Center;

		[SerializeField] TweenMethod m_TweenMethod = TweenMethod.EaseOutSine;

		[SerializeField] float m_TweenDuration = 0.5f;

		[SerializeField] bool m_Loop;

		[SerializeField] ScrollIndicator m_Indicator;



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
				if (!_layoutGroupForContent)
				{
					if (!content)
						return null;
					
					_layoutGroupForContent = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
					if (!_layoutGroupForContent)
					{
						var lg = content.GetComponent<LayoutGroup>();
						if (lg)
							GameObject.DestroyImmediate(lg);
						if (scrollRect.vertical)
							_layoutGroupForContent = content.gameObject.AddComponent<VerticalLayoutGroup>();
						else
							_layoutGroupForContent = content.gameObject.AddComponent<HorizontalLayoutGroup>();
					}
				}

				if (scrollRect.vertical != (_layoutGroupForContent is VerticalLayoutGroup))
				{
					float s = _layoutGroupForContent.spacing;
					RectOffset p = _layoutGroupForContent.padding;
					GameObject.DestroyImmediate(_layoutGroupForContent);
					if (scrollRect.vertical)
						_layoutGroupForContent = content.gameObject.AddComponent<VerticalLayoutGroup>();
					else
						_layoutGroupForContent = content.gameObject.AddComponent<HorizontalLayoutGroup>();
					_layoutGroupForContent.spacing = s;
					_layoutGroupForContent.padding = p;
				}
				return _layoutGroupForContent;
			}
		}

		/// <summary>
		/// セルビュー間のスペースを調整します.
		/// </summary>
		public float spacing
		{
			get { return layoutGroup.spacing; }
			set
			{
				layoutGroup.spacing = value;
				_needToReload = true;
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
				_needToReload = true;
			}
		}

		/// <summary>
		/// スクロール可能な領域の大きさを取得します.
		/// </summary>
		public float scrollSize { get { return contentSize - scrollRectSize; } }

		/// <summary>
		/// コンテンツ全体の大きさを取得します.
		/// </summary>
		public float contentSize { get { return (scrollRect.vertical ? content.rect.height : content.rect.width); } }

		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float scrollPosition
		{
			get { return m_ScrollPosition; }
			set
			{
				// 非ループ時はスクロールのオーバーランを防ぐために、スクロール制限をかけます.
				if (!loop && 0 < _cellViewSizeArray.Count)
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
		public List<ScrollCellView> activeCellViews { get { return _activeCellViews; } }

		/// <summary>
		/// 現在のインデックス(Alignment基準)を取得します.
		/// </summary>
		public int activeIndex
		{
			get
			{
				if (dataCount <= 0 || _cellViewOffsetArray.Count <= 0)
					return -1;

				if (!loop)
				{
					if (scrollPosition < 1)
						return 0;
					else if (scrollSize -1 < scrollPosition)
						return dataCount - 1;
				}

				var pos = scrollPosition + (scrollRectSize * Mathf.Clamp01((int)m_Alignment * 0.5f));
				var cellViewIndex = _GetIndexFromScrollPosition(pos, 0, _cellViewOffsetArray.Count - 1);
				return cellViewIndex % dataCount;
			}
		}

		/// <summary>
		/// 現在先頭にある、アクティブなセルビューのインデックス.
		/// </summary>
		public int activeStartCellIndex { get; set; }

		/// <summary>
		/// 現在末尾にある、アクティブなセルビューのインデックス.
		/// </summary>
		public int activeEndCellIndex { get; set; }

		/// <summary>
		/// データの要素数を取得します.
		/// </summary>
		public virtual int dataCount { get { return controller.GetDataCount(); } }

		public int firstPaddingSiblingIndex { get { return _firstPadding.transform.GetSiblingIndex(); } }

		public int lastPaddingSiblingIndex { get { return _lastPadding.transform.GetSiblingIndex(); } }

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
				_activeCellViews[i].OnRefresh();
			}
		}

		/// <summary>
		/// 目的のインデックスに移動できるかどうかを取得します.
		/// </summary>
		public bool CanJumpTo(int index)
		{
			return loop || (0 <= index && index < dataCount);
		}

		/// <summary>
		/// 目的のインデックスに移動します.
		/// </summary>
		public void JumpTo(int index)
		{
			JumpTo(index, m_Alignment, m_TweenMethod, m_TweenDuration);
		}

		/// <summary>
		/// 目的のインデックスに移動します.
		/// </summary>
		public void JumpTo(int index, Alignment align, TweenMethod tweenType = TweenMethod.immediate, float tweenTime = 0f)
		{
			int count = dataCount;
			if (count == 0)
				return;

			index = index % count;
			if (index < 0)
				index += count;
			
			float normalizedOffset = (int)align * 0.5f;
			bool useSpacing = align == Alignment.Center;

			var cellOffsetPosition = 0f;

			if (normalizedOffset < 0 || 0 < normalizedOffset)
			{
				var cellSize = controller.GetCellViewSize(index);

				// セルビュー間スペースを考慮します.
				if (useSpacing)
				{
					cellSize += spacing;
					if (index > 0 && index < (count - 1))
						cellSize += spacing;
				}

				cellOffsetPosition = cellSize * normalizedOffset;
			}

			var newScrollPosition = 0f;
			var offset = -(normalizedOffset * scrollRectSize) + cellOffsetPosition;

			// ループの時は最も近いセルビューにジャンプします.
			// ループには最低3つのセルビューオフセットが存在するため、そのうち一番近いセルビューが選択されます.
			if (loop)
			{
				// セルビュー座標.
				var set1Position = GetScrollPositionFromIndex(index) + offset;
				var set2Position = GetScrollPositionFromIndex(index + count) + offset;
				var set3Position = GetScrollPositionFromIndex(index + count * 2) + offset;

				// 現在地からの差異.
				var set1Diff = (Mathf.Abs(m_ScrollPosition - set1Position));
				var set2Diff = (Mathf.Abs(m_ScrollPosition - set2Position));
				var set3Diff = (Mathf.Abs(m_ScrollPosition - set3Position));

				// 最も近いセルビューを選択.
				newScrollPosition = set1Diff < set2Diff
					? set1Diff < set3Diff ? set1Position : set3Position
					: set2Diff < set3Diff ? set2Position : set3Position;
			}
			// 非ループ時は対象のセルビューにジャンプします.
			else
			{
				newScrollPosition = GetScrollPositionFromIndex(index) + offset;
			}


			if (useSpacing)
			{
				newScrollPosition -= spacing;
			}

			// Unrestricted以外の場合、スクロール領域は制限されます.
			if (scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
			{
				newScrollPosition = Mathf.Clamp(newScrollPosition, 0, scrollSize);
			}

			snapModule.StartScrollTween(tweenType, tweenTime, scrollPosition, newScrollPosition);
		}


		/// <summary>
		/// インデックスからスクロール座標を取得します.
		/// </summary>
		public float GetScrollPositionFromIndex(int cellViewIndex, bool beforeCell = true)
		{
			// 要素がない場合、0を返します.
			if (dataCount == 0 || _cellViewOffsetArray.Count == 0)
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

		readonly List<float> _cellViewSizeArray = new List<float>();

		readonly List<float> _cellViewOffsetArray = new List<float>();

		float m_ScrollPosition;

		readonly List<ScrollCellView> _activeCellViews = new List<ScrollCellView>();

		int _loopFirstCellIndex;

		int _loopLastCellIndex;

		float _loopFirstScrollPosition;

		float _loopLastScrollPosition;

		float _loopFirstJumpTrigger;

		float _loopLastJumpTrigger;

		float _lastScrollRectSize;
		float _lastScrollSize;

		bool _lastLoop;

		readonly List<int> _remainingCellIndices = new List<int>();

		HorizontalOrVerticalLayoutGroup _layoutGroupForContent;

		RectOffset _lastRectOffset;

		float _lastSpacing;

		/// <summary>
		/// スクロール領域をリサイズします.
		/// 各セルのセルサイズ、オフセットを再計算します.
		/// </summary>
		void _Resize(bool keepPosition)
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
			if (0 < _cellViewOffsetArray.Count)
			{
				float lastOffset = _cellViewOffsetArray[_cellViewOffsetArray.Count - 1];
				if (_scrollRect.vertical)
					content.sizeDelta = new Vector2(content.sizeDelta.x, lastOffset + padding.top + padding.bottom);
				else
					content.sizeDelta = new Vector2(lastOffset + padding.left + padding.right, content.sizeDelta.y);
			}

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
			else if (snapModule.snapOnEndDrag)
				JumpTo(0, m_Alignment, TweenMethod.immediate, 0);
			// それ以外の場合、初期位置に戻します.
			else
				scrollPosition = loop ? _loopFirstScrollPosition : 0;
		}

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

		/// <summary>
		/// 表示すべきセルビューをリセットします.
		/// </summary>
		void _ResetVisibleCellViews()
		{
			int startIndex;
			int endIndex;

			// 現在見えているセルビューの範囲を取得します.
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// 不要なセルビューをプールします.
			var i = 0;
			_remainingCellIndices.Clear();
			while (i < _activeCellViews.Count)
			{
				if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
				{
					_PoolCellView(_activeCellViews[i]);
				}
				else
				{
					// このセルインデックスは新しい描画範囲でも利用可能です.
					_remainingCellIndices.Add(_activeCellViews[i].cellIndex);
					i++;
				}
			}

			if (_remainingCellIndices.Count == 0)
			{
				for (i = startIndex; i <= endIndex; i++)
					_AddCellView(i, false);
			}
			else
			{
				// セルインデックスを再利用します.
				int index = _remainingCellIndices[0];
				for (i = endIndex; i >= startIndex; i--)
				{
					if (i < index)
						_AddCellView(i, true);
				}

				index = _remainingCellIndices[_remainingCellIndices.Count - 1];
				for (i = startIndex; i <= endIndex; i++)
				{
					if (i > index)
						_AddCellView(i, false);
				}
			}

			// 現在アクティブな開始/終了インデックスを更新.
			activeStartCellIndex = startIndex;
			activeEndCellIndex = endIndex;

			// パディングサイズを調整します.
			_AdjustPaddingSize(startIndex, endIndex);

			_OnChangeActiveCellPosition();
		}

		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		void _AdjustPaddingSize(int startIndex, int endIndex)
		{
			if (dataCount == 0 || controller is DefaultScrollViewController)
				return;

			var firstSize = _cellViewOffsetArray[startIndex] - _cellViewSizeArray[startIndex];
			var lastSize = _cellViewOffsetArray[_cellViewOffsetArray.Count - 1] - _cellViewOffsetArray[endIndex];

			_AdjustPaddingSize(_firstPadding, firstSize);
			_AdjustPaddingSize(_lastPadding, lastSize);
		}

		/// <summary>
		/// パディングサイズを調整します.
		/// </summary>
		void _AdjustPaddingSize(LayoutElement padder, float size)
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
		void _PoolCellView(ScrollCellView cellView)
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
		void _AddCellView(int cellIndex, bool atStart)
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
				_activeCellViews.Insert(0, cellView);
			else
				_activeCellViews.Add(cellView);

			// ヒエラルキー順を調整します.
			cellView.transform.SetSiblingIndex(atStart ? 1 : content.childCount - 2);

			cellView.OnChangedVisibility(true);
		}


		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		void _RefreshActive()
		{
			_needToReflesh = false;

			int startIndex;
			int endIndex;

			// if looping, check to see if we scrolled past a trigger
			if (loop)
			{
				if (m_ScrollPosition < _loopFirstJumpTrigger)
				{
					var v = _scrollRect.velocity;
					scrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - m_ScrollPosition);
					_scrollRect.velocity = v;
				}
				else if (m_ScrollPosition > _loopLastJumpTrigger)
				{
					var v = _scrollRect.velocity;
					scrollPosition = _loopFirstScrollPosition + (m_ScrollPosition - _loopLastJumpTrigger);
					_scrollRect.velocity = v;
				}
			}

			// get the range of visibile cells
			// 現在見えているセルビューの範囲を取得します.
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == activeStartCellIndex && endIndex == activeEndCellIndex)
			{
				_OnChangeActiveCellPosition();
				return;
			}

			// recreate the visibile cells
			_ResetVisibleCellViews();
		}


		/// <summary>
		/// 現在見えているセルビューに対し、座標変更を通知します.
		/// </summary>
		void _OnChangeActiveCellPosition()
		{
			var pos = scrollPosition;
			var rectSize = scrollRectSize;
			if (rectSize <= 0)
				return;

			for (int i = 0; i < _activeCellViews.Count; i++)
			{
				var cell = _activeCellViews[i];
				var cellPosition = GetScrollPositionFromIndex(cell.cellIndex) + controller.GetCellViewSize(cell.dataIndex) / 2;
				cell.OnPositionChanged((cellPosition - pos) / rectSize);
			}
		}

		/// <summary>
		/// 現在見えているセルビューの範囲を取得します.
		/// </summary>
		void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			// スクロール座標からインデックスを取得します.
			int lastIndex = _cellViewOffsetArray.Count - 1;
			startIndex = _GetIndexFromScrollPosition(m_ScrollPosition, 0, lastIndex);
			endIndex = _GetIndexFromScrollPosition(m_ScrollPosition + scrollRectSize, 0, lastIndex);
		}

		/// <summary>
		/// 座標からインデックスを取得します.
		/// 各セルのオフセットはリサイズ時に予め保存されています.
		/// 再帰的な二分探索により高速に探索されます.
		/// </summary>
		int _GetIndexFromScrollPosition(float position, int startIndex, int endIndex)
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

		//==== v MonoBehavior Callbacks v ====
		/// <summary>
		/// Caches and initializes the scroller
		/// </summary>
		protected virtual void Awake()
		{
			_scrollRectTransform = scrollRect.transform as RectTransform;

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
			c.localRotation = Quaternion.identity;
			c.localScale = Vector3.one;
			c.anchoredPosition = Vector2.zero;

			lg.childAlignment = TextAnchor.UpperLeft;
			lg.childForceExpandHeight = true;
			lg.childForceExpandWidth = true;
#if UNITY_5_5_OR_NEWER
			lg.childControlHeight = true;
			lg.childControlWidth = true;
#endif



			// create the padder objects
			GameObject go = new GameObject("___FirstPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(c, false);
			go.SetActive(false);
			_firstPadding = go.GetComponent<LayoutElement>();

			go = new GameObject("___LastPadder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(c, false);
			go.SetActive(false);
			_lastPadding = go.GetComponent<LayoutElement>();

			// スクロールモジュールにハンドラーを設定
			snapModule.handler = this;
			naviModule.handler = this;
			autoRotationModule.handler = this;

			// デフォルトのセルビュープールを生成.
			if (scrollPool == null)
			{
				go = new GameObject("___CellViewPool", typeof(RectTransform));
				go.transform.SetParent(_scrollRect.transform, false);
				go.SetActive(false);
				scrollPool = new DefaultCellViewPoolModule(go.GetComponent<RectTransform>());
			}

			// デフォルトのスクロールビューコントローラ生成.
			if (controller == null)
			{
				controller = new DefaultScrollViewController(this);
			}

			// インジケータコールバックを設定
			if (indicator != null)
			{
				indicator.onChangedIndex += JumpTo;
			}

			// set up the last values for updates
			_lastScrollRectSize = scrollRectSize;
			_lastLoop = loop;
			_lastRectOffset = _layoutGroupForContent.padding;
			_lastSpacing = _layoutGroupForContent.spacing;

			_Resize(false);

			// スクロール値が変化した時、コールバックを受け取ります.
			scrollRect.onValueChanged.AddListener(val =>
				{
					scrollPosition = scrollRect.vertical
						? (1f - val.y) * scrollSize
						: val.x * scrollSize;
					_RefreshActive();
				});
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

			// 各モジュールを更新.
//			indicatorModule.Update();
			snapModule.Update();
			naviModule.Update();
			autoRotationModule.Update();

			if (indicator)
			{
				indicator.index = activeIndex;
				indicator.count = dataCount;
			}

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if (
				(loop && _lastScrollRectSize != scrollRectSize)
				|| (loop != _lastLoop)
				|| (_lastScrollSize != scrollSize)
				|| (_lastRectOffset != _layoutGroupForContent.padding)
				|| (_lastSpacing != _layoutGroupForContent.spacing))
			{
				_Resize(true);
				_lastScrollRectSize = scrollRectSize;
				_lastScrollSize = scrollSize;

				_lastLoop = loop;
				_lastRectOffset = _layoutGroupForContent.padding;
				_lastSpacing = _layoutGroupForContent.spacing;
			}

			if (_needToReflesh)
			{
				_RefreshActive();
			}
		}
		//==== ^ MonoBehavior Callbacks ^ ====

		#endregion



		/// <summary>
		/// Called by the EventSystem when a Scroll event occurs.
		/// </summary>
		public virtual void OnScroll(PointerEventData eventData)
		{
			snapModule.OnScroll(eventData);
		}

		/// <summary>
		/// Called before a drag is started.
		/// </summary>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			snapModule.OnBeginDrag(eventData);
			autoRotationModule.OnBeginDrag(eventData);
		}

		/// <summary>
		/// Called by the EventSystem once dragging ends.
		/// </summary>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			snapModule.OnEndDrag(eventData);
			autoRotationModule.OnEndDrag(eventData);
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
	}
}