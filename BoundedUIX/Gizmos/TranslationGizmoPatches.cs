using FrooxEngine;
using HarmonyLib;

namespace BoundedUIX.Gizmos
{
    [HarmonyPatch(typeof(TranslationGizmo))]
    [HarmonyPatchCategory(nameof(UIXGizmos))]
    internal static class TranslationGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TranslationGizmo.SetTarget))]
        private static void SetTargetPostfix(TranslationGizmo __instance, Slot slot)
        {
            var moveableRect = slot.TryGetMovableRectTransform(out _);

            foreach (var child in __instance.Slot.Children)
                child.ActiveSelf = !moveableRect || !child.Name.Contains('Z') || !UIXGizmos.Enabled;
        }
    }
}