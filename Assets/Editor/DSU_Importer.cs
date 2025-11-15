#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using LoveAlgo;

namespace LoveAlgo.Editor
{
    /// <summary>
    /// PROJECT_BIBLE 규칙에 따라 CSV를 DialogueDatabase로 변환하는 임포터.
    /// </summary>
    public class DSU_Importer : EditorWindow
    {
        // 상수는 LoveAlgoEditorMenu에서 중앙 관리
        private const string DatabaseAssetPath = LoveAlgoEditorMenu.DatabasePath;
        private const string ResourceCatalogAssetPath = LoveAlgoEditorMenu.ResourceCatalogPath;
        
        // Story CSV 목록 (경로, Conversation 이름)
        private static readonly (string path, string conversationName)[] StoryFiles = new[]
        {
            ("Docs/Database_Source/Story 시트_Demo.csv", "Story_Demo"),
            ("Docs/Database_Source/Story/Test/Story_Test_Day01.csv", "Test_Day01"),
            ("Docs/Database_Source/Story/Test/Story_Test_Day02.csv", "Test_Day02"),
            ("Docs/Database_Source/Story/Test/Story_Test_Day03.csv", "Test_Day03"),
            ("Docs/Database_Source/Story/Test/Story_Test_Day04.csv", "Test_Day04"),
        };
        
        private const string ActorCsvRelativePath = "Docs/Database_Source/ID 시트_Actor.csv";
        private const string BgCsvRelativePath = "Docs/Database_Source/ID 시트_BG.csv";
        private const string BgmCsvRelativePath = "Docs/Database_Source/ID 시트_BGM.csv";
        private const string SfxCsvRelativePath = "Docs/Database_Source/ID 시트_SFX.csv";
        private const string SequenceCsvRelativePath = "Docs/Database_Source/ID 시트_Sequence.csv";
        private const string StandingCsvRelativePath = "Docs/Database_Source/ID 시트_Standing.csv";

        private const int PlayerActorId = 1;
        private const int NarratorActorId = 7;
        private const int RootEntryId = 0;

        private DialogueDatabase? dialogueDatabase;
        private LoveAlgoResourceCatalog? resourceCatalog;
        private Vector2 scrollPosition;
        private readonly List<string> logLines = new List<string>();
        private bool autoRunImport;
        
        // Cross-Conversation Link 처리를 위한 임시 저장소
        private readonly List<(string conversationName, DialogueEntry entry, string targetKey, int rowNumber)> pendingCrossConversationLinks = new List<(string, DialogueEntry, string, int)>();

        // 메뉴 항목은 LoveAlgoEditorMenu로 이동됨
        public static void ShowWindow()
        {
            var window = GetWindow<DSU_Importer>("DSU Story Importer");
            window.autoRunImport = true;
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateDatabase();
            LoadOrCreateResourceCatalog();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("러브 알고리즘 - DSU CSV 임포터", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);

            DrawDatabaseSection();
            EditorGUILayout.Space(8f);
            DrawResourceCatalogSection();
            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("임포트 경로", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Story CSV 목록:", EditorStyles.boldLabel);
                foreach (var (path, conversationName) in StoryFiles)
                {
                    EditorGUILayout.LabelField($"  • {conversationName}", path);
                }
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Actor CSV", ActorCsvRelativePath);
                EditorGUILayout.LabelField("BG CSV", BgCsvRelativePath);
                EditorGUILayout.LabelField("BGM CSV", BgmCsvRelativePath);
                EditorGUILayout.LabelField("SFX CSV", SfxCsvRelativePath);
                EditorGUILayout.LabelField("Sequence CSV", SequenceCsvRelativePath);
                EditorGUILayout.LabelField("Standing CSV", StandingCsvRelativePath);
            }

            EditorGUILayout.Space(10f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Import Story & Library", GUILayout.Height(32f), GUILayout.Width(220f)))
                {
                    ImportStory();
                }
            }

            EditorGUILayout.Space(12f);

            EditorGUILayout.LabelField("로그", EditorStyles.boldLabel);
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandHeight(true)))
            {
                scrollPosition = scrollView.scrollPosition;
                foreach (var line in logLines)
                {
                    EditorGUILayout.LabelField(line);
                }
            }

