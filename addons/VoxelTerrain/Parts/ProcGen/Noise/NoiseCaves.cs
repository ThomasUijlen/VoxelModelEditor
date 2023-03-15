using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class NoiseCaves : Generator
{
    public int seed = 0;
    public int radius = 4;
    public int minLength = 10;
    public int maxLength = 100;
    public int minBranches = 0;
    public int maxBranches = 5;
    public Vector3 scale = Vector3.One;
    public float caveSpawnChance = 0.01f;

    private FastNoiseLite noise = new FastNoiseLite();
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public NoiseCaves() {
        GD.Print("Added NoiseCaves");
    }

    public override void ApplySettings(Godot.Collections.Dictionary<String, Variant> data) {
        Godot.Collections.Dictionary<String, Variant> settings = (Godot.Collections.Dictionary<String, Variant>) data["Settings"];

        seed = (int) settings["Seed"];
        caveSpawnChance = (float) settings["SpawnChance"];
        radius = (int) settings["Radius"];
        minLength = (int) settings["MinLength"];
        maxLength = (int) settings["MaxLength"];
        minBranches = (int) settings["MinBranches"];
        maxBranches = (int) settings["MaxBranches"];

        base.ApplySettings(data);
    }

	public override void Generate(Chunk chunk) {
        BlockType air = BlockLibrary.GetBlockType("Air");

        rng.Seed = Convert.ToUInt64(seed + Mathf.RoundToInt(Mathf.Abs(noise.GetNoise3Dv(chunk.position*1000f))*100));

        float n = rng.RandfRange(0f,100f);

        if(n < caveSpawnChance) {
            Block block = chunk.GetRandomBlock(rng);

            int tries = 50;
            while(block.blockType == air && tries > 0) {
                tries -= 1;
                block = chunk.GetRandomBlock(rng);
            }

            CreateCave(
            block.position,
            new Vector3(rng.RandfRange(-1,1), rng.RandfRange(-1,1), rng.RandfRange(-1,1)),
            radius,
            rng.RandiRange(minLength, maxLength),
            rng.RandiRange(minBranches, maxBranches),
            air,
            chunk,
            new List<Vector3>());
        }

        base.Generate(chunk);
    }

    private void CreateCave(
        Vector3 currentPosition,
        Vector3 direction,
        int radius,
        int remainingLength,
        int branches,
        BlockType blockType,
        Chunk chunk,
        List<Vector3> cavePositions
        ) {

        direction += new Vector3(rng.RandfRange(-0.2f, 0.2f), rng.RandfRange(-0.2f, 0.2f), rng.RandfRange(-0.2f, 0.2f));
        direction = direction.Normalized();

        currentPosition += direction*2f;
        cavePositions.Add(currentPosition);
        
        for(int x = -radius; x < radius; x++) {
            for(int y = -radius; y < radius; y++) {
                for(int z = -radius; z < radius; z++) {
                    Vector3 pos = new Vector3(x,y,z);
                    float d = pos.DistanceTo(Vector3.Zero)+noise.GetNoise3Dv(currentPosition + pos * scale);
                    if(d > radius) continue;
                    Chunk.SuggestChange(chunk, currentPosition + pos, blockType, 1);
                }
            }
        }

        if(remainingLength <= 0) {
            for(int i = 0; i < branches; i++) {
                Vector3 startPosition = cavePositions[rng.RandiRange(0,cavePositions.Count-1)];
                CreateCave(
                    startPosition,
                    new Vector3(rng.RandfRange(-1,1), rng.RandfRange(-1,1), rng.RandfRange(-1,1)),
                    4,
                    rng.RandiRange(10,100),
                    branches - 1,
                    blockType,
                    chunk,
                    new List<Vector3>());
            }
        } else {
            CreateCave(currentPosition, direction, radius, remainingLength-1, branches, blockType, chunk, cavePositions);
        }
    }
}
}
