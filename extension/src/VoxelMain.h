//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef VoxelMainH
#define VoxelMainH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

enum SIDE {
    DEFAULT = 1,
    TOP = 2,
    BOTTOM = 4,
    LEFT = 8,
    RIGHT = 16,
    FRONT = 32,
    BACK = 64
};

class VoxelMain
{

};

#endif // SUMMATOR_CLASS_H