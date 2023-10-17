using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(SlotGizmo))]
    internal static class SlotGizmoPatches
    {
        private static readonly Type RotationGizmoType = typeof(RotationGizmo);

        private static BoundingBox BoundUIX(BoundingBox bounds, Slot target, Slot space)
        {
            if (!BoundedUIX.EnableUIXGizmos || !target.TryGetMovableRectTransform(out var rectTransform))
                return bounds;

            var area = rectTransform.ComputeGlobalComputeRect();
            bounds.Encapsulate(space.GlobalPointToLocal(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMin / rectTransform.Canvas.UnitScale)));
            bounds.Encapsulate(space.GlobalPointToLocal(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMax / rectTransform.Canvas.UnitScale)));

            return bounds;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SlotGizmo.OnCommonUpdate))]
        private static IEnumerable<CodeInstruction> OnCommonUpdateTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var boundUIXMethod = typeof(SlotGizmoPatches).GetMethod(nameof(BoundUIX), AccessTools.allDeclared);
            var computeBoundingBoxMethod = typeof(BoundsHelper).GetMethod(nameof(BoundsHelper.ComputeBoundingBox), AccessTools.allDeclared);
            var getGlobalPositionMethod = typeof(Slot).GetProperty(nameof(Slot.GlobalPosition), AccessTools.allDeclared).GetMethod;
            var uixBoundCenterMethod = typeof(SlotGizmoPatches).GetMethod(nameof(UIXBoundCenter), AccessTools.allDeclared);

            var instructions = codeInstructions.ToList();

            var globalPositionIndex = instructions.FindIndex(instruction => instruction.Calls(getGlobalPositionMethod));

            if (globalPositionIndex < 0)
                return instructions;

            instructions[globalPositionIndex] = new CodeInstruction(OpCodes.Call, uixBoundCenterMethod);

            var computeIndex = instructions.FindIndex(globalPositionIndex, instruction => instruction.Calls(computeBoundingBoxMethod));

            if (computeIndex < 0)
                return instructions;

            instructions.Insert(computeIndex + 1, instructions[computeIndex - 5]);
            instructions.Insert(computeIndex + 2, instructions[computeIndex - 3]);
            instructions.Insert(computeIndex + 3, new CodeInstruction(OpCodes.Call, boundUIXMethod));

            return instructions;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SlotGizmo.Setup))]
        private static void SetupPostfix(SlotGizmo __instance)
        {
            __instance.IsLocalSpace.OnValueChange += field => __instance.SwitchSpace();
            
            var moveableRect = __instance._targetSlot.Target.TryGetMovableRectTransform(out var rectTransform);

            if (moveableRect)
                rectTransform.GetOriginal().Local = __instance.IsLocalSpace.Value;

            if (__instance._scaleGizmo.Target._zSlot.Target is Slot zSlot)
                zSlot.ActiveSelf = !moveableRect || !BoundedUIX.EnableUIXGizmos;

            // Hide blue z line of the gizmo
            if (__instance._scaleGizmo.Target.Slot.GetComponent<MeshRenderer>(r => r.Materials[0] is OverlayFresnelMaterial material && material.FrontNearColor == colorX.Blue) is MeshRenderer renderer)
                renderer.Enabled = !moveableRect || !BoundedUIX.EnableUIXGizmos;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SlotGizmo.SwitchSpace))]
        private static bool SwitchSpacePrefix(SlotGizmo __instance)
        {
            var local = __instance.IsLocalSpace.Value;
            var target = __instance._targetSlot.Target;

            if (BoundedUIX.EnableUIXGizmos && target.TryGetMovableRectTransform(out var rectTransform))
            {
                // Always let it set local space for the translation gizmos on rect transforms
                rectTransform.GetOriginal().Local = local;
                local = true;
            }

            __instance._translationGizmo.Target.SetTarget(target.Position_Field, target, local);
            __instance._rotationGizmo.Target.SetTarget(target.Rotation_Field, target, local);

            return false;
        }

        private static float3 UIXBoundCenter(Slot target)
        {
            if (!BoundedUIX.EnableUIXGizmos || !target.TryGetMovableRectTransform(out var rectTransform))
                return target.GlobalPosition;

            return rectTransform.GetGlobalBounds().Center - (BoundedUIX.GizmoOffset * rectTransform.Canvas.Slot.Forward);
        }
    }
}