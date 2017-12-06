using UnityEngine;
using System;
using System.Collections;
using EnhancedUI;

namespace Mobcast.Coffee
{
	public abstract class ScrollCellView : MonoBehaviour
	{
		public int templateId { get; set;}

		public int cellIndex { get; set;}

		public int dataIndex { get; set;}

		public bool active { get; set;}

		public abstract void RefreshCellView();

		public abstract void OnChangedVisibility(bool visible);

		public abstract void OnBeforePool();

	}
}
