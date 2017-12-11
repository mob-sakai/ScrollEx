using UnityEngine;
using System.Collections;
using UnityEngine.UI;



namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// スクロールビューの制御インターフェース.
	/// スクロールに利用するデータの件数の把握と、n番目のデータのセルビューとセルサイズの把握を役割としてもたせます.
	/// 継承したクラスのインスタンスを ScrollRectEx.controller に割り当てることで、スクロールビューの制御を委任できます.
	/// </summary>
	public interface IScrollViewController
	{
		/// <summary>
		/// データの要素数を取得します.
		/// </summary>
		/// <returns>データの要素数.</returns>
		int GetDataCount();

		/// <summary>
		/// データインデックスに対するセルビューのサイズを取得します.
		/// セルビューサイズをデータに基づいて可変させたい場合、このメソッドを利用して調整できます.
		/// </summary>
		/// <returns>セルビューサイズ.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		float GetCellViewSize(int dataIndex);

		/// <summary>
		/// データインデックスに対するセルビューを取得します.
		/// オブジェクトプールを利用して取得/新規作成する場合、GetCellView([TEMPLATE_CELLVIEW_PREFAB]) でセルビューを取得できます.
		/// 取得したセルビューに対し、データを引き渡してください.
		/// </summary>
		/// <returns>セルビュー.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		ScrollCellView GetCellView(int dataIndex);
	}

	/// <summary>
	/// デフォルトのスクロールビュー制御.
	/// ScrollRectの下位互換のためにあり、データを持ちません.
	/// </summary>
	public sealed class DefaultScrollViewController : IScrollViewController
	{
		public DefaultScrollViewController(ScrollRectEx scroll)
		{
			_scroll = scroll;
		}

		readonly ScrollRectEx _scroll;

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
			int sibling = dataIndex;
			if (_scroll.firstPaddingSiblingIndex <= sibling)
				sibling++;
			if (_scroll.lastPaddingSiblingIndex <= sibling)
				sibling++;

			// デフォルトコントローラー: サイズは、RectTransformまたはLayoutElementから取得します.
			var rt = _scroll.content.GetChild(sibling) as RectTransform;
			var layoutElement = rt.GetComponent<LayoutElement>();

			if (layoutElement)
				return _scroll.scrollRect.vertical ? layoutElement.preferredHeight : layoutElement.preferredWidth;
			else
				return _scroll.scrollRect.vertical ? rt.rect.height : rt.rect.width;
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
