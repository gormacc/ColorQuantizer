using System.Collections.Generic;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class OctreeQuantizer
    {
        public const int MaxDepth = 8;

        public List<OctreeNode>[] levels = new List<OctreeNode>[MaxDepth];

        public OctreeNode Root;        

        public OctreeQuantizer()
        {
            for (int i = 0; i < MaxDepth; i++)
            {
                levels[i] = new List<OctreeNode>();
            }

            Root = new OctreeNode(0, this);
        }

        public List<OctreeNode> GetLeaves()
        {
            return Root.GetLeafNodes();
        }

        public void AddLevelNode(int level, OctreeNode node)
        {
            levels[level].Add(node);
        }

        public void AddColor(ColorRgb color)
        {
            Root.AddColor(color, 0, this);
        }

        public List<Color> MakePalette(int colorCount)
        {
            var palette = new List<Color>();
            int paletteIndex = 0;

            int leafCount = GetLeaves().Count;

            for (int i = MaxDepth - 1; i >= 0; i--)
            {
                if (levels[i] != null)
                {
                    foreach (var node in levels[i])
                    {
                        leafCount -= node.RemoveLeaves();

                        if (leafCount <= colorCount) break;
                    }

                    if (leafCount <= colorCount) break;

                    levels[i] = new List<OctreeNode>();
                }
            }

            foreach (var node in GetLeaves())
            {
                if (paletteIndex >= colorCount) break;

                if (node.IsLeaf())
                {
                    palette.Add(node.GetColor());
                }

                node.PaletteIndex = paletteIndex;
                paletteIndex += 1;
            }

            return palette;
        }

        public int GetPalletteIndex(ColorRgb color)
        {
            return Root.GetPaletteIndex(color, 0);
        }
    }
}
