using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX.Gizmos
{
    [HarmonyPatchCategory(nameof(UIXGizmos))]
    [HarmonyPatch(typeof(Gizmo), nameof(Gizmo.PositionAtTarget))]
    internal sealed class UIXGizmos : ConfiguredResoniteMonkey<UIXGizmos, UIXGizmoConfig>
    {
        public override bool CanBeDisabled => true;
        public override string Name => "UIX Gizmos";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

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