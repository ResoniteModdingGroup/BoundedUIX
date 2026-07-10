using MonkeyLoader.Configuration;

namespace BoundedUIX.Inspector
{
    internal sealed class InspectorModificationConfig : ConfigSection
    {
        internal const string TargetSlotNamePlaceholder = "{TargetName}";

        private static readonly DefiningConfigKey<string> _childSlotNameKey = new("ChildSlotName", "Default name for child Slots in UIX hierarchies. Use {TargetName} to get the parent's name.", () => "Panel", valueValidator: name => !string.IsNullOrWhiteSpace(name))
        {
            new ConfigKeyPriority(2)
        };

        private static readonly DefiningConfigKey<bool> _moveTransformToParentKey = new("MoveTransformToParent", "Move RectTransform values up when creating a parent (analog to Slot transforms).", () => true)
        {
            new ConfigKeyPriority(4)
        };

        private static readonly DefiningConfigKey<string> _parentSlotNameKey = new("ParentSlotName", "Default name for parent Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name))
        {
            new ConfigKeyPriority(3)
        };

        private static readonly DefiningConfigKey<string> _pivotSlotNameKey = new("PivotSlotName", "Default name for pivot Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name))
        {
            new ConfigKeyPriority(1)
        };

        public static string ChildSlotName => _childSlotNameKey!;

        public static bool MoveTransformToParent => _moveTransformToParentKey;

        public static string ParentSlotName => _parentSlotNameKey!;

        public static string PivotSlotName => _pivotSlotNameKey!;

        public override string Description => "Options for the Inspector modifications.";

        public override string Id => "Inspector";

        public override Version Version { get; } = new Version(1, 0, 0);
    }
}