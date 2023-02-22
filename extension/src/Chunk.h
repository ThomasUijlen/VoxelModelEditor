//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef ChunkH
#define ChunkH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include <map>
#include "godot_cpp/variant/vector3i.hpp"
#include "godot_cpp/classes/node3d.hpp"
#include "BlockType.h"
#include "Generator.h"
#include "Block.h"
#include "BlockType.h"

using namespace godot;

class VoxelWorld;
class VoxelRenderer;

class Chunk
{
private:
    static const Vector3 neighbours[6];

public:
    Chunk();
    Chunk(Vector3 position, VoxelWorld world);

    void Prepare();
    void Remove();
    void DeleteVoxelRenderer();
    void CreateVoxelGrid();
    void PrepareVoxelGrid();
    void InitBlockSides();
    void UpdateEdges();
    void Update(bool fromBlock = true);
    void UpdateSurroundingChunks(bool fromBlock = false);
    void CreateVoxelRenderer();
    static Chunk* GetChunk(Vector3 position);
    static BlockType* GetBlockType(Vector3 position);
    static BlockType* GetBlockType(Chunk* chunk, Vector3 position);
    static Block* GetBlock(Vector3 position);
    static Block* GetBlock(Chunk* chunk, Vector3 position);
    static bool SetBlock(Vector3 position, BlockType* blockType, int priority = -1);
    bool SetBlock(Chunk chunk, Vector3 position, BlockType* blockType, int priority = -1);
    bool SetBlockLocal(Vector3 position, BlockType *blockType, int priority = -1);
    Block* GetBlockLocal(Vector3 position);
    Vector3i PositionToCoord(Vector3 position);
    static Vector3i PositionToChunkCoord(Vector3 position);
    static Vector3i Vector3ToVector3I(Vector3 vector);
    static Vector3 Vector3IToVector3(Vector3i vector);

    static std::map<Vector3i, Chunk*> chunkList;

    static const int SIZE_X = 16;
    static const int SIZE_Y = 256;
    static const int SIZE_Z = 16;
    Block grid[SIZE_Y][SIZE_X][SIZE_Z];

    bool automaticUpdating = false;
    bool generating = true;
    bool hasRenderer = false;
    VoxelRenderer* voxelRenderer;
    Node3D* world;
    Vector3 position;
    Generator generator;
};

#endif // SUMMATOR_CLASS_H