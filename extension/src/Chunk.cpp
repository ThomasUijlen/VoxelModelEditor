#include "Chunk.h"
#include <cmath>
#include "VoxelWorld.h"
#include "VoxelRenderer.h"
#include "BlockLibrary.h"

std::map<Vector3i, Chunk*> Chunk::chunkList;

const Vector3 Chunk::neighbours[6] = {
    Vector3(1,0,0),
    Vector3(-1,0,0),
    Vector3(0,0,1),
    Vector3(0,0,-1),
    Vector3(0,1,0),
    Vector3(0,-1,0)
};

void Chunk::CreateVoxelGrid()
{
    BlockType* air = BlockLibrary::GetBlockType("Air");

    for(int y = 0; y < SIZE_Y; y++) {
        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                Block block = grid[SIZE_Y][SIZE_X][SIZE_Z];
                block.position = Vector3(x,y,z) + position;
                block.SetBlockType(air);
            }
        }
    }
}

void Chunk::PrepareVoxelGrid()
{
    for(int y = 0; y < SIZE_Y; y++) {
        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                grid[SIZE_Y][SIZE_X][SIZE_Z].position = Vector3(x,y,z) + position;
            }
        }
    }
}

void Chunk::InitBlockSides()
{
    for(int y = 0; y < SIZE_Y; y++) {
        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                Block block = grid[y][x][z];
                
                if(x == 0 || y == 0 || z == 0
                || x == SIZE_X-1 || y == SIZE_Y-1 || z == SIZE_Z-1) block.UpdateSurroundingBlocks();
                block.UpdateSelf();
            }
        }
    }
}

void Chunk::UpdateEdges()
{
    for(int y = 0; y < SIZE_Y; y++) {
        for(int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                if(x == 0 || y == 0 || z == 0
                || x == SIZE_X-1 || y == SIZE_Y-1 || z == SIZE_Z-1) {
                    Block block = grid[y][x][z];
                    // block.UpdateSurroundingBlocks();
                    block.UpdateSelf();
                }
            }
        }
    }
}

void Chunk::Update(bool fromBlock)
{
    if(!automaticUpdating) return;
    CreateVoxelRenderer();
    // if(voxelRenderer != nullptr) voxelRenderer->RequestUpdate(grid, fromBlock);
}

void Chunk::UpdateSurroundingChunks(bool fromBlock)
{
    for(int i = 0; i < 6; i++) {
        Chunk* chunk = Chunk::GetChunk(position + Chunk::neighbours[i]*Vector3(SIZE_X,SIZE_Y,SIZE_Z));
        if(chunk != nullptr) {
            chunk->UpdateEdges();
            chunk->Update(fromBlock);
        }
    }
}

void Chunk::CreateVoxelRenderer()
{
    if(hasRenderer) return;
    hasRenderer = true;
    world->call_deferred("CreateVoxelRenderer", position);  
}

Chunk *Chunk::GetChunk(Vector3 position)
{
    Vector3i coord = PositionToChunkCoord(position);
    if(chunkList.count(coord) > 0) {
        Chunk* chunk = chunkList[coord];
        return chunk;
    }
    return nullptr;
}

BlockType *Chunk::GetBlockType(Vector3 position)
{
    Block* block = GetBlock(position);
    if(block == nullptr) return nullptr;
    return block->blockType;
}

BlockType *Chunk::GetBlockType(Chunk *chunk, Vector3 position)
{
    Block* block = GetBlock(chunk, position);
    if(block == nullptr) return nullptr;
    return block->blockType;
}

Block *Chunk::GetBlock(Vector3 position)
{
    Vector3i chunkCoord = PositionToChunkCoord(position);
        
    if(chunkList.count(chunkCoord) > 0) {
        Chunk* chunk = chunkList[chunkCoord];
        if(chunk != nullptr && !chunk->generating) return chunk->GetBlockLocal(position);
    }
    
    return nullptr;
}

Block *Chunk::GetBlock(Chunk *chunk, Vector3 position)
{
    Block* block = (chunk != nullptr) ? chunk->GetBlockLocal(position) : nullptr;
    if(block != nullptr) return block;
    return GetBlock(position);
}

bool Chunk::SetBlock(Vector3 position, BlockType *blockType, int priority)
{
    Block* block = GetBlock(position);
    if(block != nullptr) {
        block->SetBlockType(blockType, priority);
        return true;
    }

    return false;
}

bool Chunk::SetBlock(Chunk chunk, Vector3 position, BlockType *blockType, int priority)
{
    Block* block = chunk.GetBlockLocal(position);
    if(block != nullptr) {
        block->SetBlockType(blockType, priority);
        return true;
    }

    return SetBlock(position, blockType, priority);
}

bool Chunk::SetBlockLocal(Vector3 position, BlockType *blockType, int priority)
{
    Vector3i blockCoord = PositionToCoord(position);
        if(blockCoord.x >= 0
            && blockCoord.y >= 0
            && blockCoord.z >= 0
            && blockCoord.x < SIZE_X
            && blockCoord.y < SIZE_Y
            && blockCoord.z < SIZE_Z) {
                grid[blockCoord.y][blockCoord.x][blockCoord.z].SetBlockType(blockType, priority);
                return true;
            }
        return false;
}

Block *Chunk::GetBlockLocal(Vector3 position)
{
    Vector3i blockCoord = PositionToCoord(position);
        if(blockCoord.x >= 0
            && blockCoord.y >= 0
            && blockCoord.z >= 0
            && blockCoord.x < SIZE_X
            && blockCoord.y < SIZE_Y
            && blockCoord.z < SIZE_Z) {
                return &grid[blockCoord.y][blockCoord.x][blockCoord.z];
            }
        return nullptr;
}

Vector3i Chunk::PositionToCoord(Vector3 position)
{
    return Vector3ToVector3I(position - this->position);
}

Vector3i Chunk::PositionToChunkCoord(Vector3 position)
{
    return Vector3ToVector3I(Vector3(position.x/SIZE_X, position.y/SIZE_Y, position.z/SIZE_Z));
}

Vector3i Chunk::Vector3ToVector3I(Vector3 vector)
{
    return Vector3i(static_cast<int>(std::floor(vector.x)), static_cast<int>(std::floor(vector.y)), static_cast<int>(std::floor(vector.z)));
}

Vector3 Chunk::Vector3IToVector3(Vector3i vector)
{
    return Vector3(vector.x, vector.y, vector.z);
}