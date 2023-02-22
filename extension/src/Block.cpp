//© Copyright 2014-2022, Juan Linietsky, Ariel Manzur and the Godot community (CC-BY 3.0)
#include "Block.h"
#include "Chunk.h"

const SIDE Block::neighbours[6] = {
    SIDE::LEFT,
    SIDE::RIGHT,
    SIDE::FRONT,
    SIDE::BACK,
    SIDE::TOP,
    SIDE::BOTTOM
};

Block::Block(Chunk *chunk) : chunk(chunk)
{
}

void Block::SetBlockType(BlockType *blockType, int priority, bool updateChunk)
{
    if(priority > 0 && priority < this->priority) return;
    this->blockType = blockType;
    if(updateChunk && chunk->automaticUpdating) UpdateAll();	
}

void Block::UpdateSelf()
{
    if(!blockType->rendered) return;
		for(int i = 0; i < 6; i++) {
			SIDE side = neighbours[i];
			if(HasToRender(blockType, Chunk::GetBlockType(chunk, position+SideToVector(side)))) {
				AddSide(side);
			} else {
				RemoveSide(side);
			}
		}
}

void Block::UpdateSurroundingBlocks()
{
    for(int i = 0; i < 6; i++) {
        SIDE side = neighbours[i];
        Block* block = Chunk::GetBlock(chunk, position+SideToVector(side));
        if(block == nullptr || !block->blockType->rendered) continue;

        if(HasToRender(block->blockType, blockType)) {
            block->AddSide(GetOppositeSide(side));
        } else {
            block->RemoveSide(GetOppositeSide(side));
        }
    }
}

void Block::UpdateAll()
{
    UpdateSelf();
    UpdateSurroundingBlocks();
    chunk->Update();
    
    for(int i = 0; i < 6; i++) {
        Chunk* chunk = Chunk::GetChunk(position + SideToVector(Block::neighbours[i]));
        (chunk != nullptr) ? chunk->Update() : void();
    }
}

void Block::AddSide(SIDE side)
{
    activeSides |= side;
}

void Block::RemoveSide(SIDE side)
{
    activeSides &= ~side;
}

Vector3 Block::SideToVector(SIDE side)
{
    switch(side) {
    case SIDE::TOP: return Vector3(0,1,0);
    case SIDE::BOTTOM: return Vector3(0,-1,0);
    case SIDE::LEFT: return Vector3(-1,0,0);
    case SIDE::RIGHT: return Vector3(1,0,0);
    case SIDE::FRONT: return Vector3(0,0,-1);
    case SIDE::BACK: return Vector3(0,0,1);
    }

return Vector3(0,0,0);
}

SIDE Block::GetOppositeSide(SIDE side)
{
    switch (side)
    {
    case SIDE::TOP: return SIDE::BOTTOM;
    case SIDE::BOTTOM: return SIDE::TOP;
    case SIDE::LEFT: return SIDE::RIGHT;
    case SIDE::RIGHT: return SIDE::LEFT;
    case SIDE::FRONT: return SIDE::BACK;
    case SIDE::BACK: return SIDE::FRONT;
    }

    return SIDE::DEFAULT;
}

bool Block::HasToRender(BlockType *a, BlockType *b)
{
    if(b == nullptr) return false;
    if(!b->rendered) return true;

    if(a->transparent) {
        if(b->transparent) {
            return false;
        } else {
            return true;
        }
    } else {
        if(b->transparent) {
            return true;
        } else {
            return false;
        }
    }

    return false;
}
