# 1인칭 던전 RPG(UI 버티컬 슬라이스) 적용 가이드

이 저장소는 **Unity + uGUI(Canvas)** 기준으로,
Steven 담당 범위인 **게임 상태 전환 + UI 패널 제어 + HUD 표시 레이어 + Pause 옵션 저장(PlayerPrefs)** 을 빠르게 붙일 수 있는 최소 뼈대입니다.

> ⚠️ 주의: 이동/전투/AI/스탯 계산/산소 감소/맵 탐험 로직은 포함하지 않았습니다.

---

## 포함된 스크립트

- `Assets/Scripts/Core/GameState.cs`
  - `Start`, `Playing`, `Paused`, `Dead`, `End` 상태 enum
- `Assets/Scripts/Core/GameStateManager.cs`
  - 상태 전환 중심 매니저
  - ESC 처리(Playing↔Paused)
  - NewGame/Resume/Retry/MainMenu/Quit, Debug Dead/End 메서드
- `Assets/Scripts/UI/CursorModeController.cs`
  - 상태에 따라 커서 잠금/해제
  - **사용자 액션으로 Playing 진입 시에만 잠금 허용**
- `Assets/Scripts/UI/UIRoot.cs`
  - 패널 활성/비활성 전환
  - 버튼 이벤트 연결(Start/Pause/Dead/End/Debug)
- `Assets/Scripts/UI/HUD/HUDController.cs`
  - HUD 표시 전용 API
  - `SetHealth`, `SetOxygen`, `SetCombatState`, `SetTurnState`, `SetPrompt`
- `Assets/Scripts/UI/Options/OptionsPanel.cs`
  - Pause 내 Options 열기/닫기
  - Master/BGM/SFX 슬라이더 값 `%` 텍스트 표시
  - `PlayerPrefs` 즉시 저장/로드

---

## Unity 씬 구성(클릭 순서)

아래 순서대로 설정하면 바로 동작 확인이 가능합니다.

### 1) 매니저 배치
1. 빈 오브젝트 `GameSystems` 생성
2. `GameStateManager` 컴포넌트 추가
3. `CursorModeController` 컴포넌트 추가
4. `CursorModeController > Game State Manager` 슬롯에 `GameSystems`의 `GameStateManager` 드래그

### 2) Canvas / UIRoot 구성
1. `Canvas` 생성(없으면)
2. `Canvas` 하위에 `UIRoot` 오브젝트 생성
3. `UIRoot`에 `UIRoot.cs` 컴포넌트 추가
4. `UIRoot > Game State Manager` 슬롯에 `GameSystems`의 `GameStateManager` 연결

### 3) 패널 생성 (Canvas/UIRoot 하위)
다음 오브젝트 이름으로 생성:

- `Panel_Start`
- `Panel_Pause`
- `Panel_Dead`
- `Panel_End`
- `Panel_HUD`
- `Panel_Debug`

그리고 `UIRoot` 컴포넌트의 각 Panel 슬롯에 드래그 연결:

- Panel Start → `Panel_Start`
- Panel Pause → `Panel_Pause`
- Panel Dead → `Panel_Dead`
- Panel End → `Panel_End`
- Panel Hud → `Panel_HUD`
- Panel Debug → `Panel_Debug`

### 4) 버튼 생성 및 연결

#### Start 패널
`Panel_Start` 하위:
- `Btn_NewGame`
- `Btn_Quit`

`UIRoot`의 Start Buttons 슬롯에 연결:
- Btn New Game → `Btn_NewGame`
- Btn Quit → `Btn_Quit`

#### Pause 패널
`Panel_Pause` 하위:
- `Btn_Resume`
- `Btn_MainMenu`
- `Btn_Quit`

`UIRoot`의 Pause Buttons 슬롯에 연결:
- Btn Resume → `Btn_Resume`
- Btn Main Menu Pause → `Btn_MainMenu`
- Btn Quit Pause → `Btn_Quit`

#### Dead 패널
`Panel_Dead` 하위:
- `Btn_Retry`
- `Btn_MainMenu`

`UIRoot`의 Dead Buttons 슬롯에 연결:
- Btn Retry → `Btn_Retry`
- Btn Main Menu Dead → `Btn_MainMenu`

