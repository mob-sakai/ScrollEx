using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using Mobcast.Coffee.UI;
using System;
using Alignment = Mobcast.Coffee.UI.ScrollRectEx.Alignment;

namespace Mobcast.Coffee.UI.ScrollModule
{
	static class MenuOptions
	{
		#region Menus

		[MenuItem("GameObject/UI/Scroll Rect Ex (Horizontal)")]
		static void CreateHorizontalScrollRectEx(MenuCommand command)
		{
			// Verticalを生成
			CreateVerticalScrollRectEx(null);
			var ex = Selection.activeGameObject.GetComponent<ScrollRectEx>();
			ex.name = "Scroll View (Horizontal)";

			// horizontalに設定.
			ex.scrollRect.horizontal = true;
			ex.scrollRect.vertical = false;

			// 全体を回転.
			RectTransformUtility.FlipLayoutAxes(ex.transform as RectTransform, false, true);

			// コンテンツアンカー調整
			SetRt(ex.scrollRect.content, HAnchor.Left, VAnchor.Expand, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 0));

			// スクロールバーをHorizontalに変更
			var bar = ex.scrollRect.verticalScrollbar;
			ex.scrollRect.horizontalScrollbar = bar;
			ex.scrollRect.verticalScrollbar = null;
			bar.direction = Scrollbar.Direction.LeftToRight;
			RectTransformUtility.FlipLayoutOnAxis(bar.transform as RectTransform, 1, false, false);
			(bar.transform as RectTransform).sizeDelta = new Vector2(0, 20);

			// インジケータ
			RectTransformUtility.FlipLayoutOnAxis(ex.indicator.transform as RectTransform, 1, false, false);

			// ボタン調整
			Button b = ex.naviModule.nextButton;
			b.GetComponentInChildren<Text>().text = ">";
			RectTransformUtility.FlipLayoutOnAxis(b.transform as RectTransform, 0, false, false);
			b = ex.naviModule.previousButton;
			b.GetComponentInChildren<Text>().text = "<";
			RectTransformUtility.FlipLayoutOnAxis(b.transform as RectTransform, 0, false, false);

			// VerticalLayoutを全てHorizontalLayoutに
			foreach (var vl in ex.GetComponentsInChildren<VerticalLayoutGroup>(true))
			{
				ConvertTo<HorizontalLayoutGroup>(vl);
			}
		}

		[MenuItem("GameObject/UI/Scroll Rect Ex (Verticlal)")]
		static void CreateVerticalScrollRectEx(MenuCommand command)
		{
			RectTransform rt;

			// デフォルトスクロールビューを生成
			EditorApplication.ExecuteMenuItem("GameObject/UI/Scroll View");
			var ex = Selection.activeGameObject.AddComponent<ScrollRectEx>();
			ex.name = "Scroll View (Vertical)";

			// レイアウトグループ追加
			ex.scrollRect.content.gameObject.AddComponent<VerticalLayoutGroup>();
			ex.scrollRect.viewport.sizeDelta = new Vector2(0, 0);

			// スクロールバー調整
			ex.scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
			ex.scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

			rt = ex.scrollRect.verticalScrollbar.transform as RectTransform;
			rt.name = "Scrollbar";
			SetRt(rt, HAnchor.Right, VAnchor.Expand, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(20, 0));

			// horizontal 無効化
			ex.scrollRect.horizontal = false;
			UnityEngine.Object.DestroyImmediate(ex.scrollRect.horizontalScrollbar.gameObject);
			ex.scrollRect.horizontalScrollbar = null;


			// ナビゲーション
			var navigator = AddElement<RectTransform>("GameObject/Create Empty Child", ex.transform, "Navigator", out rt);
			SetRt(rt, HAnchor.Expand, VAnchor.Expand, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);

			// 進むボタン
			var nextButton = AddElement<Button>("GameObject/UI/Button", navigator, "Next Button", out rt);
			ex.naviModule.nextButton = nextButton;
			nextButton.GetComponentInChildren<Text>(true).text = "v";
			SetRt(rt, HAnchor.Expand, VAnchor.Bottom, TextAnchor.UpperCenter, new Vector2(0, -10), new Vector2(-100, 20));

			// 戻るボタン
			var prevButton = AddElement<Button>("GameObject/UI/Button", navigator, "Prev Button", out rt);
			ex.naviModule.previousButton = prevButton;
			prevButton.GetComponentInChildren<Text>(true).text = "^";
			SetRt(rt, HAnchor.Expand, VAnchor.Top, TextAnchor.LowerCenter, new Vector2(0, 10), new Vector2(-100, 20));

			// インジケータ生成
			CreateVerticalScrollIndicator(null);
			var indicator = Selection.activeGameObject.GetComponent<ScrollIndicator>();
			ex.indicator = indicator;

			rt = (indicator.transform as RectTransform);
			rt.SetParent(ex.transform);
			SetRt(rt, HAnchor.Right, VAnchor.Expand, TextAnchor.MiddleLeft, new Vector2(20, 0), new Vector2(20, 0));

			Selection.activeGameObject = ex.gameObject;
		}

