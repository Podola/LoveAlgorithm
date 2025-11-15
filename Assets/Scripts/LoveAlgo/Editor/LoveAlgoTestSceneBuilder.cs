#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using LoveAlgo.Core;
using LoveAlgo.Data;
using LoveAlgo.UI;
using LoveAlgo.UI.Dialogue;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LoveAlgo.Editor
{
    public static class LoveAlgoTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LoveAlgo/TestGameLoopScene.unity";
        private const string DialogueScenePath = "Assets/Scenes/LoveAlgo/TestDialogueScene.unity";
        private const string ConfigPath = "Assets/Data/LoveAlgo/Config/LoveAlgoConfiguration.asset";
        private const string DialogueManagerPrefabPath = "Assets/Plugins/Pixel Crushers/Dialogue System/Prefabs/Dialogue Manager.prefab";
        private const string BackgroundCatalogAssetPath = "Assets/Data/LoveAlgo/Backgrounds/TestBackgroundCatalog.asset";
        private const string StandingCatalogAssetPath = "Assets/Data/LoveAlgo/Standing/TestStandingPoseCatalog.asset";
        private const string DialogueDatabasePath = "Assets/Data/LoveAlgo/Dialogue/TestDialogueDatabase.asset";
        private const string TestConversationTitle = "LoveAlgo.TestConversation";
        private const int TestConversationId = 1000;

        [MenuItem("LoveAlgo/Create Test Loop Scene", priority = 10)]
        public static void CreateTestScene()
        {
            if (!TryLoadConfiguration(out var configuration))
            {
                EditorUtility.DisplayDialog("LoveAlgo", "LoveAlgoConfiguration.asset 을 찾을 수 없습니다.\n경로: " + ConfigPath, "확인");
                return;
            }

            EnsureDirectoryExists(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TestGameLoopScene";

            var bootstrapper = new GameObject("LoveAlgoBootstrapper").AddComponent<LoveAlgoBootstrapper>();
            SetSerializedReference(bootstrapper, "configuration", configuration);

            var driver = new GameObject("GameLoopDriver").AddComponent<GameLoopSample>();

            var storyCanvas = CreateCanvas("StoryCanvas", new Color(0.12f, 0.16f, 0.25f), "Story Mode");
            var storyButton = CreateButton(storyCanvas.transform, "Enter Free Action", new Vector2(0f, -60f));
            UnityEventTools.AddPersistentListener(storyButton.onClick, driver.EnterFreeAction);

            var freeCanvas = CreateCanvas("FreeActionCanvas", new Color(0.16f, 0.23f, 0.18f), "Free Action");
            var freeButton = CreateButton(freeCanvas.transform, "Complete Free Action", new Vector2(0f, -60f));
            UnityEventTools.AddPersistentListener(freeButton.onClick, driver.CompleteFreeAction);

            var eventCanvas = CreateCanvas("EventCanvas", new Color(0.25f, 0.18f, 0.16f), "Event Mode");
            var eventButton = CreateButton(eventCanvas.transform, "Complete Event", new Vector2(0f, -60f));
            UnityEventTools.AddPersistentListener(eventButton.onClick, driver.CompleteEvent);

            var portal = new GameObject("UiPortal").AddComponent<UiPortal>();
            SetPortalReferences(portal, storyCanvas, freeCanvas, eventCanvas);

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            Debug.Log("LoveAlgo TestGameLoopScene 생성 완료", AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        }

        [MenuItem("LoveAlgo/Create Dialogue Test Scene", priority = 11)]
        public static void CreateDialogueTestScene()
        {
            if (!TryLoadConfiguration(out var configuration))
            {
                EditorUtility.DisplayDialog("LoveAlgo", "LoveAlgoConfiguration.asset 을 찾을 수 없습니다.\n경로: " + ConfigPath, "확인");
                return;
            }

            var dialogueDatabase = EnsureDialogueDatabaseAsset();
            var backgroundCatalog = EnsureBackgroundCatalogAsset();
            var standingCatalog = EnsureStandingCatalogAsset();

            if (dialogueDatabase == null)
            {
                EditorUtility.DisplayDialog("LoveAlgo", "Dialogue Database 생성에 실패했습니다.", "확인");
                return;
            }

            EnsureDirectoryExists(Path.GetDirectoryName(DialogueScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LoveAlgoDialogueTestScene";

            var bootstrapper = new GameObject("LoveAlgoBootstrapper").AddComponent<LoveAlgoBootstrapper>();
            SetSerializedReference(bootstrapper, "configuration", configuration);

            var dialogueManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DialogueManagerPrefabPath);
            if (dialogueManagerPrefab == null)
            {
                EditorUtility.DisplayDialog("LoveAlgo", "Dialogue Manager prefab을 찾을 수 없습니다.\n경로: " + DialogueManagerPrefabPath, "확인");
                return;
            }

            var dialogueManager = PrefabUtility.InstantiatePrefab(dialogueManagerPrefab) as GameObject;
            dialogueManager.name = "DialogueManager";
            var controller = dialogueManager.GetComponent<DialogueSystemController>();
            SetSerializedReference(controller, "initialDatabase", dialogueDatabase);
            EnsureDialogueSystemEvents(dialogueManager);

            var driver = new GameObject("LoveAlgoDialogueTestDriver").AddComponent<LoveAlgoDialogueTestDriver>();
            SetSerializedReference(driver, "dialogueController", controller);
            var driverSO = new SerializedObject(driver);
            driverSO.FindProperty("conversationTitle").stringValue = TestConversationTitle;
            driverSO.ApplyModifiedPropertiesWithoutUndo();

            var uiComponents = CreateDialogueUiHierarchy(backgroundCatalog, standingCatalog);
            driverSO.FindProperty("dialogueView").objectReferenceValue = uiComponents.view;
            driverSO.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(uiComponents.skipButton.onClick, uiComponents.view.CompleteTypewriter);
            UnityEventTools.AddPersistentListener(uiComponents.restartButton.onClick, driver.RestartConversation);

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, DialogueScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(DialogueScenePath);

            Debug.Log("LoveAlgo TestDialogueScene 생성 완료", AssetDatabase.LoadAssetAtPath<SceneAsset>(DialogueScenePath));
        }

        private static DialogueUiComponents CreateDialogueUiHierarchy(BackgroundCatalog backgroundCatalog, StandingPoseCatalog standingCatalog)
        {
            var canvasGO = new GameObject("LoveAlgoDialogueUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            var backgroundPresenter = CreateBackgroundLayer(canvasGO.transform, backgroundCatalog);
            var standingPresenter = CreateStandingLayer(canvasGO.transform, standingCatalog);
            var panelComponents = CreateDialoguePanel(canvasGO.transform);

            var view = canvasGO.AddComponent<LoveAlgoDialogueView>();
            var viewSO = new SerializedObject(view);
            viewSO.FindProperty("speakerLabel").objectReferenceValue = panelComponents.speakerLabel;
            viewSO.FindProperty("bodyLabel").objectReferenceValue = panelComponents.bodyLabel;
            viewSO.FindProperty("backgroundPresenter").objectReferenceValue = backgroundPresenter;
            viewSO.FindProperty("standingPresenter").objectReferenceValue = standingPresenter;
            viewSO.FindProperty("choicePanel").objectReferenceValue = panelComponents.choicePanel;
            viewSO.ApplyModifiedPropertiesWithoutUndo();

            return new DialogueUiComponents
            {
                view = view,
                skipButton = panelComponents.skipButton,
                restartButton = panelComponents.restartButton
            };
        }

        private static LoveAlgoBackgroundPresenter CreateBackgroundLayer(Transform parent, BackgroundCatalog catalog)
        {
            var backgroundGO = new GameObject("BackgroundLayer", typeof(RectTransform));
            backgroundGO.transform.SetParent(parent, false);
            var rect = backgroundGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var primary = CreateFullscreenImage("PrimaryBackground", backgroundGO.transform);
            var secondary = CreateFullscreenImage("SecondaryBackground", backgroundGO.transform);
            secondary.color = new Color(1f, 1f, 1f, 0f);
            secondary.enabled = false;

            var presenter = backgroundGO.AddComponent<LoveAlgoBackgroundPresenter>();
            var so = new SerializedObject(presenter);
            so.FindProperty("primary").objectReferenceValue = primary;
            so.FindProperty("secondary").objectReferenceValue = secondary;
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();
            return presenter;
        }

        private static Image CreateFullscreenImage(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static LoveAlgoStandingPresenter CreateStandingLayer(Transform parent, StandingPoseCatalog catalog)
        {
            var standingGO = new GameObject("StandingLayer", typeof(RectTransform));
            standingGO.transform.SetParent(parent, false);
            var rect = standingGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var presenter = standingGO.AddComponent<LoveAlgoStandingPresenter>();
            var so = new SerializedObject(presenter);
            so.FindProperty("catalog").objectReferenceValue = catalog;
            var slotsProp = so.FindProperty("slots");
            slotsProp.ClearArray();
            AddStandingSlot(slotsProp, StandingSlot.Left, CreateStandingImage(standingGO.transform, "LeftPose", new Vector2(-520f, 0f)));
            AddStandingSlot(slotsProp, StandingSlot.Center, CreateStandingImage(standingGO.transform, "CenterPose", Vector2.zero));
            AddStandingSlot(slotsProp, StandingSlot.Right, CreateStandingImage(standingGO.transform, "RightPose", new Vector2(520f, 0f)));
            so.ApplyModifiedPropertiesWithoutUndo();
            return presenter;
        }

        private static void AddStandingSlot(SerializedProperty slotsProp, StandingSlot slot, Image image)
        {
            var index = slotsProp.arraySize;
            slotsProp.InsertArrayElementAtIndex(index);
            var element = slotsProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("slot").enumValueIndex = (int)slot;
            element.FindPropertyRelative("image").objectReferenceValue = image;
        }

        private static Image CreateStandingImage(Transform parent, string name, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(720f, 1080f);
            rect.anchoredPosition = anchoredPosition;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            image.enabled = false;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static DialoguePanelComponents CreateDialoguePanel(Transform parent)
        {
            var panelGO = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGO.transform.SetParent(parent, false);
            var panelImage = panelGO.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 320f);
            rect.anchoredPosition = Vector2.zero;

            var speaker = CreateSpeakerLabel(rect);
            var body = CreateBodyLabel(rect);

            var controls = new GameObject("DialogueControls", typeof(RectTransform));
            controls.transform.SetParent(rect, false);
            var controlsRect = controls.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0.72f, 0.6f);
            controlsRect.anchorMax = new Vector2(0.98f, 0.95f);
            controlsRect.offsetMin = Vector2.zero;
            controlsRect.offsetMax = Vector2.zero;

            var skipButton = CreateControlButton(controlsRect, "SkipButton", "Skip Text");
            var restartButton = CreateControlButton(controlsRect, "RestartButton", "Restart Conversation");
            PositionControlButton(skipButton.GetComponent<RectTransform>(), 0f);
            PositionControlButton(restartButton.GetComponent<RectTransform>(), -90f);

            var choicePanel = CreateChoicePanel(rect);

            return new DialoguePanelComponents
            {
                speakerLabel = speaker,
                bodyLabel = body,
                skipButton = skipButton,
                restartButton = restartButton,
                choicePanel = choicePanel
            };
        }

        private static TextMeshProUGUI CreateSpeakerLabel(RectTransform parent)
        {
            var go = new GameObject("SpeakerLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0.4f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(32f, -24f);
            rect.sizeDelta = new Vector2(860f, 60f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 36f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.text = string.Empty;
            return tmp;
        }

        private static TextMeshProUGUI CreateBodyLabel(RectTransform parent)
        {
            var go = new GameObject("BodyLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0.68f, 0.8f);
            rect.offsetMin = new Vector2(32f, 32f);
            rect.offsetMax = new Vector2(-32f, -32f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 30f;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.text = string.Empty;
            return tmp;
        }

        private static Button CreateControlButton(RectTransform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 70f);
            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.15f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 8f);
            labelRect.offsetMax = new Vector2(-8f, -8f);
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return button;
        }

        private static LoveAlgoDialogueChoicePanel CreateChoicePanel(RectTransform parent)
        {
            var panelGO = new GameObject("ChoicesPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            panelGO.transform.SetParent(parent, false);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.72f, 0.05f);
            rect.anchorMax = new Vector2(0.98f, 0.55f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var background = panelGO.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.08f);

            var layout = panelGO.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var template = CreateChoiceButtonTemplate(rect, "ChoiceButtonTemplate");
            template.gameObject.SetActive(false);

            var choicePanel = panelGO.AddComponent<LoveAlgoDialogueChoicePanel>();
            var so = new SerializedObject(choicePanel);
            so.FindProperty("contentRoot").objectReferenceValue = rect;
            so.FindProperty("choiceButtonTemplate").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();
            return choicePanel;
        }

        private static Button CreateChoiceButtonTemplate(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 72f);

            var layoutElement = go.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 72f;
            layoutElement.minHeight = 60f;

            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.15f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 12f);
            labelRect.offsetMax = new Vector2(-12f, -12f);
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = "선택지";
            tmp.fontSize = 26f;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;

            return button;
        }

        private static void PositionControlButton(RectTransform rect, float yOffset)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = new Vector2(0f, yOffset);
        }

        private static BackgroundCatalog EnsureBackgroundCatalogAsset()
        {
            EnsureDirectoryExists(Path.GetDirectoryName(BackgroundCatalogAssetPath));
            var catalog = AssetDatabase.LoadAssetAtPath<BackgroundCatalog>(BackgroundCatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<BackgroundCatalog>();
                AssetDatabase.CreateAsset(catalog, BackgroundCatalogAssetPath);
            }

            var entries = new (string id, string spritePath)[]
            {
                ("campus_gate", "Assets/Resources/Backgrounds/campus_gate.png"),
                ("room_day", "Assets/Resources/Backgrounds/room_day.png")
            };

            ApplyBackgroundDefinitions(catalog, entries);
            return catalog;
        }

        private static StandingPoseCatalog EnsureStandingCatalogAsset()
        {
            EnsureDirectoryExists(Path.GetDirectoryName(StandingCatalogAssetPath));
            var catalog = AssetDatabase.LoadAssetAtPath<StandingPoseCatalog>(StandingCatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<StandingPoseCatalog>();
                AssetDatabase.CreateAsset(catalog, StandingCatalogAssetPath);
            }

            var definitions = new List<StandingPoseData>
            {
                new StandingPoseData("Roa", "normal", "Assets/Resources/Characters/Roa/roa_normal.png"),
                new StandingPoseData("Roa", "excited", "Assets/Resources/Characters/Roa/roa_excited.png")
            };

            ApplyStandingDefinitions(catalog, definitions);
            return catalog;
        }

        private static DialogueDatabase EnsureDialogueDatabaseAsset()
        {
            EnsureDirectoryExists(Path.GetDirectoryName(DialogueDatabasePath));
            var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DialogueDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<DialogueDatabase>();
                database.name = "LoveAlgoDialogueTestDatabase";
                AssetDatabase.CreateAsset(database, DialogueDatabasePath);
            }

            database.actors ??= new List<Actor>();
            database.conversations ??= new List<Conversation>();

            var playerActor = EnsureActor(database, 1, "Player", true, "테스트 플레이어");
            var heroineActor = EnsureActor(database, 2, "Roa", false, "로아");
            EnsureTestConversation(database, playerActor.id, heroineActor.id);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            return database;
        }

        private static Actor EnsureActor(DialogueDatabase database, int id, string name, bool isPlayer, string description)
        {
            var actor = database.actors.Find(a => a.id == id);
            if (actor == null)
            {
                actor = CreateActor(id, name, isPlayer, description);
                database.actors.Add(actor);
            }
            else
            {
                actor.Name = name;
                actor.Description = description;
                actor.IsPlayer = isPlayer;
            }

            return actor;
        }

        private static void EnsureTestConversation(DialogueDatabase database, int playerId, int heroineId)
        {
            var template = ResolveTemplate(database);
            var existing = database.conversations.Find(c => c.Title == TestConversationTitle);
            var conversation = CreateTestConversation(template, playerId, heroineId);

            if (existing == null)
            {
                database.conversations.Add(conversation);
            }
            else
            {
                var index = database.conversations.IndexOf(existing);
                database.conversations[index] = conversation;
            }
        }

        private static void ApplyBackgroundDefinitions(BackgroundCatalog catalog, (string id, string spritePath)[] entries)
        {
            var so = new SerializedObject(catalog);
            var itemsProp = so.FindProperty("items");
            itemsProp.ClearArray();

            for (int i = 0; i < entries.Length; i++)
            {
                itemsProp.InsertArrayElementAtIndex(i);
                var element = itemsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("id").stringValue = entries[i].id;
                element.FindPropertyRelative("sprite").objectReferenceValue = LoadSprite(entries[i].spritePath);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyStandingDefinitions(StandingPoseCatalog catalog, List<StandingPoseData> entries)
        {
            var so = new SerializedObject(catalog);
            var posesProp = so.FindProperty("poses");
            posesProp.ClearArray();

            for (int i = 0; i < entries.Count; i++)
            {
                posesProp.InsertArrayElementAtIndex(i);
                var element = posesProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("heroineId").stringValue = entries[i].heroineId;
                element.FindPropertyRelative("poseId").stringValue = entries[i].poseId;
                element.FindPropertyRelative("sprite").objectReferenceValue = LoadSprite(entries[i].spritePath);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static Sprite LoadSprite(string assetPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogWarning($"LoveAlgoTestSceneBuilder: Sprite not found at {assetPath}");
            }

            return sprite;
        }

        private static Actor CreateActor(int id, string name, bool isPlayer, string description)
        {
            var actor = new Actor
            {
                id = id,
                fields = new List<Field>()
            };
            actor.Name = name;
            actor.Description = description;
            actor.IsPlayer = isPlayer;
            return actor;
        }

        

        private sealed class DialogueUiComponents
        {
            public LoveAlgoDialogueView view;
            public Button skipButton;
            public Button restartButton;
        }

        private sealed class DialoguePanelComponents
        {
            public TextMeshProUGUI speakerLabel;
            public TextMeshProUGUI bodyLabel;
            public Button skipButton;
            public Button restartButton;
            public LoveAlgoDialogueChoicePanel choicePanel;
        }

        private readonly struct StandingPoseData
        {
            public readonly string heroineId;
            public readonly string poseId;
            public readonly string spritePath;

            public StandingPoseData(string heroineId, string poseId, string spritePath)
            {
                this.heroineId = heroineId;
                this.poseId = poseId;
                this.spritePath = spritePath;
            }
        }

        private static bool TryLoadConfiguration(out LoveAlgoConfiguration configuration)
        {
            configuration = AssetDatabase.LoadAssetAtPath<LoveAlgoConfiguration>(ConfigPath);
            return configuration != null;
        }

        private static void SetSerializedReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"LoveAlgoTestSceneBuilder: '{propertyName}' 필드를 {target.name}에서 찾을 수 없습니다.", target);
                return;
            }

            so.Update();
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static Template ResolveTemplate(DialogueDatabase database)
        {
            if (database != null && !string.IsNullOrEmpty(database.templateJson))
            {
                try
                {
                    return JsonUtility.FromJson<Template>(database.templateJson);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"LoveAlgoTestSceneBuilder: templateJson 파싱 실패, 기본 템플릿을 사용합니다. {ex.Message}");
                }
            }

            return TemplateTools.FromDefault();
        }

        private static Conversation CreateTestConversation(Template template, int playerId, int heroineId)
        {
            var conversation = template.CreateConversation(TestConversationId, TestConversationTitle);
            conversation.ActorID = playerId;
            conversation.ConversantID = heroineId;
            conversation.dialogueEntries = BuildConversationEntries(template, conversation.id, playerId, heroineId);
            return conversation;
        }

        private static List<DialogueEntry> BuildConversationEntries(Template template, int conversationId, int playerId, int heroineId)
        {
            var entries = new List<DialogueEntry>();
            entries.Add(CreateStartEntry(template, conversationId, playerId, heroineId));

            var intro = CreateDialogueEntry(template, conversationId, 1, heroineId, playerId, "Roa_Greeting", "아, 여기서 다시 만났네! 오늘 수업 어땠어?");
            Field.SetValue(intro.fields, "Background", "campus_gate");
            Field.SetValue(intro.fields, "StandingLayout", "Left=Roa@normal");
            Field.SetValue(intro.fields, "StandingFocus", "Roa");
            Field.SetValue(intro.fields, "StandingHide", "Center,Right");
            intro.outgoingLinks.Add(new Link(conversationId, 1, conversationId, 2));
            entries.Add(intro);

            var playerLine = CreateDialogueEntry(template, conversationId, 2, playerId, heroineId, "Player_Response", "꽤 괜찮았어. 그런데 너는 왜 이렇게 급해 보여?");
            Field.SetValue(playerLine.fields, "StandingFocus", "None");
            playerLine.Sequence = "LoveAlgoStandingShake(Left, 0.28, 12);";
            playerLine.outgoingLinks.Add(new Link(conversationId, 2, conversationId, 3));
            entries.Add(playerLine);

            var finale = CreateDialogueEntry(template, conversationId, 3, heroineId, playerId, "Roa_Reveal", "서프라이즈 준비 중이거든! 잠시만 기다려봐.");
            Field.SetValue(finale.fields, "Background", "room_day");
            Field.SetValue(finale.fields, "StandingLayout", "Left=Roa@excited");
            Field.SetValue(finale.fields, "StandingFocus", "Left");
            finale.Sequence = "LoveAlgoStandingShake(Left, 0.35, 18);";
            entries.Add(finale);

            return entries;
        }

        private static DialogueEntry CreateStartEntry(Template template, int conversationId, int actorId, int conversantId)
        {
            var entry = template.CreateDialogueEntry(0, conversationId, "START");
            entry.ActorID = actorId;
            entry.ConversantID = conversantId;
            entry.isRoot = true;
            entry.isGroup = false;
            entry.DialogueText = string.Empty;
            entry.MenuText = string.Empty;
            entry.Sequence = SequencerKeywords.NoneCommand;
            entry.outgoingLinks.Clear();
            entry.outgoingLinks.Add(new Link(conversationId, 0, conversationId, 1));
            return entry;
        }

        private static DialogueEntry CreateDialogueEntry(Template template, int conversationId, int entryId, int actorId, int conversantId, string title, string dialogue, bool isRoot = false)
        {
            var entry = template.CreateDialogueEntry(entryId, conversationId, title);
            entry.ActorID = actorId;
            entry.ConversantID = conversantId;
            entry.isRoot = isRoot;
            entry.isGroup = false;
            entry.DialogueText = isRoot ? string.Empty : dialogue;
            entry.MenuText = isRoot ? string.Empty : dialogue;
            entry.Sequence = string.Empty;
            entry.falseConditionAction = "Block";
            entry.outgoingLinks.Clear();
            entry.conditionsString = string.Empty;
            entry.userScript = string.Empty;
            return entry;
        }

        private static Canvas CreateCanvas(string name, Color backgroundColor, string label)
        {
            var canvasGO = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var panelImage = panel.GetComponent<Image>();
            panelImage.color = backgroundColor;
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.25f, 0.25f);
            panelRect.anchorMax = new Vector2(0.75f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGO.transform.SetParent(panel.transform, false);
            var text = textGO.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.6f);
            textRect.anchorMax = new Vector2(0.9f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return canvas;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var buttonGO = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);
            var rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 60f);
            rect.anchoredPosition = anchoredPosition;

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGO.transform.SetParent(buttonGO.transform, false);
            var text = textGO.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGO.GetComponent<Button>();
        }

        private static void SetPortalReferences(UiPortal portal, Canvas story, Canvas free, Canvas eventCanvas)
        {
            var so = new SerializedObject(portal);
            so.FindProperty("storyCanvas").objectReferenceValue = story;
            so.FindProperty("freeActionCanvas").objectReferenceValue = free;
            so.FindProperty("eventCanvas").objectReferenceValue = eventCanvas;
            so.ApplyModifiedProperties();
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void EnsureDialogueSystemEvents(GameObject dialogueManager)
        {
            if (dialogueManager == null)
            {
                return;
            }

            if (dialogueManager.GetComponent<DialogueSystemEvents>() != null)
            {
                return;
            }

            dialogueManager.AddComponent<DialogueSystemEvents>();
        }

        private static void EnsureDirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
#endif
