using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX.Inspector
{
    internal sealed class InspectorModificationConfig : ConfigSection
    {
        internal const string TargetSlotNamePlaceholder = "{TargetName}";

        private static readonly DefiningConfigKey<string> _childSlotNameKey = new("ChildSlotName", "Default name for child Slots in UIX hierarchies. Use {TargetName} to get the parent's name.", () => "Panel", valueValidator: name => !string.IsNullOrWhiteSpace(name));
        private static readonly DefiningConfigKey<bool> _moveTransformToParentKey = new("MoveTransformToParent", "Move RectTransform values up when creating a parent (analog to Slot transforms).", () => true);
        private static readonly DefiningConfigKey<string> _parentSlotNameKey = new("ParentSlotName", "Default name for parent Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name));
        private static readonly DefiningConfigKey<string> _pivotSlotNameKey = new("PivotSlotName", "Default name for pivot Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name));

        public static string ChildSlotName => _childSlotNameKey.GetValue()!;
        public static bool MoveTransformToParent => _moveTransformToParentKey.GetValue();
        public static string ParentSlotName => _parentSlotNameKey.GetValue()!;
        public static string PivotSlotName => _pivotSlotNameKey.GetValue()!;

        public override string Description => "Options for the Inspector modifications.";

        public override string Id => "Inspector";
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}