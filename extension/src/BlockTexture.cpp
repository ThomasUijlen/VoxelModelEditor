//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#include "BlockTexture.h"
#include "BlockType.h"

BlockTexture::BlockTexture()
{
}

BlockTexture::BlockTexture(BlockType *owner, int size)
{
    this->owner = owner;
    
    texture = Image::create(size, size, true, Image::FORMAT_RGBA8);

    for(int x = 0; x < size; x++) {
        for(int y = 0; y < size; y++) {
            texture->set_pixel(x,y, Color(1,1,1));
        }
    }
}

BlockTexture::BlockTexture(BlockType* owner, Ref<Image> texture)
{
    this->owner = owner;
    this->texture = texture;
}
