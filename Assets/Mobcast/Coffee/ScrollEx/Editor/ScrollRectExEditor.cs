using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Linq;
using UnityEditor.AnimatedValues;
using System.Collections;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;
using UnityEngine.Events;

namespace Mobcast.Coffee.UI
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ScrollRectEx), true)]
	public class ScrollRectExEditor : Editor
	{
		static readonly GUIContent s_ContentLayoutGroup = new GUIContent("Content Layout Group");
		static readonly GUIContent s_ContentDirection = new GUIContent("Direction");
		static readonly GUIContent[] s_ContentDirectionPopup = { new GUIContent("Horizontal"), new GUIContent("Vertical") };

		ModuleGroup _groupLayout;
		ModuleGroup _groupSnap;
		ModuleGroup _groupNavi;
		ModuleGroup _groupAutoRotation;
		ModuleGroup _groupIndicator;

		void OnEnable()
		{
			_groupLayout = new ModuleGroup(new GUIContent("Layout"), Repaint);
			_groupSnap = new ModuleGroup(new GUIContent("Snap / Smoothing"), Repaint);
			_groupNavi = new ModuleGroup(new GUIContent("Navigation"), Repaint);
			_groupIndicator = new ModuleGroup(new GUIContent("Indicator"), Repaint);
			_groupAutoRotation = new ModuleGroup(new GUIContent("AutoRotation"), Repaint);
		}

		void DrawLayoutDirection()
		{
			
		}

		void DrawLayoutPaddingAndSpace()
		{
		}

		public class ModuleGroup
		{
			AnimBool _anim;
			GUIContent _label;
			string _prefsKey;

			public ModuleGroup(GUIContent label, UnityAction callback)
			{
				_prefsKey = "ModuleGroup_" + label.text;
				_label = label;
				_anim = new AnimBool(EditorPrefs.GetBool(_prefsKey, true), callback);
			}

			public bool Begin()
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				if (GUILayout.Button(_label, EditorStyles.boldLabel))
				{
					_anim.target = !_anim.target;
					EditorPrefs.SetBool(_prefsKey, _anim.target);
				}
				return EditorGUILayout.BeginFadeGroup (_anim.faded);
			}

			public void End()
			{
				EditorGUILayout.EndFadeGroup ();
				EditorGUILayout.EndVertical();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawLayoutDirection();
			DrawLayoutPaddingAndSpace();



			//################################
			// Layout module.
			//################################
			if (_groupLayout.Begin())
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
				}

				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Loop"));

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

					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(spPadding, true);
					EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
					EditorGUI.indentLevel--;
					so.ApplyModifiedProperties();
				}
			}
			_groupLayout.End();

			SerializedProperty spModule;

			//################################
			// Snap module.
			//################################
			if (_groupSnap.Begin())
			{
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
			_groupSnap.End();

			//################################
			// Navi module.
			//################################
			if (_groupNavi.Begin())
			{
				spModule = serializedObject.FindProperty("m_NaviModule");
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_PreviousButton"));
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_NextButton"));
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_FirstButton"));
				EditorGUILayout.PropertyField(spModule.FindPropertyRelative("m_LastButton"));
			}
			_groupNavi.End();

			//################################
			// Auto rotation module.
			//################################
			if (_groupAutoRotation.Begin())
			{
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
			_groupAutoRotation.End();

			//################################
			// Indicator module.
			//################################
			if (_groupIndicator.Begin())
			{
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
			_groupIndicator.End();

			serializedObject.ApplyModifiedProperties();
		}
	}
}