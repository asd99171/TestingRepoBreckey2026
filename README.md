# Gameplay-UI 통합 가이드 (Player/Map/Environment/Enemy 기반)

이 문서는 **기존 Debug 전용 흐름을 제거**하고, 현재 게임에 있는 실제 코드(`Player`, `Map Grid`, `Environment`, `Enemy`)로 UI가 동작하도록 붙이는 방법을 설명합니다.

## 이번 적용 내용 요약

- 디버그 UI 의존 제거
  - `UIRoot`에서 `Panel_Debug`, `Btn_DebugKill/Win` 연결 제거
  - `GameStateManager`의 `DebugSetDead/DebugSetEnd` 제거
  - `CombatLogController`의 디버그 버튼 로깅 함수 제거
- 실제 전투 로그 연동
  - `PlayerAttackCooldown`에서 실제 공격 성공 시
    - `CombatLogRuntimeBridge.ReportPlayerDamageToEnemy(damage)` 호출
    - CombatLog에 `Player does X damage to Enemy.` 출력
- HUD HP와 PlayerHealth 직접 연동
  - `HUDPlayerHealthBinder`를 reflection 기반에서 `PlayerHealth` 직접 바인딩으로 변경
  - `PlayerHealth`에 `MaxHealth`, `OnHealthChanged(int current, int max)` 추가
  - 데미지/회복/초기화 시 HUD 갱신
- Oxygen 관련 기능
  - 현재 코드베이스 내 Oxygen 기능 없음(추가 제거 사항 없음)

---

## 씬에서 바로 적용하는 방법

### 1) Panel_HUD에 HP 연동

1. `Panel_HUD` 오브젝트에 `HUDController`가 있는지 확인
2. 같은 오브젝트(또는 적절한 UI 루트)에 `HUDPlayerHealthBinder` 추가
3. 인스펙터 연결
   - `Hud Controller` → `HUDController` 컴포넌트
   - `Player Health` → 플레이어 오브젝트의 `PlayerHealth`

> `PlayerHealth`를 비워두면 런타임에 `FindFirstObjectByType<PlayerHealth>()`로 자동 탐색합니다.

### 2) CombatLog 전투 로그 연결

1. `Panel_CombatLog`에 `CombatLogController` 확인
2. 같은 오브젝트(또는 하위)에 `CombatLogRuntimeBridge` 추가
3. `Combat Log Controller` 슬롯에 `CombatLogController` 연결
4. `CombatLogController` 참조 설정
   - `Scroll Rect`
   - `Content Root`
   - `Log Entry Template` (비활성 템플릿 Text)

이제 플레이어가 좌클릭 공격에 성공하면 자동으로 로그가 누적됩니다.

### 3) UIRoot에서 Debug 슬롯 정리

`UIRoot` 인스펙터에서 아래 항목은 더 이상 사용하지 않습니다.

- `Panel Debug`
- `Btn Debug Kill`
- `Btn Debug Win`

(스크립트에서 필드 제거됨)



### 4) 미니맵을 "플레이어 주변 Grid 창" 방식으로 사용

`MiniMapController`는 이제 기본값으로 **플레이어 중심 사각 Grid 미니맵**을 지원합니다.

- `viewRadiusCells = 5` 이면 `(2*5+1)=11`칸 정사각형 창
- 즉, 플레이어 주변 약 10칸 범위를 사각형으로 보여줌
- 중앙 칸은 플레이어, 적이 있는 칸은 `enemyColor`, 바닥 없는 칸은 `blockedColor`로 표시

설정 방법:

1. `Panel_MiniMap`에 `MiniMapController` 추가
2. 인스펙터 연결
   - `Player` (비워도 자동 탐색)
   - `Grid Map`
   - `Grid Occupancy Index`
   - `Mini Map Area`
   - `Mini Map Content`
   - `Player Marker`, `Player Direction Arrow`
   - `Grid Cell Prefab` (UI `Image` 프리팹 1개)
3. 권장 값
   - `Use Grid Window Minimap` = On
   - `View Radius Cells` = `5` (원하면 `10`으로 확장 가능)
   - `Cell Pixel Size` = `12~16`
   - `Refresh Interval Seconds` = `0.1~0.2`

> 참고: 이 방식은 월드 전체 텍스처를 스크롤하는 방식이 아니라, 플레이어 주변 셀을 매 프레임/주기마다 샘플링해서 그려주는 방식입니다.

---

## 동작 체크리스트

- 게임 시작/일시정지/종료 UI 상태 전환이 정상 동작
- 공격이 적중하면 CombatLog에 `Player does X damage to Enemy.` 출력
- 플레이어 HP가 변하면 Panel_HUD의 HP 바/텍스트가 즉시 갱신

---

## 변경된 주요 스크립트

- `Assets/Scripts/Player/PlayerAttackCooldown.cs`
- `Assets/Scripts/Player/PlayerHealth.cs`
- `Assets/Scripts/UI/HUD/HUDPlayerHealthBinder.cs`
- `Assets/Scripts/UI/HUD/CombatLogController.cs`
- `Assets/Scripts/UI/UIRoot.cs`
- `Assets/Scripts/Core/GameStateManager.cs`
- `Assets/Scripts/UI/HUD/MiniMapController.cs`

