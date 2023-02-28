using Godot;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VoxelPlugin {
	[Flags]
	public enum SIDE : byte {
		DEFAULT = 1 << 0,
		TOP = 1 << 1,
		BOTTOM = 1 << 2,
		LEFT = 1 << 3,
		RIGHT = 1 << 4,
		FRONT = 1 << 5,
		BACK = 1 << 6
	};

public class Block {

	static SIDE[] neighbours = new SIDE[] {
		SIDE.TOP,
		SIDE.BOTTOM,
		SIDE.LEFT,
		SIDE.RIGHT,
		SIDE.FRONT,
		SIDE.BACK
	};

	public byte activeSides = 0;

	public Vector3 position;
	public BlockType blockType;
	public Chunk chunk;
	public int priority = 0;

	public Block(Chunk chunk) {
		this.chunk = chunk;
	}

	public void SetBlockType(BlockType blockType, int priority = -1, bool updateChunk = true) {
		if(priority > 0 && priority < this.priority) return;
		if(this.blockType == blockType) return;

		// if(this.blockType == null && blockType != null) Interlocked.Increment(ref chunk.drawnBlocks);
		// else if(this.blockType != null && blockType == null) Interlocked.Decrement(ref chunk.drawnBlocks);

		this.blockType = blockType;
		if(updateChunk && chunk.automaticUpdating) UpdateAll();	
	}

	public void UpdateSelf() {
		if(!blockType.rendered) return;
		for(int i = 0; i < neighbours.Length; i++) {
			SIDE side = neighbours[i];
			if(HasToRender(blockType, Chunk.GetBlockType(chunk, position+SideToVector(side)*chunk.scale))) {
				AddSide(side);
			} else {
				RemoveSide(side);
			}
		}
	}

	public void UpdateSurroundingBlocks() {
		for(int i = 0; i < neighbours.Length; i++) {
			SIDE side = neighbours[i];
			Block block = Chunk.GetBlock(chunk, position+SideToVector(side)*chunk.scale);
			if(block == null || !block.blockType.rendered) continue;

			if(HasToRender(block.blockType, blockType)) {
				block.AddSide(GetOppositeSide(side));
			} else {
				block.RemoveSide(GetOppositeSide(side));
			}
		}
	}

	private void UpdateAll() {
		UpdateSelf();
		UpdateSurroundingBlocks();
		chunk?.Update();

		for(int i = 0; i < neighbours.Length; i++) {
			Chunk.GetChunk(position + SideToVector(neighbours[i]))?.Update();
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

	public static SIDE GetOppositeSide(SIDE side) {
		switch(side) {
			case SIDE.TOP: return SIDE.BOTTOM;
			case SIDE.BOTTOM: return SIDE.TOP;
			case SIDE.LEFT: return SIDE.RIGHT;
			case SIDE.RIGHT: return SIDE.LEFT;
			case SIDE.FRONT: return SIDE.BACK;
			case SIDE.BACK: return SIDE.FRONT;
		}

		return SIDE.DEFAULT;
	}

	public static bool HasToRender(BlockType a, BlockType b) {
		if(b == null) return false;

		if(!b.rendered) return true;
		if(!b.transparent) return false;
		if(!a.transparent && b.transparent) return true;
		if(a.transparent && b.transparent) return false;

		return false;
	}

	public void AddSide(SIDE side) {
		activeSides |= ((byte) side);
	}

	public void RemoveSide(SIDE side) {
		activeSides = (byte) (activeSides & ~((byte) side));
	}

	public IEnumerable<SIDE> GetActiveSides() {
		foreach (SIDE value in Enum.GetValues(activeSides.GetType()))
			if ((activeSides & ((byte) value)) != 0)
				yield return value;
	}
}
}