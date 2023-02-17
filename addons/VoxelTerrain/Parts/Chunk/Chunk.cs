using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class Chunk
{
    private static ConcurrentDictionary<Vector3I, Chunk> chunkList = new ConcurrentDictionary<Vector3I, Chunk>();

    public static Vector3I SIZE = new Vector3I(8,256,8);
    public Block[,,] grid;

    public bool generating = true;
    public bool hasRenderer = false;
    public VoxelRenderer voxelRenderer;
    public Node3D world;
    public Vector3 position;

    public IGenerator generator;
    public int drawnBlocks = 0;

    public void Prepare() {
        CreateVoxelGrid();
        chunkList.TryAdd(PositionToChunkCoord(position), this);

        generator = new NoiseLayer("Stone");
        generator.Generate(this);
        generating = false;
        Update();
        UpdateSurroundingChunks();
    }

    public void Remove() {
        chunkList.TryRemove(PositionToChunkCoord(position), out _);
        grid = null;
        DeleteVoxelRenderer();
    }

    public void DeleteVoxelRenderer() {
        hasRenderer = false;
        if(voxelRenderer != null) VoxelPluginMain.ReturnRenderer(voxelRenderer);
        voxelRenderer = null;
    }


    public void CreateVoxelGrid() {
        grid = new Block[SIZE.X,SIZE.Y,SIZE.Z];
        BlockType air = BlockLibrary.GetBlockType("Air");

        for(int x = 0; x < SIZE.X; x++) {
            for(int y = 0; y < SIZE.Y; y++) {
                for(int z = 0; z < SIZE.Z; z++) {
                    Block block = new Block(this);
                    block.position = new Vector3(x,y,z) + position;
                    block.SetBlockType(air);
                    grid[x,y,z] = block;
                }
            }
        }
    }

    public void Update() {
        if(drawnBlocks > 0) {
            CreateVoxelRenderer();
        } else {
            DeleteVoxelRenderer();
        }
        voxelRenderer?.RequestUpdate(grid);
    }

    static Vector3[] neighbours = new Vector3[] {
		Vector3.Up,
		Vector3.Down,
		Vector3.Left,
		Vector3.Right,
		Vector3.Forward,
		Vector3.Back
	};

    public void UpdateSurroundingChunks() {
        for(int i = 0; i < neighbours.Length; i++) {
			Chunk.GetChunk(position + neighbours[i]*SIZE)?.Update();
		}
    }

    private void CreateVoxelRenderer() {
        if(hasRenderer) return;
        hasRenderer = true;
        world.CallDeferred("CreateVoxelRenderer", position);        
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
            
            if(!(blockCoord.X < 0
            || blockCoord.Y < 0
            || blockCoord.Z < 0
            || blockCoord.X >= SIZE.X
            || blockCoord.Y >= SIZE.Y
            || blockCoord.X >= SIZE.Z)) return chunk.grid[blockCoord.X, blockCoord.Y, blockCoord.Z];
            
            return null;
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
