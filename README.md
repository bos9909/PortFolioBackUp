# 🚀 Unity Core Systems

<div align="center">
  <a href="#english"><b>🇺🇸 English</b></a> |
  <a href="#japanese"><b>🇯🇵 日本語</b></a> |
  <a href="#korean"><b>🇰🇷 한국어</b></a>
</div>

<br>

---

<h2 id="english">🇺🇸 English</h2>

A collection of core managers and systems designed to prevent common performance bottlenecks (GC allocation, physics engine overload) in Unity game development, built with high maintainability and scalability in mind.

### 🛠️ Tech Stack
- **Language**: C#
- **Engine**: Unity (2022+ Recommended)
- **Key Architecture**: Singleton, Object Pooling (`UnityEngine.Pool`), Data-Driven Design (ScriptableObject)

### 💡 Core Systems

#### 1. ♻️ Optimized Object Pool Manager
Built upon Unity's built-in `UnityEngine.Pool` API, customized for dynamic handling and physics stability.
- **Dynamic Dictionary Management**: Automatically classifies and manages multiple prefabs using a dictionary. If a requested prefab lacks an existing pool, it generates one on the fly (`CreateNewPool`), ensuring high scalability.
- **Preventing Physics/Transform Desync**: Bypassed the default auto-activation of the built-in pool upon `Get()`. To prevent physics bugs (like projectile trajectory curving), the manager manually applies the exact position and rotation *before* setting the object to active.

#### 2. 🎵 Data-Driven Sound Manager
A production-level sound manager fully integrated with Unity's AudioMixer.
- **ScriptableObject Data Management**: Uses `SoundDataContainer` (SO) to avoid hardcoding clips, allowing game designers to manage audio data intuitively via the Inspector.
- **Logarithmic dB Volume Control**: Instead of linear 0~1 values, it calculates volume using a logarithmic scale (dB) synced with the AudioMixer, mimicking natural human hearing.
- **BGM Coroutine Cross-fade**: Smoothly transitions between BGM tracks with safe coroutine logic to prevent abrupt audio cuts during scene changes.
- **SFX Overlap Optimization (`PlayOneShot`)**: Resolves audio channel cancellation during rapid fire (e.g., bullet hell games) by utilizing `PlayOneShot`, allowing dozens of sound effects to layer naturally on a single AudioSource.

#### 3. 🎯 Physics Optimization (Projectile System)
Designed to minimize physics engine overhead when handling high-speed projectiles.
- **Kinematic & Trigger Approach**: Replaced heavy `Collision` calculations with `isKinematic = true` and `Trigger` colliders.
- **Maximized Performance**: Allowed the removal of Rigidbody components from target enemies, drastically reducing CPU Physics Time.
- **Accurate Hit Detection**: Solved the lack of "Contact Points" in triggers by using `other.ClosestPoint()` to calculate a virtual hit position, ensuring precise hit effect instantiation.

### 💻 Code Usage

**1. Object Pooling (Fire Projectile)**
```csharp
// [Example Code: Commented out as requested]
// GameObject bullet = ObjectPoolManager.Instance.Pop(bulletPrefab, firePoint.position, firePoint.rotation);
```

**2. Audio Playback (BGM Fade & Rapid SFX)**
```csharp
// [Example Code: Commented out as requested]
// SoundManager.Instance.PlayBGMWithFade("Bgm_BossPhase", 1.5f);
// SoundManager.Instance.PlaySFX("Sfx_RifleFire");
```

### 📈 Trouble Shooting

**Q. Projectile trajectory curves unpredictably upon firing**
* **Cause**: Calling `SetActive(true)` immediately upon retrieving from the pool triggered `OnEnable` before the transform was updated, applying physics forces based on previous frame data.
* **Fix**: Cleared the default `ActionOnGet` in Unity's Pool. The Manager now sets the exact position and rotation first, and manually activates the object afterward.

**Q. Volume spikes during rapid BGM changes**
* **Cause**: Multiple fade coroutines executing simultaneously caused a race condition on the AudioSource's volume property.
* **Fix**: Implemented a `bgmFadeCoroutine` reference variable to ensure any running fade coroutine is stopped (`StopCoroutine`) before a new transition begins.

<br>

---

<h2 id="japanese">🇯🇵 日本語</h2>

Unityゲーム開発において頻繁に発生するパフォーマンスのボトルネック（GCアロケーション、物理演算の過負荷）を防ぎ、保守性と拡張性を考慮して設計されたコアマネージャーとシステムのコレクションです。

### 🛠️ 技術スタック
- **Language**: C#
- **Engine**: Unity (2022+ 推奨)
- **Key Architecture**: Singleton, Object Pooling (`UnityEngine.Pool`), Data-Driven Design (ScriptableObject)

### 💡 コアシステム詳細

