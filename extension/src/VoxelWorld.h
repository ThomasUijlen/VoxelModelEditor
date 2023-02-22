//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef VoxelWorldH
#define VoxelWorldH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include <godot_cpp/classes/node3d.hpp>

using namespace godot;

#include "godot_cpp/classes/image_texture.hpp"
#include "godot_cpp/classes/array_mesh.hpp"

class VoxelWorld : public Node3D
{
    GDCLASS(VoxelWorld, Node3D);

protected:
    static void _bind_methods();
    int count;

public:
    VoxelWorld();
    ~VoxelWorld();

    void _ready();
    void _process(float delta);
    void prepare(const Ref<ArrayMesh> voxelMesh);

    Ref<ImageTexture> getImage();
};

#endif // SUMMATOR_CLASS_H