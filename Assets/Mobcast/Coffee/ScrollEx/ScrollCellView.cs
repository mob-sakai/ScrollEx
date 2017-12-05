using UnityEngine;
using System;
using System.Collections;
using EnhancedUI;

namespace Mobcast.Coffee
{
	/// <summary>
	/// This is the base class that all cell views should derive from
	/// </summary>
	public abstract class ScrollCellView : MonoBehaviour
	{
		public int templateId { get; set;}
		/// <summary>
		/// The cellIdentifier is a unique string that allows the scroller
		/// to handle different types of cells in a single list. Each type
		/// of cell should have its own identifier
		/// </summary>
		public string cellIdentifier;

		/// <summary>
		/// The cell index of the cell view
		/// This will differ from the dataIndex if the list is looping
		/// </summary>
		[NonSerialized]
		public int cellIndex;

		/// <summary>
		/// The data index of the cell view
		/// </summary>
		[NonSerialized]
		public int dataIndex;

		/// <summary>
		/// Whether the cell is active or recycled
		/// </summary>
		[NonSerialized]
		public bool active;

		/// <summary>
		/// This method is called by the scroller when the RefreshActiveCellViews is called on the scroller
		/// You can override it to update your cell's view UID
		/// </summary>
		public abstract void RefreshCellView();

		/// <summary>
		/// This delegate handles the visibility changes of cell views
		/// </summary>
		/// <param name="cellView">The cell view that changed visibility</param>
		public abstract void OnChangedCellViewVisibility();

		/// <summary>
		/// This delegate will be fired just before the cell view is recycled
		/// </summary>
		/// <param name="cellView"></param>
		public abstract void OnWillRecycleCellView();

	}
}
