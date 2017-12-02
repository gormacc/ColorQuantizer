using System;

namespace ColorQuantizer
{
    public class OctreeQuantizerInstantReduction : OctreeQuantizerBase
    {
        public OctreeQuantizerInstantReduction(int colorCount) : base(colorCount)
        {
        }

        public override void AddColor(ColorRgb color)
        {
            base.AddColor(color);

            base.ReduceLeaves();
        }

        
    }
}
