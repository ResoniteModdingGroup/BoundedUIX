using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BoundedUIX
{
    [HarmonyPatchCategory(nameof(DevToolSelectableUIX))]
    [HarmonyPatch(typeof(DevTool), nameof(DevTool.TryOpenGizmo))]
    internal sealed class DevToolSelectableUIX : ConfiguredResoniteMonkey<DevToolSelectableUIX, SelectableUIXConfig>
    {
        private static float2 _lastPosition = float2.MaxValue;
        private static Slot? _lastSlot = null;
        public override bool CanBeDisabled => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static Slot CheckCanvas(RaycastHit hit)
        {
            var bestSlot = hit.Collider.Slot;

            if (Enabled && bestSlot.TryGetRectTransform(out var rectTransform) && rectTransform.Canvas.Slot == bestSlot)
            {
                bestSlot = FindBestRect(bestSlot.GlobalPointToLocal(hit.Point).xy, bestSlot);

                if (bestSlot.GetComponentInParents<Button>() is Button button && button.Slot.HierachyDepth > rectTransform.Slot.HierachyDepth)
                    bestSlot = button.Slot;
            }

            return bestSlot;
        }

        private static Slot FindBestRect(float2 hitPoint, Slot best)
        {
            var prioritizeDepth = ConfigSection.PrioritizeHierarchyDepth;
            var allowLayoutSelection = ConfigSection.AllowLayoutSelection;

            var ignoreSelected = ConfigSection.IgnoreAlreadySelected && _lastSlot == best
                && (MathX.Distance(_lastPosition, hitPoint) < ConfigSection.RepeatSelectionThreshold);

            _lastPosition = hitPoint;
            _lastSlot = best;

            var traversal = new Stack<Slot>();
            traversal.Push(best);

            while (traversal.Count > 0)
            {
                var current = traversal.Pop();

                if (!current.TryGetRectTransform(out var rectTransform)
                 || (ignoreSelected && current.TryGetGizmo<SlotGizmo>() is not null))               // Not already selected - to help when something's in the way
                    continue;

                var isHit = rectTransform.GetCanvasBounds().Contains(hitPoint);
                var hasGraphic = rectTransform.Graphic != null;

                if (isHit && (allowLayoutSelection || (hasGraphic && (!rectTransform.IsMask || rectTransform.IsMaskVisible)))   // Has anything possibly visible in the bounds
                 && (!prioritizeDepth || best.HierachyDepth <= current.HierachyDepth))              // Hierarchy depth at least as deep when prioritizing
                    best = current;

                if (rectTransform.IsMask && (!isHit || !hasGraphic))
                    continue;

                foreach (var child in current.Children.Where(child => child.ActiveSelf).Reverse())
                    traversal.Push(child);
            }

            return best;
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryOpenGizmoTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var checkCanvasHitMethod = AccessTools.DeclaredMethod(typeof(DevToolSelectableUIX), nameof(CheckCanvas));
            var colliderField = AccessTools.Field(typeof(RaycastHit), nameof(RaycastHit.Collider));

            var instructions = codeInstructions.ToList();
            var raycastValueIndex = instructions.FindLastIndex(instruction => instruction.LoadsField(colliderField));

            instructions.RemoveAt(raycastValueIndex);
            instructions[raycastValueIndex] = new CodeInstruction(OpCodes.Call, checkCanvasHitMethod);

            return instructions;
        }
    }
}