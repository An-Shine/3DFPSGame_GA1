# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 사용자 지침 우선

- 작업을 시작할 때 루트 `AGENTS.md`의 사용자 작업 지침을 읽고 함께 적용한다.
- 시스템과 실행 환경의 상위 지침을 준수하는 범위에서, 사용자가 지정한 작업 범위·설명 방식·코드 스타일을 Ponytail 및 모든 외부 스킬·플러그인의 기본 지침보다 우선한다.
- 스킬 사용 자체를 파일 수정·커밋·푸시의 허가로 해석하지 않는다. "확인해봐" 요청은 읽기와 원인 분석만 수행한다.
- 간결한 답변이나 한 줄 코드 권장 때문에 사용자가 요청한 단계별 설명, 한국어 주석, 코드 스타일을 생략하지 않는다.
- 사용자가 이번 요청에서 명시적으로 변경한 사항이 있으면 그 요청을 따른다.

## 프로젝트 개요

Unity **6000.3.22f1** (URP 17.3) 기반 3D FPS 학습용 프로젝트. 게임 코드는 모두 `Assets/02.Scripts/`에 있으며 별도 asmdef 없이 `Assembly-CSharp`로 컴파일된다. 씬은 `Assets/01.Scenes/GameScene.unity` 하나뿐이다.

## 빌드 / 실행 / 테스트

- 빌드·실행은 Unity 에디터에서 `GameScene`을 열고 Play한다. CLI 빌드 스크립트나 lint 설정은 없다.
- 에디터 밖에서 컴파일 오류만 빠르게 확인하려면: `dotnet build Assembly-CSharp.csproj` (csproj는 Unity가 생성하므로 에디터를 한 번 열어 둔 상태여야 한다).
- `com.unity.test-framework`가 설치돼 있지만 실제 테스트 코드는 없다. `Assets/02.Scripts/Test/`의 파일들은 C# 문법 연습용 스크립트이며 자동화 테스트가 아니다.
- `.unity`/`.prefab` 파일은 YAML이라 직접 편집이 가능하지만, 인스펙터 참조(fileID/guid)를 깨뜨리기 쉬우므로 가능하면 코드 변경으로 해결한다. 새 `.cs` 파일을 만들면 Unity가 `.meta`를 생성하므로 함께 커밋해야 한다.

## 아키텍처

### 게임 상태 게이트
`GameManager`(싱글톤, `GameManager.Instance`)가 `GameState`(`Ready` → `Playing` → `GameOver`)를 관리한다. 시작 시 코루틴으로 "준비... / 시작!" 연출 후 `Playing`으로 전환하고, `Player` 체력이 0이 되면 `GameOver()`를 호출한다.
**입력·AI를 처리하는 모든 `Update()`는 첫 줄에서 `if (GameManager.Instance.State != GameState.Playing) return;`으로 막는다.** 새 게임플레이 스크립트도 이 패턴을 따라야 한다.

### 데미지 시스템
`Core/IDamageable` + `Core/DamageInfo`(값, 피격 지점, 법선) 구조체로 통일돼 있다. 공격 측은 `collider.GetComponent<IDamageable>()?.TakeDamage(info)` 형태로 호출하고, `Player`, `Zombie`, `Box`, `Drum` 등이 이를 구현한다. 새로운 피격 대상은 이 인터페이스를 구현하면 총/폭탄/좀비 공격과 자동으로 연동된다.

### 플레이어
기능별로 컴포넌트가 분리돼 있다: `PlayerMove`(CharacterController 이동·점프·스프린트·스태미나), `PlayerRotate`, `PlayerGunFire`(카메라 정면 레이캐스트 히트스캔, 탄창/재장전 코루틴), `PlayerBombFire`(Fire2 차지 투척), `Player`(체력·피격). UI(`UI_PlayerStats`, `UI_HitEffect`)는 이 컴포넌트들의 public 프로퍼티를 읽어 표시한다.

### 좀비 AI
`Monster/Zombie.cs`는 `switch` 기반 FSM(`Idle`/`Patrol`/`Trace`/`Comeback`/`Attack`/`Hit`/`Death`)이다. 각 상태 메서드는 `_moveDirection`만 설정하고, 실제 이동은 `Update()` 끝에서 `CharacterController.SimpleMove`로 한 번만 처리한다. 상태 전환은 반드시 `ChangeState()`를 거쳐야 타이머가 초기화된다. NavMesh 패키지는 설치돼 있지만 사용하지 않고, 순찰 경로는 `CapsuleCast`/`Raycast`로 직접 검사한다.

### 입력 / 커서
입력은 **구 Input Manager**(`Input.GetAxis`, `Input.GetButton("Fire1")` 등)를 사용한다. `InputSystem_Actions.inputactions`는 템플릿 기본 파일일 뿐 코드에서 쓰이지 않는다.
`CursorManager`는 왼쪽 Alt를 누르는 동안 커서를 해제하고, `PlayerGunFire`는 `Cursor.visible`이 true이면 발사하지 않는다(미니맵 UI 버튼 클릭 등을 위해).

### 레이어
레이어 번호가 코드에 하드코딩돼 있다(예: `Zombie`의 `_groundMask = 1 << 8`, `_obstacleMask`). 현재 매핑: 6 Bomb, 7 Player, 8 Ground, 9 Obstacle, 10 Drum, 11 Zombie, 12 Box, 13 Minimap. Tags & Layers 설정을 바꾸면 이 값들도 함께 확인해야 한다.

### 로깅
`Game/GameLogger.Log`는 `[Conditional("ENBLE_LOG")]`로 선언돼 있지만 프로젝트의 Scripting Define Symbol은 `ENABLE_LOG`이다(철자 불일치). 따라서 현재 `GameLogger.Log` 호출은 컴파일에서 제거된다. 대부분의 코드는 `Debug.Log`를 직접 사용한다.

## 코드 컨벤션

- private 필드는 `_camelCase`, 인스펙터 노출은 `[SerializeField] private`, 외부 노출은 `=>` 읽기 전용 프로퍼티.
- 코루틴 메서드명은 `XXX_Coroutine`, UI 스크립트는 `UI_` 접두사, UI 버튼 콜백은 `OnClickXXX`.
- 주석은 한국어로 작성한다.
- 설치된 스킬·플러그인(`modern-csharp-coding-standards`, `ponytail` 등)이나 Claude의 기본 스타일과 이 프로젝트의 컨벤션이 충돌하면, 항상 이 문서의 프로젝트 컨벤션을 따른다.

## 작업 지침

- 답변은 한국어로 작성한다.
- 요청받지 않았다면 코드를 직접 변경하거나 커밋하지 않는다. 요청과 관련 없는 파일은 수정하지 않는다.
- 실행 결과를 확인하지 않았다면 실행했다고 표현하지 않는다 (Unity 에디터 Play는 직접 확인할 수 없음).
- 코드 리뷰 시에는 `Docs/CodeReviewGuide.md`의 기준(Critical/High/Medium/Low 중요도, 변경과 무관한 기존 문제는 참고사항으로 분리, 과도한 추상화 제안 지양)을 따른다.
