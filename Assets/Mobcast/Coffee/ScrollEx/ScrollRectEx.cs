using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using EnhancedUI;
using UnityEngine.EventSystems;
using TweenType = ScrollTweener.TweenType;

namespace Mobcast.Coffee
{
	/// <summary>
	/// This delegate handles the visibility changes of cell views
	/// </summary>
	/// <param name="cellView">The cell view that changed visibility</param>
	public delegate void CellViewVisibilityChangedDelegate(ScrollCellView cellView);

	/// <summary>
	/// This delegate will be fired just before the cell view is recycled
	/// </summary>
	/// <param name="cellView"></param>
	public delegate void CellViewWillRecycleDelegate(ScrollCellView cellView);

	/// <summary>
	/// This delegate handles the scrolling callback of the ScrollRect.
	/// </summary>
	/// <param name="scroller">The scroller that called the delegate</param>
	/// <param name="val">The scroll value of the scroll rect</param>
	/// <param name="scrollPosition">The scroll position in pixels from the start of the scroller</param>
	public delegate void ScrollerScrolledDelegate(ScrollRectEx scroller, Vector2 val, float scrollPosition);

	/// <summary>
	/// This delegate handles the snapping of the scroller.
	/// </summary>
	/// <param name="scroller">The scroller that called the delegate</param>
	/// <param name="cellIndex">The index of the cell view snapped on (this may be different than the data index in case of looping)</param>
	/// <param name="dataIndex">The index of the data the view snapped on</param>
	public delegate void ScrollerSnappedDelegate(ScrollRectEx scroller, int cellIndex, int dataIndex);

	/// <summary>
	/// This delegate handles the change in state of the scroller (scrolling or not scrolling)
	/// </summary>
	/// <param name="scroller">The scroller that changed state</param>
	/// <param name="scrolling">Whether or not the scroller is scrolling</param>
	public delegate void ScrollerScrollingChangedDelegate(ScrollRectEx scroller, bool scrolling);

	/// <summary>
	/// This delegate handles the change in state of the scroller (jumping or not jumping)
	/// </summary>
	/// <param name="scroller">The scroller that changed state</param>
	/// <param name="tweening">Whether or not the scroller is tweening</param>
	public delegate void ScrollerTweeningChangedDelegate(ScrollRectEx scroller, bool tweening);

