using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class NoiseCaves : IGenerator
{
    public int seed = 0;
    public Vector3 scale = Vector3.One;
    public float caveSpawnChance = 0.01f;

    private FastNoiseLite noise = new FastNoiseLite();
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public NoiseCaves(float caveSpawnChance) {
        this.caveSpawnChance = caveSpawnChance;
        noise.Seed = seed;
    }

	public void Generate(Chunk chunk) {
        BlockType air = BlockLibrary.GetBlockType("Air");

        rng.Seed = Convert.ToUInt64(seed + Mathf.RoundToInt(Mathf.Abs(noise.GetNoise3Dv(chunk.position*1000f))*10000));

        float n = rng.RandfRange(0f,100f);

        if(n > caveSpawnChance) return;
        Block block = chunk.GetRandomBlock(rng);

        int tries = 50;
        while(block.blockType == air && tries > 0) {
            tries -= 1;
            block = chunk.GetRandomBlock(rng);
        }

        CreateCave(
        block.position,
        new Vector3(rng.RandfRange(-1,1), rng.RandfRange(-1,1), rng.RandfRange(-1,1)),
        4,
        rng.RandiRange(10,100),
        rng.RandiRange(2,5),
        air,
        chunk,
        new List<Vector3>());
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
