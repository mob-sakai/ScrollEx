using System.Collections.Generic;
using Mobcast.Coffee;
using Mobcast.Coffee.UI;
using UnityEngine;

namespace Mobcast.Coffee.UI.ScrollDemo
{
	/// <summary>
	/// [デモ] スコアランキングスクロールのコントローラです.
	/// スコアには以下の要素が含まれます
	/// * 名前
	/// * スコア
	/// * ユーザーアイコン(URL)
	/// </summary>
	public class Demo_ScrollController : MonoBehaviour, IScrollViewController
	{
#region Serialize

		[Header("テンプレート")]
		[SerializeField] Demo_ScrollCellView m_CellViewTemplate;

		[Header("スクロールビュー")]
		[SerializeField] ScrollRectEx m_ScrollRectEx;

#endregion Serialize

#region Public

		/// <summary>
		/// データの要素数を取得します.
		/// </summary>
		/// <returns>データの要素数.</returns>
		public int GetDataCount()
		{
			return _dummyData.Count;
		}

		/// <summary>
		/// データインデックスに対するセルビューのサイズを取得します.
		/// セルビューサイズをデータに基づいて可変させたい場合、このメソッドを利用して調整できます.
		/// </summary>
		/// <returns>セルビューサイズ.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		public float GetCellViewSize(int dataIndex)
		{
			return _cellSize;
		}

		/// <summary>
		/// データインデックスに対するセルビューを取得します.
		/// オブジェクトプールを利用して取得/新規作成する場合、GetCellView([TEMPLATE_CELLVIEW_PREFAB]) でセルビューを取得できます.
		/// 取得したセルビューに対し、データを引き渡してください.
		/// </summary>
		/// <returns>セルビュー.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		public ScrollCellView GetCellView(int dataIndex)
		{
			// セルビューを取得. オブジェクトプールに存在する場合、優先的に利用します.
			Demo_ScrollCellView cellView = m_ScrollRectEx.GetCellView(m_CellViewTemplate) as Demo_ScrollCellView;
			cellView.gameObject.SetActive(true);

			// データをセルビューに渡します.
			cellView.data = _dummyData[dataIndex];

			return cellView;
		}

#endregion Public

		const string IMAGE_DOMAIN = "https://www.webpagefx.com/tools/emoji-cheat-sheet/graphics/emojis/";
		float _cellSize;

		/// <summary>
		/// ダミーデータ.
		/// 本来であれば、APIを通じて取得する必要があるでしょう.
		/// </summary>
		List<Demo_ScrollData> _dummyData = new List<Demo_ScrollData>()
		{
			new Demo_ScrollData(){ name = "Aさん", score = 99000000, imageUrl = IMAGE_DOMAIN + "smile.png" },
			new Demo_ScrollData(){ name = "Bさん", score = 98000000, imageUrl = IMAGE_DOMAIN + "bowtie.png" },
			new Demo_ScrollData(){ name = "Cさん", score = 97000000, imageUrl = IMAGE_DOMAIN + "laughing.png" },
			new Demo_ScrollData(){ name = "Dさん", score = 96000000, imageUrl = IMAGE_DOMAIN + "heart_eyes.png" },
			new Demo_ScrollData(){ name = "Eさん", score = 95000000, imageUrl = IMAGE_DOMAIN + "wink.png" },
			new Demo_ScrollData(){ name = "Fさん", score = 94000000, imageUrl = IMAGE_DOMAIN + "scream.png" },
//			new Demo_ScrollData(){ name = "Gさん", score = 93000000, imageUrl = IMAGE_DOMAIN + "girl.png" },
//			new Demo_ScrollData(){ name = "Hさん", score = 92000000, imageUrl = IMAGE_DOMAIN + "boy.png" },
//			new Demo_ScrollData(){ name = "Iさん", score = 91000000, imageUrl = IMAGE_DOMAIN + "fearful.png" },
//			new Demo_ScrollData(){ name = "Jさん", score = 90000000, imageUrl = IMAGE_DOMAIN + "rage.png" },
//			new Demo_ScrollData(){ name = "Kさん", score = 89000000, imageUrl = IMAGE_DOMAIN + "mask.png" },
//			new Demo_ScrollData(){ name = "Lさん", score = 88000000, imageUrl = IMAGE_DOMAIN + "sunglasses.png" },
//			new Demo_ScrollData(){ name = "Mさん", score = 87000000, imageUrl = IMAGE_DOMAIN + "joy.png" },
//			new Demo_ScrollData(){ name = "Nさん", score = 86000000, imageUrl = IMAGE_DOMAIN + "alien.png" },
//			new Demo_ScrollData(){ name = "Oさん", score = 85000000, imageUrl = IMAGE_DOMAIN + "imp.png" },
//			new Demo_ScrollData(){ name = "Pさん", score = 84000000, imageUrl = IMAGE_DOMAIN + "man.png" },
//			new Demo_ScrollData(){ name = "Qさん", score = 83000000, imageUrl = IMAGE_DOMAIN + "woman.png" },
//			new Demo_ScrollData(){ name = "Rさん", score = 82000000, imageUrl = IMAGE_DOMAIN + "baby.png" },
//			new Demo_ScrollData(){ name = "Sさん", score = 81000000, imageUrl = IMAGE_DOMAIN + "cop.png" },
//			new Demo_ScrollData(){ name = "Tさん", score = 80000000, imageUrl = IMAGE_DOMAIN + "angel.png" },
//			new Demo_ScrollData(){ name = "Uさん", score = 79000000, imageUrl = IMAGE_DOMAIN + "bust_in_silhouette.png" },
//			new Demo_ScrollData(){ name = "Vさん", score = 78000000, imageUrl = IMAGE_DOMAIN + "cat.png" },
//			new Demo_ScrollData(){ name = "Wさん", score = 77000000, imageUrl = IMAGE_DOMAIN + "dog.png" },
//			new Demo_ScrollData(){ name = "Xさん", score = 76000000, imageUrl = IMAGE_DOMAIN + "frog.png" },
//			new Demo_ScrollData(){ name = "Yさん", score = 75000000, imageUrl = IMAGE_DOMAIN + "tiger.png" },
//			new Demo_ScrollData(){ name = "Zさん", score = 74000000, imageUrl = IMAGE_DOMAIN + "pig.png" },
		};

		void Start()
		{
			// テンプレートを非表示
			m_CellViewTemplate.gameObject.SetActive(false);

			// セルサイズをテンプレートから取得
			var size = (m_CellViewTemplate.transform as RectTransform).sizeDelta;
			_cellSize = m_ScrollRectEx.scrollRect.vertical ? size.y : size.x;

			// コントローラを割り当て、リロード実行.
			m_ScrollRectEx.controller = this;
			m_ScrollRectEx.ReloadData();
		}
	}
}
