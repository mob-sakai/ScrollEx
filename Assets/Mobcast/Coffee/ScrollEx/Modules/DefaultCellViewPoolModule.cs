using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace Mobcast.Coffee.UI.ScrollModule
{
	/// <summary>
	/// セルビュー用オブジェクトプールインターフェース.
	/// </summary>
	public interface ICellViewPool
	{
		/// <summary>
		/// セルビューをプールから取得または新規作成します.
		/// </summary>
		ScrollCellView RentCellView(ScrollCellView template);

		/// <summary>
		/// セルビューをプールに返却します.
		/// </summary>
		void ReturnCellView(ScrollCellView obj);
	}

	/// <summary>
	/// セルビューのオブジェクトプール管理モジュールです(デフォルト).
	/// プール内の未使用のインスタンス、または新しいインスタンスを返します.
	/// </summary>
	public class DefaultCellViewPoolModule : ICellViewPool
	{
		public DefaultCellViewPoolModule(Transform root)
		{
			_root = root;
		}

		readonly Transform _root;
		readonly Dictionary<int, Stack<ScrollCellView>> _pool = new Dictionary<int, Stack<ScrollCellView>>();

		/// <summary>
		/// セルビューをプールから取得または新規作成します.
		/// </summary>
		public ScrollCellView RentCellView(ScrollCellView template)
		{
			var id = template.GetInstanceID();
			Stack<ScrollCellView> stack;
			if (_pool.TryGetValue(id, out stack) && 0 < stack.Count)
				return stack.Pop();

			var cellView = GameObject.Instantiate(template);
			cellView.templateId = id;
			cellView.transform.SetParent(_root, false);
			cellView.transform.localPosition = Vector3.zero;
			cellView.transform.localRotation = Quaternion.identity;
			cellView.transform.localScale = Vector3.zero;
			cellView.gameObject.SetActive(true);
			return cellView;
		}

		/// <summary>
		/// セルビューをプールに返却します.
		/// </summary>
		public void ReturnCellView(ScrollCellView obj)
		{
			int id = obj.templateId;
			Stack<ScrollCellView> stack;
			if (!_pool.TryGetValue(id, out stack))
			{
				stack = new Stack<ScrollCellView>();
				_pool.Add(id, stack);
			}

			obj.transform.SetParent(_root, false);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.zero;

			if (!stack.Contains(obj))
				stack.Push(obj);
		}
	}
}