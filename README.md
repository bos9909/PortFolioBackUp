# 🚀 Unity Core Systems (최적화된 코어 시스템 모음)

유니티(Unity) 기반 게임 개발 시 빈번하게 발생하는 **성능 병목(GC 호출, 물리 연산 과부하)을 방지**하고, **유지보수와 확장성**을 고려하여 설계한 코어 매니저 및 시스템 모음입니다. 

## 🛠️ 주요 기능 및 기술 스택
- **Language**: C#
- **Engine**: Unity (2022+ 권장)
- **Key Architecture**: Singleton, Object Pooling (`UnityEngine.Pool`), Data-Driven Design (ScriptableObject)

---

## 💡 핵심 시스템 상세

### 1. ♻️ 최적화된 Object Pool Manager
유니티 2021부터 지원되는 내장 `UnityEngine.Pool` API를 활용하여 커스텀 풀링 매니저를 구축했습니다. 

* **동적 딕셔너리 관리**: 여러 종류의 프리팹을 하나의 매니저에서 딕셔너리 기반으로 자동 분류하고 관리합니다. 새로운 프리팹을 `Pop`할 때 해당 풀이 없으면 즉석에서 생성(`CreateNewPool`)하여 확장성이 뛰어납니다.
* **물리/회전 버그 차단 (중요)**: 
  * 내장 풀의 `Get()` 호출 시 즉각적으로 `SetActive(true)`가 발동되어, 위치/회전값이 세팅되기 전에 `OnEnable` 물리 연산이 실행되는 프레임 꼬임 현상이 있었습니다. 
  * 이를 해결하기 위해 내장 풀의 자동 활성화 규칙을 제거하고, `Pop` 함수에서 **위치와 회전을 완벽히 정렬한 직후에 수동으로 오브젝트를 활성화**하도록 제어권을 가져와 물리 연산(투사체 궤적 휘어짐 등) 오류를 원천 차단했습니다.

### 2. 🎵 데이터 주도형 Sound Manager
단순 재생을 넘어 실무 레벨의 디테일과 오디오 믹서(Audio Mixer) 연동을 고려한 사운드 매니저입니다.

* **ScriptableObject 기반 데이터 관리**: 사운드 클립들을 하드코딩하지 않고, `SoundDataContainer` (SO)를 통해 인스펙터에서 기획자나 사운드 디자이너가 직관적으로 관리할 수 있도록 Data-Driven 방식으로 설계했습니다.
* **오디오 믹서 dB 단위 제어**: 유니티의 AudioMixer를 연동하여, 선형적인 0~1 볼륨값이 아닌 **로그 스케일(Logarithmic Scale) 기반의 데시벨(dB) 연산**을 적용하여 인간의 청각에 자연스러운 볼륨 조절을 구현했습니다.
* **BGM 코루틴 크로스페이드**: 씬이나 상황 전환 시 음악이 뚝 끊기지 않도록, 기존 곡의 Fade-Out과 새 곡의 Fade-In을 부드럽게 처리하는 안전한 코루틴(Coroutine) 전환 로직을 구현했습니다.
* **SFX 중첩 재생(PlayOneShot) 최적화**: 탄막 게임이나 연사 무기 사용 시 사운드 채널이 씹히는 문제를 방지하기 위해 `PlayOneShot`을 적용, 단일 AudioSource에서도 수십 개의 효과음이 자연스럽게 겹쳐 재생되도록 처리했습니다.

### 3. 🎯 물리 연산 최적화 (투사체 시스템)
고속으로 이동하는 투사체(발사체) 처리 시 물리 엔진 부하를 최소화하는 구조를 채택했습니다.

* **Kinematic & Trigger 방식 채택**: `Rigidbody`의 연산이 무거운 `Collision` 방식 대신, 투사체를 `isKinematic = true` 로 설정하고 통과형 `Trigger` 방식으로 충돌을 판정합니다.
* **성능 극대화**: 다수의 적(Target) 오브젝트가 존재하는 환경에서 적의 Rigidbody를 제거할 수 있는 기반을 마련하여 물리 연산 비용(CPU Physics Time)을 획기적으로 절감했습니다.
* **정확한 타격 판정**: 트리거 방식의 단점인 '충돌 지점(Contact Point)' 부재를 해결하기 위해 `other.ClosestPoint()`를 활용하여 가상의 피격 위치와 노말 벡터를 계산, 정확한 피격 이펙트 출력을 지원합니다.

---

📈 트러블 슈팅 (Trouble Shooting) & 회고

Q. 투사체 발사 시 궤적이 제멋대로 휘어지는 현상
  원인: 오브젝트 풀에서 꺼낼 때 SetActive(true)가 먼저 호출되면서, 투사체의 OnEnable에서 이전 프레임의 잘못된 회전값을 기준으로 물리력(Velocity)이 더해졌기 때문.
  해결: UnityEngine.Pool의 기본 ActionOnGet 콜백을 비우고, Manager가 위치/회전을 먼저 초기화한 뒤 명시적으로 활성화하도록 로직을 수정하여 해결. 추가로 물리 연산과 Transform 이동의 충돌을 막기 위해 Kinematic 기반 직선 이동으로 구조 변경.

Q. BGM 교체 시 코루틴 중복 실행으로 인한 볼륨 튀음 현상
  원인: BGM 교체가 빠르게 여러 번 일어날 경우, 다수의 코루틴이 동시에 하나의 AudioSource 볼륨을 제어하며 Race Condition 발생.
  해결: bgmFadeCoroutine 변수를 두어 새로운 BGM 실행 시 기존 코루틴을 강제로 StopCoroutine 시키고 초기화하여 안전하게 전환되도록 제어 로직 추가.
