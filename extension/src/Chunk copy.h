//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#ifndef GeneratorH
#define GeneratorH

// We don't need windows.h in this plugin but many others do and it throws up on itself all the time
// So best to include it and make sure CI warns us when we use something Microsoft took for their own goals....
#ifdef WIN32
#include <windows.h>
#endif

class Chunk;

class Generator
{
public:
    void Generate(Chunk* chunk);
};

#endif // SUMMATOR_CLASS_H