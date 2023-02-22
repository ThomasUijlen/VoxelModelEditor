//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef BlockH
#define BlockH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include <map>
#include <vector>
#include <bitset>
#include "godot_cpp/variant/vector3.hpp"
#include "BlockTexture.h"
#include "BlockType.h"
#include "godot_cpp/variant/color.hpp"
#include "godot_cpp/classes/image.hpp"
#include "VoxelMain.h"

using namespace godot;

class Chunk;

class Block
{

public:
    Block();
    Block(Chunk* chunk);

    void SetBlockType(BlockType* blockType, int priority = -1, bool updateChunk = true);
    void UpdateSelf();
    void UpdateSurroundingBlocks();
    void UpdateAll();
    void AddSide(SIDE side);
    void RemoveSide(SIDE side);
    static Vector3 SideToVector(SIDE side);
    static SIDE GetOppositeSide(SIDE side);
    static bool HasToRender(BlockType* a, BlockType* b);

    static const SIDE neighbours[6];

    std::bitset<8> activeSides;
    Vector3 position;
    BlockType* blockType;
    Chunk* chunk;
    int priority = 0;
};

#endif // SUMMATOR_CLASS_H