#### End 패널
`Panel_End` 하위:
- `Btn_MainMenu`
- `Btn_NewGame`

`UIRoot`의 End Buttons 슬롯에 연결:
- Btn Main Menu End → `Btn_MainMenu`
- Btn New Game End → `Btn_NewGame`

#### Debug 패널
`Panel_Debug` 하위:
- `Btn_DebugKill`
- `Btn_DebugWin`

`UIRoot`의 Debug Buttons 슬롯에 연결:
- Btn Debug Kill → `Btn_DebugKill`
- Btn Debug Win → `Btn_DebugWin`

---

## HUD 연결 방법

1. `Panel_HUD`에 `HUDController` 컴포넌트 추가
2. `Panel_HUD` 하위에 다음 오브젝트 생성 및 연결

- `HealthBar` (Image, **Type=Filled** 권장)
- `Txt_Health` (Text)
- `OxygenBar` (Image, **Type=Filled** 권장)
- `Txt_Oxygen` (Text)
- `Txt_CombatState` (Text)
- `Txt_TurnState` (Text)
- `Txt_Prompt` (Text)

3. `HUDController` 슬롯에 각각 드래그

외부 시스템(Sami 코드)에서 나중에 아래 메서드 호출로 값 주입:

- `SetHealth(current, max)`
- `SetOxygen(current, max)`
- `SetCombatState(inCombat)`
- `SetTurnState(playerTurn)`
- `SetPrompt(text)`

---

## Pause > Options 연결 방법

### 1) 오브젝트 생성
`Panel_Pause` 하위에:
- `Btn_OptionsOpen`
- `Panel_Options` (기본 비활성 권장)

`Panel_Options` 하위에:
- `Btn_OptionsClose`
- `Sld_Master`, `Txt_MasterValue`
- `Sld_BGM`, `Txt_BGMValue`
- `Sld_SFX`, `Txt_SFXValue`

### 2) 스크립트 부착
- `OptionsPanel.cs`를 `Panel_Pause`(또는 별도 옵션 루트 오브젝트)에 추가

### 3) 인스펙터 연결
`OptionsPanel` 필드에 아래 매핑:
- Panel Options → `Panel_Options`
- Btn Options Open → `Btn_OptionsOpen`
- Btn Options Close → `Btn_OptionsClose`
- Sld Master/Bgm/Sfx → 각 슬라이더
- Txt Master/Bgm/Sfx Value → 각 값 텍스트

### 4) 저장 키
자동 저장/로드 키:
- `audio_master`
- `audio_bgm`
- `audio_sfx`

슬라이더를 움직이면 즉시 저장되고 `%` 텍스트가 즉시 갱신됩니다.

---

## 동작 검증 체크리스트

Play 모드에서 아래를 확인하세요:

1. 시작 시 `Start` 패널 보임
2. `Btn_NewGame` 클릭 시 `Playing` 진입
   - HUD + Debug 패널 활성
   - 커서 잠김/숨김
3. ESC 누르면 `Paused` 진입
   - Pause 패널 활성
   - 커서 해제/표시
4. ESC 한 번 더 누르면 `Playing` 복귀(Resume과 동일)
5. `Btn_DebugKill` 클릭 시 `Dead`
6. `Btn_DebugWin` 클릭 시 `End`
7. Pause에서 Options 열고 닫기 가능
8. 슬라이더 조절 시 `%` 값 갱신 + 재실행 후 값 유지(PlayerPrefs)

---

## 트러블슈팅

- 버튼 눌러도 반응이 없으면:
  - `UIRoot`의 버튼 참조 누락 여부 확인
  - `GameStateManager` 참조가 `UIRoot`, `CursorModeController`에 연결되었는지 확인
- HUD 바가 안 차면:
  - `HealthBar/OxygenBar` Image 타입이 Filled인지 확인
- 옵션 값이 저장 안 되면:
  - 플레이 중 슬라이더 이동 후 에디터 중지 전 값이 변경되었는지 확인

---

## 담당 범위 준수 메모

본 구현은 Steven 담당 항목(UI/HUD 표시/상태 전환/입력 라우팅/옵션 UI 저장)만 포함합니다.
Sami 및 Ryan 담당 로직/에셋 영역은 건드리지 않도록 구성했습니다.
