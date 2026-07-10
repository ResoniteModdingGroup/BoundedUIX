using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI;

namespace BoundedUIX.Gizmos
{
    [HarmonyPatchCategory(nameof(UIXGizmos))]
    [HarmonyPatch(typeof(Gizmo), nameof(Gizmo.PositionAtTarget))]
    internal sealed class UIXGizmos : ConfiguredResoniteMonkey<UIXGizmos, UIXGizmoConfig>
    {
        public override bool CanBeDisabled => true;

        public override string Name => "UIX Gizmos";

        [HarmonyPrefix]
        private static bool PositionAtTargetPrefix(Gizmo __instance)
        {
            if (!Enabled || !__instance.TargetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
                return true;

            var center = rectTransform.GetGlobalBounds().Center;
            rectTransform.GetOriginal().Center = center;

            __instance.Slot.GlobalPosition = center - (UIXGizmoConfig.Offset * rectTransform.Canvas.Slot.Forward);
            __instance.Slot.GlobalRotation = rectTransform.Canvas.Slot.GlobalRotation;

            return false;
        }
    }
}