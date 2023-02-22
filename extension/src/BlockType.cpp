//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#include "BlockType.h"

BlockType::BlockType()
{
}

BlockType::BlockType(Color modulate, int size, bool transparent) : modulate(modulate), transparent(transparent)
{
    textures[SIDE::DEFAULT] = new BlockTexture(this, size);
    CreateTextureTable();
}

BlockType::BlockType(Ref<Image> defaultTexture, Color modulate, int size, bool transparent) : modulate(modulate), transparent(transparent)
{
    CreateTextureTable();
}

BlockType::BlockType(std::map<SIDE, Ref<Image>> textures, Color modulate, int size, bool transparent) : modulate(modulate), transparent(transparent)
{
    for (auto it = textures.begin(); it != textures.end(); ++it) {
        textures[it->first] = it->second;
    }

    CreateTextureTable();
}

void BlockType::CreateTextureTable()
{
    for (auto it = textureTable.begin(); it != textureTable.end(); ++it) {
        const SIDE side = it->first;
        if(textures.count(side) > 0) {
            textureTable[side] = textures[side];
        } else {
            textureTable[side] = textures[SIDE::DEFAULT];
        }
    }
}

std::vector<BlockTexture*> BlockType::GetAllTextures()
{
    std::vector<BlockTexture*> textureList;
    
    for(std::map<SIDE, BlockTexture*>::iterator it = textures.begin(); it != textures.end(); ++it ) {
        textureList.push_back(it->second);
    }

    return textureList;
}
