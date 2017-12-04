#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Component converter for editor.
/// </summary>
public static class ComponentConverter
{
	/// <summary>
	/// Verify whether it can be converted to the specified component.
	/// </summary>
	public static bool CanConvertTo<T>(Object context) where T : MonoBehaviour
	{
		return CanConvertTo(context, typeof(T));
	}

	/// <summary>
	/// Verify whether it can be converted to the specified component.
	/// </summary>
	public static bool CanConvertTo(Object context, System.Type type)
	{
		return context && context.GetType() != type;
	}

	/// <summary>
	/// Convert to the specified component.
	/// </summary>
	public static void ConvertTo<T>(Object context) where T : MonoBehaviour
	{
		ConvertTo(context, typeof(T));
	}

	/// <summary>
	/// Convert to the specified component.
	/// </summary>
	public static void ConvertTo(Object context, System.Type type)
	{
		if (!CanConvertTo(context, type))
			return;

		var target = context as MonoBehaviour;
		var so = new SerializedObject(target);
		so.Update();

		bool oldEnable = target.enabled;
		target.enabled = false;

		// Find MonoScript of the specified component.
		foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
		{
			if (script.GetClass() != type)
				continue;

			// Set 'm_Script' to convert.
			so.FindProperty("m_Script").objectReferenceValue = script;
			so.ApplyModifiedProperties();
			break;
		}

		(so.targetObject as MonoBehaviour).enabled = oldEnable;
	}
}
#endif
