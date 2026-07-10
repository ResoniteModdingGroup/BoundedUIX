using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;

namespace BoundedUIX
{
    internal sealed class SelectableUIXConfig : ConfigSection
    {
        private readonly DefiningConfigKey<bool> _allowLayoutSelectionKey = new("AllowLayoutSelection", "Allow selecting UIX RectTransforms that don't have any visual elements on them. Helps when wanting to select layout parents using repeated selection.", () => true)
        {
            new ConfigKeyPriority(4)
        };

        private readonly DefiningConfigKey<bool> _ignoreAlreadySelectedKey = new("IgnoreAlreadySelected", "Skip already selected elements in the targeting process. Helps with layered elements.", () => true)
        {
            new ConfigKeyPriority(2)
        };

        private readonly DefiningConfigKey<bool> _prioritizeHierarchyDepthKey = new("PrioritizeHierarchyDepth", "Prioritize the hierarchy depth of a potentially hit RectTransform over the layout order. Can help instead of or in addition to skipping already selected elements.", () => false)
        {
            new ConfigKeyPriority(3)
        };

        private readonly DefiningConfigKey<float> _repeatSelectionThresholdKey = new("RepeatSelectionThreshold", "The minimum local distance between targeting hits to consider it a 'new' selection attempt. Works in tandem with IgnoreAlreadySelected. Increase for more leniency. May need to be adjusted depending on the item.", () => 15, valueValidator: value => value >= 0)
        {
            new ConfigKeyPriority(1)
        };

        public bool AllowLayoutSelection => _allowLayoutSelectionKey;

        public override string Description => "Options for selecting UIX Elements with the Developer Tool.";

        public override string Id => "SelectableUIX";

        public bool IgnoreAlreadySelected => _ignoreAlreadySelectedKey;

        public override string Name => "Selectable UIX";

        public bool PrioritizeHierarchyDepth => _prioritizeHierarchyDepthKey;

        public float RepeatSelectionThreshold => _repeatSelectionThresholdKey;

        public override Version Version { get; } = new Version(1, 0, 0);

        public SelectableUIXConfig()
        {
            _repeatSelectionThresholdKey.Components.Add(new ConfigKeyEnabledSource<float>(_ignoreAlreadySelectedKey));
        }
    }
}