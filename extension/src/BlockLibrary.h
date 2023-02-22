//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef BlockLibraryH
#define BlockLibraryH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include <string>
#include <map>
#include <vector>
#include "BlockType.h"
#include "BlockTexture.h"
#include "godot_cpp/variant/vector2i.hpp"
#include "godot_cpp/classes/array_mesh.hpp"
#include "godot_cpp/classes/image.hpp"
#include "godot_cpp/classes/image_texture.hpp"

class Chunk;

class BlockLibrary
{
public:
    static void Prepare();
    static BlockType* AddBlockType(std::string name, BlockType* blockType);
    static BlockType* GetBlockType(std::string name);
    static void ConstructTextureAtlas();
    static void ApplyTexture(Vector2i origin, int atlasWidth, float totalSize, BlockTexture* blockTexture);
    static int CalculateAtlasSize(int textureCount, int width = 1);

    static Ref<ArrayMesh> voxelMesh;
    static std::map<std::string, BlockType*> blockTypes;
    static Ref<Image> textureAtlas;
    static Ref<ImageTexture> texture;
    static int textureWidth;
    static float atlasScale;
};

#endif // SUMMATOR_CLASS_H