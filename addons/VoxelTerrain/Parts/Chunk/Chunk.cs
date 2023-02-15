using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public partial class Chunk : Node3D
{
    private static Dictionary<Vector3I, Chunk> chunkList = new Dictionary<Vector3I, Chunk>();

    public const int SIZE = 16;
    public Block[,,] grid;

    public VoxelRenderer voxelRenderer;

    public override void _Ready() {
        AddChild(voxelRenderer);
    }

    public override void _EnterTree() {
        if(voxelRenderer == null) {
            CreateVoxelGrid();
            voxelRenderer = new VoxelRenderer();
        }
        chunkList.Add(PositionToChunkCoord(GlobalPosition), this);
    }

    public override void _ExitTree() {
        chunkList.Remove(PositionToChunkCoord(GlobalPosition));
    }


    public void CreateVoxelGrid() {
        grid = new Block[SIZE,SIZE,SIZE];

        for(int x = 0; x < SIZE; x++) {
            for(int y = 0; y < SIZE; y++) {
                for(int z = 0; z < SIZE; z++) {
                    Block block = new Block(this);
                    block.coord = new Vector3I(x,y,z);
                    block.position = block.coord + GlobalPosition;
                    grid[x,y,z] = block;
                }
            }
        }
    }

    public void Update() {
        voxelRenderer.RequestUpdate(grid);
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

    public static void SetBlock(Vector3 position, BlockType blockType) {
        Block block = GetBlock(position);
        if(block != null) block.SetBlockType(blockType);
    }

    public Vector3I PositionToCoord(Vector3 position) {
        return Vector3ToVector3I(position - GlobalPosition);
    }

    public static Vector3I PositionToChunkCoord(Vector3 position) {
        return Vector3ToVector3I(position/SIZE);
    }

    public static Vector3I Vector3ToVector3I(Vector3 vector) {
        return new Vector3I(Mathf.FloorToInt(vector.X), Mathf.FloorToInt(vector.Y), Mathf.FloorToInt(vector.Z));
    }
}
}
