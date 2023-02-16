using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace VoxelPlugin {
public partial class VoxelWorld : Node3D
{
	[Export]
	public int renderDistance = 500;
	public int lazyDistance = 200;

	private int CHUNK_SIZE = 0;
	private ConcurrentDictionary<Vector3I, Chunk> chunks = new ConcurrentDictionary<Vector3I, Chunk>();
	private List<Vector3I> loadedCoords = new List<Vector3I>();
	private List<Vector3I> lazyCoords = new List<Vector3I>();

	private bool threadActive = false;

	public override void _Ready() {
		CHUNK_SIZE = Chunk.SIZE;
	}

	float timer = 0.0f;
	public override void _Process(double delta) {
		timer -= Convert.ToSingle(delta);
		if(timer > 0.0f) return;
		timer = 1.0f;

		if(threadActive) return;
		threadActive = true;
		VoxelPluginMain.GetThreadPool(this).RequestFunctionCall(this, "ChunkCheck");
	}

	public void ChunkCheck() {
		UpdateCoordLists();
		CreateNewChunks();
		DeleteOldChunks();

		GD.Print(chunks.Count);
		threadActive = false;
	}

	private void UpdateCoordLists() {
		Vector3 cameraPosition = GetViewport().GetCamera3D().GlobalPosition;
		Vector3I cameraChunkCoord = Chunk.PositionToChunkCoord(cameraPosition);

		loadedCoords.Clear();
		lazyCoords.Clear();

		int renderDistance = Mathf.CeilToInt(this.renderDistance/Chunk.SIZE);
		int totalRenderDistance = Mathf.CeilToInt((this.renderDistance + lazyDistance)/Chunk.SIZE);

		for(int x = -totalRenderDistance; x < totalRenderDistance; x++) {
			for(int y = -totalRenderDistance; y < totalRenderDistance; y++) {
				for(int z = -totalRenderDistance; z < totalRenderDistance; z++) {
					Vector3 chunkCoord = cameraChunkCoord + new Vector3(x,y,z);

					float distance = chunkCoord.DistanceTo(cameraChunkCoord);
					if(distance < totalRenderDistance) lazyCoords.Add(Chunk.Vector3ToVector3I(chunkCoord));
					if(distance < renderDistance) loadedCoords.Add(Chunk.Vector3ToVector3I(chunkCoord));
				}
			}
		}

		loadedCoords = loadedCoords.OrderBy(c => Chunk.Vector3IToVector3(c).DistanceTo(cameraChunkCoord)).ToList();
		lazyCoords = lazyCoords.OrderBy(c => Chunk.Vector3IToVector3(c).DistanceTo(cameraChunkCoord)).ToList();
	}

	private void CreateNewChunks() {
		foreach(Vector3I coord in loadedCoords) {
			if(chunks.ContainsKey(coord)) continue;
			chunks.TryAdd(coord, null);
			VoxelPluginMain.GetThreadPool(this).RequestFunctionCall(this, "CreateChunk", new Godot.Collections.Array() {coord});
		}
	}

	public void CreateChunk(Vector3I coord) {
		Chunk chunk = new Chunk();
		chunk.position = coord*Chunk.SIZE;
		chunk.world = this;
		chunks.AddOrUpdate(coord, chunk, (coord, nullChunk) => chunk);
		chunk.Prepare();
	}

	private void DeleteOldChunks() {
		foreach(Vector3I coord in chunks.Keys) {
			if(lazyCoords.Contains(coord)) continue;
			Chunk chunk = chunks[coord];
			chunks.TryRemove(coord, out _);

			if(chunk == null) continue;
			chunk.Remove();
		}
	}
}
}