	/// <summary>
	/// The ScrollRectEx allows you to easily set up a dynamic scroller that will recycle views for you. This means
	/// that using only a handful of views, you can display thousands of rows. This will save memory and processing
	/// power in your application.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectEx : MonoBehaviour
//	, IBeginDragHandler, IEndDragHandler, IScrollHandler
	{
		int m_ScrollCount;
		Coroutine m_CoTweening;

//		public void OnScroll (PointerEventData eventData)
//		{
//			StopTweening ();
//			m_ScrollCount = 10;
//			IsDragging = true;
//		}
//
//		public void OnBeginDrag (PointerEventData eventData)
//		{
//			StopTweening ();
//			m_ScrollCount = 0;
//			IsDragging = true;
//		}
//
//		public void OnEndDrag (PointerEventData eventData)
//		{
//			m_ScrollCount = 0;
//			IsDragging = false;
//		}

		/// <summary>
		/// Gets the number of cells in a list of data
		/// </summary>
		/// <param name="scroller"></param>
		/// <returns></returns>
		public virtual int GetNumberOfCells(ScrollRectEx scroller)
		{
			return 0;
		}

		/// <summary>
		/// Gets the size of a cell view given the index of the data set.
		/// This allows you to have different sized cells
		/// </summary>
		/// <param name="scroller"></param>
		/// <param name="dataIndex"></param>
		/// <returns></returns>
		public virtual float GetCellViewSize(ScrollRectEx scroller, int dataIndex)
		{
			return 0;
		}

		/// <summary>
		/// Gets the cell view that should be used for the data index. Your implementation
		/// of this function should request a new cell from the scroller so that it can
		/// properly recycle old cells.
		/// </summary>
		/// <param name="scroller"></param>
		/// <param name="dataIndex"></param>
		/// <param name="cellIndex"></param>
		/// <returns></returns>
		public virtual ScrollCellView GetCellView(ScrollRectEx scroller, int dataIndex, int cellIndex)
		{
			return null;
		}


		#region Public

		/// <summary>
		/// Which side of a cell to reference.
		/// For vertical scrollers, before means above, after means below.
		/// For horizontal scrollers, before means to left of, after means to the right of.
		/// </summary>
		public enum CellViewPositionEnum
		{
			Before,
			After
		}

		/// <summary>
		/// The direction the scroller is handling
		/// </summary>
//		public ScrollDirectionEnum scrollDirection;

		/// <summary>
		/// The number of pixels between cell views, starting after the first cell view
		/// </summary>
		public float spacing{get{return layoutGroupXXX.spacing;}}

		/// <summary>
		/// The padding inside of the scroller: top, bottom, left, right.
		/// </summary>
		public RectOffset padding{ get{ return layoutGroupXXX.padding;}}

		/// <summary>
		/// Whether the scroller should loop the cell views
		/// </summary>
		[SerializeField]
		private bool loop;

		/// <summary>
		/// Whether the scollbar should be shown
		/// </summary>
//		[SerializeField]
		private ScrollRect.ScrollbarVisibility _scrollbarVisibility;

		/// <summary>
		/// Whether snapping is turned on
		/// </summary>
//		public bool snapping;

		/// <summary>
		/// This is the speed that will initiate the snap. When the
		/// scroller slows down to this speed it will snap to the location
		/// specified.
		/// </summary>
//		public float snapVelocityThreshold;

		/// <summary>
		/// The snap offset to watch for. When the snap occurs, this
		/// location in the scroller will be how which cell to snap to 
		/// is determined.
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the scroller and 1 being the bottom / right.
		/// In most situations the watch offset and the jump offset 
		/// will be the same, they are just separated in case you need
		/// that added functionality.
		/// </summary>
//		public float snapWatchOffset;

		/// <summary>
		/// The snap location to move the cell to. When the snap occurs,
		/// this location in the scroller will be where the snapped cell
		/// is moved to.
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the scroller and 1 being the bottom / right.
		/// In most situations the watch offset and the jump offset 
		/// will be the same, they are just separated in case you need
		/// that added functionality.
		/// </summary>
//		public float snapJumpToOffset;

		/// <summary>
		/// Once the cell has been snapped to the scroller location, this
		/// value will determine how the cell is centered on that scroller
		/// location. 
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the cell and 1 being the bottom / right.
		/// </summary>
//		public float snapCellCenterOffset;

		/// <summary>
		/// Whether to include the spacing between cells when determining the
		/// cell offset centering.
		/// </summary>
//		public bool snapUseCellSpacing;

		/// <summary>
		/// What function to use when interpolating between the current 
		/// scroll position and the snap location. This is also known as easing. 
		/// If you want to go immediately to the snap location you can either 
		/// set the snapTweenType to immediate or set the snapTweenTime to zero.
		/// </summary>
//		public TweenType snapTweenType;

		/// <summary>
		/// The time it takes to interpolate between the current scroll 
		/// position and the snap location.
		/// If you want to go immediately to the snap location you can either 
		/// set the snapTweenType to immediate or set the snapTweenTime to zero.
		/// </summary>
//		public float snapTweenTime;

		/// <summary>
		/// This delegate is called when a cell view is hidden or shown
		/// </summary>
		public CellViewVisibilityChangedDelegate cellViewVisibilityChanged;

		/// <summary>
		/// This delegate is called just before a cell view is hidden by recycling
		/// </summary>
		public CellViewWillRecycleDelegate cellViewWillRecycle;

		/// <summary>
		/// This delegate is called when the scroll rect scrolls
		/// </summary>
//		public ScrollerScrolledDelegate scrollerScrolled;

		/// <summary>
		/// This delegate is called when the scroller has snapped to a position
		/// </summary>
//		public ScrollerSnappedDelegate scrollerSnapped;

		/// <summary>
		/// This delegate is called when the scroller has started or stopped scrolling
		/// </summary>
//		public ScrollerScrollingChangedDelegate scrollerScrollingChanged;

		/// <summary>
		/// This delegate is called when the scroller has started or stopped tweening
		/// </summary>
//		public ScrollerTweeningChangedDelegate scrollerTweeningChanged;

		/// <summary>
		/// The Delegate is what the scroller will call when it needs to know information about
		/// the underlying data or views. This allows a true MVC process.
		/// </summary>
//		public ScrollRectEx Delegate { get { return this; } set { _reloadData = true; } }

		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float ScrollPosition
		{
			get
			{
				return _scrollPosition;
			}
			set
			{
				// make sure the position is in the bounds of the current set of views
				if(!Loop)
				{
					var min = -ScrollRectSize;
					var max = GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before) + ScrollRectSize;
					if (value < min || max < value)
					{
						value = Mathf.Clamp(value, min, max);
						scrollRectXXX.velocity = Vector2.zero;
					}
//					value = Mathf.Clamp(value, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
				}

				// only if the value has changed
				if (_scrollPosition != value)
				{
					_scrollPosition = value;
					if (m_ScrollRect.vertical)
					{
						// set the vertical position
						m_ScrollRect.verticalNormalizedPosition = 1f - (_scrollPosition / _ScrollSize);
					}
					else
					{
						// set the horizontal position
						m_ScrollRect.horizontalNormalizedPosition = (_scrollPosition / _ScrollSize);
					}

					// flag that we need to refresh
					_refreshActive = true;
				}
			}
		}

		/// <summary>
		/// Whether the scroller should loop the resulting cell views.
		/// Looping creates three sets of internal size data, attempting
		/// to keep the scroller in the middle set. If the scroller goes
		/// outside of this set, it will jump back into the middle set,
		/// giving the illusion of an infinite set of data.
		/// </summary>
		public bool Loop
		{
			get
			{
				return loop;
			}
			set
			{
				// only if the value has changed
				if (loop != value)
				{
					// get the original position so that when we turn looping on
					// we can jump back to this position
					var originalScrollPosition = _scrollPosition;

					loop = value;

					// call resize to generate more internal elements if loop is on,
					// remove the elements if loop is off
					_Resize(false);

					if (loop)
					{
						// set the new scroll position based on the middle set of data + the original position
						ScrollPosition = _loopFirstScrollPosition + originalScrollPosition;
					}
					else
					{
						// set the new scroll position based on the original position and the first loop position
						ScrollPosition = originalScrollPosition - _loopFirstScrollPosition;
					}

					// update the scrollbars
					ScrollbarVisibility = _scrollbarVisibility;
				}
			}
		}

		/// <summary>
		/// Sets how the visibility of the scrollbars should be handled
		/// </summary>
		public ScrollRect.ScrollbarVisibility ScrollbarVisibility
		{
			get
			{
				return _scrollbarVisibility;
			}
			set
			{
				_scrollbarVisibility = value;

				// only if the scrollbar exists
				if (_scrollbar != null)
				{
					// make sure we actually have some cell views
					if (_cellViewOffsetArray != null && _cellViewOffsetArray.Count > 0)
					{
						if (_cellViewOffsetArray.Last() < ScrollRectSize || loop)
						{
							// if the size of the scrollable area is smaller than the scroller
							// or if we have looping on, hide the scrollbar unless the visibility
							// is set to Always.
							_scrollbar.gameObject.SetActive(_scrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent);
						}
						else
						{
							// if the size of the scrollable areas is larger than the scroller
							// or looping is off, then show the scrollbars unless visibility
							// is set to Never.
							_scrollbar.gameObject.SetActive(true);
						}
					}
				}
			}
		}

//		/// <summary>
//		/// This is the velocity of the scroller.
//		/// </summary>
//		public Vector2 Velocity
//		{
//			get
//			{
//				return _scrollRect.velocity;
//			}
//			set
//			{
//				_scrollRect.velocity = value;
//			}
//		}

