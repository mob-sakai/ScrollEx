//using UnityEngine;
//using UnityEngine.UI;
//using UnityEditor;
//using UnityEditor.UI;
//using System.Collections;
//
//
//namespace Mobcast.Coffee
//{
//
//	[CustomEditor(typeof(ScrollIndicator))]
//	public class ScrollPagerEditor : Editor
//	{
//
//		static Types[] s_Types = new Types[]{ typeof(VerticalLayoutGroup), typeof(HorizontalLayoutGroup) };
//		static GUIContent[] s_Directions = new GUIContent[]{ new GUIContent("Vertical"), new GUIContent("Horizontal") };
//
////		public enum Direction
////		{
////			Vertical,
////			Vertical,
////		}
//
//		public override void OnInspectorGUI()
//		{
//			base.OnInspectorGUI();
//
//			var current = target as ScrollIndicator;
//
//			var lg = current.gameObject.GetComponent<LayoutGroup>() ?? current.gameObject.AddComponent<HorizontalLayoutGroup>();
//			var index = ArrayUtility.IndexOf(s_Types, lg.GetType());
//
//			index = EditorGUILayout.Popup(Mathf.Max(0, index), s_Directions);
//
//			if (lg.GetType() != s_Types[index])
//			{
//				Editor/s_Types:][
//				ComponentConverter.ConvertTo
//			}
//		}
//
//	}
//
//}