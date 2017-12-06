using UnityEngine;
using System.Collections;
using UnityEngine.UI;



namespace Mobcast.Coffee
{
	/// <summary>
	/// スクロールビューコントローラインターフェース.
	/// </summary>
	public interface IScrollViewController
	{
		/// <summary>
		/// コントローラーがもつ、データの要素数を取得します.
		/// </summary>
		int GetDataCount();

		/// <summary>
		/// データインデックスに対するセルビューのサイズを取得します.
		/// </summary>
		float GetCellViewSize(int dataIndex);

		/// <summary>
		/// データインデックスに紐付けるセルビューを取得します.
		/// </summary>
		ScrollCellView GetCellView(int dataIndex);
	}

	/// <summary>
	/// デフォルトスクロールビューコントローラ.
	/// このコントローラはデータを持たず、
	/// </summary>
	public class DefaultScrollViewController : IScrollViewController
	{
		
		public DefaultScrollViewController(ScrollRect scroll)
		{
			_scroll = scroll;
		}

		ScrollRect _scroll;

		/// <summary>
		/// コントローラーがもつ、データの要素数を取得します.
		/// </summary>
		public int GetDataCount()
		{
			// デフォルトコントローラー: (content内のオブジェクト数)-(paddingオブジェクト)により求められます.
			return _scroll.content.childCount - 2;
		}

		/// <summary>
		/// データインデックスに対するセルビューのサイズを取得します.
		/// </summary>
		public float GetCellViewSize(int dataIndex)
		{
			// デフォルトコントローラー: サイズは、RectTransformまたはLayoutElementから取得します.
			var rt = _scroll.content.GetChild(dataIndex) as RectTransform;
			var layoutElement = rt.GetComponent<LayoutElement>();

			if (layoutElement)
				return _scroll.vertical ? layoutElement.preferredHeight : layoutElement.preferredWidth;
			else
				return _scroll.vertical ? rt.rect.height : rt.rect.width;
		}

		/// <summary>
		/// データインデックスに紐付けるセルビューを取得します.
		/// </summary>
		public ScrollCellView GetCellView(int dataIndex)
		{
			// デフォルトコントローラー: セルビューは利用しません.
			return null;
		}
	}
}
