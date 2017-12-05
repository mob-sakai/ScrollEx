

using UnityEngine;
using UnityEngine.UI;
//using EnhancedUI.EnhancedScroller;
using Mobcast.Coffee;

namespace EnhancedScrollerDemos.SuperSimpleDemo
{
	/// <summary>
	/// This is the view of our cell which handles how the cell looks.
	/// </summary>
	public class TestCellView : ScrollCellView
	{
		/// <summary>
		/// A reference to the UI Text element to display the cell data
		/// </summary>
		public Text someTextText;

		/// <summary>
		/// This function just takes the Demo data and displays it
		/// </summary>
		/// <param name="data"></param>
		public void SetData(Data data)
		{
			// update the UI text with the cell data
			someTextText.text = data.someText;
		}


		#region implemented abstract members of ScrollCellView
		public override void RefreshCellView()
		{
		}
		public override void OnChangedCellViewVisibility()
		{
		}
		public override void OnWillRecycleCellView()
		{
		}
		#endregion
	}
}