using FrooxEngine.UIX;
using FrooxEngine;
using HarmonyLib;
using Elements.Core;
using BoundedUIX.Gizmos;
using MonkeyLoader.Resonite.UI;
using MonkeyLoader.Resonite.UI.Inspectors;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;

namespace BoundedUIX
{
    internal sealed class RectTransformDiagnosis : ResoniteInspectorMonkey<RectTransformDiagnosis, BuildInspectorBodyEvent, RectTransform>
    {
        private readonly ConfigKeySessionShare<bool> _enabledShare = new(true);

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Low;

        // Exclude Enabled check to always generate, but use session share for visibility
        protected override bool AppliesTo(BuildInspectorBodyEvent eventData)
            => eventData.Worker is RectTransform;

        protected override void Handle(BuildInspectorBodyEvent eventData)
        {
            var rectTransform = (RectTransform)eventData.Worker;

            var button = eventData.UI.LocalActionButton("Visualize Preferred Area", button =>
            {
                button.Enabled = false;

                rectTransform.StartTask(async () =>
                {
                    while (!button.IsRemoved && !rectTransform.IsRemoved && (!rectTransform?.Canvas.IsRemoved ?? false))
                    {
                        var horizontal = rectTransform!.GetHorizontalMetrics().preferred;
                        var vertical = rectTransform.GetVerticalMetrics().preferred;
                        var area = rectTransform.ComputeGlobalComputeRect();
                        var color = colorX.Blue;

                        if (horizontal <= 0 && vertical <= 0)
                        {
                            horizontal = area.width;
                            vertical = area.height;
                            color = colorX.Red;
                        }
                        else if (horizontal <= 0)
                        {
                            horizontal = rectTransform.Canvas.UnitScale;
                            color = colorX.Purple;
                        }
                        else if (vertical <= 0)
                        {
                            vertical = rectTransform.Canvas.UnitScale;
                            color = colorX.Purple;
                        }

                        var pos = rectTransform.Canvas.Slot.LocalPointToGlobal(new float3(area.Center / rectTransform.Canvas.UnitScale));
                        pos -= 0.5f * UIXGizmoConfig.Offset * rectTransform.Canvas.Slot.Forward;

                        var size = rectTransform.Canvas.Slot.LocalScaleToGlobal(new float3(horizontal, vertical) / rectTransform.Canvas.UnitScale);

                        rectTransform.World.Debug.Box(pos, size, color.SetA(0.5f), rectTransform.Canvas.Slot.GlobalRotation);

                        await default(NextUpdate);
                    }
                });
            });

            _enabledShare.DriveFromVariable(button.Slot.ActiveSelf_Field);
        }

        protected override bool OnEngineReady()
        {
            ((IEntity<IDefiningConfigKey<bool>>)EnabledToggle!).Components.Add(_enabledShare);

            return base.OnEngineReady();
        }
    }
}