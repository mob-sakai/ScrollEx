using System.Collections;
using Mobcast.Coffee;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Mobcast.Coffee.UI.ScrollDemo
{
	/// <summary>
	/// 名前、アイコン、スコアが含まれるセルビュー.
	/// </summary>
	public class Demo_ScrollCellView : ScrollCellView
	{
		
#region Serialize

		[Header("名前")]
		public Text m_TextName;

		[Header("スコア")]
		public Text m_TextScore;

		[Header("アイコン画像")]
		public RawImage m_ImageIcon;

#endregion Serialize

#region Public

		/// <summary>
		/// セルビューに表示させるデータ.
		/// </summary>
		public Demo_ScrollData data { get; set; }

		/// <summary>
		/// データリロードや明示的なリフレッシュがされたときにコールされます.
		/// </summary>
		public override void OnRefresh()
		{
		}

		/// <summary>
		/// 表示状態が変化したときにコールされます.
		/// </summary>
		/// <param name="visible">表示状態.</param>
		public override void OnChangedVisibility(bool visible)
		{
			Dispose();
			if (visible)
			{
				// テキスト設定
				name = data.name;
				m_TextName.text = data.name;
				m_TextScore.text = data.score.ToString();

				// アイコンのロード.
				StartCoroutine(Co_GetTexture(data.imageUrl));
			}
		}

		/// <summary>
		/// セルビューがオブジェクトプールに返却される前にコールされます.
		/// </summary>
		public override void OnBeforePool()
		{
		}

		/// <summary>
		/// 座標が変更されたときに、をコールされます.
		/// </summary>
		/// <param name="normalizedPosition">スクロール領域における正規化された位置.</param>
		public override void OnPositionChanged(float normalizedPosition)
		{
		}

#endregion Public

		Coroutine _coroutine;
		UnityWebRequest _request;

		/// <summary>
		/// テスクチャをWebからロードします(コルーチン).
		/// </summary>
		IEnumerator Co_GetTexture(string url)
		{
			_request = UnityWebRequest.GetTexture(url);
//			_request.disposeDownloadHandlerOnDispose = false;
			yield return _request.Send();

			if (!_request.isError)
				m_ImageIcon.texture = (_request.downloadHandler as DownloadHandlerTexture).texture;
			else
				Debug.LogError(_request.error);
			_request.Dispose();
			_request = null;
		}

		/// <summary>
		/// 進行中のテスクチャロードコルーチンを停止させます.
		/// </summary>
		void Dispose()
		{
			m_ImageIcon.texture = null;
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
			if (_request != null)
			{
				_request.Abort();
				_request.Dispose();
				_request = null;
			}
		}
	}
}