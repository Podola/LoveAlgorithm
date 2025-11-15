# LoveAlgo Runtime Skeleton

이 폴더는 레거시 스크립트와 분리된 러브알고리즘 신규 런타임 구조를 위한 공간입니다. 모든 클래스는 `LoveAlgo.*` 네임스페이스를 사용하며, 핵심 목표는 **간결한 책임 분리**와 **서비스 간 낮은 결합도**입니다.

## 폴더 가이드

  - 부트스트랩 및 컨텍스트 초기화 스크립트
  - 런타임 전역 상태를 통제하는 경량 진입점만 존재
  - 스탯, 호감도, 아이템, 일정 등 게임 로직을 담은 POCO 서비스
  - Unity 의존성을 최소화하고, 이벤트/콜백 기반으로 상호작용
  - ScriptableObject 정의(히로인, 자유행동, 선물 계층, 일정 등)
  - 에디터에서만 수정되며 런타임에는 읽기 전용
  - 공용 UI 조정기(포털), 모드 전환 시 UI 토글을 담당하는 스크립트
- `Backgrounds/`
  - `BackgroundCatalog`에 등록할 배경 정의
- `Standing/`
  - `StandingPoseCatalog` 등 스탠딩 자산 정의

## 기본 네임스페이스 규칙

| 폴더 | 네임스페이스 접두어 |
| --- | --- |
| Core | `LoveAlgo.Core` |
| Services | `LoveAlgo.Services` |
| Data | `LoveAlgo.Data` |
| UI | `LoveAlgo.UI` |

## 의존 관계

```
Data (SO)
   ↑          ↑
Services  ←  Core (Context, Mode)
   ↓          ↓
Dialogue Bridge ↔ DSU
   ↓
UI/Scene
```

- **Data → Services**: 서비스는 ScriptableObject를 참조하되 수정하지 않습니다.
- **Services → Core**: Core는 서비스들을 수명 주기와 상태 머신 관점에서 조립합니다.
- **UI/DSU → Services**: 모든 외부 진입점은 서비스 API를 통해 게임 데이터를 바꿉니다.

## 최소 루프 예시

`Core/GameLoopSample.cs`는 DSU 또는 UI 버튼에서 다음 순서로 호출해 기본 흐름을 확인할 수 있는 참조 스크립트입니다.

1. 부트스트랩 → `EnterStory()` 호출: DSU 대사 시작
2. `<command=EnterFreeAction>` → `EnterFreeAction()` 호출: 자유행동 UI 노출
3. 자유행동 버튼 → `CompleteFreeAction()` 호출: 스탯 반영 및 낮/밤 전환
4. 하루 종료 시 `CompleteEvent()` → `AdvanceDay()` 실행, 다음 일정 검사

실제 제작에서는 각 단계에서 DSU Conversation 또는 UI 이벤트를 바인딩하여 동일한 순서를 유지하면 충분합니다.

## Dialogue UI 확장

- `UI/Dialogue/LoveAlgoDialogueView`는 DSU Conversation 이벤트를 받아 화자명/본문/배경/스탠딩을 업데이트하며, 선택적으로 타자기 효과를 제공합니다. 애니메이션을 건너뛰고 전체 문장을 즉시 노출하고 싶으면 Dialogue UI 버튼 `OnClick`에 `CompleteTypewriter()`를 바인딩하세요.
- `LoveAlgoBackgroundPresenter`는 `BackgroundCatalog`에서 Sprite를 찾아 더블 버퍼 페이드로 교체합니다.
- `LoveAlgoStandingPresenter`는 `StandingLayout` 필드를 파싱해 Left/Center/Right 슬롯을 갱신하고, `StandingFocus` 필드(슬롯명 또는 히로인 ID)를 통해 지정한 대상만 하이라이트하며 나머지는 자동으로 디밍합니다.
- DSU 대사 항목에 `Background`, `StandingLayout`, `StandingHide`, `StandingFocus` 필드를 추가해 연출을 제어하세요. 예시) `StandingLayout="Left=HaYeEun@smile;Right=SeoDaEun@shy"`, `StandingFocus="Left"`, `StandingHide="Right"`.
- 추가 연출이 필요할 때는 `Sequencer` 칸에 `LoveAlgoStandingShake(Left, 0.3, 18)` 형태로 호출해 해당 슬롯을 단순 쉐이크 애니메이션 시킬 수 있습니다. 기간/세기는 생략 시 기본값이 사용됩니다.
- 메뉴 `LoveAlgo/Create Dialogue Test Scene`을 실행하면 `TestDialogueScene`이 생성되며, LoveAlgo Context + Dialogue Manager + 커스텀 UI + 샘플 Conversation(배경/스탠딩/쉐이크 포함)이 자동으로 배치됩니다.