		/// <summary>
		/// The linear velocity is the velocity on one axis.
		/// The scroller should only be moving one one axix.
		/// </summary>
//		public float LinearVelocity
//		{
//			get
//			{
//				// return the velocity component depending on which direction this is scrolling
//				return (m_ScrollRect.vertical ? m_ScrollRect.velocity.y : m_ScrollRect.velocity.x);
//			}
//			set
//			{
//				// set the appropriate component of the velocity
//				if (m_ScrollRect.vertical)
//				{
//					m_ScrollRect.velocity = new Vector2(0, value);
//				}
//				else
//				{
//					m_ScrollRect.velocity = new Vector2(value, 0);
//				}
//			}
//		}

		/// <summary>
		/// Whether the scroller is scrolling or not
		/// </summary>
		public bool IsScrolling
		{
			get; private set;
		}

		public bool IsDragging
		{
			get; private set;
		}
//		static System.Reflection.FieldInfo fiDragging = typeof(ScrollRect).GetField("m_Dragging", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

		/// <summary>
		/// Whether the scroller is tweening or not
		/// </summary>
//		public bool IsTweening
//		{
//			get; private set;
//		}

		/// <summary>
		/// This is the first cell view index showing in the scroller's visible area
		/// </summary>
		public int StartCellViewIndex
		{
			get
			{
				return _activeCellViewsStartIndex;
			}
		}

		/// <summary>
		/// This is the last cell view index showing in the scroller's visible area
		/// </summary>
		public int EndCellViewIndex
		{
			get
			{
				return _activeCellViewsEndIndex;
			}
		}

		/// <summary>
		/// This is the first data index showing in the scroller's visible area
		/// </summary>
		public int StartDataIndex
		{
			get
			{
				return _activeCellViewsStartIndex % NumberOfCells;
			}
		}

		/// <summary>
		/// This is the last data index showing in the scroller's visible area
		/// </summary>
		public int EndDataIndex
		{
			get
			{
				return _activeCellViewsEndIndex % NumberOfCells;
			}
		}

		/// <summary>
		/// This is the number of cells in the scroller
		/// </summary>
		public int NumberOfCells
		{
			get
			{
				return this.GetNumberOfCells(this);
			}
		}

		/// <summary>
		/// This is a convenience link to the scroller's scroll rect
		/// </summary>
		public ScrollRect ScrollRect
		{
			get
			{
				return m_ScrollRect;
			}
		}

		/// <summary>
		/// The size of the visible portion of the scroller
		/// </summary>
		public float ScrollRectSize
		{
			get
			{
				if (m_ScrollRect.vertical)
					return m_ScrollRectTransform.rect.height;
				else
					return m_ScrollRectTransform.rect.width;
			}
		}

		/// <summary>
		/// Create a cell view, or recycle one if it already exists
		/// </summary>
		/// <param name="cellPrefab">The prefab to use to create the cell view</param>
		/// <returns></returns>
		public ScrollCellView GetCellView(ScrollCellView cellPrefab)
		{
			// see if there is a view to recycle
			var cellView = _GetRecycledCellView(cellPrefab);
			if (cellView == null)
			{
				// no recyleable cell found, so we create a new view
				// and attach it to our container
				var go = Instantiate(cellPrefab.gameObject);
				cellView = go.GetComponent<ScrollCellView>();
				cellView.transform.SetParent(_container);
				cellView.transform.localPosition = Vector3.zero;
				cellView.transform.localRotation = Quaternion.identity;
			}

			return cellView;
		}

		/// <summary>
		/// This resets the internal size list and refreshes the cell views
		/// </summary>
		/// <param name="scrollPositionFactor">The percentage of the scroller to start at between 0 and 1, 0 being the start of the scroller</param>
		public void ReloadData(float scrollPositionFactor = 0)
		{
			_reloadData = false;

			// recycle all the active cells so
			// that we are sure to get fresh views
			_RecycleAllCells();

			// if we have a delegate handling our data, then
			// call the resize
			//if (Delegate != null)
				_Resize(false);

			_scrollPosition = scrollPositionFactor * _ScrollSize;
			if (m_ScrollRect.vertical)
			{
				// set the vertical position
				m_ScrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
			}
			else
			{
				// set the horizontal position
				m_ScrollRect.horizontalNormalizedPosition = scrollPositionFactor;
			}
		}

		/// <summary>
		/// This calls the RefreshCellView method on each active cell.
		/// If you override the RefreshCellView method in your cells
		/// then you can update the UI without having to reload the data.
		/// Note: this will not change the cell sizes, you will need
		/// to call ReloadData for that to work.
		/// </summary>
		public void RefreshActiveCellViews()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				_activeCellViews[i].RefreshCellView();
			}
		}

		/// <summary>
		/// Removes all cells, both active and recycled from the scroller.
		/// This will call garbage collection.
		/// </summary>
		public void ClearAll()
		{
			ClearActive();
			ClearRecycled();
		}

		/// <summary>
		/// Removes all the active cell views. This should only be used if you want
		/// to get rid of cells because of settings set by Unity that cannot be
		/// changed at runtime. This will call garbage collection.
		/// </summary>
		public void ClearActive()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				DestroyImmediate(_activeCellViews[i].gameObject);
			}
			_activeCellViews.Clear();
		}

		/// <summary>
		/// Removes all the recycled cell views. This should only be used after you
		/// load in a completely different set of cell views that will not use the 
		/// recycled views. This will call garbage collection.
		/// </summary>
		public void ClearRecycled()
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				DestroyImmediate(_recycledCellViews[i].gameObject);
			}
			_recycledCellViews.Clear();
		}

