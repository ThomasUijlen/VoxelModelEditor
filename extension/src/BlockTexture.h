//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef BlockTextureH
#define BlockTextureH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include "godot_cpp/variant/vector2.hpp"
#include "godot_cpp/classes/image.hpp"

using namespace godot;

class BlockType;

class BlockTexture : RefCounted
{

public:
    BlockTexture();
    BlockTexture(BlockType* owner, int size);
    BlockTexture(BlockType* owner, Ref<Image> texture);
    Vector2 UVPosition;
    Vector2 UVSize;
    Ref<Image> texture;
    BlockType* owner;
};

#endif // SUMMATOR_CLASS_H