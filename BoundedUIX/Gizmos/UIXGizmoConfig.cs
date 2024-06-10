using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX.Gizmos
{
    internal sealed class UIXGizmoConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<float> _offsetKey = new("Offset", "Distance to raise the movement gizmos from the surface of the canvas. World scale.", () => 0.02f)
        {
            new ConfigKeyRange<float>(0, .2f)
        };

        public static float Offset => _offsetKey.GetValue();

        public override string Description => "Options for the modified UIX RectTransform Gizmos.";

        public override string Id => "Gizmos";

        public override Version Version { get; } = new Version(1, 0, 0);
    }
}