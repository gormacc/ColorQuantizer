﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ColorQuantizer
{
    public class OctreeNode
    {
        public ColorRgb Color = new ColorRgb(0,0,0);
        public int PixelCount = 0;
        public int PaletteIndex = 0;
        private OctreeNode[] _children = new OctreeNode[8];

        private const int MaxDepth = 8;

        public OctreeNode(int level, OctreeQuantizer parent)
        {
            if (level < MaxDepth - 1)
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

        public int GetNodesPixelCount()
        {
            return PixelCount + _children.Sum(node => node.PixelCount);
        }

        public void AddColor(ColorRgb color, int level, OctreeQuantizer parent)
        {
            if (level >= MaxDepth)
            {
                Color.Red += color.Red;
                Color.Green += color.Green;
                Color.Blue += color.Blue;
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
                    Color.Red += node.Color.Red;
                    Color.Green += node.Color.Green;
                    Color.Blue += node.Color.Blue;
                    PixelCount += node.PixelCount;
                    result += 1;
                }
            }

            return result - 1;
        }

        public int GetColorIndexForLevel(ColorRgb color, int level) // do rozkminy
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
