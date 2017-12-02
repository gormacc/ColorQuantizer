using System.Collections.Generic;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class OctreeQuantizerNormal : OctreeQuantizerBase
    {
        public OctreeQuantizerNormal(int colorCount) : base(colorCount)
        {
        }

        public override List<Color> MakePalette()
        {
            base.ReduceLeaves();

            return base.MakePalette();
        }
    }
}
