#if TOOLS
using Godot;
using System;

[Tool]
public partial class VoxelTerrain : EditorPlugin
{
	public override void _EnterTree()
	{
		GD.Print("Voxel Terrain plugin ready!");
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}
}
#endif
