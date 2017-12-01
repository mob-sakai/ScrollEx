using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;
using UnityEngine.UI;

namespace Mobcast.Coffee
{
	[CustomEditor(typeof(ScrollRectEx), true)]
	public class ScrollRectExEditor : Editor
	{
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
				EditorGUILayout.PropertyField(so.FindProperty("m_Padding"), true);
				EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
				so.ApplyModifiedProperties();
			}


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