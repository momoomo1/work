using UnityEditor;
using UnityEngine;

// Assets/Characters 아래 모든 FBX의 Rig를 Humanoid로 통일 (모델과 애니메이션 FBX의 Rig 타입이 다르면 재생되지 않음)
public static class FixCharacterRigs
{
    [MenuItem("Tools/Setup/Fix Character Rigs (Humanoid)")]
    static void Fix()
    {
        int changed = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Characters" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
            if (importer.animationType == ModelImporterAnimationType.Human) continue;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            changed++;
            Debug.Log($"Humanoid로 변경: {path}");
        }
        Debug.Log(changed > 0 ? $"Rig 수정 완료 ({changed}개). 이제 Play해서 확인하세요."
                              : "수정할 FBX가 없습니다. 모두 이미 Humanoid입니다.");
    }
}
