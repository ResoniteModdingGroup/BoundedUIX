using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BoundedUIX
{
    internal static class Helpers
    {
        private static readonly ConditionalWeakTable<RectTransform, OriginalRect> _originalRects = [];

        public static bool Contains(this BoundingBox2D boundingBox, float2 point)
            => (point >= boundingBox.Min).All() && (point <= boundingBox.Max).All();

        public static OriginalRect GetOriginal(this RectTransform rectTransform)
            => _originalRects.GetOrCreateValue(rectTransform);

        public static bool TryGetMovableRectTransform(this Slot slot, [NotNullWhen(true)] out RectTransform? rectTransform)
            => slot.TryGetRectTransform(out rectTransform) && rectTransform.Slot != rectTransform.Canvas.Slot;

        public static bool TryGetRectTransform(this Slot slot, [NotNullWhen(true)] out RectTransform? rectTransform)
        {
            if (slot?.GetComponent<RectTransform>() is RectTransform rt && rt.Canvas != null)
            {
                rectTransform = rt;
                return true;
            }

            rectTransform = null;
            return false;
        }
    }
}