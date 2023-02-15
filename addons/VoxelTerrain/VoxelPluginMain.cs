#if TOOLS
using Godot;
using System;
using VoxelPlugin;

[Tool]
public partial class VoxelPluginMain : EditorPlugin
{
	public static ThreadPool threadPool;

	public static ThreadPool GetThreadPool(Node node) {
		if(threadPool != null) return threadPool;
		threadPool = new ThreadPool();
		node.GetTree().Root.AddChild(threadPool);
		return threadPool;
	}

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
