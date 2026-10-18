using System;
using System.Reflection;
using UnityEngine;

// VFX Graph(UnityEngine.VFX.VisualEffect) 이펙트를 리플렉션으로 다룬다.
// 패키지(com.unity.visualeffectgraph)가 없어도 컴파일되도록 직접 참조하지 않는다.
public static class VisualEffectUtil
{
    static readonly Type vfxType = FindType();
    static readonly MethodInfo reinit = vfxType?.GetMethod("Reinit", Type.EmptyTypes);
    static readonly MethodInfo play = vfxType?.GetMethod("Play", Type.EmptyTypes);

    static readonly Component[] none = new Component[0];

    static Type FindType()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("UnityEngine.VFX.VisualEffect", false);
            if (t != null) return t;
        }
        return null;
    }

    public static Component[] GetAll(GameObject root)
        => vfxType == null ? none : root.GetComponentsInChildren(vfxType, true);

    // 풀에서 꺼낸 이펙트를 처음부터 다시 재생
    public static void Restart(Component[] effects)
    {
        if (effects == null) return;
        foreach (var e in effects)
        {
            if (e == null) continue;
            reinit?.Invoke(e, null);
            play?.Invoke(e, null);
        }
    }
}
