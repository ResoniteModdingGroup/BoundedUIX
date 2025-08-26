using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Resonite.UI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BoundedUIX.Inspector
{
    [HarmonyPatch(typeof(SceneInspector))]
    [HarmonyPatchCategory(nameof(SlotAddingPatches))]
    internal static class SceneInspectorPatches
    {
        private static IEnumerable<CodeInstruction> LoadFromSceneInspector
        {
            get
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SceneInspector), nameof(SceneInspector.ComponentView)));
                yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(SyncRef<Slot>), nameof(SyncRef<Slot>.Target)));
            }
        }

        private static Slot OnAddChildPostfix(Slot newSlot, Slot targetSlot)
        {
            if (targetSlot.TryGetRectTransform(out _))
            {
                newSlot.Name = InspectorModificationConfig.ChildSlotName.Replace(InspectorModificationConfig.TargetSlotNamePlaceholder, targetSlot.Name);
                newSlot.AttachComponent<RectTransform>();
            }

            return newSlot;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SceneInspector.OnAddChildPressed))]
        private static IEnumerable<CodeInstruction> OnAddChildPressedTranspiler(IEnumerable<CodeInstruction> codeInstructions)
            => SlotAddingPatches.PostfixToAddSlot(codeInstructions, LoadFromSceneInspector, OnAddChildPostfix);

        private static Slot OnInsertParentPostfix(Slot newSlot, Slot targetSlot)
        {
            if (targetSlot.TryGetMovableRectTransform(out var originalTransform))
            {
                newSlot.Name = InspectorModificationConfig.ParentSlotName.Replace(InspectorModificationConfig.TargetSlotNamePlaceholder, targetSlot.Name);
                var newTransform = newSlot.AttachComponent<RectTransform>();

                if (InspectorModificationConfig.MoveTransformToParent)
                {
                    newTransform.CopyValues(originalTransform);
                    originalTransform.Reset();
                }
            }

            return newSlot;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SceneInspector.OnInsertParentPressed))]
        private static IEnumerable<CodeInstruction> OnInsertParentPressedTranspiler(IEnumerable<CodeInstruction> codeInstructions)
            => SlotAddingPatches.PostfixToAddSlot(codeInstructions, LoadFromSceneInspector, OnInsertParentPostfix);
    }
}