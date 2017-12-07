//using UnityEngine;
//using System.Collections;
//
//public class TestScroller : MonoBehaviour {
//
//	// Use this for initialization
//	void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
//}

using UnityEngine;
using System.Collections;
using EnhancedUI;

//using EnhancedUI.EnhancedScroller;
using Mobcast.Coffee;
using Mobcast.Coffee.UI;

namespace EnhancedScrollerDemos.SuperSimpleDemo
{
	/// <summary>
	/// Set up our demo script as a delegate for the scroller by inheriting from the IEnhancedScrollerDelegate interface
	/// 
	/// EnhancedScroller delegates will handle telling the scroller:
	///  - How many cells it should allocate room for (GetNumberOfCells)
	///  - What each cell size is (GetCellSize)
	///  - What the cell at a given index should be (GetCell)
	/// </summary>
	public class ScrollController_Demo : MonoBehaviour, IScrollViewController
	{
		#region IScrollViewDelegate implementation

		public int GetDataCount()
		{
			return _data.Count;
		}

		public float GetCellViewSize(int dataIndex)
		{
			return (dataIndex % 2 == 0 ? 200 : 100f);
		}

		public ScrollCellView GetCellView(int dataIndex)
		{
			CellView_Demo cellView = scrollView.GetCellView(cellViewPrefab) as CellView_Demo;
			cellView.name = "Cell " + dataIndex.ToString();
			cellView.SetData(_data[dataIndex]);
			return cellView;
		}

		#endregion

		/// <summary>
		/// Internal representation of our data. Note that the scroller will never see
		/// this, so it separates the data from the layout using MVC principles.
		/// </summary>
		private SmallList<Data> _data;

		/// <summary>
		/// This will be the prefab of each cell in our scroller. Note that you can use more
		/// than one kind of cell, but this example just has the one type.
		/// </summary>
		public ScrollCellView cellViewPrefab;

		public ScrollRectEx scrollView;

		/// <summary>
		/// Be sure to set up your references to the scroller after the Awake function. The 
		/// scroller does some internal configuration in its own Awake function. If you need to
		/// do this in the Awake function, you can set up the script order through the Unity editor.
		/// In this case, be sure to set the EnhancedScroller's script before your delegate.
		/// 
		/// In this example, we are calling our initializations in the delegate's Start function,
		/// but it could have been done later, perhaps in the Update function.
		/// </summary>
		void Start()
		{
			scrollView.controller = this;
			LoadLargeData();
		}

		/// <summary>
		/// Populates the data with a lot of records
		/// </summary>
		private void LoadLargeData()
		{
			// set up some simple data
			_data = new SmallList<Data>();
			for (var i = 0; i < 1000; i++)
				_data.Add(new Data() { someText = "Cell Data Index " + i.ToString() });

			// tell the scroller to reload now that we have the data
			scrollView.ReloadData();
		}

		/// <summary>
		/// Populates the data with a small set of records
		/// </summary>
		private void LoadSmallData()
		{
			// set up some simple data
			_data = new SmallList<Data>();

			_data.Add(new Data() { someText = "A" });
			_data.Add(new Data() { someText = "B" });
			_data.Add(new Data() { someText = "C" });

			// tell the scroller to reload now that we have the data
			scrollView.ReloadData();
		}

		#region UI Handlers

		/// <summary>
		/// Button handler for the large data loader
		/// </summary>
		public void LoadLargeDataButton_OnClick()
		{
			LoadLargeData();
		}

		/// <summary>
		/// Button handler for the small data loader
		/// </summary>
		public void LoadSmallDataButton_OnClick()
		{
			LoadSmallData();
		}

		#endregion

		#region EnhancedScroller Handlers


		#endregion
	}
}
