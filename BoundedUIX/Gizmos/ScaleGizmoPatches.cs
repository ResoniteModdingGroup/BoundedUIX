using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;

namespace BoundedUIX.Gizmos
{
    [HarmonyPatch(typeof(ScaleGizmo))]
    [HarmonyPatchCategory(nameof(UIXGizmos))]
    internal static class ScaleGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ScaleGizmo.OnInteractionBegin))]
        private static void OnInteractionBeginPostfix(ScaleGizmo __instance)
        {
            if (!UIXGizmos.Enabled || !__instance.TargetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
                return;

            var originalTransform = rectTransform.GetOriginal();
            originalTransform.Update(rectTransform);

            __instance.World.BeginUndoBatch("Scale Element");

            if (!originalTransform.Local)
            {
                rectTransform.OffsetMin.CreateUndoPoint(true);
                rectTransform.OffsetMax.CreateUndoPoint(true);
            }
            else
            {
                rectTransform.AnchorMin.CreateUndoPoint(true);
                rectTransform.AnchorMax.CreateUndoPoint(true);
            }

            __instance.World.EndUndoBatch();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ScaleGizmo.UpdatePoint))]
        private static void UpdatePointPostfix(ScaleGizmo __instance)
        {
            var targetSlot = __instance.TargetSlot.Target;
            if (!UIXGizmos.Enabled || !targetSlot.TryGetMovableRectTransform(out var rectTransform))
                return;

            var originalRect = rectTransform.GetOriginal();
            var scale = (targetSlot.LocalScale - originalRect.Scale).xy;
            var pxOffset = scale * originalRect.Size / 2f;

            if (!originalRect.Local)
            {
                if (rectTransform.OffsetMin.CanSet())
                    rectTransform.OffsetMin.Value = originalRect.OffsetMin - pxOffset;

                if (rectTransform.OffsetMax.CanSet())
                    rectTransform.OffsetMax.Value = originalRect.OffsetMax + pxOffset;
            }
            else
            {
                var anchorOffset = pxOffset / rectTransform.RectParent.ComputeGlobalComputeRect().size;

                if (rectTransform.AnchorMin.CanSet())
                    rectTransform.AnchorMin.Value = originalRect.AnchorMin - anchorOffset;

                if (rectTransform.AnchorMax.CanSet())
                    rectTransform.AnchorMax.Value = originalRect.AnchorMax + anchorOffset;
            }

            // Reset slot scale
            targetSlot.LocalScale = originalRect.Scale;
        }
    }
}