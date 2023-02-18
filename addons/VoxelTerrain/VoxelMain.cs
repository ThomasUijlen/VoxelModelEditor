using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class VoxelMain
{
	public enum POOL_TYPE {
		GENERATION,
		SUGGESTIONS,
		RENDERING,
		RENDERING_CLOSE
	};

	public static Dictionary<POOL_TYPE, ThreadPool> poolList = new Dictionary<POOL_TYPE, ThreadPool>();

	public static ThreadPool GetThreadPool(POOL_TYPE type, Node node) {
		if(poolList.ContainsKey(type)) return poolList[type];
		ThreadPool threadPool = new ThreadPool();
		node.GetTree().Root.AddChild(threadPool);
		poolList.Add(type, threadPool);
		return threadPool;
	}




	public static Queue<VoxelRenderer> renderers = new Queue<VoxelRenderer>();

	public static VoxelRenderer GetRenderer(VoxelWorld world) {
		VoxelRenderer renderer = null;

		if(renderers.Count > 0) {renderer = renderers.Dequeue();} else {renderer = new VoxelRenderer(); renderer.world = world;}

		renderer.Activate();
		return renderer;
	}

	public static void ReturnRenderer(VoxelRenderer renderer) {
		renderers.Enqueue(renderer);
		renderer.Deactivate();
	}

	public static int getSeed(Vector3 origin)
	{
		return origin.GetHashCode();
	}
}
}
