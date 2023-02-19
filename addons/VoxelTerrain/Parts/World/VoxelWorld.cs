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

	private Vector3 CHUNK_SIZE = Vector3.Zero;
	private ConcurrentDictionary<Vector3I, Chunk> chunks = new ConcurrentDictionary<Vector3I, Chunk>();
	private List<Vector3I> loadedCoords = new List<Vector3I>();
	private List<Vector3I> lazyCoords = new List<Vector3I>();

	private bool chunkThreadActive = false;

	public override void _Ready() {
		CHUNK_SIZE = Chunk.SIZE;
		ProcessPriority = -10;
	}

	
	public override void _Process(double delta) {
		ChunkUpdateTimer(Convert.ToSingle(delta));
		SuggestionTimer(Convert.ToSingle(delta));
	}

	float chunkUpdateTimer = 0.0f;
	private void ChunkUpdateTimer(float delta) {
		chunkUpdateTimer -= delta;
		if(chunkUpdateTimer > 0.0f) return;
		chunkUpdateTimer = 1.0f;

		if(chunkThreadActive) return;
		chunkThreadActive = true;
		VoxelMain.GetThreadPool(VoxelMain.POOL_TYPE.GENERATION, this).RequestFunctionCall(this, "ChunkCheck");
	}


	List<Chunk> suggestionChunks = new List<Chunk>();
	float suggestionTimer = 0.0f;
	bool suggestionThreadActive = false;
	private void SuggestionTimer(float delta) {
		suggestionTimer -= delta;
		if(suggestionTimer > 0.0f) return;
		suggestionTimer = 1.0f;

		if(suggestionThreadActive) return;
		suggestionThreadActive = true;
		suggestionChunks.AddRange(chunks.Values);
		VoxelMain.GetThreadPool(VoxelMain.POOL_TYPE.SUGGESTIONS, this).RequestFunctionCall(this, "ProcessChunkSuggestions");
	}

	public void ChunkCheck() {
		UpdateCoordLists();
		DeleteOldChunks();
		CreateNewChunks();
		chunkThreadActive = false;
	}

	public void ProcessChunkSuggestions() {
		for(int i = 0; i < suggestionChunks.Count; i++) suggestionChunks[i]?.ProcessSuggestions();
		suggestionChunks.Clear();
		suggestionThreadActive = false;
	}

	private void UpdateCoordLists() {
		Vector3 cameraPosition = GetViewport().GetCamera3D().GlobalPosition;
		Vector3I cameraChunkCoord = Chunk.PositionToChunkCoord(cameraPosition);
		cameraChunkCoord.Y = 0;

		loadedCoords.Clear();
		lazyCoords.Clear();

		int renderDistance = Mathf.CeilToInt(this.renderDistance);
		int lazyDistance = Mathf.CeilToInt((this.renderDistance + this.lazyDistance));

		for(int x = -lazyDistance; x < lazyDistance; x++) {
			for(int y = 0; y < 1; y++) {
				for(int z = -lazyDistance; z < lazyDistance; z++) {
					Vector3 chunkCoord = cameraChunkCoord + new Vector3(x,y,z);
					lazyCoords.Add(Chunk.Vector3ToVector3I(chunkCoord));
				}
			}
		}

		for(int x = -renderDistance; x < renderDistance; x++) {
			for(int y = 0; y < 1; y++) {
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
		foreach(Vector3I coord in loadedCoords) {
			if(chunks.ContainsKey(coord)) continue;
			ThreadPool pool = VoxelMain.GetThreadPool(VoxelMain.POOL_TYPE.GENERATION,this);

			if(pool.ThreadFree()) {
				chunks.TryAdd(coord, null);
				pool.RequestFunctionCall(this, "CreateChunk", new Godot.Collections.Array() {coord});
			}
		}
	}

	public void CreateChunk(Vector3I coord) {
		Chunk chunk = new Chunk();
		chunk.position = coord*Chunk.SIZE;
		chunk.world = this;
		chunks.AddOrUpdate(coord, chunk, (coord, nullChunk) => chunk);
		chunk.Prepare();
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