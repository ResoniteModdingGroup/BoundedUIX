using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BoundedUIX.Inspector
{
    [HarmonyPatchCategory(nameof(SlotAddingPatches))]
    [HarmonyPatch(typeof(SlotPositioning), nameof(SlotPositioning.CreatePivotAtCenter), new[] { typeof(Slot), typeof(BoundingBox), typeof(bool) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
    internal static class CreatePivotPatch
    {
        private static Slot CreatePivotAddPostfix(Slot newSlot, Slot targetSlot)
        {
            if (targetSlot.TryGetMovableRectTransform(out var originalTransform))
            {
                newSlot.Name = InspectorModificationConfig.PivotSlotName.Replace(InspectorModificationConfig.TargetSlotNamePlaceholder, targetSlot.Name);
                newSlot.AttachComponent<RectTransform>();
            }

            return newSlot;
        }

        private static void Postfix(Slot slot, ref Slot __result)
        {
            // Can't be root slot when this gives true
            if (!slot.TryGetMovableRectTransform(out var originalTransform)
             || !__result.TryGetMovableRectTransform(out var pivotTransform))
                return;

            if (slot == __result)
            {
                __result = slot.Parent.AddSlot(InspectorModificationConfig.PivotSlotName.Replace(InspectorModificationConfig.TargetSlotNamePlaceholder, slot.Name));
                pivotTransform = __result.AttachComponent<RectTransform>();

                slot.SetParent(__result);
            }

            var originalArea = originalTransform.ComputeGlobalComputeRect();
            var parentArea = originalTransform.RectParent.ComputeGlobalComputeRect();

            var pivotAnchor = (originalArea.Center - parentArea.ExtentMin) / parentArea.size;
            var pivotOffset = originalArea.size / 2f;

            pivotTransform.AnchorMin.Value = pivotAnchor;
            pivotTransform.AnchorMax.Value = pivotAnchor;
            pivotTransform.OffsetMin.Value = -pivotOffset;
            pivotTransform.OffsetMax.Value = pivotOffset;

            originalTransform.ResetTransform();
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            return SlotAddingPatches.PostfixToAddSlot(codeInstructions, new[] { new CodeInstruction(OpCodes.Ldarg_0) }, CreatePivotAddPostfix);
        }
    }
}