//		/// <summary>
//		/// Turn looping on or off. This is just a helper function so 
//		/// you don't have to keep track of the state of the looping
//		/// in your own scripts.
//		/// </summary>
//		public void ToggleLoop()
//		{
//			Loop = !loop;
//		}


		/// <summary>
		/// Turn looping on or off. This is just a helper function so 
		/// you don't have to keep track of the state of the looping
		/// in your own scripts.
		/// </summary>
		public void SetPosition(float value, float dir)
		{
			if (loop)
			{
				// if we are looping, we need to make sure the new position isn't past the jump trigger.
				// if it is we need to reset back to the jump position on the other side of the area.
			
				if (0 < dir && value > _loopLastJumpTrigger)
				{
					//Debug.Log("name: " + name + " went past the last jump trigger, looping back around");
					value = _loopFirstScrollPosition + (value - _loopLastJumpTrigger);
				}
				else if (dir < 0 && value < _loopFirstJumpTrigger)
				{
					//Debug.Log("name: " + name + " went past the first jump trigger, looping back around");
					value = _loopLastScrollPosition - (_loopFirstJumpTrigger - value);
				}
			}
			
			// set the scroll position to the tweened position
			ScrollPosition = value;
		}


		public void JumpToDataIndex(int dataIndex,
			ScrollTweener.Alignment align,
			TweenType tweenType = TweenType.immediate,
			float tweenTime = 0f,
			Action jumpComplete = null
		)
		{
			float offset = (int)align * 0.5f;
			bool useSpacing = align == ScrollTweener.Alignment.Center;
			JumpToDataIndex(dataIndex, offset, offset, useSpacing, tweenType, tweenTime, jumpComplete);
		}

		/// <summary>
		/// Jump to a position in the scroller based on a dataIndex. This overload allows you
		/// to specify a specific offset within a cell as well.
		/// </summary>
		/// <param name="dataIndex">he data index to jump to</param>
		/// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
		/// Outside this range will jump to the location before or after the scroller's viewable area</param>
		/// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
		/// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
		/// <param name="tweenType">What easing to use for the jump</param>
		/// <param name="tweenTime">How long to interpolate to the jump point</param>
		/// <param name="jumpComplete">This delegate is fired when the jump completes</param>
		public void JumpToDataIndex(int dataIndex,
			float scrollerOffset = 0,
			float cellOffset = 0,
			bool useSpacing = true,
			TweenType tweenType = TweenType.immediate,
			float tweenTime = 0f,
			Action jumpComplete = null
		)
		{
			var cellOffsetPosition = 0f;

			if (cellOffset != 0f)
			{
				// calculate the cell offset position

				// get the cell's size
				var cellSize = this.GetCellViewSize(this, dataIndex);

				if (useSpacing)
				{
					// if using spacing add spacing from one side
					cellSize += spacing;

					// if this is not a bounday cell, then add spacing from the other side
					if (dataIndex > 0 && dataIndex < (NumberOfCells - 1)) cellSize += spacing;
				}

				// calculate the position based on the size of the cell and the offset within that cell
				cellOffsetPosition = cellSize * cellOffset;
			}

			var newScrollPosition = 0f;

			// cache the offset for quicker calculation
			var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

			if (loop)
			{
				// if looping, then we need to determine the closest jump position.
				// we do that by checking all three sets of data locations, and returning the closest one

				// get the scroll positions for each data set.
				// Note: we are calculating the position based on the cell view index, not the data index here
				var set1Position = GetScrollPositionForCellViewIndex(dataIndex, CellViewPositionEnum.Before) + offset;
				var set2Position = GetScrollPositionForCellViewIndex(dataIndex + NumberOfCells, CellViewPositionEnum.Before) + offset;
				var set3Position = GetScrollPositionForCellViewIndex(dataIndex + (NumberOfCells * 2), CellViewPositionEnum.Before) + offset;

				// get the offsets of each scroll position from the current scroll position
				var set1Diff = (Mathf.Abs(_scrollPosition - set1Position));
				var set2Diff = (Mathf.Abs(_scrollPosition - set2Position));
				var set3Diff = (Mathf.Abs(_scrollPosition - set3Position));

				// choose the smallest offset from the current position (the closest position)
				if (set1Diff < set2Diff)
				{
					if (set1Diff < set3Diff)
					{
						newScrollPosition = set1Position;
					}
					else
					{
						newScrollPosition = set3Position;
					}
				}
				else
				{
					if (set2Diff < set3Diff)
					{
						newScrollPosition = set2Position;
					}
					else
					{
						newScrollPosition = set3Position;
					}
				}
			}
			else
			{
				// not looping, so just get the scroll position from the dataIndex
				newScrollPosition = GetScrollPositionForDataIndex(dataIndex, CellViewPositionEnum.Before) + offset;
			}



			// clamp the scroll position to a valid location
//			newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));

			// if spacing is used, adjust the final position
			if (useSpacing)
			{
				newScrollPosition -= spacing;
				// move back by the spacing if necessary
//				newScrollPosition = Mathf.Clamp(newScrollPosition - spacing, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
			}

			if (scrollRectXXX.movementType == ScrollRect.MovementType.Clamped)
			{
				newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
			}

			scrollSnap.StartSnapping(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete, null);

			// start tweening
//			m_CoTweening = StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
		}

		/// <summary>
		/// Snaps the scroller on command. This is called internally when snapping is set to true and the velocity
		/// has dropped below the threshold. You can use this to manually snap whenever you like.
		/// </summary>
		public void Snap()
		{
			if (NumberOfCells == 0) return;

			// set snap jumping to true so other events won't process while tweening
//			_snapJumping = true;

			// stop the scroller
//			LinearVelocity = 0;

			// cache the current inertia state and turn off inertia
//			_snapInertia = m_ScrollRect.inertia;
//			m_ScrollRect.inertia = false;


			// calculate the snap position
//			var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(scrollSnap.snapWatchOffset));
			var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01((int)scrollSnap.alignment * 0.5f));

			// get the cell view index of cell at the watch location
			_snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);

			// get the data index of the cell at the watch location
			_snapDataIndex = _snapCellViewIndex % NumberOfCells;

			// jump the snapped cell to the jump offset location and center it on the cell offset
			JumpToDataIndex(_snapDataIndex, scrollSnap.alignment, scrollSnap.snapTweenType, scrollSnap.snapTweenTime, null);
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
		/// </summary>
		/// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForCellViewIndex(int cellViewIndex, CellViewPositionEnum insertPosition)
		{
			if (NumberOfCells == 0) return 0;

			if (cellViewIndex == 0 && insertPosition == CellViewPositionEnum.Before)
			{
				return m_ScrollRect.vertical ? padding.top : padding.left;
			}
			else
			{
				if (cellViewIndex < _cellViewOffsetArray.Count)
				{
					// the index is in the range of cell view offsets

					if (insertPosition == CellViewPositionEnum.Before)
					{
						// return the previous cell view's offset + the spacing between cell views
						return _cellViewOffsetArray[cellViewIndex - 1] + spacing + (m_ScrollRect.vertical ? padding.top : padding.left);
					}
					else
					{
						// return the offset of the cell view (offset is after the cell)
						return _cellViewOffsetArray[cellViewIndex] + (m_ScrollRect.vertical ? padding.top : padding.left);
					}
				}
				else
				{
					// get the start position of the last cell (the offset of the second to last cell)
					return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];
				}
			}
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
		/// </summary>
		/// <param name="dataIndex">The data index to look for</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForDataIndex(int dataIndex, CellViewPositionEnum insertPosition)
		{
			return GetScrollPositionForCellViewIndex(loop ? this.GetNumberOfCells(this) + dataIndex : dataIndex, insertPosition);
		}

		/// <summary>
		/// Gets the index of a cell view at a given position
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <returns></returns>
		public int GetCellViewIndexAtPosition(float position)
		{
			// call the overrloaded method on the entire range of the list
			return _GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
		}

		#endregion

		#region Private

		/// <summary>
		/// Cached reference to the scrollRect
		/// </summary>
		private ScrollRect m_ScrollRect;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		private RectTransform m_ScrollRectTransform;

		/// <summary>
		/// Cached reference to the scrollbar if it exists
		/// </summary>
		private Scrollbar _scrollbar;

		/// <summary>
		/// Cached reference to the active cell view container
		/// </summary>
		private RectTransform _container;

		/// <summary>
		/// Cached reference to the layout group that handles view positioning
		/// </summary>
		public HorizontalOrVerticalLayoutGroup _layoutGroup;

		/// <summary>
		/// Reference to the delegate that will tell this scroller information
		/// about the underlying data
		/// </summary>
