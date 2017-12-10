using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;

namespace Mobcast.Coffee.UI
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ScrollRectEx), true)]
	public class ScrollRectExEditor : Editor
	{
		static readonly GUIContent s_ContentSnap = new GUIContent("Snap / Smoothing");
		static readonly GUIContent s_ContentNavi = new GUIContent("Navigation");
		static readonly GUIContent s_ContentIndicator = new GUIContent("Indicator");
		static readonly GUIContent s_ContentAutoRotation = new GUIContent("AutoRotation");
		static readonly GUIContent s_ContentLayoutGroup = new GUIContent("Content Layout Group");
		static readonly GUIContent s_ContentDirection = new GUIContent("Direction");
		static readonly GUIContent[] s_ContentDirectionPopup = { new GUIContent("Horizontal"), new GUIContent("Vertical") };

		void DrawLayoutDirection()
		{
			var scrolls = targets
				.OfType<ScrollRectEx>()
				.Select(x => x.scrollRect)
				.Where(x => !object.ReferenceEquals(x, null))
				.ToArray();

			if (0 < scrolls.Length)
			{
				var so = new SerializedObject(scrolls);
				var spVertical = so.FindProperty("m_Vertical");
				var spHorizontal = so.FindProperty("m_Horizontal");
				var id = spVertical.boolValue ? 1 : 0;
				EditorGUI.showMixedValue = spVertical.hasMultipleDifferentValues;

				EditorGUI.BeginChangeCheck();
				id = EditorGUILayout.Popup(s_ContentDirection, id, s_ContentDirectionPopup);
				if (EditorGUI.EndChangeCheck())
				{
					spVertical.boolValue = id == 1;
					spHorizontal.boolValue = id == 0;
					so.ApplyModifiedProperties();
				}
//				EditorGUI.showMixedValue = false;
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Loop"));
		}

		void DrawLayoutPaddingAndSpace()
		{
			var layouts = targets
				.OfType<ScrollRectEx>()
				.Select(x => x.layoutGroup)
				.Where(x => !object.ReferenceEquals(x, null))
				.ToArray();
			
			if (0 < layouts.Length)
			{
				EditorGUI.showMixedValue = 1 < layouts.Length;
				EditorGUILayout.ObjectField(s_ContentLayoutGroup, (target as ScrollRectEx).layoutGroup, typeof(HorizontalOrVerticalLayoutGroup), true);

				var so = new SerializedObject(layouts);
				var spPadding = so.FindProperty("m_Padding");
//				spPadding.isExpanded = true;

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(spPadding, true);
				EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
				EditorGUI.indentLevel--;
				so.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

//			GUILayout.Space(10);
//			EditorGUILayout.LabelField(s_ContentLayout, EditorStyles.boldLabel);
			DrawLayoutDirection();
			DrawLayoutPaddingAndSpace();

			SerializedProperty spModule;

			GUILayout.Space(10);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(s_ContentSnap, EditorStyles.boldLabel);
				spModule = serializedObject.FindProperty("m_SnapModule");
				var spSnapOnEndDrag = spModule.FindPropertyRelative("m_SnapOnEndDrag");

				EditorGUILayout.PropertyField(spSnapOnEndDrag);
				using (new EditorGUI.DisabledGroupScope(!spSnapOnEndDrag.boolValue))
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_VelocityThreshold"));
					EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_SwipeThreshold"));
					EditorGUI.indentLevel--;
				}
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Alignment"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TweenMethod"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TweenDuration"));
			}

//			GUILayout.Space(10);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(s_ContentNavi, EditorStyles.boldLabel);
				spModule = serializedObject.FindProperty("m_NaviModule");
//				var spJumpOnSwipe = spModule.FindPropertyRelative("m_JumpOnSwipe");
//				EditorGUILayout.PropertyField(spJumpOnSwipe);
//				using (new EditorGUI.DisabledGroupScope(!spJumpOnSwipe.boolValue))
//				{
//					EditorGUI.indentLevel++;
//					EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_SwipeThreshold"));
//					EditorGUI.indentLevel--;
//				}
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_PreviousButton"));
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_NextButton"));
			}

			//GUILayout.Space(10);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(s_ContentAutoRotation, EditorStyles.boldLabel);
				spModule = serializedObject.FindProperty("m_AutoRotationModule");
				var spAutoJumpToNext = spModule.FindPropertyRelative("m_AutoJumpToNext");
				EditorGUILayout.PropertyField(spAutoJumpToNext);
				using (new EditorGUI.DisabledGroupScope(!spAutoJumpToNext.boolValue))
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_Delay"));
					EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_Interval"));
					EditorGUI.indentLevel--;
				}
			}

//			GUILayout.Space(10);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(s_ContentIndicator, EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Indicator"));

				var indicators = targets
					.OfType<ScrollRectEx>()
					.Select(x => x.indicator)
					.Where(x => x != null)
					.ToArray();

				if (0 < indicators.Length)
				{
					var so = new SerializedObject(indicators);
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(so.FindProperty("m_Template"), true);
					EditorGUILayout.PropertyField(so.FindProperty("m_Limit"));
					EditorGUI.indentLevel--;
					so.ApplyModifiedProperties();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}