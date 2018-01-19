using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI
{
	public class NestableScrollRect : ScrollRect
	{

		private bool routeToParent = false;

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			DoParentEventSystemHandler<IInitializePotentialDragHandler>((parent) =>
			{
				parent.OnInitializePotentialDrag(eventData);
			});
			DoEventSystemHandler<IInitializePotentialDragHandler>(transform, (parent) =>
				{
					if(parent == this)
						base.OnInitializePotentialDrag(eventData);
					else
						parent.OnInitializePotentialDrag(eventData);
				});
		}

		public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (routeToParent)
				DoParentEventSystemHandler<IDragHandler>((parent) =>
				{
					parent.OnDrag(eventData);
				});
			else
			{
				DoEventSystemHandler<IDragHandler>(transform, (parent) =>
				{
					if(parent == this)
						base.OnDrag(eventData);
					else
						parent.OnDrag(eventData);
				});
			}
		}

		public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
				routeToParent = true;
			else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
				routeToParent = true;
			else
				routeToParent = false;

			if (routeToParent)
				DoParentEventSystemHandler<IBeginDragHandler>((parent) =>
				{
					parent.OnBeginDrag(eventData);
				});
			else
			{
				DoEventSystemHandler<IBeginDragHandler>(transform, (parent) =>
					{
						if(parent == this)
							base.OnBeginDrag(eventData);
						else
							parent.OnBeginDrag(eventData);
					});
			}
		}

		public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (routeToParent)
				DoParentEventSystemHandler<IEndDragHandler>((parent) =>
				{
					parent.OnEndDrag(eventData);
				});
			else
			{
				DoEventSystemHandler<IEndDragHandler>(transform, (parent) =>
					{
						if(parent == this)
							base.OnEndDrag(eventData);
						else
							parent.OnEndDrag(eventData);
					});
			}
			routeToParent = false;
		}

		/// <summary>
		/// Do the parent event system handler.
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="action">Action.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void DoParentEventSystemHandler<T>(Action<T> action) where T:IEventSystemHandler
		{
			Transform parent = transform.parent;
			while (parent != null)
			{
				DoEventSystemHandler<T>(parent, action);
				parent = parent.parent;
			}
		}

		/// <summary>
		/// Do the event system handler.
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="action">Action.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		void DoEventSystemHandler<T>(Transform self, Action<T> action) where T:IEventSystemHandler
		{
			List<Component> components = new List<Component>();
			self.GetComponents<Component>(components);
			foreach (Component c in components)
			{
				if(c is T)
					action((T)(IEventSystemHandler)c);
			}
		}
	}
}