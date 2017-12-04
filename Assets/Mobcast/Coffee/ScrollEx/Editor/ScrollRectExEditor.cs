//using UnityEngine;
//using UnityEditor;
//using UnityEditor.UI;
//using System.Collections;
//using UnityEngine.UI;
//using Axis = UnityEngine.RectTransform.Axis;
//
//namespace Mobcast.Coffee
//{
//	[CustomEditor(typeof(ScrollRectEx), true)]
//	public class ScrollRectExEditor : Editor
//	{
//		public override void OnInspectorGUI()
//		{
//			ScrollRectEx current = target as ScrollRectEx;
//
////			if (current.scrollRectXXX.vertical != (current.layoutGroupXXX is VerticalLayoutGroup))
////			{
////				EditorApplication.delayCall += () =>
////				{
////					var type = current.scrollRectXXX.vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
////					var l = current.contentXXX.GetComponent<LayoutGroup>();
////					if (l)
////						ComponentConverter.ConvertTo(l, type);
////					else
////						current.contentXXX.gameObject.AddComponent(type);
////				};
////			}
//
////			var layout = current.layoutGroupXXX;
//
//			base.OnInspectorGUI();
//
//			serializedObject.Update();
//
////			ScrollRectEx current = target as ScrollRectEx;
//
////			if(!current._layoutGroup || )
//
//			if (current.layoutGroup)
//			{
//				var so = new SerializedObject(current.layoutGroup);
//				so.Update();
//				EditorGUILayout.PropertyField(so.FindProperty("m_Padding"), true);
//				EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
//				so.ApplyModifiedProperties();
//			}
//
//
//			/*
//			EditorGUI.BeginChangeCheck();
//			var spDirection = serializedObject.FindProperty("m_Direction");
//			EditorGUILayout.PropertyField(spDirection);
//			if (EditorGUI.EndChangeCheck())
//			{
//				bool vertical = (Axis)spDirection.intValue == Axis.Vertical;
//				var type = vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
//				current.scrollRect.vertical = vertical;
//				current.scrollRect.horizontal = !vertical;
//
//				// インジケータのレイアウトグループを修正.
//				if (current.m_ScrollIndicator)
//				{
//					if (!current.m_ScrollIndicator.layoutGroup)
//					{
//						current.m_ScrollIndicator.gameObject.AddComponent(type);
//					}
//					ComponentConverter.ConvertTo(current.m_ScrollIndicator.layoutGroup, type);
//
//
//
//
////					var lg = new GameObject("Pager", typeof(RectTransform), type).GetComponent(type) as HorizontalOrVerticalLayoutGroup;
////					lg.transform.SetParent(current.transform);
////					current.m_ScrollIndicator.layoutGroup = lg;
////					lg.childAlignment = TextAnchor.MiddleCenter;
////					lg.childForceExpandHeight = false;
////					lg.childForceExpandWidth = false;
////
////
////					EditorApplication.ExecuteMenuItem("GameObject/UI/Toggle");
////					var toggle = Selection.activeGameObject.GetComponent<Toggle>();
////					var le = toggle.gameObject.AddComponent<LayoutElement>();
////					le.preferredHeight = 20;
////					le.preferredWidth = 20;
////					toggle.transform.SetParent(lg.transform);
////					current.m_ScrollIndicator.m_PagerToggle = toggle;
////					EditorApplication.delayCall += () =>
////					{
////						var t = toggle.GetComponentInChildren<Text>();
////						if (t)
////						{
////							Object.DestroyImmediate(t.gameObject);
////						}
////					};
////					
//				}
////
////
////				ComponentConverter.ConvertTo(current.layoutGroup, type);
////				ComponentConverter.ConvertTo(current.pagerLayoutGroup, type);
//			}
//			*/
//			serializedObject.ApplyModifiedProperties();
//
//
//
////			if (current.scrollRectXXX.vertical != (current.layoutGroupXXX is VerticalLayoutGroup))
////			{
////				EditorGUILayout.HelpBox("layout", MessageType.Warning);
////				if (GUILayout.Button("Fix"))
////				{
////					var l = current.contentXXX.GetComponent<LayoutGroup>();
////					if (l)
////					{
////						if (current.scrollRectXXX.vertical)
////							ComponentConverter.ConvertTo<VerticalLayoutGroup>(l);
////						else
////							ComponentConverter.ConvertTo<HorizontalLayoutGroup>(l);
////					}
////					else
////					{
////						if (current.scrollRectXXX.vertical)
////							current.contentXXX.gameObject.AddComponent<VerticalLayoutGroup>();
////						else
////							current.contentXXX.gameObject.AddComponent<HorizontalLayoutGroup>();
////					}
////				}
////			}
//
//		}
//	}
//}