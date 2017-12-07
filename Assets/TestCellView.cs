

using UnityEngine;
using UnityEngine.UI;
//using EnhancedUI.EnhancedScroller;
using Mobcast.Coffee;
using Mobcast.Coffee.UI;
using Mobcast.Coffee.Transition;

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

		public UIAnimation anim;

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
		public override void OnChangedVisibility(bool visible)
		{
		}
		public override void OnBeforePool()
		{
		}


		public override void OnPositionChanged(float normalizedPosition)
		{
			// someTextText.text = string.Format("{0:P1}",normalizedPosition);
			anim.Sample(Mathf.Clamp01(normalizedPosition));
		}

		#endregion
	}
}