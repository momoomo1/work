using UnityEditor;
using UnityEngine;

// 이미 준비된 개별 Setup 메뉴를 한 번에 실행 (순서 중요)
public static class SetupAll
{
    [MenuItem("Tools/Setup/Apply Everything (Skills, VFX, Weapons, Map, Audio)")]
    static void Run()
    {
        if (!EditorUtility.DisplayDialog("전체 적용",
                "스킬 이름, 이펙트(Cartoon FX + Free Slash + UNI VFX), 무기 장착, 공장 맵, 오디오를 한 번에 적용합니다.\n" +
                "씬의 DungeonMap/FactoryProps 오브젝트는 새로 만들어집니다. 진행할까요?", "진행", "취소"))
            return;

        RenameSkills.Rename();
        SetupVfxWindow.ApplyUniPreset();
        SetupWeapons.Attach();
        SetupMapWindow.BuildFactoryPreset();
        SetupAudio.Create();

        AssetDatabase.SaveAssets();
        Debug.Log("전체 적용 완료. 콘솔의 경고(프리셋 에셋을 찾지 못함 등)를 확인하고 씬을 저장(Ctrl+S)하세요.");
    }
}
