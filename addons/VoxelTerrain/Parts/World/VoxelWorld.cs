using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace VoxelPlugin {
public partial class VoxelWorld : Node3D
{
	[Export]
	public int renderDistance = 5;
	[Export]
	public int lazyDistance = 2;

	private List<Vector3I> loadedCoords = new List<Vector3I>();
	private List<Vector3I> lazyCoords = new List<Vector3I>();

	private bool chunkThreadActive = false;

	public Vector3 playerPosition = Vector3.Zero;

	public override void _Ready() {
		ProcessPriority = -10;
	}

	//API functions
	public void SetBlock(Vector3 coord, String rawName) {
		string name = rawName;
		BlockType type = BlockLibrary.GetBlockType(rawName);
		if(type == null) return;
		Chunk.SetBlock(coord, type);
	}

	public String GetBlockType(Vector3 coord) {
		BlockType type = Chunk.GetBlockType(coord);
		if(type == null) return "Air";
		return type.name;
	}

	
	public override void _Process(double delta) {
		ChunkUpdateTimer(Convert.ToSingle(delta));
		playerPosition = GetViewport().GetCamera3D().GlobalPosition;
	}

	float chunkUpdateTimer = 0.0f;
	private void ChunkUpdateTimer(float delta) {
		chunkUpdateTimer -= delta;
		if(chunkUpdateTimer > 0.0f) return;
		chunkUpdateTimer = 0.1f;

		if(chunkThreadActive) return;
		chunkThreadActive = true;
		VoxelMain.GetThreadPool(VoxelMain.POOL_TYPE.GENERATION, this).RequestFunctionCall(this, "ChunkCheck");
	}

	public void ChunkCheck() {
		UpdateCoordLists();
		DeleteOldChunks();
		CreateNewChunks();
		chunkThreadActive = false;
	}

	private void UpdateCoordLists() {
		Vector3I cameraChunkCoord = Chunk.PositionToChunkCoord(playerPosition);

		loadedCoords.Clear();
		lazyCoords.Clear();

		int renderDistance = Mathf.CeilToInt(this.renderDistance);
		int lazyDistance = Mathf.CeilToInt((this.renderDistance + this.lazyDistance));

		for(int x = -lazyDistance; x < lazyDistance; x++) {
			for(int y = -lazyDistance; y < lazyDistance; y++) {
				for(int z = -lazyDistance; z < lazyDistance; z++) {
					Vector3 chunkCoord = cameraChunkCoord + new Vector3(x,y,z);
					lazyCoords.Add(Chunk.Vector3ToVector3I(chunkCoord));
				}
			}
		}

		for(int x = -renderDistance; x < renderDistance; x++) {
			for(int y = -lazyDistance; y < lazyDistance; y++) {
				for(int z = -renderDistance; z < renderDistance; z++) {
					Vector3 chunkCoord = cameraChunkCoord + new Vector3(x,y,z);
					loadedCoords.Add(Chunk.Vector3ToVector3I(chunkCoord));
				}
			}
		}

		loadedCoords = loadedCoords.OrderBy(c => Chunk.Vector3IToVector3(c).DistanceTo(cameraChunkCoord)).ToList();
		lazyCoords = lazyCoords.OrderBy(c => Chunk.Vector3IToVector3(c).DistanceTo(cameraChunkCoord)).ToList();
	}

	private void CreateNewChunks() {
		ThreadPool pool = VoxelMain.GetThreadPool(VoxelMain.POOL_TYPE.GENERATION,this);
		int freeThreads = pool.GetFreeThreads();

		foreach(Vector3I coord in loadedCoords) {
			if(freeThreads <= 0) break;
			if(Chunk.chunkList.ContainsKey(coord)) continue;
			Chunk.chunkList.TryAdd(coord, null);
			pool.RequestFunctionCall(this, "CreateChunk", new Godot.Collections.Array() {coord});
			freeThreads -= 1;
		}
	}

	public void CreateChunk(Vector3I coord) {
		Vector3 chunkPosition = coord*Chunk.SIZE;

		Vector3 flatChunkPos = new Vector3(chunkPosition.X, 0, chunkPosition.Z);
		Vector3 flatPlayerPos = new Vector3(playerPosition.X, 0, playerPosition.Z);

		Chunk chunk = new Chunk(chunkPosition, this);
		Chunk.chunkList[coord] = chunk;
		chunk.GenerateChunk();
	}

	public void CreateVoxelRenderer(Vector3 position) {
		Chunk chunk = Chunk.GetChunk(position);
		if(chunk == null) return;
		
		VoxelRenderer voxelRenderer = VoxelMain.GetRenderer(this);
        voxelRenderer.Position = chunk.position;
		chunk.voxelRenderer = voxelRenderer;
		voxelRenderer.chunk = chunk;
		voxelRenderer.RequestUpdate(chunk.grid);
        if(!voxelRenderer.IsInsideTree()) AddChild(voxelRenderer);
	}

	private void DeleteOldChunks() {
		foreach(Vector3I coord in Chunk.chunkList.Keys) {
			if(lazyCoords.Contains(coord)) continue;
			Chunk chunk = Chunk.chunkList[coord];
			Chunk.chunkList.Remove(coord);

			if(chunk == null) continue;
			chunk.Remove();
		}
	}
}
}