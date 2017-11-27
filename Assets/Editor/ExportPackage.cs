using UnityEditor;

namespace Mobcast.Coffee.Api
{
	public static class ExportPackage
	{
		const string kPackageName = "ScrollEx.unitypackage";
		static readonly string[] kAssetPathes = {
			"Assets/Mobcast/Coffee/ScrollEx",
		};

		[MenuItem ("Export Package/" + kPackageName)]
		[InitializeOnLoadMethod]
		static void Export ()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			
			AssetDatabase.ExportPackage (kAssetPathes, kPackageName, ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			UnityEngine.Debug.Log ("Export successfully : " + kPackageName);
		}
	}
}