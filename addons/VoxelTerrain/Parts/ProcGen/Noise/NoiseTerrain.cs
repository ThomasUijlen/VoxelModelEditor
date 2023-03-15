using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class NoiseTerrain : Generator
{
    public int seed = 0;
    public Vector3 scale = Vector3.One;
    public float noiseWeight = 3.0f;
    public float startHeight = 10.0f;
    public float heightModifier = 0.1f;
    public List<string> layers = new List<string>();

    private FastNoiseLite noise = new FastNoiseLite();

    public NoiseTerrain() {
        GD.Print("Added NoiseTerrain");
    }

    public override void ApplySettings(Godot.Collections.Dictionary<String, Variant> data) {
        Godot.Collections.Dictionary<String, Variant> settings = (Godot.Collections.Dictionary<String, Variant>) data["Settings"];

        seed = (int) settings["Seed"];
        startHeight = (float) settings["StartHeight"];
        foreach(String layer in ((Godot.Collections.Array) settings["Layers"])) {
            AddLayer(layer);
        }

        base.ApplySettings(data);
    }

    public void AddLayer(string block) {
        layers.Add(block);
    }

	public override void Generate(Chunk chunk) {
        noise.Seed = seed;
        int airLayers = 0;

        for(int y = 0; y < Chunk.SIZE.Y; y++) {
            bool airLayer = true;
            for(int x = 0; x < Chunk.SIZE.X; x++) {
                for(int z = 0; z < Chunk.SIZE.Z; z++) {
					Block block = chunk.grid[y,x,z];

                    float n = noise.GetNoise3Dv(block.position * scale)*noiseWeight + startHeight/noiseWeight;
                    n -= block.position.Y * heightModifier;

                    if(n > 0.0f) {
                        airLayer = false;
                        for(int layer = 0; layer < layers.Count; layer++) {
                            Vector3I pos = new Vector3I(0,layer,0);
                            Chunk.SuggestChange(chunk, block.position+pos, BlockLibrary.GetBlockType(layers[layer]), 0);
                        }
                    }
                }
            }

            if(airLayer) airLayers++;
            if(airLayers > 3) break;
        }

        base.Generate(chunk);
    }
}
}
