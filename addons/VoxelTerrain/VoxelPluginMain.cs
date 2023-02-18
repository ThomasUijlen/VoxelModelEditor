#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using VoxelPlugin;

[Tool]
public partial class VoxelPluginMain : EditorPlugin
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
