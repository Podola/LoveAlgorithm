#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LoveAlgo.UI;
using LoveAlgo.UI.Modules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static LoveAlgo.Editor.PrefabFactories.PrefabBuilderUtils;

namespace LoveAlgo.Editor.PrefabFactories
{
    public sealed class HudPrefabFactory : IPrefabFactory
    {
        private const string PrefabPath = "Assets/Prefabs/Simple/HUD/LoveAlgoHUDRoot.prefab";
        public static string DefaultOutputPath => PrefabPath;

        private readonly string _freeActionModulePath;
        private readonly string _shopModulePath;

        public HudPrefabFactory(string freeActionModulePath = null, string shopModulePath = null)
        {
            _freeActionModulePath = freeActionModulePath ?? FreeActionPanelFactory.DefaultOutputPath;
            _shopModulePath = shopModulePath ?? ShopPanelFactory.DefaultOutputPath;
        }

        public string DisplayName => "HUD Root";
        public string OutputPath => PrefabPath;
        public IEnumerable<FactoryDependency> Dependencies => new[]
        {
            new FactoryDependency(typeof(FreeActionPanelFactory), DependencyRequirement.Required),
            new FactoryDependency(typeof(ShopPanelFactory), DependencyRequirement.Required)
        };

        public GameObject BuildPrefab()
        {
            var freeActionModulePrefab = AssetDatabase.LoadAssetAtPath<LoveAlgoUIModule>(_freeActionModulePath);
            if (freeActionModulePrefab == null)
            {
                Debug.LogError($"HUD factory could not load FreeAction module prefab at {_freeActionModulePath}");
                return null;
            }

            var shopModulePrefab = AssetDatabase.LoadAssetAtPath<LoveAlgoUIModule>(_shopModulePath);
            if (shopModulePrefab == null)
            {
                Debug.LogError($"HUD factory could not load Shop module prefab at {_shopModulePath}");
                return null;
            }

            var canvasGo = CreateUIRoot("LoveAlgoHUDRoot");
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hudRoot = canvasGo.AddComponent<LoveAlgoHUDRoot>();

            var navHost = CreateUIObject("ModuleHost", canvasGo.transform);
            var navRect = navHost.GetComponent<RectTransform>();
            Stretch(navRect);
            navRect.offsetMin = new Vector2(0f, 0f);
            navRect.offsetMax = new Vector2(0f, -80f);
            var navigation = navHost.AddComponent<UINavigationController>();

            var topBar = CreateUIObject("TopBar", canvasGo.transform);
            var topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 80f);
            topRect.anchoredPosition = Vector2.zero;
            var topImage = topBar.AddComponent<Image>();
            topImage.color = new Color(0.09f, 0.12f, 0.2f, 0.85f);

            var buttonContainer = CreateUIObject("NavButtons", topBar.transform);
            var buttonsRect = buttonContainer.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0f, 0f);
            buttonsRect.anchorMax = new Vector2(1f, 1f);
            buttonsRect.offsetMin = new Vector2(200f, 10f);
            buttonsRect.offsetMax = new Vector2(-200f, -10f);
            var hLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.childForceExpandHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.spacing = 15f;
            hLayout.padding = new RectOffset(10, 10, 10, 10);

            var freeActionButton = CreateNavButton("FreeActionButton", buttonContainer.transform, "자유행동");
            var shopButton = CreateNavButton("ShopButton", buttonContainer.transform, "상점");
            var messengerButton = CreateNavButton("MessengerButton", buttonContainer.transform, "메신저");
            ConfigureNavLayoutElement(freeActionButton);
            ConfigureNavLayoutElement(shopButton);
            ConfigureNavLayoutElement(messengerButton);

            var hudSerialized = new SerializedObject(hudRoot);
            hudSerialized.FindProperty("navigationController").objectReferenceValue = navigation;
            hudSerialized.FindProperty("freeActionModule").objectReferenceValue = freeActionModulePrefab;
            hudSerialized.FindProperty("shopModule").objectReferenceValue = shopModulePrefab;
            hudSerialized.FindProperty("freeActionNavButton").objectReferenceValue = freeActionButton;
            hudSerialized.FindProperty("shopNavButton").objectReferenceValue = shopButton;
            hudSerialized.FindProperty("messengerNavButton").objectReferenceValue = messengerButton;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            var navSerialized = new SerializedObject(navigation);
            navSerialized.FindProperty("moduleHost").objectReferenceValue = navRect;
            navSerialized.ApplyModifiedPropertiesWithoutUndo();

            return canvasGo;
        }

        private static Button CreateNavButton(string name, Transform parent, string label)
        {
            return CreateStyledButton(name, parent, label, ButtonStyle.Primary);
        }

        private static void ConfigureNavLayoutElement(Button button)
        {
            var layout = button != null ? button.GetComponent<LayoutElement>() : null;
            if (layout == null)
            {
                return;
            }

            layout.flexibleWidth = 1f;
        }
    }
}
#endif
