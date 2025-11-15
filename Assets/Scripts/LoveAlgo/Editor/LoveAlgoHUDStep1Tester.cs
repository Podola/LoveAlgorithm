#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using LoveAlgo.UI;
using LoveAlgo.UI.Modules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace LoveAlgo.Editor
{
    /// <summary>
    /// Runs automated checks that validate the Step1 HUD skeleton (HUD root + FreeAction module wiring).
    /// Ensures prefabs exist, instantiate correctly, and expose the expected references.
    /// </summary>
    public static class LoveAlgoHUDStep1Tester
    {
        private const string HudPrefabPath = "Assets/Prefabs/Simple/HUD/LoveAlgoHUDRoot.prefab";

        [MenuItem("LoveAlgo/Run Step1 HUD Test", priority = 45)]
        public static void RunStep1HudTest()
        {
            var report = new Step1TestReport();
            try
            {
                report.AddInfo("Step1 HUD 테스트를 시작합니다.");
                LoveAlgoHUDScaffolder.BuildPrefabs();
                report.AddInfo("HUD 및 모듈 프리팹을 재생성했습니다.");

                var hudPrefab = AssetDatabase.LoadAssetAtPath<LoveAlgoHUDRoot>(HudPrefabPath);
                if (hudPrefab == null)
                {
                    report.AddFailure($"HUD 프리팹을 찾을 수 없습니다. 경로: {HudPrefabPath}");
                    return;
                }

                var previousActiveScene = SceneManager.GetActiveScene();
                var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(testScene);
                EnsureEventSystemExists(testScene);

                if (!(PrefabUtility.InstantiatePrefab(hudPrefab) is LoveAlgoHUDRoot hudInstance))
                {
                    report.AddFailure("HUD 프리팹 인스턴스화에 실패했습니다.");
                    CleanupScene(testScene);
                    EditorSceneManager.CloseScene(testScene, true);
                    SceneManager.SetActiveScene(previousActiveScene);
                    return;
                }

                hudInstance.name = "HUD_TestInstance";
                report.AddInfo("HUD 프리팹 인스턴스화 성공.");

                ValidateHudHierarchy(hudInstance, report);

                UnityEngine.Object.DestroyImmediate(hudInstance.gameObject);
                CleanupScene(testScene);
                EditorSceneManager.CloseScene(testScene, true);
                SceneManager.SetActiveScene(previousActiveScene);
                report.AddInfo("임시 씬을 정리했습니다.");
            }
            catch (Exception ex)
            {
                report.AddFailure("예외가 발생했습니다: " + ex.Message);
                report.AddInfo(ex.StackTrace ?? string.Empty);
            }
            finally
            {
                report.PushResult();
            }
        }

        private static void ValidateHudHierarchy(LoveAlgoHUDRoot hudInstance, Step1TestReport report)
        {
            var nav = hudInstance.GetComponentInChildren<UINavigationController>();
            if (nav == null)
            {
                report.AddFailure("UINavigationController 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            report.AddInfo("NavigationController가 배치되어 있습니다.");

            hudInstance.ShowFreeAction();
            var activeModule = nav.ActiveModule;
            if (activeModule == null)
            {
                report.AddFailure("ShowFreeAction 호출 후 활성 모듈이 생성되지 않았습니다.");
                return;
            }

            report.AddInfo($"활성 모듈 타입: {activeModule.GetType().Name}");
            if (activeModule is FreeActionPanelController controller)
            {
                ValidateFreeActionController(controller, report);
            }
            else
            {
                report.AddFailure("FreeActionPanelController가 활성화되지 않았습니다.");
            }

            hudInstance.HideActiveModule();
        }

        private static void ValidateFreeActionController(FreeActionPanelController controller, Step1TestReport report)
        {
            var serialized = new SerializedObject(controller);
            ValidateReference(serialized, "exerciseButton", "운동 버튼", report);
            ValidateReference(serialized, "studyButton", "공부 버튼", report);
            ValidateReference(serialized, "convenienceButton", "편의점 알바 버튼", report);
            ValidateReference(serialized, "warehouseButton", "상하차 알바 버튼", report);
            ValidateReference(serialized, "investmentButton", "투자 버튼", report);
            ValidateReference(serialized, "openShopButton", "상점 이동 버튼", report);
            ValidateReference(serialized, "titleLabel", "제목 라벨", report);
            ValidateReference(serialized, "summaryLabel", "요약 라벨", report);
            ValidateReference(serialized, "expectedResultLabel", "결과 라벨", report);
            ValidateReference(serialized, "popupRoot", "확인 팝업", report);
            ValidateReference(serialized, "popupTitleLabel", "팝업 제목", report);
            ValidateReference(serialized, "popupBodyLabel", "팝업 본문", report);
            ValidateReference(serialized, "popupConfirmButton", "팝업 확인 버튼", report);
            ValidateReference(serialized, "popupCancelButton", "팝업 취소 버튼", report);

            var optionDefinitions = serialized.FindProperty("optionDefinitions");
            if (optionDefinitions == null || optionDefinitions.arraySize < 6)
            {
                report.AddFailure("자유행동 옵션 정의가 최소 6개 이상 등록되지 않았습니다.");
            }
            else
            {
                report.AddInfo($"옵션 정의 {optionDefinitions.arraySize}개 확인.");
            }
        }

        private static void ValidateReference(SerializedObject serialized, string propertyName, string label, Step1TestReport report)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                report.AddFailure($"SerializedProperty를 찾을 수 없습니다: {propertyName}");
                return;
            }

            if (property.objectReferenceValue == null)
            {
                report.AddFailure($"{label} 참조가 비어 있습니다.");
            }
            else
            {
                report.AddInfo($"{label} 연결 확인.");
            }
        }

        private static void EnsureEventSystemExists(Scene targetScene)
        {
            foreach (var root in targetScene.GetRootGameObjects())
            {
                if (root.GetComponent<EventSystem>() != null)
                {
                    return;
                }
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, targetScene);
        }

        private static void CleanupScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private sealed class Step1TestReport
        {
            private readonly List<string> infos = new();
            private readonly List<string> failures = new();

            public void AddInfo(string message)
            {
                infos.Add("[INFO] " + message);
                Debug.Log("[Step1HUD] " + message);
            }

            public void AddFailure(string message)
            {
                failures.Add("[ERROR] " + message);
                Debug.LogError("[Step1HUD] " + message);
            }

            public void PushResult()
            {
                var builder = new StringBuilder();
                foreach (var info in infos)
                {
                    builder.AppendLine(info);
                }

                foreach (var failure in failures)
                {
                    builder.AppendLine(failure);
                }

                var summary = builder.ToString();
                if (failures.Count == 0)
                {
                    EditorUtility.DisplayDialog("Step1 HUD Test", "모든 검증을 통과했습니다.\n\n" + summary, "확인");
                }
                else
                {
                    EditorUtility.DisplayDialog("Step1 HUD Test", "일부 검증에서 실패했습니다.\n\n" + summary, "확인");
                }
            }
        }
    }
}
#endif