//		private ScrollRectEx _delegate;

		/// <summary>
		/// Flag to tell the scroller to reload the data
		/// </summary>
		private bool _reloadData = true;

		/// <summary>
		/// Flag to tell the scroller to refresh the active list of cell views
		/// </summary>
		private bool _refreshActive;

		/// <summary>
		/// List of views that have been recycled
		/// </summary>
		private SmallList<ScrollCellView> _recycledCellViews = new SmallList<ScrollCellView>();

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
		private float _scrollPosition;

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
		private int _snapCellViewIndex;

		/// <summary>
		/// The data index we are snapping to
		/// </summary>
		private int _snapDataIndex;

		/// <summary>
		/// The cached value of the last scrollbar visibility setting. This is checked every
		/// frame to see if the scrollbar visibility needs to be changed.
		/// </summary>
		private ScrollRect.ScrollbarVisibility _lastScrollbarVisibility;

		/// <summary>
		/// The size of the active cell view container minus the visibile portion
		/// of the scroller
		/// </summary>
		private float _ScrollSize
		{
			get
			{
				if (m_ScrollRect.vertical)
					return _container.rect.height - m_ScrollRectTransform.rect.height;
				else
					return _container.rect.width - m_ScrollRectTransform.rect.width;
			}
		}

		/// <summary>
		/// This function will create an internal list of sizes and offsets to be used in all calculations.
		/// It also sets up the loop triggers and positions and initializes the cell views.
		/// </summary>
		/// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
		private void _Resize(bool keepPosition)
		{
			// cache the original position
			var originalScrollPosition = _scrollPosition;

			// clear out the list of cell view sizes and create a new list
			_cellViewSizeArray.Clear();
			var offset = _AddCellViewSizes();

			// if looping, we need to create three sets of size data
			if (loop)
			{
				// if the cells don't entirely fill up the scroll area, 
				// make some more size entries to fill it up
				if (offset < ScrollRectSize)
				{
					int additionalRounds = Mathf.CeilToInt(ScrollRectSize / offset);
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
			if (m_ScrollRect.vertical)
				_container.sizeDelta = new Vector2(_container.sizeDelta.x, _cellViewOffsetArray.Last() + padding.top + padding.bottom);
			else
				_container.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, _container.sizeDelta.y);

			// if looping, set up the loop positions and triggers
			if (loop)
			{
				_loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, CellViewPositionEnum.Before) + (spacing * 0.5f);
				_loopLastScrollPosition = GetScrollPositionForCellViewIndex(_loopLastCellIndex, CellViewPositionEnum.After) - ScrollRectSize + (spacing * 0.5f);

				_loopFirstJumpTrigger = _loopFirstScrollPosition - ScrollRectSize;
				_loopLastJumpTrigger = _loopLastScrollPosition + ScrollRectSize;
			}

			// create the visibile cells
			_ResetVisibleCellViews();

			// if we need to maintain our original position
			if (keepPosition)
			{
				ScrollPosition = originalScrollPosition;
			}
			else
			{
				if (loop)
				{
					ScrollPosition = _loopFirstScrollPosition;
				}
				else
				{
					ScrollPosition = 0;
				}
			}

			// set up the visibility of the scrollbar
			ScrollbarVisibility = _scrollbarVisibility;
		}

		/// <summary>
		/// Creates a list of cell view sizes for faster access
		/// </summary>
		/// <returns></returns>
		private float _AddCellViewSizes()
		{
			var offset = 0f;
			// add a size for each row in our data based on how many the delegate tells us to create
			for (var i = 0; i < NumberOfCells; i++)
			{
				// add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
				_cellViewSizeArray.Add(this.GetCellViewSize(this, i) + (i == 0 ? 0 : _layoutGroup.spacing));
				offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
			}

			return offset;
		}

		/// <summary>
		/// Create a copy of the cell view sizes. This is only used in looping
		/// </summary>
		/// <param name="numberOfTimes">How many times the copy should be made</param>
		/// <param name="cellCount">How many cells to copy</param>
		private void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			for (var i = 0; i < numberOfTimes; i++)
			{
				for (var j = 0; j < cellCount; j++)
				{
					_cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? _layoutGroup.spacing : 0));
				}
			}
		}

		/// <summary>
		/// Calculates the offset of each cell, accumulating the values from previous cells
		/// </summary>
		private void _CalculateCellViewOffsets()
		{
			_cellViewOffsetArray.Clear();
			var offset = 0f;
			for (var i = 0; i < _cellViewSizeArray.Count; i++)
			{
				offset += _cellViewSizeArray[i];
				_cellViewOffsetArray.Add(offset);
			}
		}

		/// <summary>
		/// Get a recycled cell with a given identifier if available
		/// </summary>
		/// <param name="cellPrefab">The prefab to check for</param>
		/// <returns></returns>
		private ScrollCellView _GetRecycledCellView(ScrollCellView cellPrefab)
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				if (_recycledCellViews[i].cellIdentifier == cellPrefab.cellIdentifier)
				{
					// the cell view was found, so we use this recycled one.
					// we also remove it from the recycled list
					var cellView = _recycledCellViews.RemoveAt(i);
					return cellView;
				}
			}

			return null;
		}

		/// <summary>
		/// This sets up the visible cells, adding and recycling as necessary
		/// </summary>
		private void _ResetVisibleCellViews()
		{
			int startIndex;
			int endIndex;

			// calculate the range of the visible cells
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// go through each previous active cell and recycle it if it no longer falls in the range
			var i = 0;
			SmallList<int> remainingCellIndices = new SmallList<int>();
			while (i < _activeCellViews.Count)
			{
				if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
				{
					_RecycleCell(_activeCellViews[i]);
				}
				else
				{
					// this cell index falls in the new range, so we add its
					// index to the reusable list
					remainingCellIndices.Add(_activeCellViews[i].cellIndex);
					i++;
				}
			}

			if (remainingCellIndices.Count == 0)
			{
				// there were no previous active cells remaining, 
				// this list is either brand new, or we jumped to 
				// an entirely different part of the list.
				// just add all the new cell views

				for (i = startIndex; i <= endIndex; i++)
				{
					_AddCellView(i, false);
				}
			}
			else
			{
				// we are able to reuse some of the previous
				// cell views

				// first add the views that come before the 
				// previous list, going backward so that the
				// new views get added to the front
				for (i = endIndex; i >= startIndex; i--)
				{
					if (i < remainingCellIndices.First())
					{
						_AddCellView(i, true);
					}
				}

				// next add teh views that come after the
				// previous list, going forward and adding
				// at the end of the list
				for (i = startIndex; i <= endIndex; i++)
				{
					if (i > remainingCellIndices.Last())
					{
						_AddCellView(i, false);
					}
				}
			}

			// update the start and end indices
			_activeCellViewsStartIndex = startIndex;
			_activeCellViewsEndIndex = endIndex;

			// adjust the padding elements to offset the cell views correctly
			_SetPadders();
		}

		/// <summary>
		/// Recycles all the active cells
		/// </summary>
		private void _RecycleAllCells()
		{
			while (_activeCellViews.Count > 0) _RecycleCell(_activeCellViews[0]);
			_activeCellViewsStartIndex = 0;
			_activeCellViewsEndIndex = 0;
		}

		/// <summary>
		/// Recycles one cell view
		/// </summary>
		/// <param name="cellView"></param>
		private void _RecycleCell(ScrollCellView cellView)
		{
			if (cellViewWillRecycle != null) cellViewWillRecycle(cellView);

			// remove the cell view from the active list
			_activeCellViews.Remove(cellView);

			// add the cell view to the recycled list
			_recycledCellViews.Add(cellView);

			// move the GameObject to the recycled container
			cellView.transform.SetParent(_recycledCellViewContainer);

			// reset the cellView's properties
			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active = false;

			if (cellViewVisibilityChanged != null) cellViewVisibilityChanged(cellView);
		}

		/// <summary>
		/// Creates a cell view, or recycles if it can
		/// </summary>
		/// <param name="cellIndex">The index of the cell view</param>
		/// <param name="listPosition">Whether to add the cell to the beginning or the end</param>
		private void _AddCellView(int cellIndex, bool atStart)
		{
			if (NumberOfCells == 0) return;

			// get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
			var dataIndex = cellIndex % NumberOfCells;
			// request a cell view from the delegate
			var cellView = this.GetCellView(this, dataIndex, cellIndex);

			// set the cell's properties
			cellView.cellIndex = cellIndex;
			cellView.dataIndex = dataIndex;
			cellView.active = true;

			// add the cell view to the active container
			cellView.transform.SetParent(_container, false);
			cellView.transform.localScale = Vector3.one;

			// add a layout element to the cellView
			LayoutElement layoutElement = cellView.GetComponent<LayoutElement>();
			if (layoutElement == null) layoutElement = cellView.gameObject.AddComponent<LayoutElement>();

			// set the size of the layout element
			if (m_ScrollRect.vertical)
				layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
			else
				layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);

			// add the cell to the active list
			if (atStart)
				_activeCellViews.AddStart(cellView);
			else
				_activeCellViews.Add(cellView);

			// set the hierarchy position of the cell view in the container
			cellView.transform.SetSiblingIndex(atStart ? 1 : _container.childCount - 2);

			// call the visibility change delegate if available
			if (cellViewVisibilityChanged != null) cellViewVisibilityChanged(cellView);
		}

		/// <summary>
		/// This function adjusts the two padders that control the first cell view's
		/// offset and the overall size of each cell.
		/// </summary>
		private void _SetPadders()
		{
			if (NumberOfCells == 0) return;

			// calculate the size of each padder
			var firstSize = _cellViewOffsetArray[_activeCellViewsStartIndex] - _cellViewSizeArray[_activeCellViewsStartIndex];
			var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[_activeCellViewsEndIndex];

			if (m_ScrollRect.vertical)
			{
				// set the first padder and toggle its visibility
				_firstPadder.minHeight = firstSize;
				_firstPadder.gameObject.SetActive(_firstPadder.minHeight > 0);

				// set the last padder and toggle its visibility
				_lastPadder.minHeight = lastSize;
				_lastPadder.gameObject.SetActive(_lastPadder.minHeight > 0);
			}
			else
			{
				// set the first padder and toggle its visibility
				_firstPadder.minWidth = firstSize;
				_firstPadder.gameObject.SetActive(_firstPadder.minWidth > 0);

				// set the last padder and toggle its visibility
				_lastPadder.minWidth = lastSize;
				_lastPadder.gameObject.SetActive(_lastPadder.minWidth > 0);
			}
		}

		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		private void _RefreshActive()
		{
			_refreshActive = false;

			int startIndex;
			int endIndex;
			var nextVelocity = Vector2.zero;

			// if looping, check to see if we scrolled past a trigger
			if (loop)
			{
				if (_scrollPosition < _loopFirstJumpTrigger)
				{
					nextVelocity = m_ScrollRect.velocity;
					ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition);
					m_ScrollRect.velocity = nextVelocity;
				}
				else if (_scrollPosition > _loopLastJumpTrigger)
				{
					nextVelocity = m_ScrollRect.velocity;
					ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger);
					m_ScrollRect.velocity = nextVelocity;
				}
			}

			// get the range of visibile cells
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == _activeCellViewsStartIndex && endIndex == _activeCellViewsEndIndex) return;

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
			var startPosition = _scrollPosition;
			var endPosition = _scrollPosition + (m_ScrollRect.vertical ? m_ScrollRectTransform.rect.height : m_ScrollRectTransform.rect.width);

			// calculate each index based on the positions
			startIndex = GetCellViewIndexAtPosition(startPosition);
			endIndex = GetCellViewIndexAtPosition(endPosition);
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
			if (startIndex >= endIndex) return startIndex;

			// determine the middle point of our binary search
			var middleIndex = (startIndex + endIndex) / 2;

			// if the middle index is greater than the position, then search the last
			// half of the binary tree, else search the first half
			if ((_cellViewOffsetArray[middleIndex] + (m_ScrollRect.vertical ? padding.top : padding.left)) >= position)
				return _GetCellIndexAtPosition(position, startIndex, middleIndex);
			else
				return _GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
		}

		public RectTransform contentXXX
		{
			get
			{
				if (!m_ContentXXX)
				{
					var sr = scrollRectXXX;
					if (!sr.content)
					{
						var go = new GameObject("Content", typeof(RectTransform));
						go.transform.SetParent(sr.transform);
						sr.content = go.GetComponent<RectTransform>();
					}
					m_ContentXXX = scrollRectXXX.content;
				}
				return m_ContentXXX;
			}
		}
		RectTransform m_ContentXXX;


		public ScrollRect scrollRectXXX
		{
			get
			{
				if (!m_ScrollRectXXX)
				{
					m_ScrollRectXXX = GetComponent<ScrollRect>();
				}
				return m_ScrollRectXXX;
			}
		}
		ScrollRect m_ScrollRectXXX;

		public ScrollTweener scrollSnap
		{
			get
			{
				if (!m_ScrollSnap)
				{
					m_ScrollSnap = GetComponent<ScrollTweener>() ?? gameObject.AddComponent<ScrollTweener>();
				}
				return m_ScrollSnap;
			}
		}
		ScrollTweener m_ScrollSnap;

		public HorizontalOrVerticalLayoutGroup layoutGroupXXX
		{
			get
			{
				if (!m_LayoutGroupXXX)
				{
					m_LayoutGroupXXX = contentXXX.GetComponent<HorizontalOrVerticalLayoutGroup>();
				}

				if (!m_LayoutGroupXXX || scrollRectXXX.vertical != (m_LayoutGroupXXX is VerticalLayoutGroup))
				{
#if UNITY_EDITOR
					if (!Application.isPlaying)
					{
						UnityEditor.EditorApplication.delayCall += UpdateLauyout;
					}
					else
#endif
					{
						UpdateLauyout();
					}
				}

				return m_LayoutGroupXXX;
			}
		}
		HorizontalOrVerticalLayoutGroup m_LayoutGroupXXX;


		void UpdateLauyout()
		{
			var type = scrollRectXXX.vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
			var layout = contentXXX.GetComponent<LayoutGroup>();
			if (!layout)
			{
				m_LayoutGroupXXX = contentXXX.gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
			}
			else if (scrollRectXXX.vertical != (layout is VerticalLayoutGroup))
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					ComponentConverter.ConvertTo(layout, type);
					m_LayoutGroupXXX = contentXXX.GetComponent<HorizontalOrVerticalLayoutGroup>();
				}
				else
