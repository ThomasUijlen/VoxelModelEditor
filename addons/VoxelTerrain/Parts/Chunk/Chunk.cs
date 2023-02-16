using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class Chunk
{
    private static ConcurrentDictionary<Vector3I, Chunk> chunkList = new ConcurrentDictionary<Vector3I, Chunk>();

    public const int SIZE = 16;
    public Block[,,] grid;

    public VoxelRenderer voxelRenderer;
    public Node3D world;
    public Vector3 position;

    public void Prepare() {
        CreateVoxelGrid();
        chunkList.TryAdd(PositionToChunkCoord(position), this);
    }

    public void Remove() {
        chunkList.TryRemove(PositionToChunkCoord(position), out _);
        if(voxelRenderer != null) voxelRenderer.QueueFree();
        grid = null;
    }


    public void CreateVoxelGrid() {
        grid = new Block[SIZE,SIZE,SIZE];

        for(int x = 0; x < SIZE; x++) {
            for(int y = 0; y < SIZE; y++) {
                for(int z = 0; z < SIZE; z++) {
                    Block block = new Block(this);
                    block.position = new Vector3(x,y,z) + position;
                    grid[x,y,z] = block;
                }
            }
        }
    }

    public void Update() {
        CreateVoxelRenderer();
        voxelRenderer.RequestUpdate(grid);
    }

    private void CreateVoxelRenderer() {
        if(voxelRenderer != null) return;
        voxelRenderer = new VoxelRenderer();
        voxelRenderer.Position = position;
        world.CallDeferred("add_child", voxelRenderer);
    }

    public static Chunk GetChunk(Vector3 position) {
        Vector3I coord = PositionToChunkCoord(position);
        if(chunkList.ContainsKey(coord)) return chunkList[coord];
        return null;
    }

    public static BlockType GetBlockType(Vector3 position) {
        Block block = GetBlock(position);
        if(block == null) return null;
        return block.blockType;
    }

    public static Block GetBlock(Vector3 position) {
        Vector3I chunkCoord = PositionToChunkCoord(position);
        
        if(chunkList.ContainsKey(chunkCoord)) {
            Chunk chunk = chunkList[chunkCoord];
            Vector3I blockCoord = chunk.PositionToCoord(position);
            
            if(blockCoord.X < 0
            || blockCoord.Y < 0
            || blockCoord.Z < 0
            || blockCoord.X >= SIZE
            || blockCoord.Y >= SIZE
            || blockCoord.X >= SIZE) return null;
            
            return chunk.grid[blockCoord.X, blockCoord.Y, blockCoord.Z];
        }
        
        return null;
    }

    public static bool SetBlock(Vector3 position, BlockType blockType, int priority = -1) {
        Block block = GetBlock(position);
        if(block != null) {
            block.SetBlockType(blockType, priority);
            return true;
        }

        return false;
    }

    public Vector3I PositionToCoord(Vector3 position) {
        return Vector3ToVector3I(position - this.position);
    }

    public static Vector3I PositionToChunkCoord(Vector3 position) {
        return Vector3ToVector3I(position/SIZE);
    }

    public static Vector3I Vector3ToVector3I(Vector3 vector) {
        return new Vector3I(Mathf.FloorToInt(vector.X), Mathf.FloorToInt(vector.Y), Mathf.FloorToInt(vector.Z));
    }

    public static Vector3 Vector3IToVector3(Vector3I vector) {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}
}
