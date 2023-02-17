using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class NoiseLayer : IGenerator
{
    public int seed = 0;
    public Vector3 scale = Vector3.One;
    public float noiseWeight = 3.0f;
    public float startHeight = 10.0f;
    public float heightModifier = 0.1f;
    public List<string> layers = new List<string>();

    private FastNoiseLite noise = new FastNoiseLite();

    public NoiseLayer(string firstLayer, float startHeight) {
        AddLayer(firstLayer);
        this.startHeight = startHeight;
        noise.Seed = seed;
    }

    public void AddLayer(string block) {
        layers.Add(block);
    }

	public void Generate(Chunk chunk) {
        for(int x = 0; x < Chunk.SIZE.X; x++) {
            for(int y = 0; y < Chunk.SIZE.Y; y++) {
                for(int z = 0; z < Chunk.SIZE.Z; z++) {
                    Block block = chunk.grid[x,y,z];

                    float n = noise.GetNoise3Dv(block.position * scale)*noiseWeight + startHeight/noiseWeight;
                    n -= block.position.Y * heightModifier;

                    if(n > 0.0f) {
                        for(int layer = 0; layer < layers.Count; layer++) {
                            Vector3I pos = new Vector3I(0,layer,0);
                            Chunk.SuggestChange(block.position+pos, BlockLibrary.GetBlockType(layers[layer]));
                        }
                    }
                }
            }
        }
    }
}
}
