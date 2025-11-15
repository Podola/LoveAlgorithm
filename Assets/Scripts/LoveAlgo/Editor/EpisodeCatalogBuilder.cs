#if UNITY_EDITOR
using System.Collections.Generic;
using LoveAlgo.Data;
using UnityEditor;
using UnityEngine;

namespace LoveAlgo.Editor
{
    public static class EpisodeCatalogBuilder
    {
        private const string EpisodeFolderPath = "Assets/Data/LoveAlgo/Episodes";
        private const string CatalogAssetPath = EpisodeFolderPath + "/EpisodeCatalog.asset";
        private const string ConfigAssetPath = "Assets/Data/LoveAlgo/Config/LoveAlgoConfiguration.asset";

        [MenuItem("LoveAlgo/Generate Episode Catalog (GDD)", priority = 30)]
        public static void Generate()
        {
            EnsureFolder(EpisodeFolderPath);
            var definitions = new List<EpisodeDefinition>();
            foreach (var seed in Seeds)
            {
                var assetPath = EpisodeFolderPath + "/" + seed.AssetName + ".asset";
                var definition = AssetDatabase.LoadAssetAtPath<EpisodeDefinition>(assetPath);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<EpisodeDefinition>();
                    AssetDatabase.CreateAsset(definition, assetPath);
                }

                ApplySeed(definition, seed);
                definitions.Add(definition);
            }

            var catalog = AssetDatabase.LoadAssetAtPath<EpisodeCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<EpisodeCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            var catalogSO = new SerializedObject(catalog);
            var episodesProp = catalogSO.FindProperty("episodes");
            episodesProp.arraySize = definitions.Count;
            for (var i = 0; i < definitions.Count; i++)
            {
                episodesProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            catalogSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);