            if (autoRunImport && Event.current.type == EventType.Repaint)
            {
                autoRunImport = false;
                ImportStory();
            }
        }

        private void DrawDatabaseSection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Target DialogueDatabase", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                var selected = EditorGUILayout.ObjectField("Database Asset", dialogueDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
                if (EditorGUI.EndChangeCheck())
                {
                    dialogueDatabase = selected; // nullable 필드에 nullable 값 할당 (의도된 동작)
                    if (dialogueDatabase != null)
                    {
                        logLines.Add($"[INFO] 사용자 지정 데이터베이스를 사용합니다: {AssetDatabase.GetAssetPath(dialogueDatabase)}");
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("자동 경로", GUILayout.Width(70f));
                EditorGUILayout.SelectableLabel(DatabaseAssetPath, EditorStyles.textField, GUILayout.Height(18f));
                EditorGUILayout.EndHorizontal();

                if (dialogueDatabase == null)
                {
                    if (GUILayout.Button("데이터베이스 생성/로드", GUILayout.Height(24f)))
                    {
                        LoadOrCreateDatabase(forceCreate: true);
                    }
                }
            }
        }

        private void DrawResourceCatalogSection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Resource Catalog", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                var selected = EditorGUILayout.ObjectField("Catalog Asset", resourceCatalog, typeof(LoveAlgoResourceCatalog), false) as LoveAlgoResourceCatalog;
                if (EditorGUI.EndChangeCheck())
                {
                    resourceCatalog = selected;
                    if (resourceCatalog != null)
                    {
                        logLines.Add($"[INFO] 리소스 카탈로그를 지정했습니다: {AssetDatabase.GetAssetPath(resourceCatalog)}");
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("자동 경로", GUILayout.Width(70f));
                EditorGUILayout.SelectableLabel(ResourceCatalogAssetPath, EditorStyles.textField, GUILayout.Height(18f));
                EditorGUILayout.EndHorizontal();

                if (resourceCatalog == null)
                {
                    if (GUILayout.Button("카탈로그 생성/로드", GUILayout.Height(24f)))
                    {
                        LoadOrCreateResourceCatalog(forceCreate: true);
                    }
                }
            }
        }

        private void LoadOrCreateResourceCatalog(bool forceCreate = false)
        {
            if (!forceCreate && resourceCatalog != null)
            {
                return;
            }

            resourceCatalog = AssetDatabase.LoadAssetAtPath<LoveAlgoResourceCatalog>(ResourceCatalogAssetPath);
            if (resourceCatalog == null)
            {
                resourceCatalog = ScriptableObject.CreateInstance<LoveAlgoResourceCatalog>();
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Application.dataPath, "../", ResourceCatalogAssetPath)) ?? string.Empty);
                AssetDatabase.CreateAsset(resourceCatalog, ResourceCatalogAssetPath);
                AssetDatabase.SaveAssets();
                logLines.Add($"[INFO] 새 리소스 카탈로그를 생성했습니다: {ResourceCatalogAssetPath}");
            }
            else
            {
                logLines.Add($"[INFO] 리소스 카탈로그를 로드했습니다: {ResourceCatalogAssetPath}");
            }
        }

        private void LoadOrCreateDatabase(bool forceCreate = false)
        {
            if (!forceCreate && dialogueDatabase != null)
            {
                return;
            }

            dialogueDatabase = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DatabaseAssetPath);
            if (dialogueDatabase == null)
            {
                dialogueDatabase = ScriptableObject.CreateInstance<DialogueDatabase>();
                dialogueDatabase.name = "LoveAlgo_Database";
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Application.dataPath, "../", DatabaseAssetPath)) ?? string.Empty);
                AssetDatabase.CreateAsset(dialogueDatabase, DatabaseAssetPath);
                AssetDatabase.SaveAssets();
                logLines.Add($"[INFO] 새 DialogueDatabase 에셋을 생성했습니다: {DatabaseAssetPath}");
            }
            else
            {
                logLines.Add($"[INFO] DialogueDatabase 에셋을 로드했습니다: {DatabaseAssetPath}");
            }
        }

        private void ImportStory()
        {
            if (dialogueDatabase == null)
            {
                logLines.Add("[ERROR] DialogueDatabase가 선택되지 않았습니다.");
                return;
            }

            var db = dialogueDatabase; // Null 체크 후 로컬 변수로 캡처

            try
            {
                logLines.Clear();
                EditorUtility.DisplayProgressBar("LoveAlgo DSU Importer", "CSV 데이터를 읽는 중...", 0.05f);

                var projectRoot = GetProjectRoot();
                
                // Story CSV 경로 검증
                var storyPaths = new List<(string path, string conversationName)>();
                foreach (var (relativePath, conversationName) in StoryFiles)
                {
                    var fullPath = Path.Combine(projectRoot, relativePath);
                    ValidateFile(fullPath);
                    storyPaths.Add((fullPath, conversationName));
                }
                
                var actorPath = Path.Combine(projectRoot, ActorCsvRelativePath);
                var bgPath = Path.Combine(projectRoot, BgCsvRelativePath);
                var bgmPath = Path.Combine(projectRoot, BgmCsvRelativePath);
                var sfxPath = Path.Combine(projectRoot, SfxCsvRelativePath);
                var seqPath = Path.Combine(projectRoot, SequenceCsvRelativePath);
                var standingPath = Path.Combine(projectRoot, StandingCsvRelativePath);

                ValidateFile(actorPath);
                ValidateFile(bgPath);
                ValidateFile(bgmPath);
                ValidateFile(sfxPath);
                ValidateFile(seqPath);
                ValidateFile(standingPath);

                var actorsInfo = LoadActorSheet(actorPath);
                var backgrounds = LoadSimpleSheet(bgPath, "id", "description");
                var bgms = LoadSimpleSheet(bgmPath, "id", "resourceName");
                var sfxs = LoadSimpleSheet(sfxPath, "id", "resourceName");
                var sequences = LoadSimpleSheet(seqPath, "id", "dsuCommand");
                var standings = LoadStandingSheet(standingPath);

                LoadOrCreateResourceCatalog();
                if (resourceCatalog == null)
                {
                    throw new InvalidOperationException("리소스 카탈로그 에셋을 찾을 수 없습니다.");
                }
                var catalog = resourceCatalog;

                EditorUtility.DisplayProgressBar("LoveAlgo DSU Importer", "DialogueDatabase 업데이트 중...", 0.45f);

                Undo.RecordObject(db, "Import Dialogue Database");
                Undo.RecordObject(catalog, "Update Resource Catalog");

                db.actors = BuildActors(actorsInfo);
                db.variables = db.variables ?? new List<Variable>();
                db.items = db.items ?? new List<Item>();
                
                // 리소스 관련 Items 제거 (이제 ResourceCatalog로 관리)
                RemoveResourceItems(db);
                
                // 여러 Story CSV를 각각 Conversation으로 변환
                db.conversations = new List<Conversation>();
                int totalRows = 0;
                int conversationId = 1;
                
                for (int i = 0; i < storyPaths.Count; i++)
                {
                    var (storyPath, conversationName) = storyPaths[i];
                    float progress = 0.5f + (0.4f * i / storyPaths.Count);
                    EditorUtility.DisplayProgressBar("LoveAlgo DSU Importer", 
                        $"Conversation 생성 중: {conversationName}...", progress);
                    
                    var storyRows = LoadStoryRows(storyPath);
                    var conversation = BuildConversation(conversationId, conversationName, storyRows);
                    db.conversations.Add(conversation);
                    
                    totalRows += storyRows.Count;
                    conversationId++;
                    
                    logLines.Add($"[INFO] {conversationName}: {storyRows.Count}행 Import 완료");
                }

                UpdateResourceCatalog(catalog, backgrounds, bgms, sfxs, sequences, standings);

                // 모든 Conversation이 생성된 후 Cross-Conversation Link 처리
                EditorUtility.DisplayProgressBar("LoveAlgo DSU Importer", "Cross-Conversation Link 처리 중...", 0.90f);
                ProcessCrossConversationLinks(db);

                EditorUtility.SetDirty(db);
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayProgressBar("LoveAlgo DSU Importer", "완료 정리 중...", 0.95f);
                logLines.Add($"[OK] 총 {db.conversations.Count}개 Conversation, {totalRows}행 Import 완료");
                EditorUtility.DisplayDialog("LoveAlgo DSU Importer", 
                    $"CSV 임포트를 완료했습니다.\n\n" +
                    $"Conversation: {db.conversations.Count}개\n" +
                    $"총 대화 행: {totalRows}개", "확인");
            }
            catch (Exception ex)
            {
                logLines.Add("[ERROR] 임포트 중 예외가 발생했습니다. 콘솔 로그를 확인하세요.");
                Debug.LogError($"[LoveAlgo DSU Importer] {ex}");
                EditorUtility.DisplayDialog("LoveAlgo DSU Importer", $"임포트 중 오류가 발생했습니다:\n{ex.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Repaint();
            }
        }

        private static string GetProjectRoot()
        {
            // Application.dataPath = LoveAlgorithmProject/Assets
            // 한 단계 위: LoveAlgorithmProject
            // 두 단계 위: LoveAlgorithm (CSV 파일들이 있는 루트)
            var assetsPath = Application.dataPath;
            return Path.GetFullPath(Path.Combine(assetsPath, "..", ".."));
        }

        private static void ValidateFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"필수 CSV 파일을 찾을 수 없습니다: {path}");
            }
        }

        private static Dictionary<int, string> LoadActorSheet(string path)
        {
            var rows = LoadCsv(path);
            var header = rows.FirstOrDefault();
            if (header == null)
            {
                throw new InvalidDataException("Actor CSV에 헤더가 없습니다.");
            }

            var idIndex = Array.IndexOf(header, "id");
            var displayIndex = Array.IndexOf(header, "displayName");
            if (idIndex < 0 || displayIndex < 0)
            {
                throw new InvalidDataException("Actor CSV 헤더에 id/displayName이 없습니다.");
            }

            var result = new Dictionary<int, string>();

            for (var i = 1; i < rows.Count; i++)
            {
                var record = rows[i];
                if (record.Length <= Math.Max(idIndex, displayIndex))
                {
                    continue;
                }

                var idString = record[idIndex].Trim();
                if (!int.TryParse(idString, out var id))
                {
                    continue;
                }

                var name = record[displayIndex].Trim();
                result[id] = name;
            }

            return result;
        }

        private static List<Dictionary<string, string>> LoadSimpleSheet(string path, string keyColumn, string valueColumn)
        {
            var rows = LoadCsv(path);
            if (rows.Count == 0)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} CSV가 비어 있습니다.");
            }

            var header = rows[0].Select(h => h.Trim('\uFEFF')).ToArray();
            var keyIndex = Array.FindIndex(header, h => string.Equals(h, keyColumn, StringComparison.OrdinalIgnoreCase));
            var valueIndex = Array.FindIndex(header, h => string.Equals(h, valueColumn, StringComparison.OrdinalIgnoreCase));

            if (keyIndex < 0 || valueIndex < 0)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} 헤더에 {keyColumn}/{valueColumn}가 없습니다.");
            }

            var list = new List<Dictionary<string, string>>();
            for (var i = 1; i < rows.Count; i++)
            {
                var record = rows[i];
                if (record.Length <= Math.Max(keyIndex, valueIndex))
                {
                    continue;
                }

                list.Add(new Dictionary<string, string>
                {
                    ["id"] = record[keyIndex].Trim(),
                    [valueColumn] = record[valueIndex].Trim()
                });
            }

            return list;
        }

        private static List<Dictionary<string, string>> LoadStandingSheet(string path)
        {
            var rows = LoadCsv(path);
            if (rows.Count == 0)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} CSV가 비어 있습니다.");
            }

            var header = rows[0].Select(h => h.Trim('\uFEFF')).ToArray();
            var actorIdIndex = Array.FindIndex(header, h => string.Equals(h, "actorId", StringComparison.OrdinalIgnoreCase));
            var expressionIndex = Array.FindIndex(header, h => string.Equals(h, "expression", StringComparison.OrdinalIgnoreCase));
            var resourcePathIndex = Array.FindIndex(header, h => string.Equals(h, "resourcePath", StringComparison.OrdinalIgnoreCase));

            if (actorIdIndex < 0 || expressionIndex < 0 || resourcePathIndex < 0)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} 헤더에 actorId/expression/resourcePath가 없습니다.");
            }

            var list = new List<Dictionary<string, string>>();
            for (var i = 1; i < rows.Count; i++)
            {
                var record = rows[i];
                if (record.Length <= Math.Max(Math.Max(actorIdIndex, expressionIndex), resourcePathIndex))
                {
                    continue;
                }

                var actorIdStr = record[actorIdIndex].Trim();
                var expression = record[expressionIndex].Trim();
                var resourcePath = record[resourcePathIndex].Trim();

                // 빈 행 스킵
                if (string.IsNullOrEmpty(actorIdStr) && string.IsNullOrEmpty(expression) && string.IsNullOrEmpty(resourcePath))
                {
                    continue;
                }

                list.Add(new Dictionary<string, string>
                {
                    ["actorId"] = actorIdStr,
                    ["expression"] = expression,
                    ["resourcePath"] = resourcePath
                });
            }

            return list;
        }

        private static List<StoryRow> LoadStoryRows(string path)
        {
            var rows = LoadCsv(path);
            if (rows.Count == 0)
            {
                throw new InvalidDataException("Story CSV가 비어 있습니다.");
            }

            var header = rows[0].Select(h => h.Trim('\uFEFF')).ToArray();
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < header.Length; i++)
            {
                if (!headerMap.ContainsKey(header[i]))
                {
                    headerMap[header[i]] = i;
                }
            }

            string Get(string column, string[] record)
            {
                return headerMap.TryGetValue(column, out var index) && index < record.Length
                    ? record[index]
                    : string.Empty;
            }

            var storyRows = new List<StoryRow>();
            for (var i = 1; i < rows.Count; i++)
            {
                var record = rows[i];
                if (record.Length == 0 || string.IsNullOrWhiteSpace(string.Join(string.Empty, record)))
                {
                    continue;
                }

                var row = new StoryRow
                {
                    RowNumber = i,
                    ActorRaw = Get("Actor", record).Trim(),
                    DialogueText = NormalizeDialogue(Get("DialogueText", record)),
                    NodeId = Get("NodeID", record).Trim(),
                    LinkToId = Get("LinkToID", record).Trim(),
                    ChoiceGroup = Get("ChoiceGroup", record).Trim(),
                    BackgroundId = Get("BG", record).Trim(),
                    BgmId = Get("BGM", record).Trim(),
                    SfxId = Get("SFX", record).Trim(),
                    Expression = Get("Expression", record).Trim(),
                    SequenceToken = Get("Sequence", record).Trim(),
                    AutoProgressLocked = Get("AutoProgressLocked", record).Trim(),
                    StandingLeft = Get("StandingLeft", record).Trim(),
                    StandingCenter = Get("StandingCenter", record).Trim(),
                    StandingRight = Get("StandingRight", record).Trim()
                };

                storyRows.Add(row);
            }

            return storyRows;
        }

        private static string NormalizeDialogue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .TrimEnd();
        }

        private List<Actor> BuildActors(Dictionary<int, string> actorsInfo)
        {
            var actors = new List<Actor>();
            foreach (var pair in actorsInfo.OrderBy(p => p.Key))
            {
                // fields를 먼저 초기화한 후 Name을 설정해야 함
                var actor = new Actor
                {
                    id = pair.Key,
                    fields = new List<Field>()
                };
                
                // fields가 초기화된 후 Name 설정
                actor.Name = pair.Value;

                actor.fields.Add(new Field("Display Name", pair.Value, FieldType.Text));
                actor.fields.Add(new Field("IsPlayer", (pair.Key == PlayerActorId).ToString(), FieldType.Boolean));

                actors.Add(actor);
            }

            return actors;
        }

        private void RemoveResourceItems(DialogueDatabase db)
        {
            if (db.items == null || db.items.Count == 0)
            {
                return;
            }

            var resourceCategories = new[] { "BG", "BGM", "SFX", "Sequence" };
            var itemsToRemove = new List<Item>();

            foreach (var item in db.items)
            {
                if (item.fields == null)
                {
                    continue;
                }

                var categoryField = item.fields.FirstOrDefault(f => f.title == "Category");
                if (categoryField != null && resourceCategories.Contains(categoryField.value))
                {
                    itemsToRemove.Add(item);
                }
            }

            if (itemsToRemove.Count > 0)
            {
                foreach (var item in itemsToRemove)
                {
                    db.items.Remove(item);
                }
                logLines.Add($"[INFO] 리소스 관련 Items {itemsToRemove.Count}개를 제거했습니다. (이제 ResourceCatalog로 관리)");
            }
        }

        private void UpdateResourceCatalog(
            LoveAlgoResourceCatalog catalog,
            List<Dictionary<string, string>> backgrounds,
            List<Dictionary<string, string>> bgms,
            List<Dictionary<string, string>> sfxs,
            List<Dictionary<string, string>> sequences,
            List<Dictionary<string, string>> standings)
        {
            if (catalog == null)
            {
                return;
            }

            static string SafeGet(Dictionary<string, string> source, string key)
            {
                return source.TryGetValue(key, out var value) ? value : string.Empty;
            }

            var bgEntries = backgrounds.Select(bg => new LoveAlgoResourceCatalog.BackgroundEntry
            {
                id = SafeGet(bg, "id"),
                description = SafeGet(bg, "description")
            });

            var bgmEntries = bgms.Select(bgm => new LoveAlgoResourceCatalog.BgmEntry
            {
                id = SafeGet(bgm, "id"),
                resourceName = SafeGet(bgm, "resourceName")
            });

            var sfxEntries = sfxs.Select(sfx => new LoveAlgoResourceCatalog.SfxEntry
            {
                id = SafeGet(sfx, "id"),
                resourceName = SafeGet(sfx, "resourceName")
            });

            var sequenceEntries = sequences.Select(seq => new LoveAlgoResourceCatalog.SequenceEntry
            {
                id = SafeGet(seq, "id"),
                command = SafeGet(seq, "dsuCommand")
            });

            var standingEntries = standings.Select(st =>
            {
                var actorIdStr = SafeGet(st, "actorId");
                int.TryParse(actorIdStr, out var actorId);
                
                return new LoveAlgoResourceCatalog.StandingEntry
                {
                    actorId = actorId,
                    expression = SafeGet(st, "expression"),
                    resourcePath = SafeGet(st, "resourcePath")
                };
            });

            catalog.SetBackgrounds(bgEntries);
            catalog.SetBgmTracks(bgmEntries);
            catalog.SetSoundEffects(sfxEntries);
            catalog.SetSequenceTemplates(sequenceEntries);
            catalog.SetStandingSprites(standingEntries);

            logLines.Add($"[INFO] 리소스 카탈로그 업데이트 완료 - BG {catalog.backgrounds.Count}, BGM {catalog.bgmTracks.Count}, SFX {catalog.soundEffects.Count}, Sequence {catalog.sequenceTemplates.Count}, Standing {catalog.standingSprites.Count}");
        }

        private Conversation BuildConversation(int conversationId, string conversationName, List<StoryRow> storyRows)
        {
            // fields를 먼저 초기화한 후 ActorID와 ConversantID를 설정해야 함
            var conversation = new Conversation
            {
                id = conversationId,
                fields = new List<Field> { new Field("Title", conversationName, FieldType.Text) },
                dialogueEntries = new List<DialogueEntry>()
            };
            
            // fields가 초기화된 후 ActorID와 ConversantID 설정
            conversation.ActorID = PlayerActorId;
            conversation.ConversantID = DetectPrimaryConversant(storyRows);
            conversation.overrideSettings.useOverrides = true;
            conversation.overrideSettings.overrideSubtitleSettings = true;
            conversation.overrideSettings.minSubtitleSeconds = 3f;
            conversation.overrideSettings.continueButton = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;
            conversation.overrideSettings.alwaysForceResponseMenu = false;
            conversation.overrideSettings.showNPCSubtitlesDuringLine = true;
            conversation.overrideSettings.showNPCSubtitlesWithResponses = true;
            conversation.overrideSettings.showPCSubtitlesDuringLine = true;
            conversation.overrideSettings.skipPCSubtitleAfterResponseMenu = false;
            conversation.overrideSettings.overrideInputSettings = true;

            var rootEntry = CreateDialogueEntry(conversation.id, RootEntryId, PlayerActorId, conversation.ConversantID);
            rootEntry.Title = "START";
            rootEntry.DialogueText = string.Empty;
            rootEntry.Sequence = string.Empty;
            rootEntry.isGroup = false;
            rootEntry.isRoot = true;
            conversation.dialogueEntries.Add(rootEntry);

            var nodeLookup = new Dictionary<string, DialogueEntry>(StringComparer.OrdinalIgnoreCase);
            var rowEntries = new List<(StoryRow row, DialogueEntry entry)>();
            var nextEntryId = 1;
            var lastNonPlayerSpeaker = conversation.ConversantID;
            var lastActorId = NarratorActorId; // 기본값: 나레이션

            foreach (var row in storyRows)
            {
                if (!row.HasMeaningfulData())
                {
                    continue;
                }

                int actorId;
                bool isActorEmpty = string.IsNullOrWhiteSpace(row.ActorRaw);
                
                if (isActorEmpty)
                {
                    // Actor가 비어있으면 이전 Actor를 유지
                    actorId = lastActorId;
                }
                else if (!TryResolveActor(row.ActorRaw, out actorId))
                {
                    logLines.Add($"[WARN] Actor ID '{row.ActorRaw}'를 숫자로 변환할 수 없어 이전 Actor를 사용합니다. (행 {row.RowNumber})");
                    actorId = lastActorId;
                }

                var isChoice = actorId == 99;
                if (isChoice)
                {
                    actorId = PlayerActorId;
                }

                // Actor ID를 업데이트 (선택지가 아닌 경우에만)
                if (!isChoice)
                {
                    lastActorId = actorId;
                }

                var conversantId = DetermineConversant(actorId, lastNonPlayerSpeaker);
                if (!isChoice && actorId != PlayerActorId && actorId != NarratorActorId)
                {
                    lastNonPlayerSpeaker = actorId;
                }

                var entry = CreateDialogueEntry(conversation.id, nextEntryId++, actorId, conversantId);
                entry.DialogueText = row.DialogueText;
                entry.MenuText = isChoice ? row.DialogueText : string.Empty;
                entry.Sequence = BuildSequence(row);
                entry.fields = entry.fields ?? new List<Field>();

                // Background 필드 추가 (VN Framework HandleBackgroundFields용)
                if (!string.IsNullOrEmpty(row.BackgroundId))
                {
                    entry.fields.Add(new Field("Background", row.BackgroundId, FieldType.Text));
                }

                if (!string.IsNullOrEmpty(row.Expression))
                {
                    entry.fields.Add(new Field("Expression", row.Expression, FieldType.Text));
                }

                if (!string.IsNullOrEmpty(row.AutoProgressLocked))
                {
                    entry.fields.Add(new Field("AutoProgressLocked", row.AutoProgressLocked, FieldType.Text));
                }

                var nodeKey = !string.IsNullOrEmpty(row.NodeId)
                    ? row.NodeId
                    : $"__row_{row.RowNumber}";

                if (!nodeLookup.ContainsKey(nodeKey))
                {
                    nodeLookup.Add(nodeKey, entry);
                }

                conversation.dialogueEntries.Add(entry);
                rowEntries.Add((row, entry));
            }

            var crossConversationLinks = new List<(DialogueEntry entry, string targetKey, int rowNumber)>();
            LinkEntries(conversation, rowEntries, nodeLookup, crossConversationLinks);

            if (rowEntries.Count > 0)
            {
                AddLink(rootEntry, rowEntries[0].entry);
            }

            // Ensure only one START entry (the root) remains.
            conversation.dialogueEntries.RemoveAll(entry => entry != null && entry.id != RootEntryId && string.Equals(entry.Title, "START"));
            conversation.dialogueEntries.RemoveAll(entry => entry != null && entry.id != RootEntryId && string.Equals(entry.DialogueText, "START", System.StringComparison.Ordinal));

            logLines.Add($"[INFO] Conversation '{conversation.fields.FirstOrDefault(f => f.title == "Title")?.value}'에 {rowEntries.Count}개의 DialogueEntry를 생성했습니다.");
            
            // Cross-Conversation Links를 나중에 처리하기 위해 저장
            foreach (var crossLink in crossConversationLinks)
            {
                pendingCrossConversationLinks.Add((conversationName, crossLink.entry, crossLink.targetKey, crossLink.rowNumber));
            }
            
            return conversation;
        }

        private static int DetectPrimaryConversant(List<StoryRow> rows)
        {
            foreach (var row in rows)
            {
                if (TryResolveActor(row.ActorRaw, out var actorId) && actorId != PlayerActorId && actorId != NarratorActorId && actorId != 99)
                {
                    return actorId;
                }
            }

            return 2; // 기본값: 로아
        }

        private static DialogueEntry CreateDialogueEntry(int conversationId, int entryId, int actorId, int conversantId)
        {
            // fields를 먼저 초기화한 후 ActorID와 ConversantID를 설정해야 함
            var entry = new DialogueEntry
            {
                id = entryId,
                conversationID = conversationId,
                fields = new List<Field>(),
                outgoingLinks = new List<Link>()
            };
            
            // fields가 초기화된 후 ActorID와 ConversantID 설정
            entry.ActorID = actorId;
            entry.ConversantID = conversantId;
            
            return entry;
        }

        private static int DetermineConversant(int actorId, int lastNonPlayerSpeaker)
        {
            if (actorId == PlayerActorId || actorId == NarratorActorId)
            {
                return lastNonPlayerSpeaker != 0 ? lastNonPlayerSpeaker : 2;
            }

            return PlayerActorId;
        }

        private static string BuildSequence(StoryRow row)
        {
            var commands = new List<string>();
            
            // 배경 전환
            if (!string.IsNullOrEmpty(row.BackgroundId))
            {
                commands.Add($"ChangeBG({row.BackgroundId});");
            }

            // Standing 명령 추가
            if (!string.IsNullOrEmpty(row.StandingLeft))
            {
                if (row.StandingLeft.Equals("hide", System.StringComparison.OrdinalIgnoreCase))
                {
                    commands.Add("HideStanding(Left);");
                }
                else
                {
                    var parts = row.StandingLeft.Split('_');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var actorId))
                    {
                        commands.Add($"ShowStanding(Left, {actorId}, {parts[1]});");
                    }
                }
            }

            if (!string.IsNullOrEmpty(row.StandingCenter))
            {
                if (row.StandingCenter.Equals("hide", System.StringComparison.OrdinalIgnoreCase))
                {
                    commands.Add("HideStanding(Center);");
                }
                else
                {
                    var parts = row.StandingCenter.Split('_');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var actorId))
                    {
                        commands.Add($"ShowStanding(Center, {actorId}, {parts[1]});");
                    }
                }
            }

            if (!string.IsNullOrEmpty(row.StandingRight))
            {
                if (row.StandingRight.Equals("hide", System.StringComparison.OrdinalIgnoreCase))
                {
                    commands.Add("HideStanding(Right);");
                }
                else
                {
                    var parts = row.StandingRight.Split('_');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var actorId))
                    {
                        commands.Add($"ShowStanding(Right, {actorId}, {parts[1]});");
                    }
                }
            }

            // BGM 재생
            if (!string.IsNullOrEmpty(row.BgmId))
            {
                commands.Add($"PlaySound({row.BgmId});");
            }

            // SFX 재생
            if (!string.IsNullOrEmpty(row.SfxId))
            {
                commands.Add($"PlaySound({row.SfxId});");
            }

            // 추가 Sequence 명령
            if (!string.IsNullOrEmpty(row.SequenceToken))
            {
                Debug.Log($"[DSU_Importer] Row {row.RowNumber}: SequenceToken = '{row.SequenceToken}'");
                commands.Add(row.SequenceToken);
            }

            var finalSequence = string.Join("\n", commands.Where(c => !string.IsNullOrWhiteSpace(c)));
            if (!string.IsNullOrEmpty(finalSequence))
            {
                Debug.Log($"[DSU_Importer] Row {row.RowNumber}: Final Sequence:\n{finalSequence}");
            }
            return finalSequence;
        }

        private static void LinkEntries(
            Conversation conversation,
            List<(StoryRow row, DialogueEntry entry)> entries,
            Dictionary<string, DialogueEntry> nodeLookup,
            List<(DialogueEntry entry, string targetKey, int rowNumber)> crossConversationLinks)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var (row, entry) = entries[i];
                var isChoice = TryResolveActor(row.ActorRaw, out var actorId) && actorId == 99;

                if (!string.IsNullOrEmpty(row.LinkToId))
                {
                    foreach (var targetKey in SplitLinkTargets(row.LinkToId))
                    {
                        // Cross-Conversation Link 감지: "ConversationName:NodeID" 형식
                        if (targetKey.Contains(':'))
                        {
                            crossConversationLinks.Add((entry, targetKey, row.RowNumber));
                            continue;
                        }

                        // 같은 Conversation 내 Link
                        if (nodeLookup.TryGetValue(targetKey, out var targetEntry))
                        {
                            AddLink(entry, targetEntry);
                        }
                        else
                        {
                            Debug.LogWarning($"[LoveAlgo DSU Importer] Link 대상 '{targetKey}'를 찾을 수 없습니다. (행 {row.RowNumber})");
                        }
                    }
                }
                else if (i + 1 < entries.Count && !entries[i + 1].row.IsChoiceRow())
                {
                    AddLink(entry, entries[i + 1].entry);
                }

                if (!isChoice && i + 1 < entries.Count && entries[i + 1].row.IsChoiceRow())
                {
                    var offset = 1;
                    while (i + offset < entries.Count && entries[i + offset].row.IsChoiceRow())
                    {
                        AddLink(entry, entries[i + offset].entry);
                        offset++;
                    }
                }
            }
        }

        private static IEnumerable<string> SplitLinkTargets(string linkValue)
        {
            return linkValue
                .Split(new[] { '|', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim());
        }

        private static void AddLink(DialogueEntry source, DialogueEntry destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            if (source.outgoingLinks == null)
            {
                source.outgoingLinks = new List<Link>();
            }

            if (source.outgoingLinks.Any(l => l.destinationDialogueID == destination.id && l.destinationConversationID == destination.conversationID))
            {
                return;
            }

            var link = new Link
            {
                originConversationID = source.conversationID,
                originDialogueID = source.id,
                destinationConversationID = destination.conversationID,
                destinationDialogueID = destination.id
            };
            source.outgoingLinks.Add(link);
        }

        private void ProcessCrossConversationLinks(DialogueDatabase db)
        {
            if (pendingCrossConversationLinks.Count == 0)
            {
                return;
            }

            logLines.Add($"[INFO] {pendingCrossConversationLinks.Count}개의 Cross-Conversation Link 처리 시작");

            foreach (var (sourceConvName, sourceEntry, targetKey, rowNumber) in pendingCrossConversationLinks)
            {
                // targetKey 형식: "ConversationName:NodeID"
                var parts = targetKey.Split(':');
                if (parts.Length != 2)
                {
                    logLines.Add($"[WARN] 잘못된 Cross-Conversation Link 형식: '{targetKey}' (행 {rowNumber})");
                    continue;
                }

                var targetConvName = parts[0].Trim();
                var targetNodeId = parts[1].Trim();

                // 대상 Conversation 찾기
                var targetConv = db.conversations?.FirstOrDefault(c => 
                    string.Equals(c.Title, targetConvName, StringComparison.OrdinalIgnoreCase));

                if (targetConv == null)
                {
                    logLines.Add($"[ERROR] Cross-Conversation Link: Conversation '{targetConvName}'를 찾을 수 없습니다. (행 {rowNumber})");
                    continue;
                }

                // 대상 DialogueEntry 찾기 (NodeID 또는 "start"는 root entry)
                DialogueEntry? targetEntry = null;
                
                if (string.Equals(targetNodeId, "start", StringComparison.OrdinalIgnoreCase))
                {
                    targetEntry = targetConv.dialogueEntries.FirstOrDefault(e => e.id == RootEntryId);
                }
                else
                {
                    // NodeID가 명시적으로 지정된 경우
                    targetEntry = targetConv.dialogueEntries.FirstOrDefault(e => 
                        string.Equals(e.Title, targetNodeId, StringComparison.OrdinalIgnoreCase) ||
                        e.id.ToString() == targetNodeId);
                }

                if (targetEntry == null)
                {
                    logLines.Add($"[ERROR] Cross-Conversation Link: Conversation '{targetConvName}'에서 NodeID '{targetNodeId}'를 찾을 수 없습니다. (행 {rowNumber})");
                    continue;
                }

                // Cross-Conversation Link 생성
                AddLink(sourceEntry, targetEntry);
                logLines.Add($"[OK] Cross-Conversation Link 생성: {sourceConvName} → {targetConvName}:{targetNodeId}");
            }

            // 처리 완료 후 리스트 초기화
            pendingCrossConversationLinks.Clear();
        }

        private static List<string[]> LoadCsv(string path)
        {
            var records = new List<string[]>();
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                string? line;
                while ((line = ReadCsvRecord(reader)) != null)
                {
                    records.Add(ParseCsvLine(line).ToArray());
                }
            }

            return records;
        }

        private static string? ReadCsvRecord(StreamReader reader)
        {
            var sb = new StringBuilder();
            var inQuotes = false;
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                sb.Append(line);

                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i] != '"')
                    {
                        continue;
                    }

                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    break;
                }
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        values.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            values.Add(sb.ToString());
            return values;
        }

        private static bool TryResolveActor(string actorRaw, out int actorId)
        {
            return int.TryParse(actorRaw, out actorId);
        }

        private class StoryRow
        {
            public int RowNumber;
            public string ActorRaw = string.Empty;
            public string DialogueText = string.Empty;
            public string NodeId = string.Empty;
            public string LinkToId = string.Empty;
            public string ChoiceGroup = string.Empty;
            public string BackgroundId = string.Empty;
            public string BgmId = string.Empty;
            public string SfxId = string.Empty;
            public string Expression = string.Empty;
            public string SequenceToken = string.Empty;
            public string AutoProgressLocked = string.Empty;
            public string StandingLeft = string.Empty;
            public string StandingCenter = string.Empty;
            public string StandingRight = string.Empty;

            public bool IsChoiceRow()
            {
                return TryResolveActor(ActorRaw, out var actorId) && actorId == 99;
            }

            public bool HasMeaningfulData()
            {
                return !string.IsNullOrWhiteSpace(DialogueText) ||
                       !string.IsNullOrWhiteSpace(NodeId) ||
                       !string.IsNullOrWhiteSpace(LinkToId) ||
                       !string.IsNullOrWhiteSpace(BackgroundId) ||
                       !string.IsNullOrWhiteSpace(BgmId) ||
                       !string.IsNullOrWhiteSpace(SfxId) ||
                       !string.IsNullOrWhiteSpace(SequenceToken) ||
                       !string.IsNullOrWhiteSpace(ActorRaw);
            }
        }
    }
}