		static void SetRt(RectTransform rt, HAnchor horizontal, VAnchor vertical, TextAnchor pivot, Vector2 pos, Vector2 sizeDelta)
		{
			float ah = (int)horizontal * 0.5f;
			float av = (int)vertical * 0.5f;
			rt.pivot = new Vector2(((int)pivot % 3) * 0.5f, (2 - (int)pivot / 3) * 0.5f);
			rt.anchorMin = new Vector2(ah < 0 ? 0 : ah, av < 0 ? 0 : av);
			rt.anchorMax = new Vector2(ah < 0 ? 1 : ah, av < 0 ? 1 : av);
			rt.anchoredPosition = pos;
			rt.sizeDelta = sizeDelta;
		}

		public enum HAnchor
		{
			Expand = -1,
			Left,
			Center,
			Right,
		}

		public enum VAnchor
		{
			Expand = -1,
			Bottom,
			Center,
			Top,
		}

		//		ScrollRectEx.Alignment.

		[MenuItem("GameObject/UI/Scroll Indicator (Horizontal)")]
		static void CreateHorizontalScrollIndicator(MenuCommand command)
		{
			// Verticalを生成
			CreateVerticalScrollIndicator(null);
			var indicator = Selection.activeGameObject.GetComponent<ScrollIndicator>();

			RectTransformUtility.FlipLayoutAxes(indicator.transform as RectTransform, false, true);

			ConvertTo<HorizontalLayoutGroup>(indicator.layoutGroup);
		}

		[MenuItem("GameObject/UI/Scroll Indicator (Vertical)")]
		static void CreateVerticalScrollIndicator(MenuCommand command)
		{
			RectTransform rt;

			// インジケータ生成
			var indicator = AddElement<ScrollIndicator>("GameObject/UI/Image", null, "Indicator", out rt);
			rt.sizeDelta = new Vector2(30, 300);

			// レイアウトグループ追加
			var layout = indicator.gameObject.AddComponent<VerticalLayoutGroup>();
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.childAlignment = TextAnchor.MiddleCenter;
#if UNITY_5_5_OR_NEWER
			layout.childControlHeight = true;
			layout.childControlWidth = true;
#endif

			// インジケータの背景調整
			var image = indicator.gameObject.GetComponent<Image>();
			image.color = new Color(0, 0, 0, 0.25f);
			image.type = Image.Type.Sliced;
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

			// テンプレート用のトグル生成
			var toggle = AddElement<Toggle>("GameObject/UI/Toggle", layout.transform, "Template Toggle", out rt);
			SetImage(toggle.targetGraphic as Image, "UI/Skin/Knob.psd", new Color(0.65f, 0.65f, 0.65f), new Vector2(16, 16), Image.Type.Simple);
			SetImage(toggle.graphic as Image, "UI/Skin/Knob.psd", Color.white, new Vector2(10, 10), Image.Type.Simple);
			SetRt(toggle.targetGraphic.transform as RectTransform, HAnchor.Center, VAnchor.Center, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(16, 16));
//			image = (toggle.targetGraphic as Image);
//			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
//			image.color = new Color(0.65f,0.65f,0.65f);
//			image.type = Image.Type.Simple;

//			image = (toggle.graphic as Image);
//			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
//			(image.transform as RectTransform).sizeDelta = new Vector2(10, 10);

			var element = toggle.gameObject.AddComponent<LayoutElement>();
			element.preferredWidth = 20;
			element.preferredHeight = 20;

			UnityEngine.Object.DestroyImmediate(toggle.GetComponentInChildren<Text>().gameObject);
			indicator.template = toggle;
			toggle.gameObject.SetActive(true);

			Selection.activeGameObject = indicator.gameObject;
		}



