#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Component converter for editor.
/// </summary>
public static class ComponentConverter
{
	//%%%% v Context menu for editor v %%%%
	[MenuItem("CONTEXT/Graphic/Convert To Image", true)]
	static bool _ConvertToButtonEx(MenuCommand command)
	{
		return CanConvertTo<UnityEngine.UI.Image>(command.context);
	}
	
	[MenuItem("CONTEXT/Button/Convert To Image", false)]
	static void ConvertToButtonEx(MenuCommand command)
	{
		ConvertTo<UnityEngine.UI.Image>(command.context);
	}
	
	[MenuItem("CONTEXT/Button/Convert To RawImage", true)]
	static bool _ConvertToButton(MenuCommand command)
	{
		return CanConvertTo<UnityEngine.UI.RawImage>(command.context);
	}
	
	[MenuItem("CONTEXT/Button/Convert To RawImage", false)]
	static void ConvertToButton(MenuCommand command)
	{
		ConvertTo<UnityEngine.UI.RawImage>(command.context);
	}
	//%%%% ^ Context menu for editor ^ %%%%

	/// <summary>
	/// Verify whether it can be converted to the specified component.
	/// </summary>
	public static bool CanConvertTo<T>(Object context) where T : MonoBehaviour
	{
		return context && context.GetType() != typeof(T);
	}

	/// <summary>
	/// Convert to the specified component.
	/// </summary>
	public static void ConvertTo<T>(Object context) where T : MonoBehaviour
	{
		var target = context as MonoBehaviour;
		var so = new SerializedObject(target);
		so.Update();

		bool oldEnable = target.enabled;
		target.enabled = false;

		// Find MonoScript of the specified component.
		foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
		{
			if (script.GetClass() != typeof(T))
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
