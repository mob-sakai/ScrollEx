using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace Mobcast.Coffee
{
	[CustomEditor (typeof(ScrollRectEx), true)]
	public class ScrollRectExEditor : Editor
	{
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.Update ();

			ScrollRectEx current = target as ScrollRectEx;

//			if(!current._layoutGroup || )

			var so = new SerializedObject(current.layoutGroupXXX);
			so.Update();
			EditorGUILayout.PropertyField(so.FindProperty("m_Padding"), true);
			EditorGUILayout.PropertyField(so.FindProperty("m_Spacing"));
			so.ApplyModifiedProperties();


			serializedObject.ApplyModifiedProperties ();
		}
	}
}