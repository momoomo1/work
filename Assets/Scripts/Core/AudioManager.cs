using System;
using System.Collections.Generic;
using UnityEngine;

// 배경음악 + 효과음. 이벤트(레벨업, 보스, 게임오버 등)는 Start에서 직접 구독.
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class SfxEntry
    {
        public Sfx id;
        public AudioClip[] clips;           // 여러 개면 랜덤 선택
        [Range(0f, 1f)] public float volume = 1f;
        public float minInterval = 0.05f;   // 같은 소리가 너무 자주 겹치지 않게
        public bool randomPitch = true;
    }

    [SerializeField] AudioClip bgm;
    [SerializeField, Range(0f, 1f)] float bgmVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] float masterVolume = 0.5f; // 전체 볼륨 (BGM과 효과음 모두에 곱해짐)
    [SerializeField] int voiceCount = 8;
    [SerializeField] List<SfxEntry> sfx = new List<SfxEntry>();

    static AudioManager instance;

    readonly Dictionary<Sfx, SfxEntry> map = new Dictionary<Sfx, SfxEntry>();
    readonly Dictionary<int, float> lastPlayed = new Dictionary<int, float>();
    AudioSource bgmSource;
    AudioSource[] voices;
    int nextVoice;

    GameManager gm;
    PlayerExperience exp;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        foreach (var e in sfx) map[e.id] = e;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = bgmVolume * masterVolume;

        voices = new AudioSource[Mathf.Max(1, voiceCount)];
        for (int i = 0; i < voices.Length; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
            voices[i].spatialBlend = 0f;
        }
    }

    void Start()
    {
        if (instance != this) return;

        if (bgm != null)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
        }

        gm = GameManager.Instance;
        gm.OnBossSpawned += HandleBossSpawned;
        gm.OnPlayerDied += HandlePlayerDied;
        gm.OnBossDefeated += HandleBossDefeated;

        var player = PlayerLocator.Player;
        if (player != null && player.TryGetComponent(out exp)) exp.OnLevelUp += HandleLevelUp;
    }

    void OnDestroy()
    {
        if (gm != null)
        {
            gm.OnBossSpawned -= HandleBossSpawned;
            gm.OnPlayerDied -= HandlePlayerDied;
            gm.OnBossDefeated -= HandleBossDefeated;
        }
        if (exp != null) exp.OnLevelUp -= HandleLevelUp;
        if (instance == this) instance = null;
    }

    void HandleBossSpawned(Boss _) => Play(Sfx.BossSpawn);
    void HandlePlayerDied() => Play(Sfx.GameOver);
    void HandleBossDefeated() => Play(Sfx.StageClear);
    void HandleLevelUp(int _) => Play(Sfx.LevelUp);

    // 씬에 AudioManager가 없으면 조용히 무시
    public static void Play(Sfx id)
    {
        if (instance != null) instance.PlayInternal(id);
    }

    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null || instance == null) return;
        if (!instance.Throttle(clip.GetInstanceID(), 0.05f)) return;
        instance.Emit(clip, volume, false);
    }

    void PlayInternal(Sfx id)
    {
        if (id == Sfx.None || !map.TryGetValue(id, out var e)) return;
        if (e.clips == null || e.clips.Length == 0) return;
        if (!Throttle(1000 + (int)id, e.minInterval)) return;
        Emit(e.clips[UnityEngine.Random.Range(0, e.clips.Length)], e.volume, e.randomPitch);
    }

    bool Throttle(int key, float interval)
    {
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(key, out var t) && now - t < interval) return false;
        lastPlayed[key] = now;
        return true;
    }

    void Emit(AudioClip clip, float volume, bool randomPitch)
    {
        if (clip == null) return;
        var src = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        src.pitch = randomPitch ? UnityEngine.Random.Range(0.95f, 1.05f) : 1f;
        src.PlayOneShot(clip, volume * sfxVolume * masterVolume);
    }
}
