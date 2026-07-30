# CLAUDE.md

이 저장소에서 작업할 때 지켜야 할 규칙. 기기가 바뀌어도 적용된다.

## 프로젝트

스컬: 히어로 슬레이어를 레퍼런스로 한 **2D 액션 로그라이크** (Unity 6000.3 / URP 2D).
1인 · 6주 · 주 20시간 = 약 120시간이 1차 범위.

- [docs/GDD.md](docs/GDD.md) — 기획서. 확장 지점과 이연 항목이 여기 있다
- [docs/roadmap.md](docs/roadmap.md) — 주차별 계획과 절단 순서
- [docs/learning-log.md](docs/learning-log.md) — 막혔던 지점과 판단 근거

---

## 작업 규칙

### 코드를 직접 작성하지 않는다

**사용자가 명시적으로 요청할 때만 예외.** 포트폴리오 프로젝트이고 사용자 본인의
학습이 목적이다. 대신 코드를 쓰면 포트폴리오로서의 의미도, 학습 효과도 사라진다.

- ❌ `.cs` 파일 작성·수정, Unity 에셋 직접 편집
- ❌ 요청 없이 채팅에 실제 C# 코드 제시
- ✅ **의사코드 · 클래스 구성 · 흐름 설명까지는 OK** — 설계 감각은 주고 구현은 본인이
- ✅ **코드 리뷰** — 작성한 코드를 읽고 피드백. 문제를 지적하되 고쳐주지는 않는다

막혔다고 해서 코드로 넘어가지 말 것. 개념 → 의사코드 순으로 올리고, 실제 코드는
명시 요청 시에만.

### 산출물은 한국어로

커밋 메시지, PR 본문, GitHub 이슈, 문서 전부 한국어.
코드 식별자·타입명은 원문 유지 (`IPatternSelector`, `linearVelocityX` 등).
`Closes #N` 같은 GitHub 키워드와 `Co-Authored-By` 트레일러는 영어 그대로.

### 이슈를 머지할 때마다 학습 로그를 갱신한다

별도 요청 없이 자동으로 [docs/learning-log.md](docs/learning-log.md) 를 갱신한다.
작업 직후가 아니면 "왜 그렇게 판단했는지"가 사라진다.

- 막혔던 지점 / 새로 이해한 것 / 판단 근거를 **주제별 절**에 추가 (시간순 아님)
- **결론이 아니라 근거를 남긴다.** "bounds 를 쓴다"가 아니라 "왜 `transform.position`
  이 안 되는가"까지
- 반복된 실수는 6절에 횟수와 함께 누적
- 7절 "아직 안 겪은 것"에서 해당 항목을 실제 겪은 내용으로 옮긴다
- 상단 `최종 수정` 날짜 갱신
- 특별히 배운 게 없는 이슈면 억지로 채우지 않는다

### 사용자 대신 처리하는 것

문서(GDD·로드맵·학습 로그), git 커밋·푸시, GitHub 이슈·PR 관리, 코드 리뷰.

---

## 워크플로

```
브랜치 생성   feat/<이슈번호>-<슬러그>     예: feat/6-coyote-jump-buffer
              fix/<이슈번호>-<슬러그>
이슈 assign   gh issue edit <N> --add-assignee @me
작업          (사용자가 구현, 나는 리뷰)
커밋          한국어. 본문에 판단 근거를 남긴다. 마지막에 Closes #N
PR 생성       한국어. 개요 / 판단 근거 / 검증 / 남은 것
머지          --merge --delete-branch  → Closes #N 이 발동해 이슈 자동 종료
정리          로컬 브랜치 삭제, git remote prune origin
```

이슈 종료는 커밋·PR의 `Closes #N` 으로 처리한다. 이 키워드는 **기본 브랜치에 도달할
때만** 발동하므로, 피처 브랜치 커밋 시점에는 닫히지 않는다.

---

## 이 프로젝트 고유 주의사항

### `Assets/Imported/` 는 별도 저장소다

유료 에셋 보관용 중첩 git 저장소(`sonjuhawn-source/PersonalProject-AssetImport`, private).
부모 `.gitignore` 에서 폴더와 `Assets/Imported.meta` 를 함께 제외한다.

**에셋을 넣기 전에 LFS 설정이 끝나 있어야 한다.** LFS 없이 커밋된 바이너리는 히스토리에
남아 나중에 LFS를 걸어도 용량이 줄지 않는다. `.gitattributes` 는 이미 커밋돼 있다.

### asmdef 의존 방향

```
Game.Core  ←  Game.Gameplay  ←  Game.UI
```

역류하지 않는다. 셋 다 `autoReferenced: false`.
새 어셈블리를 만들 때 에디터 전용이면 `includePlatforms: ["Editor"]` 를 빼먹지 말 것.

**asmdef 어셈블리는 `Assembly-CSharp` 를 참조할 수 없다.** 생성 코드나 유료 에셋 코드가
`Assets/` 루트에 있으면 `Game.*` 에서 쓸 수 없다.

### `.meta` 는 항상 커밋한다

GUID와 임포트 설정이 들어 있어서, 빠지면 프리팹·머티리얼 참조가 끊긴다.
예외는 **무시하는 폴더의 `.meta`** 뿐 (`Assets/Imported.meta`).

### 확장 지점 4개는 1차에서 반드시 지킨다

무기 SO 래퍼 / 적·보스 공용 상태머신 / 방 타입 핸들러 / 이벤트 훅 기반 유물.
상세는 [GDD 9장](docs/GDD.md). 이걸 지키면 후속 작업이 재작성이 아니라 데이터 작성이 된다.

### 에디터 저장을 먼저 확인한다

사용자가 "다 했어"라고 해도 Unity가 디스크에 안 썼을 수 있다. 실제로 반복된 문제다.

- **씬** — `Ctrl+S`. Auto-Save 없음
- **Input Actions 창** — `Save Asset` 버튼 (창의 `Auto-Save` 체크박스 권장)
- 확인: `git status` 또는 파일 수정 시각

리뷰 요청을 받으면 파일을 읽기 전에 저장 여부를 확인하는 편이 왕복을 줄인다.
