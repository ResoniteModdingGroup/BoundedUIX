using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX.Inspector
{
    internal delegate Slot AddSlotPostfix(Slot newSlot, Slot targetSlot);

    internal sealed class SlotAddingPatches : ConfiguredResoniteMonkey<SlotAddingPatches, InspectorModificationConfig>
    {
        internal static IEnumerable<CodeInstruction> PostfixToAddSlot(IEnumerable<CodeInstruction> codeInstructions, IEnumerable<CodeInstruction> targetSlotLoadInstructions, AddSlotPostfix postfix)
        {
            var addSlotMethod = typeof(Slot).GetMethod(nameof(Slot.AddSlot));

            foreach (var code in codeInstructions)
            {
                yield return code;

                if (code.Calls(addSlotMethod))
                {
                    foreach (var loadInstruction in targetSlotLoadInstructions)
                        yield return loadInstruction;

                    yield return new CodeInstruction(OpCodes.Call, postfix.Method);
                }
            }
        }

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();
    }
}