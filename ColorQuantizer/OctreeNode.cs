using System.Collections.Generic;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class OctreeNode
    {
        public ColorRgb Color = new ColorRgb(0,0,0);
        public int PixelCount = 0;
        public int PaletteIndex = 0;
        private readonly OctreeNode[] _children = new OctreeNode[8];

        private const int MaxDepth = 8;

        public OctreeNode(int level, OctreeQuantizerBase parent)
        {
            if (level < MaxDepth)
            {
                parent.AddLevelNode(level, this);
            }
        }

        public bool IsLeaf()
        {
            return PixelCount > 0;
        }

        public List<OctreeNode> GetLeafNodes()
        {
            var retList = new List<OctreeNode>();

            foreach (var node in _children)
            {
                if(node == null) continue;

                if (node.IsLeaf())
                {
                    retList.Add(node);
                }
                else
                {
                    retList.AddRange(node.GetLeafNodes());
                }
            }

            return retList;
        }

        public void AddColor(ColorRgb color, int level, OctreeQuantizerBase parent)
        {
            if (level >= MaxDepth || IsLeaf())
            {
                Color.AddColor(color);
                PixelCount += 1;
                return;
            }

            int index = GetColorIndexForLevel(color, level);
            if (_children[index] == null)
            {
                _children[index] = new OctreeNode(level, parent);
            }
            _children[index].AddColor(color,level + 1, parent);
        }

        public int GetPaletteIndex(ColorRgb color, int level)
        {
            if (this.IsLeaf()) return this.PaletteIndex;

            int index = GetColorIndexForLevel(color, level);

            if (_children[index] != null)
            {
                return _children[index].GetPaletteIndex(color, level + 1);
            }
            else
            {
                foreach (var node in _children)
                {
                    node?.GetColorIndexForLevel(color, level + 1);
                }
            }

            return 0;
        }

        public int RemoveLeaves()
        {
            int result = 0;

            foreach (var node in _children)
            {
                if (node != null)
                {

                    Color.AddColor(node.Color);
                    PixelCount += node.PixelCount;
                    if (node.PixelCount != 0)
                    {
                        result += 1;
                        node.PixelCount = 0;
                        node.Color = new ColorRgb(0, 0, 0);
                    }                   
                }
            }

            return result == 0 ? 0 : result - 1;
        }

        public int GetColorIndexForLevel(ColorRgb color, int level)
        {
            var index = 0;
            var mask = 0x80 >> level;

            if ((color.Red & mask) > 0)
            {
                index |= 4;
            }

            if ((color.Green & mask) > 0)
            {
                index |= 2;
            }

            if ((color.Blue & mask) > 0)
            {
                index |= 1;
            }

            return index;
        }

        public Color GetColor()
        {
            ColorRgb color = new ColorRgb(Color.Red / PixelCount,
                Color.Green / PixelCount, Color.Blue / PixelCount);

            return color.GetColor();
        }

    }
}
