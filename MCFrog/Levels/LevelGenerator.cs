using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineFrog
{
    public abstract class LevelGenerator
    {
        public abstract void Generate(Level lvl);
    }

    public class FlatGrassGenerator : LevelGenerator
    {
        public FlatGrassGenerator(Level lvl)
        {
            Generate(lvl);
        }
        public override void Generate(Level lvl)
        {
            var half = (ushort)(lvl.SizeY / 2);
            for (ushort x = 0; x < lvl.SizeX; ++x)
            {
                for (ushort z = 0; z < lvl.SizeZ; ++z)
                {
                    for (ushort y = 0; y < lvl.SizeY; ++y)
                    {
                        if (y != half)
                        {
                            lvl.SetTile(x, y, z, (byte)((y >= half) ? MCBlocks.Air : MCBlocks.Dirt));
                        }
                        else
                        {
                            lvl.SetTile(x, y, z, (byte)MCBlocks.Grass);
                        }
                    }
                }
            }
        }
    }
}
