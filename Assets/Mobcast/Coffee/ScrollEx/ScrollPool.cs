using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace Mobcast.Coffee
{
	public interface IScrollPool
	{
		ScrollCellView RentCellView(ScrollCellView template);
		void ReturnCellView(ScrollCellView obj);
	}

//	public class ScrollPool : IScrollPool
//	{
//		readonly Dictionary<int, Stack<ScrollCellView>> pool = new Dictionary<int, Stack<ScrollCellView>>();
//
//		public Transform parent { get; set;}
//
//		public ScrollCellView Rent(ScrollCellView template)
//		{
//			var id = template.GetInstanceID();
//			Stack<ScrollCellView> stack;
//			if (pool.TryGetValue(id, out stack) && 0 < stack.Count)
//				return stack.Pop();
//
//			var cellView = Object.Instantiate(template);
//			cellView.templateId = id;
//			cellView.transform.SetParent(parent, false);
//			cellView.transform.localPosition = Vector3.zero;
//			cellView.transform.localRotation = Quaternion.identity;
//			cellView.transform.localScale = Vector3.zero;
//			return cellView;
//		}
//
//
//		public void Return(ScrollCellView obj)
//		{
//			int id = obj.templateId;
//			Stack<ScrollCellView> stack;
//			if (pool.TryGetValue(id, out stack))
//			{
//				if (!stack.Contains(obj))
//					stack.Push(obj);
//				return;
//			}
//
//			stack = new Stack<ScrollCellView>(){obj};
//			pool.Add(id, stack);
//		}
//	}
}