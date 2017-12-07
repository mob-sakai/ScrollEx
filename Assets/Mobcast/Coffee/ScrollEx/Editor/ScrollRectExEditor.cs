using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;

namespace Mobcast.Coffee.UI
{
	[CustomEditor(typeof(ScrollRectEx), true)]
	public class ScrollRectExEditor : Editor
	{
//		void _FixLayout(ScrollRectEx current)
//		{
//			bool vertical = current.scrollRect.vertical;
//			var type = current.scrollRect.vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
//			LayoutGroup layout = current.layoutGroup ?? current.content.GetComponent<LayoutGroup>();
//			if (!layout)
//				current.content.gameObject.AddComponent(type);
//			else
//				ComponentConverter.ConvertTo(layout, type);
//
//			current.scrollRect.vertical = vertical;
//			current.scrollRect.horizontal = !vertical;
//		}
//
//
//
//		bool _oldVertical;
//		bool _oldHorizontal;


//		void OnEnable()
//		{
//			ScrollRectEx current = target as ScrollRectEx;
//			_oldHorizontal = current.scrollRect.horizontal;
//		}

		public override void OnInspectorGUI()
		{
			ScrollRectEx current = target as ScrollRectEx;

//			if (current.scrollRectXXX.vertical != (current.layoutGroupXXX is VerticalLayoutGroup))
//			{
//				EditorApplication.delayCall += () =>
//				{
//					var type = current.scrollRectXXX.vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
//					var l = current.contentXXX.GetComponent<LayoutGroup>();
//					if (l)
//						ComponentConverter.ConvertTo(l, type);
//					else
//						current.contentXXX.gameObject.AddComponent(type);
//				};
//			}

//			var layout = current.layoutGroupXXX;

			base.OnInspectorGUI();

			serializedObject.Update();

//			ScrollRectEx current = target as ScrollRectEx;

//			if(!current._layoutGroup || )

			if (current.layoutGroup)
			{
				var so = new SerializedObject(current.layoutGroup);
				so.Update();
				EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(so.FindProperty("m_Padding"), true);
				EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
				so.ApplyModifiedProperties();
			}


			EditorGUILayout.LabelField("Snap / Smoothing", EditorStyles.boldLabel);
			var spSnapModule = serializedObject.FindProperty("m_SnapModule");
			var spSnapOnEndDrag = spSnapModule.FindPropertyRelative("m_SnapOnEndDrag");
			EditorGUILayout.PropertyField(spSnapOnEndDrag);
			using (new EditorGUI.DisabledGroupScope(!spSnapOnEndDrag.boolValue))
			{
				EditorGUILayout.PropertyField(spSnapModule.FindPropertyRelative("m_VelocityThreshold"));
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TweenMethod"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TweenDuration"));



			EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
			var spNaviModule = serializedObject.FindProperty("m_NaviModule");
			var spJumpOnSwipe = spNaviModule.FindPropertyRelative("m_JumpOnSwipe");
			EditorGUILayout.PropertyField(spJumpOnSwipe);
			using (new EditorGUI.DisabledGroupScope(!spJumpOnSwipe.boolValue))
			{
				EditorGUILayout.PropertyField(spNaviModule.FindPropertyRelative("m_SwipeThreshold"));
			}
			EditorGUILayout.PropertyField(spNaviModule.FindPropertyRelative("m_PreviousButton"));
			EditorGUILayout.PropertyField(spNaviModule.FindPropertyRelative("m_NextButton"));


			EditorGUILayout.LabelField("AutoRotation", EditorStyles.boldLabel);
			var spAutoRotationModule = serializedObject.FindProperty("m_AutoRotationModule");
			var spAutoJumpToNext = spAutoRotationModule.FindPropertyRelative("m_AutoJumpToNext");
			EditorGUILayout.PropertyField(spAutoJumpToNext);
			using (new EditorGUI.DisabledGroupScope(!spAutoJumpToNext.boolValue))
			{
				EditorGUILayout.PropertyField(spAutoRotationModule.FindPropertyRelative("m_Delay"));
				EditorGUILayout.PropertyField(spAutoRotationModule.FindPropertyRelative("m_Interval"));
			}

//			if(current.scrollRect.horizontal && !_oldHorizontal)
//				current.scrollRect.vertical = false;
//			else if(!current.scrollRect.horizontal && _oldHorizontal)
//				current.scrollRect.vertical = true;
//			
//			current.scrollRect.horizontal = !current.scrollRect.vertical;
////
//			_oldHorizontal = current.scrollRect.horizontal;
//			EditorApplication.delayCall +=() =>_FixLayout(current);

			/*
			EditorGUI.BeginChangeCheck();
			var spDirection = serializedObject.FindProperty("m_Direction");
			EditorGUILayout.PropertyField(spDirection);
			if (EditorGUI.EndChangeCheck())
			{
				bool vertical = (Axis)spDirection.intValue == Axis.Vertical;
				var type = vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
				current.scrollRect.vertical = vertical;
				current.scrollRect.horizontal = !vertical;

				// インジケータのレイアウトグループを修正.
				if (current.m_ScrollIndicator)
				{
					if (!current.m_ScrollIndicator.layoutGroup)
					{
						current.m_ScrollIndicator.gameObject.AddComponent(type);
					}
					ComponentConverter.ConvertTo(current.m_ScrollIndicator.layoutGroup, type);




//					var lg = new GameObject("Pager", typeof(RectTransform), type).GetComponent(type) as HorizontalOrVerticalLayoutGroup;
//					lg.transform.SetParent(current.transform);
//					current.m_ScrollIndicator.layoutGroup = lg;
//					lg.childAlignment = TextAnchor.MiddleCenter;
//					lg.childForceExpandHeight = false;
//					lg.childForceExpandWidth = false;
//
//
//					EditorApplication.ExecuteMenuItem("GameObject/UI/Toggle");
//					var toggle = Selection.activeGameObject.GetComponent<Toggle>();
//					var le = toggle.gameObject.AddComponent<LayoutElement>();
//					le.preferredHeight = 20;
//					le.preferredWidth = 20;
//					toggle.transform.SetParent(lg.transform);
//					current.m_ScrollIndicator.m_PagerToggle = toggle;
//					EditorApplication.delayCall += () =>
//					{
//						var t = toggle.GetComponentInChildren<Text>();
//						if (t)
//						{
//							Object.DestroyImmediate(t.gameObject);
//						}
//					};
//					
				}
//
//
//				ComponentConverter.ConvertTo(current.layoutGroup, type);
//				ComponentConverter.ConvertTo(current.pagerLayoutGroup, type);
			}
			*/
			serializedObject.ApplyModifiedProperties();



//			if (current.scrollRectXXX.vertical != (current.layoutGroupXXX is VerticalLayoutGroup))
//			{
//				EditorGUILayout.HelpBox("layout", MessageType.Warning);
//				if (GUILayout.Button("Fix"))
//				{
//					var l = current.contentXXX.GetComponent<LayoutGroup>();
//					if (l)
//					{
//						if (current.scrollRectXXX.vertical)
//							ComponentConverter.ConvertTo<VerticalLayoutGroup>(l);
//						else
//							ComponentConverter.ConvertTo<HorizontalLayoutGroup>(l);
//					}
//					else
//					{
//						if (current.scrollRectXXX.vertical)
//							current.contentXXX.gameObject.AddComponent<VerticalLayoutGroup>();
//						else
//							current.contentXXX.gameObject.AddComponent<HorizontalLayoutGroup>();
//					}
//				}
//			}

		}
	}
}