#### 1. ♻️ 最適化されたオブジェクトプールマネージャー
Unity内蔵の `UnityEngine.Pool` APIを活用し、動的処理と物理演算の安定性を両立させたカスタムプールマネージャーです。
- **動的ディクショナリ管理**: 複数のプレハブをディクショナリで自動分類・管理。要求されたプレハブのプールが存在しない場合は即座に生成（`CreateNewPool`）し、高い拡張性を実現しています。
- **物理・Transformのズレ防止（重要）**: 内蔵プールの `Get()` 呼び出し時の自動アクティブ化を無効化。弾の軌道が曲がるなどの物理バグを防ぐため、オブジェクトをアクティブにする前に位置と回転を完全にセットしてから手動でアクティブ化するよう制御しています。

#### 2. 🎵 データ駆動型サウンドマネージャー
AudioMixerと完全に連動し、実務レベルのディテールを考慮したサウンドマネージャーです。
- **ScriptableObjectによるデータ管理**: 音源クリップをハードコーディングせず、`SoundDataContainer` (SO) を介してインスペクター上でプランナーが直感的に管理できるデータ駆動型（Data-Driven）設計を採用。
- **dB（デシベル）単位のボリューム制御**: 直線的な0～1の値ではなく、AudioMixerと連動した対数スケール（Logarithmic Scale）を適用し、人間の聴覚に自然なボリューム調整を実現。
- **BGMクロスフェード**: シーン移行時に音楽が途切れないよう、自然なフェードアウト・フェードイン処理を安全なコルーチンで実装。
- **SFXの多重再生最適化（`PlayOneShot`）**: 弾幕ゲームや連射武器の使用時に音が途切れる問題を解決するため `PlayOneShot` を適用し、単一のAudioSourceで数十個の効果音を自然に重ねて再生可能にしました。

#### 3. 🎯 物理演算の最適化（発射体システム）
高速で移動する発射体の処理において、物理エンジンの負荷を最小限に抑える構造を採用しています。
- **Kinematic & Trigger方式**: 処理が重い `Collision` ではなく、発射体を `isKinematic = true` に設定し、`Trigger` による衝突判定を採用。
- **パフォーマンスの最大化**: 多数の敵が存在する環境で、敵のRigidbodyを削除できる基盤を作り、物理演算コスト（CPU Physics Time）を劇的に削減しました。
- **正確なヒット判定**: Trigger方式の欠点である「衝突座標（Contact Point）」の欠如を `other.ClosestPoint()` を用いて解決し、正確な被弾エフェクトの表示を実現しています。

### 💻 コード使用例

**1. オブジェクトプーリング（弾の発射）**
```csharp
// [Example Code: Commented out as requested]
// GameObject bullet = ObjectPoolManager.Instance.Pop(bulletPrefab, firePoint.position, firePoint.rotation);
```

**2. サウンド再生（BGMフェード＆SFX連射）**
```csharp
// [Example Code: Commented out as requested]
// SoundManager.Instance.PlayBGMWithFade("Bgm_BossPhase", 1.5f);
// SoundManager.Instance.PlaySFX("Sfx_RifleFire");
```

### 📈 トラブルシューティング

**Q. 発射体の軌道が予期せず曲がってしまう現象**
* **原因**: プールから取り出す際に `SetActive(true)` が先に呼ばれ、位置・回転が更新される前の古いデータに基づいて `OnEnable` で物理力（Velocity）が加算されていたため。
* **解決策**: Unity標準プールの `ActionOnGet` を空にし、マネージャー側で位置・回転を先に初期化してから明示的にアクティブ化するようにロジックを修正しました。

**Q. BGM切り替え時のボリュームの乱れ（スパイク）現象**
* **原因**: 短時間でBGMが複数回切り替わった際、複数のコルーチンが同時に1つのAudioSourceのボリュームを操作し、競合（Race Condition）が発生。
* **解決策**: `bgmFadeCoroutine` 変数を用意し、新しいBGMの遷移を開始する前に実行中のコルーチンを強制終了（`StopCoroutine`）させて安全に切り替わるように制御ロジックを追加しました。

<br>

---

<h2 id="korean">🇰🇷 한국어</h2>

유니티(Unity) 기반 게임 개발 시 빈번하게 발생하는 **성능 병목(GC 호출, 물리 연산 과부하)을 방지**하고, **유지보수와 확장성**을 고려하여 설계한 코어 매니저 및 시스템 모음입니다. 

### 🛠️ 주요 기능 및 기술 스택
- **Language**: C#
- **Engine**: Unity (2022+ 권장)
- **Key Architecture**: Singleton, Object Pooling (`UnityEngine.Pool`), Data-Driven Design (ScriptableObject)

### 💡 핵심 시스템 상세

