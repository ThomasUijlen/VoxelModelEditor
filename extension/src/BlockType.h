//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef BlockTypeH
#define BlockTypeH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include <map>
#include <vector>
#include "BlockTexture.h"
#include "godot_cpp/variant/color.hpp"
#include "godot_cpp/classes/image.hpp"
#include "VoxelMain.h"

using namespace godot;

class BlockType
{
private:
    std::map<SIDE, BlockTexture*> textures;

public:
    BlockType();
    BlockType(Color modulate, int size, bool transparent);
    BlockType(Ref<Image> defaultTexture, Color modulate, int size, bool transparent);
    BlockType(std::map<SIDE, Ref<Image>> textures, Color modulate, int size, bool transparent);

    void CreateTextureTable();
    std::vector<BlockTexture*> GetAllTextures();

    std::map<SIDE, BlockTexture*> textureTable {
        {SIDE::TOP, nullptr},
		{SIDE::BOTTOM, nullptr},
		{SIDE::FRONT, nullptr},
		{SIDE::BACK, nullptr},
		{SIDE::LEFT, nullptr},
		{SIDE::RIGHT, nullptr}
    };
    Color modulate;
    bool rendered;
    bool transparent = false;
};

#endif // SUMMATOR_CLASS_H