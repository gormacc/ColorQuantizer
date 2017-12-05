using System.Collections.Generic;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class OctreeQuantizerBase
    {
        public const int MaxDepth = 8;

        public List<OctreeNode>[] Levels = new List<OctreeNode>[MaxDepth];

        protected int ColorCount = 0;

        public OctreeNode Root;        

        public OctreeQuantizerBase(int colorCount)
        {
            ColorCount = colorCount;

            for (int i = 0; i < MaxDepth; i++)
            {
                Levels[i] = new List<OctreeNode>();
            }

            Root = new OctreeNode(0, this);
        }

        public List<OctreeNode> GetLeaves()
        {
            return Root.GetLeafNodes();
        }

        public void AddLevelNode(int level, OctreeNode node)
        {
            Levels[level].Add(node);
        }

        public virtual void AddColor(ColorRgb color)
        {
            Root.AddColor(color, 0, this);
        }

        public virtual List<Color> MakePalette()
        {
            var palette = new List<Color>();
            int paletteIndex = 0;

            var list = GetLeaves();

            foreach (var node in list)
            {
                if (paletteIndex >= ColorCount) break;

                if (node.IsLeaf())
                {
                    palette.Add(node.GetColor());
                }

                node.PaletteIndex = paletteIndex;
                paletteIndex += 1;
            }

            return palette;
        }

        protected void ReduceLeaves()
        {
            int leafCount = GetLeaves().Count; // do poprawy

            if(leafCount <= ColorCount) return;

            for (int i = MaxDepth - 1; i >= 0; i--)
            {
                if (Levels[i].Count != 0)
                {
                    foreach (var node in Levels[i])
                    {
                        leafCount -= node.RemoveLeaves();

                        if (leafCount <= ColorCount) break;
                    }

                    if (leafCount <= ColorCount) break;
                }
            }

        }

        public int GetPalletteIndex(ColorRgb color)
        {
            return Root.GetPaletteIndex(color, 0);
        }
    }
}
