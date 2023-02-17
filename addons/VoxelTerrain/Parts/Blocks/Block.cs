using Godot;
using System;
using System.Threading;

namespace VoxelPlugin {
	public enum SIDE {DEFAULT, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK};

public class Block {

	static Vector3[] neighbours = new Vector3[] {
		Vector3.Zero,
		Vector3.Up,
		Vector3.Down,
		Vector3.Left,
		Vector3.Right,
		Vector3.Forward,
		Vector3.Back
	};

	public Vector3 position;
	public BlockType blockType;
	public Chunk chunk;
	public int priority = 0;

	public Block(Chunk chunk) {
		this.chunk = chunk;
	}

	public void SetBlockType(BlockType blockType, int priority = -1) {
		if(priority > 0 && priority < this.priority) return;

		// if(this.blockType == null && blockType != null) Interlocked.Increment(ref chunk.drawnBlocks);
		// else if(this.blockType != null && blockType == null) Interlocked.Decrement(ref chunk.drawnBlocks);

		this.blockType = blockType;
		if(!chunk.generating) UpdateBlocks();	
	}

	private void UpdateBlocks() {
		for(int i = 0; i < neighbours.Length; i++) {
			Chunk.GetChunk(position + neighbours[i])?.Update();
		}
	}

	public static Vector3 SideToVector(SIDE side) {
		switch(side) {
			case SIDE.TOP: return Vector3.Up;
			case SIDE.BOTTOM: return Vector3.Down;
			case SIDE.LEFT: return Vector3.Left;
			case SIDE.RIGHT: return Vector3.Right;
			case SIDE.FRONT: return Vector3.Forward;
			case SIDE.BACK: return Vector3.Back;
		}

		return Vector3.Zero;
	}
}
}