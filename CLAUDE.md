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
- 수동 에디터 작업이 많으면 Tools 메뉴에 자동 세팅용 에디터 스크립트를 만든다 (프리팹/오브젝트 자동 생성). 에디터 스크립트는 Assets/Scripts/Editor 에 둔다
- 작업이 끝나면 (1) 만든 파일 목록 (2) 사용자가 Unity 에디터에서 해야 할 작업(번호) (3) 확인 방법을 짧게 알린다
