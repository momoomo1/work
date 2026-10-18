using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 프로젝트 안의 AudioClip을 파일 이름 키워드로 찾아 AudioManager와 스킬에 자동 연결 (이후 인스펙터에서 수정 가능)
public static class SetupAudio
{
    static readonly Dictionary<Sfx, string[]> Keys = new Dictionary<Sfx, string[]>
    {
        { Sfx.LevelUp,    new[] { "levelup", "level_up", "level up", "powerup", "power_up", "upgrade", "win" } },
        { Sfx.BossSpawn,  new[] { "boss", "roar", "growl", "alarm", "warning" } },
        { Sfx.PlayerHit,  new[] { "player_hit", "hurt", "pain", "damage", "grunt" } },
        { Sfx.EnemyHit,   new[] { "hit", "impact", "punch", "slap", "hurt" } },
        { Sfx.EnemyDeath, new[] { "death", "die", "kill", "squish", "monster" } },
        { Sfx.GameOver,   new[] { "gameover", "game_over", "game over", "lose", "fail", "defeat" } },
        { Sfx.StageClear, new[] { "victory", "clear", "fanfare", "win", "success" } },
    };

    static readonly Dictionary<string, string[]> SkillKeys = new Dictionary<string, string[]>
    {
        { "sura",      new[] { "shoot", "laser", "magic", "slash", "swish" } },
        { "vacuum",    new[] { "whoosh", "swoosh", "wind", "slash" } },
        { "hellfire",  new[] { "explosion", "explode", "boom", "fire" } },
        { "frost",     new[] { "ice", "frost", "freeze", "cold", "glass" } },
    };

    [MenuItem("Tools/Setup/Create AudioManager")]
    static void Create()
    {
        var mgr = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
        if (mgr == null)
        {
            var go = new GameObject("AudioManager");
            mgr = go.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create AudioManager");
        }

        var clips = LoadAllClips();
        if (clips.Count == 0)
            Debug.LogWarning("프로젝트에 AudioClip이 없습니다. 사운드를 임포트한 뒤 이 메뉴를 다시 실행하거나 인스펙터에서 직접 연결하세요.");

        var so = new SerializedObject(mgr);

        // BGM: 키워드 일치 우선, 없으면 40초 이상인 가장 긴 클립
        var bgm = Find(clips, new[] { "bgm", "music", "theme", "ambient", "loop" }, 1, c => c.length >= 20f);
        AudioClip bgmClip = bgm.Count > 0 ? bgm[0] : null;
        if (bgmClip == null)
            foreach (var c in clips)
                if (c.length >= 40f && (bgmClip == null || c.length > bgmClip.length)) bgmClip = c;
        if (bgmClip != null) so.FindProperty("bgm").objectReferenceValue = bgmClip;

        // SFX 목록 (기존 항목은 새로 구성)
        var list = so.FindProperty("sfx");
        var ids = new List<Sfx>((Sfx[])Enum.GetValues(typeof(Sfx)));
        ids.Remove(Sfx.None);
        list.arraySize = ids.Count;
        var report = new System.Text.StringBuilder();
        for (int i = 0; i < ids.Count; i++)
        {
            var found = Find(clips, Keys[ids[i]], 3, c => c.length <= 8f);
            var e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("id").enumValueIndex = (int)ids[i];
            e.FindPropertyRelative("volume").floatValue = 1f;
            e.FindPropertyRelative("minInterval").floatValue = ids[i] == Sfx.EnemyHit || ids[i] == Sfx.EnemyDeath ? 0.06f : 0.1f;
            e.FindPropertyRelative("randomPitch").boolValue = ids[i] == Sfx.EnemyHit || ids[i] == Sfx.EnemyDeath || ids[i] == Sfx.PlayerHit;
            var arr = e.FindPropertyRelative("clips");
            arr.arraySize = found.Count;
            for (int k = 0; k < found.Count; k++) arr.GetArrayElementAtIndex(k).objectReferenceValue = found[k];
            report.AppendLine($"  {ids[i]}: {(found.Count > 0 ? found[0].name + (found.Count > 1 ? $" 외 {found.Count - 1}" : "") : "(없음)")}");
        }
        so.ApplyModifiedProperties();

        // 스킬 발동 사운드 (비어 있는 것만)
        foreach (var kv in SkillKeys)
        {
            var data = AssetDatabase.LoadAssetAtPath<SkillData>($"Assets/Data/Skills/Skill_{Cap(kv.Key)}.asset");
            if (data == null || data.castSfx != null) continue;
            var found = Find(clips, kv.Value, 1, c => c.length <= 5f);
            if (found.Count > 0)
            {
                data.castSfx = found[0];
                EditorUtility.SetDirty(data);
                report.AppendLine($"  스킬 {kv.Key}: {found[0].name}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"AudioManager 설정 완료. BGM: {(bgmClip != null ? bgmClip.name : "(없음)")}\n{report}마음에 안 드는 항목은 AudioManager 인스펙터에서 교체하세요. 씬을 저장(Ctrl+S)하세요.");
    }

    static string Cap(string s) => char.ToUpper(s[0]) + s.Substring(1);

    static List<AudioClip> LoadAllClips()
    {
        var result = new List<AudioClip>();
        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
        {
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
            if (c != null) result.Add(c);
        }
        return result;
    }

    // 키워드 순서대로 이름이 일치하는 클립을 최대 max개 반환
    static List<AudioClip> Find(List<AudioClip> clips, string[] keys, int max, Func<AudioClip, bool> filter)
    {
        var found = new List<AudioClip>();
        foreach (var key in keys)
        {
            foreach (var c in clips)
            {
                if (found.Count >= max) return found;
                if (found.Contains(c) || !filter(c)) continue;
                if (c.name.ToLowerInvariant().Contains(key)) found.Add(c);
            }
        }
        return found;
    }
}
