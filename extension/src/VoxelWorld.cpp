//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#include "VoxelWorld.h"

#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/utility_functions.hpp>
#include "BlockLibrary.h"

using namespace godot;

VoxelWorld::VoxelWorld()
{
    
}

VoxelWorld::~VoxelWorld()
{
}

void VoxelWorld::_ready()
{
    
    UtilityFunctions::print("Ready!");
}

void VoxelWorld::_process(float delta)
{
    
}

void VoxelWorld::prepare(const Ref<ArrayMesh> voxelMesh)
{
    BlockLibrary::voxelMesh = voxelMesh;
    UtilityFunctions::print(voxelMesh);
    UtilityFunctions::print(*voxelMesh);
    BlockLibrary::Prepare();
}

Ref<ImageTexture> VoxelWorld::getImage()
{
    return BlockLibrary::texture.ptr();
}

void VoxelWorld::_bind_methods()
{
    ClassDB::bind_method(D_METHOD("prepare", "voxelMesh"), &VoxelWorld::prepare);
    ClassDB::bind_method(D_METHOD("getImage"), &VoxelWorld::getImage);
    // ClassDB::bind_method(D_METHOD("_process"), &VoxelWorld::_process);
    // ClassDB::bind_method(D_METHOD("_physics_process"), &VoxelWorld::_physics_process);
}