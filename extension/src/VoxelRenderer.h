//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef VoxelRendererH
#define VoxelRendererH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

#include "godot_cpp/variant/vector3.hpp"
#include "godot_cpp/variant/vector2.hpp"
#include "godot_cpp/classes/node3d.hpp"
#include "godot_cpp/variant/rid.hpp"

#include "Chunk.h"
#include "BlockTexture.h"
#include <vector>
#include <bitset>

using namespace godot;

class VoxelRenderer : public Node3D
{
    GDCLASS(VoxelRenderer, Node3D);

protected:
    static void _bind_methods();
    int count;

public:
    VoxelRenderer();
    ~VoxelRenderer();

    void Activate();
    void Deactivate();
    void RequestUpdate(Block grid[Chunk::SIZE_Y][Chunk::SIZE_X][Chunk::SIZE_Z], bool close = true);
    void UpdateMesh();
    void CollectFaces(Block grid[Chunk::SIZE_Y][Chunk::SIZE_X][Chunk::SIZE_Z]);
    void CollectFace(Block* block, SIDE side);
    void CollectQuads();
    int ScanDirection(Vector3 position, int x, Vector3 axis, std::vector<Vector3> faces);
    // void GenerateMesh(MeshInstance meshInstance);

    class MeshInstance {
    public:
        RID instance;
		RID mesh;
	};

    class Quad {
    public:
        BlockTexture* blockTexture;
		Vector3 position;
		Vector2 scale2D;
		Vector3 scale;
		std::bitset<8> quadIndex = 0;
	};
};

#endif // SUMMATOR_CLASS_H