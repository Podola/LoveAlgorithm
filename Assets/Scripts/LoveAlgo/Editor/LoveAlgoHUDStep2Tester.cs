#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Runs automated checks that validate the Step2 HUD scope (Shop module prefab, navigation, and serialized references).
    /// </summary>
    public static class LoveAlgoHUDStep2Tester
    {
        private const string HudPrefabPath = "Assets/Prefabs/Simple/HUD/LoveAlgoHUDRoot.prefab";

        [MenuItem("LoveAlgo/Run Step2 HUD Test", priority = 46)]
        public static void RunStep2HudTest()
        {
            var report = new Step2TestReport();
            try
            {
                report.AddInfo("Step2 HUD 테스트를 시작합니다.");
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

                hudInstance.name = "HUD_Step2_TestInstance";
                report.AddInfo("HUD 프리팹 인스턴스화 성공.");

                ValidateShopNavigation(hudInstance, report);

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

        private static void ValidateShopNavigation(LoveAlgoHUDRoot hudInstance, Step2TestReport report)
        {
            var nav = hudInstance.GetComponentInChildren<UINavigationController>();
            if (nav == null)
            {
                report.AddFailure("UINavigationController 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            report.AddInfo("NavigationController 연결 확인.");

            hudInstance.ShowShop();
            if (nav.ActiveModule == null)
            {
                report.AddFailure("ShowShop 호출 후 활성 모듈이 생성되지 않았습니다.");
                return;
            }

            report.AddInfo($"Shop 모듈 활성화: {nav.ActiveModule.GetType().Name}");
            if (nav.ActiveModule is ShopPanelController controller)
            {
                ValidateShopController(controller, report);
            }
            else
            {
                report.AddFailure("ShopPanelController가 활성화되지 않았습니다.");
            }

            hudInstance.HideActiveModule();
        }

        private static void ValidateShopController(ShopPanelController controller, Step2TestReport report)
        {
            var serialized = new SerializedObject(controller);
            ValidateReference(serialized, "moneyLabel", "보유 금액 라벨", report);
            ValidateReference(serialized, "totalLabel", "합계 금액 라벨", report);
            ValidateReference(serialized, "purchaseButton", "구매 버튼", report);
            ValidateReference(serialized, "exitButton", "돌아가기 버튼", report);
            ValidateReference(serialized, "gachaButton", "가챠 버튼", report);
            ValidateReference(serialized, "itemGrid", "상품 그리드", report);
            ValidateReference(serialized, "itemCardPrototype", "상품 카드 프로토타입", report);
            ValidateReference(serialized, "cartList", "장바구니 리스트", report);
            ValidateReference(serialized, "cartEntryPrototype", "장바구니 항목 프로토타입", report);
            ValidateReference(serialized, "tooltipPanel", "툴팁 패널", report);

            var inventoryProperty = serialized.FindProperty("mockInventory");
            if (inventoryProperty == null || inventoryProperty.arraySize == 0)
            {
                report.AddFailure("Mock 인벤토리 데이터가 비어 있습니다.");
            }
            else
            {
                report.AddInfo($"Mock 인벤토리 {inventoryProperty.arraySize}개 확인.");
            }

            var tooltipPanel = serialized.FindProperty("tooltipPanel").objectReferenceValue as ShopTooltipPanel;
            if (tooltipPanel != null)
            {
                ValidateTooltipPanel(tooltipPanel, report);
            }

            var cartEntry = serialized.FindProperty("cartEntryPrototype").objectReferenceValue as ShopCartEntryView;
            if (cartEntry != null)
            {
                ValidateCartEntryPrototype(cartEntry, report);
            }

            var itemCard = serialized.FindProperty("itemCardPrototype").objectReferenceValue as ShopItemCard;
            if (itemCard != null)
            {
                ValidateItemCardPrototype(itemCard, report);
            }
        }

        private static void ValidateTooltipPanel(ShopTooltipPanel tooltip, Step2TestReport report)
        {
            var serialized = new SerializedObject(tooltip);
            ValidateReference(serialized, "root", "툴팁 루트", report);
            ValidateReference(serialized, "titleLabel", "툴팁 제목", report);
            ValidateReference(serialized, "bodyLabel", "툴팁 본문", report);
        }

        private static void ValidateCartEntryPrototype(ShopCartEntryView entryView, Step2TestReport report)
        {
            var serialized = new SerializedObject(entryView);
            ValidateReference(serialized, "nameLabel", "장바구니 이름 라벨", report);
            ValidateReference(serialized, "priceLabel", "장바구니 가격 라벨", report);
            ValidateReference(serialized, "quantityLabel", "장바구니 수량 라벨", report);
            ValidateReference(serialized, "incrementButton", "수량 증가 버튼", report);
            ValidateReference(serialized, "decrementButton", "수량 감소 버튼", report);
            ValidateReference(serialized, "removeButton", "삭제 버튼", report);
        }

        private static void ValidateItemCardPrototype(ShopItemCard itemCard, Step2TestReport report)
        {
            var serialized = new SerializedObject(itemCard);
            ValidateReference(serialized, "selectButton", "상품 선택 버튼", report);
            ValidateReference(serialized, "nameLabel", "상품 이름 라벨", report);
            ValidateReference(serialized, "priceLabel", "상품 가격 라벨", report);
            ValidateReference(serialized, "selectionIndicator", "선택 표시자", report);
        }

        private static void ValidateReference(SerializedObject serialized, string propertyName, string label, Step2TestReport report)
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

        private sealed class Step2TestReport
        {
            private readonly List<string> infos = new();
            private readonly List<string> failures = new();
            private readonly string reportPath;

            public Step2TestReport()
            {
                reportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Step2TestReport.txt"));
            }

            public void AddInfo(string message)
            {
                infos.Add("[INFO] " + message);
                Debug.Log("[Step2HUD] " + message);
            }

            public void AddFailure(string message)
            {
                failures.Add("[ERROR] " + message);
                Debug.LogError("[Step2HUD] " + message);
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
                try
                {
                    File.WriteAllText(reportPath, "Full dialog message :" + Environment.NewLine + Environment.NewLine + summary);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Step2HUD] 리포트 파일 저장 중 문제가 발생했습니다: " + ex.Message);
                }

                var title = "Step2 HUD Test";
                if (failures.Count == 0)
                {
                    EditorUtility.DisplayDialog(title, "모든 검증을 통과했습니다.\n\n" + summary, "확인");
                }
                else
                {
                    EditorUtility.DisplayDialog(title, "일부 검증에서 실패했습니다.\n\n" + summary, "확인");
                }
            }
        }
    }
}
#endif
