using Godot;
using System;
using System.Collections.Generic;

namespace VoxelTerrainPlugin {
public class BlockLibrary {
	List<BlockType> blockTypes = new List<BlockType>();

	public Image textureAtlas;

	int textureWidth = 16;

	public BlockType AddBlockType(Image texture, Color modulate) {
		BlockType blockType = new BlockType(texture, modulate);
		blockTypes.Add(blockType);
		ConstructTextureAtlas();
		return blockType;
	}

	public void ConstructTextureAtlas() {
		int atlasWidth = CalculateAtlasSize();
		textureAtlas = Image.Create(atlasWidth*textureWidth, atlasWidth*textureWidth, true, Image.Format.Rgba8);

		for(int x = 0, blockI = 0; x < atlasWidth; x++) {
			for(int y = 0; y < atlasWidth; y++, blockI++) {
				if(blockI >= blockTypes.Count) break;
				ApplyTexture(new Vector2I(x*textureWidth, y*textureWidth), atlasWidth, atlasWidth*textureWidth, blockTypes[blockI]);
			}
		}
	}

	private void ApplyTexture(Vector2I origin, int atlasWidth, float totalSize, BlockType blockType) {
		blockType.UVSize = Vector2.One/atlasWidth;

		for(int x = 0; x < textureWidth; x++) {
			for(int y = 0; y < textureWidth; y++) {
				Vector2I localPixel = new Vector2I(x,y);
				Vector2I globalPixel = origin + localPixel;
				Vector2 UVPosition = new Vector2(globalPixel.X, globalPixel.Y)/totalSize;
				textureAtlas.SetPixelv(globalPixel, blockType.texture.GetPixelv(localPixel)*blockType.modulate);
			}
		}
	}

	private int CalculateAtlasSize(int width = 1) {
		int textureCount = blockTypes.Count;
		int atlasSize = width*width;

		if(textureCount > atlasSize) return CalculateAtlasSize(width*2);
		return width;
	}
}
}