		static void DestroyImmidiate(params UnityEngine.Object[] objs)
		{
			foreach (var o in objs)
				UnityEngine.Object.DestroyImmediate(o);
		}

		static void SetImage(Image image, string resource, Color color, Vector2 size, Image.Type type)
		{
			image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(resource);
			image.color = color;
			(image.transform as RectTransform).sizeDelta = size;
			image.type = type;
		}

		static T AddElement<T>(string menuItem, Transform parent, string name, out RectTransform rt) where T : Component
		{
			EditorApplication.ExecuteMenuItem(menuItem);
			var go = Selection.activeGameObject;
			go.name = name;
			if (parent)
				go.transform.SetParent(parent, false);
			rt = go.transform as RectTransform;
			return go.GetComponentInChildren<T>() ?? go.AddComponent<T>();
		}

		#endregion Menus

		#region Context Menus

		[MenuItem("CONTEXT/RectTransform/Flip Axes")]
		static void FlipLayoutAxes(MenuCommand command)
		{
			RectTransformUtility.FlipLayoutAxes((command.context as RectTransform), false, false);
		}

		[MenuItem("CONTEXT/RectTransform/Flip Vertical")]
		static void FlipLayoutOnVertical(MenuCommand command)
		{
			RectTransformUtility.FlipLayoutOnAxis((command.context as RectTransform), 1, false, false);
		}

		[MenuItem("CONTEXT/RectTransform/Flip Horizontal")]
		static void FlipLayoutOnHorizontal(MenuCommand command)
		{
			RectTransformUtility.FlipLayoutOnAxis((command.context as RectTransform), 0, false, false);
		}



		[MenuItem("CONTEXT/LayoutGroup/Convert To HorizontalLayoutGroup", true)]
		static bool _ConvertToHorizontalLayoutGroup(MenuCommand command)
		{
			return CanConvertTo<UnityEngine.UI.HorizontalLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/LayoutGroup/Convert To HorizontalLayoutGroup", false)]
		static void ConvertToHorizontalLayoutGroup(MenuCommand command)
		{
			ConvertTo<UnityEngine.UI.HorizontalLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/LayoutGroup/Convert To VerticalLayoutGroup", true)]
		static bool _ConvertToVerticalLayoutGroup(MenuCommand command)
		{
			return CanConvertTo<UnityEngine.UI.VerticalLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/LayoutGroup/Convert To VerticalLayoutGroup", false)]
		static void ConvertToVerticalLayoutGroup(MenuCommand command)
		{
			ConvertTo<UnityEngine.UI.VerticalLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/LayoutGroup/Convert To GridLayoutGroup", true)]
		static bool _ConvertToGridLayoutGroup(MenuCommand command)
		{
			return CanConvertTo<UnityEngine.UI.GridLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/LayoutGroup/Convert To GridLayoutGroup", false)]
		static void ConvertToGridLayoutGroup(MenuCommand command)
		{
			ConvertTo<UnityEngine.UI.GridLayoutGroup>(command.context);
		}

		[MenuItem("CONTEXT/ScrollRect/Convert To ScrollRect", true)]
		static bool _ConvertToScrollRect(MenuCommand command)
		{
			return CanConvertTo<UnityEngine.UI.ScrollRect>(command.context);
		}

		[MenuItem("CONTEXT/ScrollRect/Convert To ScrollRect", false)]
		static void ConvertToScrollRect(MenuCommand command)
		{
			ConvertTo<UnityEngine.UI.ScrollRect>(command.context);
		}

		[MenuItem("CONTEXT/ScrollRect/Convert To NestableScrollRect", true)]
		static bool _ConvertToNestableScrollRect(MenuCommand command)
		{
			return CanConvertTo<NestableScrollRect>(command.context);
		}

		[MenuItem("CONTEXT/ScrollRect/Convert To NestableScrollRect", false)]
		static void ConvertToNestableScrollRect(MenuCommand command)
		{
			ConvertTo<NestableScrollRect>(command.context);
		}

		#endregion Context Menus

		/// <summary>
		/// Verify whether it can be converted to the specified component.
		/// </summary>
		static bool CanConvertTo<T>(UnityEngine.Object context) where T : MonoBehaviour
		{
			return context && context.GetType() != typeof(T);
		}

		/// <summary>
		/// Convert to the specified component.
		/// </summary>
		static void ConvertTo<T>(UnityEngine.Object context) where T : MonoBehaviour
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
}


