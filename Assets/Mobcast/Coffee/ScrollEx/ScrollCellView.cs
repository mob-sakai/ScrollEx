using UnityEngine;

namespace Mobcast.Coffee
{
	/// <summary>
	/// スクロールセルビュー.
	/// </summary>
	public abstract class ScrollCellView : MonoBehaviour
	{
		/// <summary>
		/// テンプレートセルビューのインスタンスIDです.
		/// このIDは生成元のテンプレートセルビューを特定するために利用します.
		/// </summary>
		public int templateId { get; set; }

		/// <summary>
		/// セルインデックス.
		/// このインデックスは、データサイズよりも大きな値を持つ可能性があります.
		/// </summary>
		public int cellIndex { get; set; }

		/// <summary>
		/// データインデックス.
		/// このインデックスは、0〜(データサイズ-1)の値です.
		/// </summary>
		public int dataIndex { get; set; }

		/// <summary>
		/// 現在アクティブなセルか.
		/// </summary>
		public bool active { get; set; }

		/// <summary>
		/// データリロードや明示的なリフレッシュがされたときにコールされます.
		/// </summary>
		public abstract void OnRefresh();

		/// <summary>
		/// 表示状態が変化したときにコールされます.
		/// </summary>
		/// <param name="visible">表示状態.</param>
		public abstract void OnChangedVisibility(bool visible);

		/// <summary>
		/// セルビューがオブジェクトプールに返却される前にコールされます.
		/// </summary>
		public abstract void OnBeforePool();

		/// <summary>
		/// 座標が変更されたときに、をコールされます.
		/// </summary>
		/// <param name="normalizedPosition">スクロール領域における正規化された位置.</param>
		public abstract void OnPositionChanged(float normalizedPosition);

	}
}