#### 1. ♻️ 최적화된 Object Pool Manager
유니티 2021부터 지원되는 내장 `UnityEngine.Pool` API를 활용하여 커스텀 풀링 매니저를 구축했습니다. 
- **동적 딕셔너리 관리**: 여러 종류의 프리팹을 하나의 매니저에서 딕셔너리 기반으로 자동 분류하고 관리합니다. 새로운 프리팹을 `Pop`할 때 해당 풀이 없으면 즉석에서 생성(`CreateNewPool`)하여 확장성이 뛰어납니다.
- **물리/회전 버그 차단 (중요)**: 내장 풀의 `Get()` 호출 시 즉각적으로 `SetActive(true)`가 발동되어, 위치/회전값이 세팅되기 전에 `OnEnable` 물리 연산이 실행되는 프레임 꼬임 현상을 해결했습니다. `Pop` 함수에서 **위치와 회전을 완벽히 정렬한 직후에 수동으로 오브젝트를 활성화**하도록 제어권을 가져와 물리 연산(투사체 궤적 휘어짐 등) 오류를 원천 차단했습니다.

#### 2. 🎵 데이터 주도형 Sound Manager
단순 재생을 넘어 실무 레벨의 디테일과 오디오 믹서(Audio Mixer) 연동을 고려한 사운드 매니저입니다.
- **ScriptableObject 기반 데이터 관리**: 사운드 클립들을 하드코딩하지 않고, `SoundDataContainer` (SO)를 통해 인스펙터에서 기획자나 사운드 디자이너가 직관적으로 관리할 수 있도록 Data-Driven 방식으로 설계했습니다.
- **오디오 믹서 dB 단위 제어**: 선형적인 0~1 볼륨값이 아닌 **로그 스케일(Logarithmic Scale) 기반의 데시벨(dB) 연산**을 적용하여 인간의 청각에 자연스러운 볼륨 조절을 구현했습니다.
- **BGM 코루틴 크로스페이드**: 기존 곡의 Fade-Out과 새 곡의 Fade-In을 부드럽게 처리하는 안전한 코루틴(Coroutine) 전환 로직을 구현했습니다.
- **SFX 중첩 재생(`PlayOneShot`) 최적화**: 연사 무기 사용 시 사운드 채널이 씹히는 문제를 방지하기 위해 `PlayOneShot`을 적용, 수십 개의 효과음이 단일 소스에서 자연스럽게 겹쳐 재생되도록 처리했습니다.

#### 3. 🎯 물리 연산 최적화 (투사체 시스템)
고속으로 이동하는 투사체 처리 시 물리 엔진 부하를 최소화하는 구조를 채택했습니다.
- **Kinematic & Trigger 방식 채택**: `Rigidbody`의 연산이 무거운 `Collision` 방식 대신, 투사체를 `isKinematic = true` 로 설정하고 통과형 `Trigger` 방식으로 충돌을 판정합니다.
- **성능 극대화**: 다수의 적 오브젝트가 존재하는 환경에서 적의 Rigidbody를 제거할 수 있는 기반을 마련하여 물리 연산 비용(CPU Physics Time)을 획기적으로 절감했습니다.
- **정확한 타격 판정**: 트리거 방식의 단점인 '충돌 지점(Contact Point)' 부재를 해결하기 위해 `other.ClosestPoint()`를 활용하여 가상의 피격 위치를 계산, 정확한 피격 이펙트 출력을 지원합니다.

### 💻 코드 사용 예시 (How to Use)

**1. 오브젝트 풀링 (투사체 발사)**
```csharp
// [Example Code: Commented out as requested]
// GameObject bullet = ObjectPoolManager.Instance.Pop(bulletPrefab, firePoint.position, firePoint.rotation);
```

**2. 사운드 재생 (BGM 페이드 및 SFX 연사)**
```csharp
// [Example Code: Commented out as requested]
// SoundManager.Instance.PlayBGMWithFade("Bgm_BossPhase", 1.5f);
// SoundManager.Instance.PlaySFX("Sfx_RifleFire");
```

### 📈 트러블 슈팅 (Trouble Shooting) & 회고

**Q. 투사체 발사 시 궤적이 제멋대로 휘어지는 현상**
* **원인**: 오브젝트 풀에서 꺼낼 때 `SetActive(true)`가 먼저 호출되면서, 투사체의 `OnEnable`에서 이전 프레임의 잘못된 회전값을 기준으로 물리력(Velocity)이 더해졌기 때문. 
* **해결**: `UnityEngine.Pool`의 기본 `ActionOnGet` 콜백을 비우고, Manager가 위치/회전을 먼저 초기화한 뒤 명시적으로 활성화하도록 로직을 수정하여 해결했습니다.

**Q. BGM 교체 시 코루틴 중복 실행으로 인한 볼륨 튀음 현상**
* **원인**: BGM 교체가 빠르게 여러 번 일어날 경우, 다수의 코루틴이 동시에 하나의 AudioSource 볼륨을 제어하며 Race Condition 발생.
* **해결**: `bgmFadeCoroutine` 변수를 두어 새로운 BGM 실행 시 기존 코루틴을 강제로 `StopCoroutine` 시키고 초기화하여 안전하게 전환되도록 제어 로직을 추가했습니다.