#endif
				{
					RectOffset padding = layout ? layout.padding : new RectOffset();
					float spacing = (layout is HorizontalOrVerticalLayoutGroup) ? (layout as HorizontalOrVerticalLayoutGroup).spacing : 0;
					DestroyImmediate(layout);
					m_LayoutGroupXXX = contentXXX.gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
					m_LayoutGroupXXX.padding = padding;
					m_LayoutGroupXXX.spacing = spacing;
				}
			}
		}

		public Scrollbar scrollbarXXX
		{
			get
			{
				return scrollRectXXX.vertical ? m_ScrollRect.verticalScrollbar : m_ScrollRect.horizontalScrollbar;
			}
		}

		/// <summary>
		/// Caches and initializes the scroller
		/// </summary>
		protected virtual void Awake()
		{
			GameObject go;

			// cache some components
			m_ScrollRect = this.GetComponent<ScrollRect>();
			m_ScrollRectTransform = m_ScrollRect.transform as RectTransform;

			if (!m_ScrollRect || !m_ScrollRect.vertical && !m_ScrollRect.horizontal)
			{
				//無効なスクロール
				return;
			}

			_container = contentXXX;
			_layoutGroup = layoutGroupXXX;
			_scrollbar = scrollbarXXX;

			// force the scroller to scroll in the direction we want
			// set the containers anchor and pivot
			if (m_ScrollRect.vertical)
			{
				m_ScrollRect.horizontal = false;
				_container.anchorMin = new Vector2(0, 1);
				_container.anchorMax = Vector2.one;
				_container.pivot = new Vector2(0.5f, 1f);
				_scrollbarVisibility = m_ScrollRect.verticalScrollbarVisibility;
			}
			else
			{
				m_ScrollRect.vertical = false;
				_container.anchorMin = Vector2.zero;
				_container.anchorMax = new Vector2(0, 1f);
				_container.pivot = new Vector2(0, 0.5f);
				_scrollbarVisibility = m_ScrollRect.horizontalScrollbarVisibility;
			}

			_container.offsetMax = Vector2.zero;
			_container.offsetMin = Vector2.zero;
			_container.localPosition = Vector3.zero;
			_container.localRotation = Quaternion.identity;
			_container.localScale = Vector3.one;

			m_ScrollRect.content = _container;

			_layoutGroup.childAlignment = TextAnchor.UpperLeft;
			_layoutGroup.childForceExpandHeight = true;
			_layoutGroup.childForceExpandWidth = true;

			// create the padder objects
			go = new GameObject("First Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(_container, false);
			_firstPadder = go.GetComponent<LayoutElement>();

			go = new GameObject("Last Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(_container, false);
			_lastPadder = go.GetComponent<LayoutElement>();

			// create the recycled cell view container
			go = new GameObject("Recycled Cells", typeof(RectTransform));
			go.transform.SetParent(m_ScrollRect.transform, false);
			_recycledCellViewContainer = go.GetComponent<RectTransform>();
			_recycledCellViewContainer.gameObject.SetActive(false);

			// set up the last values for updates
			_lastScrollRectSize = ScrollRectSize;
			_lastLoop = loop;
			_lastScrollbarVisibility = _scrollbarVisibility;
		}

//		protected virtual void Update()
//		{
//			if (_reloadData)
//			{
//				// if the reload flag is true, then reload the data
//				ReloadData();
//			}
//
//			// if the scroll rect size has changed and looping is on,
//			// or the loop setting has changed, then we need to resize
//			if (
//				(loop && _lastScrollRectSize != ScrollRectSize)
//				||
//				(loop != _lastLoop)
//			)
//			{
//				_Resize(true);
//				_lastScrollRectSize = ScrollRectSize;
//
//				_lastLoop = loop;
//			}
//
//			// update the scroll bar visibility if it has changed
//			if (_lastScrollbarVisibility != _scrollbarVisibility)
//			{
//				ScrollbarVisibility = _scrollbarVisibility;
//				_lastScrollbarVisibility = _scrollbarVisibility;
//			}
//		}

		void LateUpdate()
		{
			if (_reloadData)
			{
				// if the reload flag is true, then reload the data
				ReloadData();
			}

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if (
				(loop && _lastScrollRectSize != ScrollRectSize)
				||
				(loop != _lastLoop)
			)
			{
				_Resize(true);
				_lastScrollRectSize = ScrollRectSize;

				_lastLoop = loop;
			}

			// update the scroll bar visibility if it has changed
			if (_lastScrollbarVisibility != _scrollbarVisibility)
			{
				ScrollbarVisibility = _scrollbarVisibility;
				_lastScrollbarVisibility = _scrollbarVisibility;
			}




			if (_refreshActive)
			{
				_RefreshActive();
			}
		}

		protected virtual void OnEnable()
		{
			// when the scroller is enabled, add a listener to the onValueChanged handler
			m_ScrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
		}

		protected virtual void OnDisable()
		{
			// when the scroller is disabled, remove the listener
			m_ScrollRect.onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
		}

		/// <summary>
		/// Handler for when the scroller changes value
		/// </summary>
		/// <param name="val">The scroll rect's value</param>
		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			// set the internal scroll position
			if (m_ScrollRect.vertical)
				ScrollPosition = (1f - val.y) * _ScrollSize;
			else
				ScrollPosition = val.x * _ScrollSize;
			_refreshActive = true;

			_RefreshActive();
		}

		#endregion
	}
}