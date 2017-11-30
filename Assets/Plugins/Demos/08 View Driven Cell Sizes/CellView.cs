using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.ViewDrivenCellSizes
{
    public class CellView : EnhancedScrollerCellView
    {
        public Text someTextText;

        /// <summary>
        /// A reference to the rect transform which will be
        /// updated by the content size fitter
        /// </summary>
        public RectTransform textRectTransform;

        /// <summary>
        /// The space around the text label so that we
        /// aren't up against the edges of the cell
        /// </summary>
        public RectOffset textBuffer;

        public void SetData(Data data)
        {
            someTextText.text = data.someText;

            // get the size of the rect transform and store it.
            // First Pass (frame countdown 2): this will be zero
            // Second Pass (frame countdown 1): this will be the size set by the content size fitter
            // Third Pass (frame countdown 0): this will be redundantly set, but the scroller needs to refresh to pull the new sizes from the second pass
            var sizeY = textRectTransform.sizeDelta.y;
            if (sizeY > 0)
            {
                // if the size has been set by the content size fitter, then we add in some padding so the
                // the text isn't up against the border of the cell
                sizeY += textBuffer.top + textBuffer.bottom;
            }

            // set the size of the cell in the model
            data.cellSize = sizeY;
        }
    }
}