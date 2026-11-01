# 🏹 Archery

> Unity 6 기반 3D 캐주얼 액션 로그라이트 게임

> ⚠️ **이 저장소는 `Assets/Scripts`(직접 작성한 C# 코드)만 담고 있습니다.**
> 모델·애니메이션·프리팹·머티리얼·사운드 등 외부 에셋은 라이선스 문제로 포함하지 않았으므로, 클론만으로는 Unity에서 실행되지 않습니다.
> 사용한 외부 에셋의 출처는 [UNITY_IMPORTED_ASSETS.md](./UNITY_IMPORTED_ASSETS.md)에 정리했습니다.

---

## 🎬 플레이 영상

[![플레이 영상](https://img.youtube.com/vi/FTMjjKMawrQ/0.jpg)](https://youtu.be/FTMjjKMawrQ)

---

## 🎮 게임 소개

플레이어는 이동으로 적의 공격을 피하고, 멈춰 있을 때 자동으로 공격합니다.
적을 처치해 경험치와 골드를 얻고, 레벨업 때 제시되는 스킬을 골라 그 회차의 전투 능력을 만듭니다.
로비에서는 캐릭터와 장비를 구매·선택·강화하며, 이 영구 성장은 회차별 임시 성장과 분리해 관리합니다.

---

## 🛠 기술 스택

| 영역 | 기술 |
|------|------|
| 🎮 엔진 | Unity 6 (`6000.0.62f1`), URP |
| 💻 언어 | C# |
| ⚡ 비동기 | Unity `Awaitable`, Addressables |
| 📦 데이터 | ScriptableObject, JSON (Newtonsoft.Json) |
| 🕹️ 입력 | Unity Input System |
| ✏️ UI | Unity UI, TextMesh Pro |

---

## 🏗 아키텍처 요약

- **매니저 중심 구조** — 책임별 싱글턴 매니저(게임 진행, 맵, 풀링, 저장, 사운드 등)를 `InitManager`가 순서대로 초기화합니다.
- **행동과 데이터의 분리** — 캐릭터·적·맵·스킬·장비의 정의는 ScriptableObject/JSON, 실행 로직은 컴포넌트가 맡습니다.
- **오브젝트 풀링** — 적, 투사체, 드롭 아이템, 이펙트, 맵 프리팹을 Addressables와 풀로 재사용합니다.
- **FSM 기반 전투** — 플레이어와 적 모두 상태 머신과 행동 컴포넌트 조합으로 동작합니다.
- **영구/회차 성장 분리** — 구매·장착·강화 정보는 저장하고, 회차별 경험치·스킬·버프는 별도 수명으로 관리합니다.
- **UI 역할 분리** — View와 Presenter를 나누고, 이벤트로 게임 진행·UI·저장을 연결합니다.

자세한 내용은 [ARCHERY_SYSTEM_ARCHITECTURE.md](./ARCHERY_SYSTEM_ARCHITECTURE.md)를 참고하세요.

---

## 📂 코드 구조

```
Assets/Scripts/
├── Managers/        # 게임 진행·맵·풀링·저장·사운드 등 매니저
├── Players/         # 플레이어 FSM, 스킬 모듈
├── Enemy/           # 적 FSM과 행동 모듈
├── Map/             # 맵·스테이지 진행
├── Objects/         # 투사체, 오브, 배럴, 드롭 아이템
├── Stats/           # 스탯 시스템
├── Effects/         # 상태이상 효과
├── UI/              # 로비·인게임 UI
├── SaveSystem/      # 저장/로드
├── Camera/          # 카메라
├── Utils/           # EventBus 등 유틸리티
└── Editor/          # 콘텐츠 저작 도구
```

---

## 📄 라이선스

[MIT License](./LICENSE) — 코드에만 적용되며, 외부 에셋은 각 제작사의 라이선스를 따릅니다.
