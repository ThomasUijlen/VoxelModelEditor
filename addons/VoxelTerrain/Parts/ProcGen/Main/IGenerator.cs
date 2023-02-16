using Godot;
using System;

namespace VoxelPlugin {
public interface IGenerator
{
	void Generate(Chunk chunk);
}
}