## 테스트 체크리스트

- `Tools/LoveAlgo/Build Test Scene` 메뉴로 자동 테스트 씬을 만든 뒤 Dialogue UI 프리팹에 `LoveAlgoDialogueView`, `LoveAlgoBackgroundPresenter`, `LoveAlgoStandingPresenter`를 연결하세요.
- 각 Presenter에 알맞은 `BackgroundCatalog`, `StandingPoseCatalog`를 지정하고 슬롯 이미지에 Sprite가 표시되는지 확인합니다.
- DSU Conversation 데이터에 `Background`, `StandingLayout`, `StandingHide`, `StandingFocus` 필드를 채워서 줄 단위로 배경/스탠딩/포커스가 전환되는지 검사합니다.
- `StandingFocus="None"` 또는 공백 상태에서도 화면이 즉시 원상복귀되는지, `CompleteTypewriter()`를 호출하면 텍스트가 즉시 표시되는지 확인합니다.
- `Sequencer` 란에 `LoveAlgoStandingShake(Left)` 등을 넣고 슬롯 쉐이크 연출이 실행되는지 검증합니다.
- `LoveAlgo/Create Dialogue Test Scene` 메뉴로 생성된 씬에선 `LoveAlgoDialogueTestDriver`가 자동으로 Conversation을 재생하므로, 버튼으로 Skip/Restart를 눌러 반복 검증할 수 있습니다.

## Scene & Prefab Layout

- **Root Scene (`Scenes/LoveAlgo/Root.unity`)**: `LoveAlgoBootstrapper`, 전역 서비스 오브젝트, `SceneLoader` 빈(Title/Gameplay additive 전환). 씬은 비주얼 자산 없이 최소 부트스트랩만 포함합니다.
- **Title Scene (`Scenes/LoveAlgo/Title.unity`)**: `TitleCanvas`(배경 이미지, Start/Continue/Config 버튼), `LoveAlgoTitleController`가 버튼 → SceneLoader 이벤트를 보내 Gameplay 로드. UI는 `UI/title/` 리소스를 기준으로 생성 예정.
- **Gameplay Scene (`Scenes/LoveAlgo/Gameplay.unity`)**: 기존 `TestGameLoopScene`을 확장하여 Story/Free/Event Canvas + HUD를 한 장면에 배치. `LoveAlgoDialogueUI` 프리팹과 상점/자유행동/스탯 HUD 프리팹이 모두 이 씬에서 참조됩니다.
- **필수 프리팹 작업물**
  - `Prefabs/LoveAlgo/Hud/StatsHud.prefab`: DAY/MONEY/STAT 바를 묶은 `Canvas` (스크린샷 2 참고), `LoveAlgoStatsPresenter`로 게임 서비스와 연결
  - `Prefabs/LoveAlgo/Hud/ActivityMenu.prefab`: 알바/운동/공부/아이템 구매 네비게이션 + 세부 카드 리스트
  - `Prefabs/LoveAlgo/Title/TitleCanvas.prefab`: 타이틀용 전용 캔버스, Start 버튼은 `GameLoopSample.EnterStory()` 와 동일한 흐름을 호출하도록 설정
- **배치 원칙**: Root 씬만 Build Settings index 0, Title/Gameplay는 additively 로드. HUD/Activity 프리팹은 Gameplay 씬에서만 Instantiate, Dialogue UI는 스토리 모드 전환 시 `UiPortal`을 통해 활성화합니다.

이 문서는 신규 스크립트 추가 시 검토해야 할 최소 규칙을 설명합니다.
