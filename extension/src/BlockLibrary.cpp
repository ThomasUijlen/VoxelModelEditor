//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#include "BlockLibrary.h"

#include <godot_cpp/variant/utility_functions.hpp>
#include <godot_cpp/classes/resource_loader.hpp>
#include "godot_cpp/classes/material.hpp"
#include "godot_cpp/variant/vector2i.hpp"

using namespace godot;

Ref<ArrayMesh> BlockLibrary::voxelMesh;
std::map<std::string, BlockType*> BlockLibrary::blockTypes;
Ref<Image> BlockLibrary::textureAtlas;
Ref<ImageTexture> BlockLibrary::texture;
int BlockLibrary::textureWidth = 16;
float BlockLibrary::atlasScale = 0.0;

void BlockLibrary::Prepare()
{
    if(voxelMesh != nullptr) return;
    //voxelMesh = ResourceLoader::get_singleton()->load("res://addons/VoxelTerrain/Parts/Blocks/Mesh/VoxelFace.tres", "Mesh");
    AddBlockType("Air", new BlockType(Color(1,1,1,0), textureWidth, true));
    GetBlockType("Air")->rendered = false;
    
}

BlockType *BlockLibrary::AddBlockType(std::string name, BlockType *blockType)
{
    blockTypes[name] = blockType;
    ConstructTextureAtlas();
    return blockType;
}

BlockType *BlockLibrary::GetBlockType(std::string name)
{
    if(blockTypes.count(name) > 0) return blockTypes[name];
    return nullptr;
}

void BlockLibrary::ConstructTextureAtlas()
{
    std::map<std::string, std::vector<BlockTexture*>*> textures;
    
    for(auto pair : blockTypes) {
        BlockType* blockType = pair.second;
        std::vector<BlockTexture*> blockTextures = blockType->GetAllTextures();
        for(BlockTexture* blockTexture : blockTextures) {
            
            std::string id = UtilityFunctions::var_to_str(blockTexture->texture->get_data()).utf8().get_data();
            
            if(textures.count(id) <= 0) textures[id] = new std::vector<BlockTexture*>;
            
            textures[id]->push_back(blockTexture);
        }
    }

    //Create the texture atlas image and color it purple
    UtilityFunctions::print("");
    UtilityFunctions::print("Color purple");
    int atlasWidth = CalculateAtlasSize(textures.size());
    textureAtlas = Image::create(atlasWidth*textureWidth, atlasWidth*textureWidth, true, Image::FORMAT_RGBA8);

    for(int x = 0; x < atlasWidth*textureWidth; x++) {
        for(int y = 0; y < atlasWidth*textureWidth; y++) {
            textureAtlas->set_pixel(x, y, Color(0.71f, 0.05f, 0.89f));
        }
    }

    UtilityFunctions::print("Populate");
    auto i = textures.begin();
    int textureI = 0;
    for(int x = 0; x < atlasWidth; x++) {
        for(int y = 0; y < atlasWidth; y++) {
            if(textureI >= textures.size()) continue;
            UtilityFunctions::print("grab texture");
            std::vector<BlockTexture*>* textureList = i->second;
            for(BlockTexture* blockTexture : *textureList) ApplyTexture(Vector2i(x*textureWidth, y*textureWidth), atlasWidth, atlasWidth*textureWidth, blockTexture);
            UtilityFunctions::print("increase");
            textureI += 1;
            UtilityFunctions::print("break");
            if(textureI >= textures.size()) continue;
            UtilityFunctions::print("next");
            i++;
        }
    }

    UtilityFunctions::print("create map");
    textureAtlas->generate_mipmaps();
    UtilityFunctions::print("create map");
    texture = ImageTexture::create_from_image(textureAtlas);
    UtilityFunctions::print("create map");
    UtilityFunctions::print(voxelMesh);
    UtilityFunctions::print(*voxelMesh);
    Ref<Material> voxelMaterial = voxelMesh->surface_get_material(0);
    UtilityFunctions::print("create map");
    voxelMaterial->set("shader_parameter/textureAtlas", BlockLibrary::texture);
    UtilityFunctions::print("create map");
    voxelMaterial->set("shader_parameter/atlasScale", BlockLibrary::atlasScale);
    UtilityFunctions::print("create map");
    UtilityFunctions::print("texture atlas constructed");
}

void BlockLibrary::ApplyTexture(Vector2i origin, int atlasWidth, float totalSize, BlockTexture *blockTexture)
{
    UtilityFunctions::print("Apply texture to atlas");

    blockTexture->UVSize = Vector2(1,1)/atlasWidth;
    blockTexture->UVPosition = Vector2(origin.x, origin.y)/totalSize;
    atlasScale = blockTexture->UVSize.x;

    UtilityFunctions::print("Start loop");
    for(int x = 0; x < textureWidth; x++) {
        for(int y = 0; y < textureWidth; y++) {
            Vector2i localPixel = Vector2i(x,y);
            Vector2i globalPixel = origin + localPixel;
            Vector2 UVPosition = Vector2(globalPixel.x, globalPixel.y)/totalSize;
            textureAtlas->set_pixelv(globalPixel, blockTexture->texture->get_pixelv(localPixel)*blockTexture->owner->modulate);
        }
    }

    UtilityFunctions::print("Texture applied");
}

int BlockLibrary::CalculateAtlasSize(int textureCount, int width)
{
    int atlasSize = width*width;
    if(textureCount > atlasSize) return CalculateAtlasSize(textureCount, width+1);
    return width;
}
