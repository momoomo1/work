# 프로젝트 규칙 (Unity 3D URP)

## 환경 (확인됨)
- Unity 6000.3.26f1, URP 17.3.0 (PC_RPAsset / Mobile_RPAsset 존재, PC 사용)
- 입력: **새 Input System 전용** (activeInputHandler=1, com.unity.inputsystem 1.20.0). `UnityEngine.Input` 사용 금지 → `UnityEngine.InputSystem`의 `Keyboard.current` 등 또는 `InputSystem_Actions.inputactions` 사용
- 패키지: AI Navigation 2.0.15, Timeline, uGUI 2.0.0, Test Framework. **Cinemachine 미설치** (카메라는 직접 구현한 쿼터뷰 추적 스크립트 사용)

## 게임
- 3D 쿼터뷰 액션 로그라이크 서바이벌, PC 플랫폼
- WASD 이동, 공격은 자동 타겟팅
- 루프: 몬스터 처치 → 경험치 → 레벨업 → 스킬 3택1 → 반복 → 적 5마리 처치 시 보스 등장
- 스킬은 관통형/방사형 모듈로 구성. 수라참(관통 투사체), 헬파이어(방사형 폭발), 회전 칼날(궤도 관통), 서리 구름(방사형 장판), 진공참(수라참 강화형). 이후 10종 이상으로 확장 예정

## 코드 규칙
- 스크립트 폴더: Assets/Scripts/{Core, Player, Enemy, Skills, UI}
- C#, 네임스페이스 없이 간단하게. 한 파일에 한 클래스
- 수치는 [SerializeField]로 인스펙터에서 조절 가능하게
- 적, 투사체, 이펙트는 오브젝트 풀을 사용해서 Instantiate/Destroy를 반복하지 않기
- 스킬은 데이터(ScriptableObject)와 동작(MonoBehaviour)을 분리해서 새 스킬을 쉽게 추가할 수 있게
- 지금은 외부 에셋 없이 Unity 기본 프리미티브(Capsule, Sphere, Cube)로 임시 비주얼을 쓰고, 나중에 모델과 이펙트로 교체할 수 있게 비주얼과 로직을 분리
- 입력은 프로젝트에 설정된 입력 방식(위 "환경" 참고: 새 Input System)에 맞게 작성

## 작업 방식
- 시간이 매우 부족한 프로젝트라 과하게 설계하지 말고, 동작하는 최소 구현을 우선
- **씬/프리팹/ScriptableObject 에셋은 사용자에게 시키지 말고 Claude가 YAML 파일로 직접 만들어 준다.** 새 스크립트는 `.cs.meta`를 직접 작성해 GUID를 미리 정하고(32자리 hex), 씬/프리팹 YAML이 그 GUID로 참조하게 한다. 새 `.meta`도 함께 만든다
- 직접 만들기 어려운 것(머티리얼 셰이더 설정 등 복잡한 것)만 Tools 메뉴 에디터 스크립트(Assets/Scripts/Editor)로 대체한다
- Unity 에디터가 열려 있으면 파일 수정이 충돌할 수 있으니, 씬/에셋 편집 전 사용자에게 에디터에서 해당 씬을 닫거나 저장 후 리로드하라고 알린다
- 작업이 끝나면 (1) 만든 파일 목록 (2) 사용자가 Unity 에디터에서 해야 할 작업(번호) (3) 확인 방법을 짧게 알린다