            if (TryLoadConfiguration(out var configuration))
            {
                var configSO = new SerializedObject(configuration);
                var episodeCatalogProp = configSO.FindProperty("episodeCatalog");
                if (episodeCatalogProp != null)
                {
                    episodeCatalogProp.objectReferenceValue = catalog;
                    configSO.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(configuration);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Episode catalog regenerated from GDD snapshot.", catalog);
        }

        private static bool TryLoadConfiguration(out LoveAlgoConfiguration configuration)
        {
            configuration = AssetDatabase.LoadAssetAtPath<LoveAlgoConfiguration>(ConfigAssetPath);
            return configuration != null;
        }

        private static void ApplySeed(EpisodeDefinition definition, EpisodeSeed seed)
        {
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("episodeId").stringValue = seed.Id;
            serialized.FindProperty("displayName").stringValue = seed.DisplayName;
            serialized.FindProperty("stage").enumValueIndex = (int)seed.Stage;
            serialized.FindProperty("scheduleMode").enumValueIndex = (int)seed.Mode;
            serialized.FindProperty("giftWindow").enumValueIndex = (int)seed.GiftWindow;
            serialized.FindProperty("locksFreeActions").boolValue = seed.LocksFreeActions;
            serialized.FindProperty("summary").stringValue = seed.Summary;

            var pointsProp = serialized.FindProperty("points");
            pointsProp.FindPropertyRelative("eventPoints").intValue = seed.EventPoints;
            pointsProp.FindPropertyRelative("dialoguePointCap").intValue = seed.DialoguePoints;
            pointsProp.FindPropertyRelative("messengerPointCap").intValue = seed.MessengerPoints;
            pointsProp.FindPropertyRelative("miniGamePointCap").intValue = seed.MiniGamePoints;
            pointsProp.FindPropertyRelative("giftBonusCap").intValue = seed.GiftBonusCap;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var segments = path.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        private readonly struct EpisodeSeed
        {
            public EpisodeSeed(string id, string displayName, string summary, EpisodeStage stage, ScheduleMode mode, int eventPoints, int dialoguePoints, int messengerPoints, int miniGamePoints, int giftBonusCap, EpisodeGiftWindow giftWindow, bool locksFreeActions, string assetName)
            {
                Id = id;
                DisplayName = displayName;
                Summary = summary;
                Stage = stage;
                Mode = mode;
                EventPoints = eventPoints;
                DialoguePoints = dialoguePoints;
                MessengerPoints = messengerPoints;
                MiniGamePoints = miniGamePoints;
                GiftBonusCap = giftBonusCap;
                GiftWindow = giftWindow;
                LocksFreeActions = locksFreeActions;
                AssetName = assetName;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public string Summary { get; }
            public EpisodeStage Stage { get; }
            public ScheduleMode Mode { get; }
            public int EventPoints { get; }
            public int DialoguePoints { get; }
            public int MessengerPoints { get; }
            public int MiniGamePoints { get; }
            public int GiftBonusCap { get; }
            public EpisodeGiftWindow GiftWindow { get; }
            public bool LocksFreeActions { get; }
            public string AssetName { get; }
        }

        private static readonly EpisodeSeed[] Seeds =
        {
            new EpisodeSeed("semester_intro", "개강", "한 학기 루프를 소개하는 오프닝." , EpisodeStage.Intro, ScheduleMode.StoryEvent, 0, 0, 0, 0, 0, EpisodeGiftWindow.None, true, "SemesterIntro_Episode"),
            new EpisodeSeed("first_meeting", "1차 개인 이벤트", "모든 히로인의 첫 번째 데이트. 기본 +3, 대화 2, 미니게임 2." , EpisodeStage.FirstEvent, ScheduleMode.StoryEvent, 3, 2, 0, 2, 0, EpisodeGiftWindow.None, true, "FirstEvent_Episode"),
            new EpisodeSeed("school_festival", "축제", "단체 이벤트: 자유행동 불가, 귀가 선택으로 분기." , EpisodeStage.Festival, ScheduleMode.Festival, 4, 0, 0, 0, 0, EpisodeGiftWindow.None, true, "Festival_Episode"),
            new EpisodeSeed("second_date", "2차 개인 이벤트", "두 번째 데이트: +6, 대화 2, 미니게임 3, 선물 창구(2차 규칙)." , EpisodeStage.SecondEvent, ScheduleMode.StoryEvent, 6, 2, 0, 3, 3, EpisodeGiftWindow.SecondEvent, true, "SecondEvent_Episode"),
            new EpisodeSeed("mt_retreat", "MT (바다)", "3일간 진행되는 단체 여행. 총 +5, 둘째 날 밤 선택 분기." , EpisodeStage.Retreat, ScheduleMode.Retreat, 5, 0, 0, 0, 0, EpisodeGiftWindow.None, true, "Retreat_Episode"),
            new EpisodeSeed("final_date", "3차 개인 이벤트", "세 번째 데이트: +9, 대화 3, 선물 창구(3차 규칙)." , EpisodeStage.ThirdEvent, ScheduleMode.StoryEvent, 9, 3, 0, 0, 5, EpisodeGiftWindow.ThirdEvent, true, "ThirdEvent_Episode"),
            new EpisodeSeed("confession_event", "고백 이벤트", "누적 포인트와 스탯으로 루트 확정. 로아는 피로 조건 필요." , EpisodeStage.Confession, ScheduleMode.Confession, 0, 0, 0, 0, 0, EpisodeGiftWindow.None, true, "Confession_Episode"),
            new EpisodeSeed("ending_event", "엔딩", "히로인별 해피/새드/히든 엔딩 및 노고백 엔딩." , EpisodeStage.Ending, ScheduleMode.Ending, 0, 0, 0, 0, 0, EpisodeGiftWindow.None, true, "Ending_Episode"),
            new EpisodeSeed("messenger_window", "메신저", "메신저 대화 3회분: 총 +3, 자유행동 가능." , EpisodeStage.MessengerWindow, ScheduleMode.Free, 0, 0, 3, 0, 0, EpisodeGiftWindow.None, false, "MessengerWindow_Episode"),
            new EpisodeSeed("daily_slice", "일상 대화", "자유시간 대화 선택: 최대 +5." , EpisodeStage.DailySlice, ScheduleMode.Free, 0, 5, 0, 0, 0, EpisodeGiftWindow.None, false, "DailySlice_Episode")
        };
    }
}
#endif
