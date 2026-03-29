# 🏹 Archery

> **궁수의 전설 2** 모티브 로그라이크 아처 서바이벌 게임

Unity 6 (6000.0.62f1) 기반의 탑다운 슈터 로그라이크 게임.  
스테이지를 클리어하며 스킬을 선택·강화하고, 다양한 맵과 캐릭터를 해금해 나가는 구조.

---

## 🎬 플레이 영상

<video src="Recordings/Movie_005.mp4" controls width="100%"></video>

---

## �🎯 프로젝트 목표

| 구분 | 목표 |
|------|------|
| 🕹️ **게임 목표** | 하이퍼 캐주얼 액션 게임을 직접 만들어보고 싶었습니다 |
| ⚙️ **기술적 목표** | Live-Ops 시스템을 직접 설계·구현해보고 싶었습니다 |

---

## 📑 목차

- [기술 스택](#-기술-스택)
- [프로젝트 구조](#-프로젝트-구조)
- [아키텍처 개요](#-아키텍처-개요)
  - [레이어 구조](#-레이어-구조)
  - [매니저 시스템](#-매니저-시스템)
  - [EventBus](#-eventbus)
  - [MVP UI 패턴](#-mvp-ui-패턴)
  - [오브젝트 풀링 & Addressables](#-오브젝트-풀링--addressables)
  - [3계층 스탯 시스템](#-3계층-스탯-시스템)
  - [플레이어 모듈 시스템](#-플레이어-모듈-시스템)
  - [적 행동 시스템](#-적-행동-시스템)
  - [이펙트 시스템](#-이펙트-시스템)
- [게임 흐름](#-게임-흐름)
- [데이터 파이프라인](#-데이터-파이프라인)
- [사용 디자인 패턴 정리](#-사용-디자인-패턴-정리)

---

## 🛠 기술 스택

| 영역 | 기술 |
|------|------|
| 🎮 엔진 | Unity 6 (6000.0.62f1) |
| 💻 언어 | C# |
| ⚡ 비동기 | Unity `Awaitable` async/await |
| 📦 에셋 관리 | Unity Addressables (라벨 기반 로딩) |
| 📄 JSON 파싱 | Newtonsoft.Json |
| 🕹️ 입력 | Unity New Input System |
| ✏️ UI 텍스트 | TextMesh Pro |

---

## 📂 프로젝트 구조

```
Assets/Scripts/
├── Managers/                 # 싱글턴 매니저 (16개)
├── UI/
│   ├── Lobby/                # 로비 UI
│   │   └── CharacterSetting/ # 세팅 팝업 (MVP)
│   ├── InGame/               # 인게임 UI
│   │   ├── Game Over/        # 게임오버 팝업 + Presenter
│   │   ├── Map Clear/        # 맵 클리어 팝업 + Presenter
│   │   └── Skill/            # 스킬 선택 팝업 + Presenter
│   └── Loading/              # 로딩 UI
├── Players/
│   ├── Player Module/        # 핵심 플레이어 컴포넌트
│   └── Skill Module/         # 스킬 구현체 (8종)
├── Enemy/
│   └── Enemy Components/     # 적 행동 모듈 (FSM)
├── Map/                      # 맵 & 레벨 시스템
├── Camera/                   # 카메라 컨트롤러
├── Objects/                  # 투사체, 이펙트, 아이템
│   └── PlayerObjects/        # Barrel, Orb, Meteor
├── Stats/                    # 스탯 시스템
├── Effects/                  # 상태이상 핸들러
├── Utils/                    # 유틸리티 (EventBus 등)
└── Attributes/               # 커스텀 어트리뷰트

Assets/Resources/Data/        # JSON 데이터 (enemyData, mapData)
Assets/Scriptable Data/       # ScriptableObject 에셋
```

---

## 🏗 아키텍처 개요

### 🧱 레이어 구조

```
┌──────────────────────────────────────────────────────────┐
│                    UI Layer (MVP)                         │
│   View (MonoBehaviour)  ←→  Presenter (Plain C#)         │
├──────────────────────────────────────────────────────────┤
│                  Manager Layer (Singletons)               │
│   GameManager · MapManager · CharacterManager · ...      │
│          ↕ EventBus (Priority-based Pub/Sub) ↕           │
├──────────────────────────────────────────────────────────┤
│                   Gameplay Layer                          │
│   PlayerController  ─┬─ Movement · Attack · Hurt · Skill │
│   EnemyController   ─┴─ Idle · Move · Attack · Hurt · Die│
├──────────────────────────────────────────────────────────┤
│                    Data Layer                             │
│   DataManager (JSON)  ·  Addressables  ·  ScriptableObj  │
├──────────────────────────────────────────────────────────┤
│                   Infrastructure                         │
│   PoolManager · EventBus · InputManager · CameraCtrl     │
└──────────────────────────────────────────────────────────┘
```

### ⚙️ 매니저 시스템

모든 매니저는 **싱글턴 + DontDestroyOnLoad** 패턴을 사용하며, `InitManager`가 2단계 비동기 파이프라인으로 순차 초기화한다.

```
InitManager
  ├─ Phase 1: 데이터 로드
  │    DataManager.LoadAllData()
  │    CharacterManager.LoadCharacterIdentities()    ← Addressables 라벨
  │    SkillManager.LoadAllSkillDefinitions()         ← Addressables 라벨
  │    BarrelManager.LoadAllBarrelData()
  │    OrbManager.LoadAllOrbData()
  │    MapManager.LoadAllMapScriptables()
  │
  └─ Phase 2: 프리로드
       PoolManager.PreloadPools()                     ← 프리팹 풀 예열
```

| 매니저 | 책임 |
|--------|------|
| `GameManager` | 씬 전환, 게임 일시정지/재개, SceneLoaded → EventBus 발행 |
| `InitManager` | 초기화 시퀀싱, 프로그레스 트래킹 |
| `DataManager` | JSON 데이터 로드 (맵, 적) |
| `MapManager` | 맵 스크립터블 캐시, 맵 활성화, 적 스폰 설정 |
| `CharacterManager` | 캐릭터 Identity 로드/선택, 해금 시스템 |
| `PlayerManager` | 플레이어 인스턴스 관리, 스탯 초기화 |
| `EnemyManager` | 적 스폰, 적 카운트 관리 |
| `StageManager` | 스테이지 FSM (Combat → Clear → Loading) |
| `LevelManager` | 경험치, 레벨업 처리 |
| `SkillManager` | 스킬 정의 캐시, 스킬 선택/적용 |
| `OrbManager` | 오브 스킬 설정 데이터 |
| `BarrelManager` | 배럴 스킬 설정 데이터 |
| `PoolManager` | 오브젝트 풀링, Addressable 에셋 관리 |
| `UIManager` | UI 팝업 생성/관리, Presenter 소유 |
| `InputManager` | New Input System 래핑 |
| `SoundManager` | 오디오 관리 (미구현) |
| `SaveManager` | 세이브/로드 (미구현) |

### 📡 EventBus

**우선순위 기반 Pub/Sub** 이벤트 시스템. 매니저 간 결합도를 낮추는 핵심 인프라.

```csharp
// 구독 (낮은 priority 먼저 실행)
EventBus.Subscribe(EventType.LevelUp, OnLevelUp, priority: 10);

// 발행
EventBus.Publish(EventType.LevelUp);
```

**등록된 이벤트 타입:**

| 이벤트 | 발행자 | 구독자 |
|--------|--------|--------|
| `LobbySceneLoaded` | GameManager | UIManager, LobbyMapManager, LobbyCharacterManager |
| `InGameSceneLoaded` | GameManager | — |
| `StageCombatStarted` | StageManager | InputManager, UIManager |
| `LevelUp` | LevelManager | GameManager(일시정지), UIManager(스킬팝업) |
| `MapCleared` | StageManager | InputManager, UIManager |
| `TransitionToLobby` | UIManager | 각 매니저(데이터 정리) |
| `Retry` | UIManager | 각 매니저(재시작) |
| `AllEnemiesDefeated` | EnemyManager | StageManager |
| `PlayerDied` | Health | UIManager(게임오버 팝업) |

### 🖼 MVP UI 패턴

UI는 **View(MonoBehaviour)** + **Presenter(Plain C#)** 로 분리.  
Presenter는 `UIManager`가 소유하고, View는 Canvas 하위 컴포넌트로 존재.

```
UIManager (Presenter 소유)
  ├─ GameOverPopupPresenter    → GameOverPopup (View)
  ├─ MapClearPopupPresenter    → MapClearPopup (View)
  ├─ SkillChoicePresenter      → SkillChoicePopup (View)
  └─ SettingPopupPresenter     → SettingPopup (View)
                                   └─ CharacterTabPresenter → UI_CharacterTabView
```

**SettingPopup 구조 (탭 기반):**
```
SettingPopup
  ├─ tabButtons[]      탭 전환 버튼 (캐릭터 / 장비 / 룬)
  ├─ tabContents[]     탭별 콘텐츠 패널
  └─ UI_CharacterTabView
       ├─ RawImage      (RenderTexture → 3D 캐릭터 더미)
       ├─ Left / Right  (캐릭터 전환)
       ├─ Select         (선택 버튼)
       ├─ Lock Icon      (잠금 아이콘)
       └─ Name Text      (캐릭터 이름)
```

### ♻️ 오브젝트 풀링 & Addressables

모든 런타임 프리팹(적, 투사체, 이펙트, 맵)은 **PoolManager**를 통해 관리.

```
PoolManager
  ├─ enemyPool          적 프리팹 풀
  ├─ projectilePool     투사체 풀
  ├─ effectPool         파티클 이펙트 풀
  └─ mapPool            맵 프리팹 풀

로드: Addressables.LoadAssetAsync → 풀 등록
사용: PoolManager.GetObjectAsync(ref, pool) → 오브젝트 반환
반납: PoolManager.ReturnObject(obj, pool) → 비활성화 후 재사용
```

**Addressables 라벨:**
- `character_identity` — 캐릭터 ScriptableObject
- `skill_config` — 스킬 정의
- `barrel_config` / `orb_config` — 스킬 설정
- `map_config` — 맵 ScriptableObject

### 📊 3계층 스탯 시스템

```
Total = Base + Equipment + InGameBuff
        ───    ─────────   ──────────
         │         │            │
  Inspector    장비 보너스   스킬 버프
  기본값       (영구)        (인게임 한정)
```

`PlayerStat`과 `EnemyStat` 모두 `BaseStat`을 상속하며, 각 계층을 독립적으로 관리.
- **Base**: Inspector에서 설정하는 기본값
- **Equipment**: 장착 장비에 의한 영구 보너스
- **InGameBuff**: 스킬 선택 등에 의한 인게임 한정 버프

### 🧩 플레이어 모듈 시스템

`PlayerController`가 독립 모듈을 조합하여 동작.

```
PlayerController (Orchestrator)
  ├─ PlayerMovement    이동 + 넉백 처리
  ├─ PlayerAttack      공격 로직 + 투사체 발사
  ├─ PlayerHurt        피격 + 무적 시간
  └─ PlayerSkill       스킬 모듈 관리
       ├─ AttackSpeedUp
       ├─ MoveSpeedUp
       ├─ MultiShot
       ├─ DiagonalShot
       ├─ HorizontalShot
       ├─ IceBarrel       (투하형 범위 공격)
       ├─ IceOrb          (공전형 투사체)
       └─ VenomOrb
```

각 스킬은 `ISkillModule` 인터페이스를 구현하며, `SkillManager`의 정의 데이터를 기반으로 동적 적용.

### 👾 적 행동 시스템

적은 **상태 머신 + Behavior Module** 구조로 동작.  
`EnemyBehaviorFactory`가 `EnemyTag`에 따라 적절한 행동 모듈을 생성.

```
EnemyController (State Machine)
  ActionTable: Dictionary<EnemyState, (OnEnter, OnExit, OnTick)>
  
  States:
    Idle   → 대기 타이머 후 이동
    Move   → 플레이어 추적, 슬로우 효과 적용
    Attack → 투사체/메테오 발사
    Hurt   → 넉백 리액션
    Die    → 사망 애니메이션 + 정리
```

**EnemyTag (비트 플래그):** Boss, Melee, Ranged, Flying 등 조합 가능.

### 🔥 이펙트 시스템

**Strategy 패턴**으로 상태이상 처리.

```
EffectType (Flags): Normal | Fire | Poison | Ice | Lightning | Magma | Dark

IEffectHandler
  ├─ DotEffectHandler (Fire)     도트 데미지
  ├─ DotEffectHandler (Poison)   도트 데미지
  └─ IceEffectHandler            이동속도 감소
```

**방어력 공식:** `finalDamage = damage × (100 / (100 + armor))`

---

## 🎯 게임 흐름

```
Loading Scene
  │  InitManager: 데이터 로드 → 프리팹 프리로드 → 완료
  │  (Space 키 입력)
  ▼
Lobby Scene
  │  GameManager.OnSceneLoaded()
  │    → EventBus.Publish(LobbySceneLoaded)
  │      → UIManager: 세팅 팝업 초기화
  │      → LobbyMapManager: 맵 프리로드
  │      → LobbyCharacterManager: 캐릭터 더미 생성
  │  (캐릭터 & 맵 선택 후 게임 시작)
  ▼
InGame Scene
  │  StageManager 루프:
  │
  │  ┌─ Combat ──────────────────────────┐
  │  │  적 스폰 → 전투 → AllEnemiesDefeated │
  │  └──────────────┬─────────────────────┘
  │                 ▼
  │  ┌─ Clear ───────────────────────────┐
  │  │  포탈 생성 → 플레이어 진입           │
  │  └──────────────┬─────────────────────┘
  │                 ▼
  │  ┌─ Loading ─────────────────────────┐
  │  │  다음 스테이지 셋업                  │
  │  └──────────────┬─────────────────────┘
  │                 ▼
  │          (반복 until 맵 클리어)
  │
  ├─ 맵 클리어 → TransitionToLobby → Lobby Scene
  ├─ 플레이어 사망 → GameOverPopup (재도전 / 로비)
  └─ 레벨업 → SkillChoicePopup (게임 일시정지)
```

---

## 💾 데이터 파이프라인

```
JSON Files (Resources/Data/)
  │  enemyData.json     List<EnemyData>
  │  mapData.json        List<MapData>
  ▼
DataManager.LoadAllData()
  │  Resources.Load<TextAsset>()
  │  JsonConvert.DeserializeObject<List<T>>()
  ▼
Manager Dictionaries (메모리 캐시)
  │  MapManager._mapDataDict
  │  EnemyManager._enemyDataDict
  ▼
Gameplay Systems (런타임 참조)
```

**ScriptableObject는 Addressables 라벨로 비동기 로드:**
```
Addressables.LoadAssetsAsync<T>(label)
  → 매니저 Dictionary에 캐시
  → 런타임에서 키로 조회
```

---

## 🧬 사용 디자인 패턴 정리

| 패턴 | 적용 위치 | 설명 |
|------|-----------|------|
| **Singleton** | 모든 매니저 | `Instance` 프로퍼티 + `DontDestroyOnLoad` |
| **Observer (EventBus)** | 매니저 간 통신 | 우선순위 기반 Pub/Sub |
| **MVP** | UI 시스템 | View(MonoBehaviour) + Presenter(C# 클래스) |
| **Object Pool** | PoolManager | Addressable 기반 프리팹 재사용 |
| **Factory** | EnemyBehaviorFactory | EnemyTag 기반 행동 모듈 생성 |
| **State Machine** | EnemyController, StageManager | 상태별 OnEnter/OnExit/OnTick |
| **Strategy** | IEffectHandler | 상태이상 종류별 독립 핸들러 |
| **Module/Component** | PlayerController | 이동·공격·피격·스킬 모듈 조합 |
| **Async Pipeline** | InitManager | `Awaitable` 기반 순차 초기화 |

---

## 📜 라이선스

이 프로젝트는 [MIT License](LICENSE)로 배포됩니다.
