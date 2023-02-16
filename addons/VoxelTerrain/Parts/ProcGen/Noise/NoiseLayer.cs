using Godot;
using System;

namespace VoxelPlugin {
public class NoiseLayer : IGenerator
{
    public int seed = 0;
    public Vector3 scale = Vector3.One;
    public float noiseWeight = 10.0f;
    public float startHeight = 10.0f;
    public float heightModifier = 0.1f;
    public string block = "";

    private FastNoiseLite noise = new FastNoiseLite();

    public NoiseLayer(string block) {
        this.block = block;
        noise.Seed = seed;
    }

	public void Generate(Chunk chunk) {
        BlockType blockType = BlockLibrary.GetBlockType(block);

        for(int x = 0; x < Chunk.SIZE.X; x++) {
            for(int y = 0; y < Chunk.SIZE.Y; y++) {
                for(int z = 0; z < Chunk.SIZE.Z; z++) {
                    Block block = chunk.grid[x,y,z];

                    float n = noise.GetNoise3Dv(block.position * scale)*noiseWeight + startHeight;
                    n -= block.position.Y * heightModifier;

                    if(n > 0.0f) Chunk.SuggestChange(block.position, blockType);
                }
            }
        }
    }
}
}
