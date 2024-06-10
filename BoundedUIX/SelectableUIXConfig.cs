using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    internal sealed class SelectableUIXConfig : ConfigSection
    {
        private readonly DefiningConfigKey<bool> _ignoreAlreadySelectedKey = new("IgnoreAlreadySelected", "Skip already selected elements in the targeting process. Helps with layered elements.", () => true);
        private readonly DefiningConfigKey<bool> _prioritizeHierarchyDepthKey = new("PrioritizeHierarchyDepth", "Prioritize the hierarchy depth of a potentially hit RectTransform over the layout order. Can help instead of or in addition to skipping already selected elements.", () => false);
        private readonly DefiningConfigKey<float> _repeatSelectionThresholdKey = new("RepeatSelectionThreshold", "The minimum local distance between targeting hits to consider it a 'new' selection attempt. Works in tandem with IgnoreAlreadySelected. Increase for more leniency. May need to be adjusted depending on the item.", () => 15, valueValidator: value => value >= 0);

        public override string Description => "Options for selecting UIX Elements with the Developer Tool.";
        public override string Id => "SelectableUIX";

        public bool IgnoreAlreadySelected => _ignoreAlreadySelectedKey.GetValue();

        public override string Name => "Selectable UIX";

        public bool PrioritizeHierarchyDepth => _prioritizeHierarchyDepthKey.GetValue();

        public float RepeatSelectionThreshold => _repeatSelectionThresholdKey.GetValue();

        public override Version Version { get; } = new Version(1, 0, 0);
    }
}