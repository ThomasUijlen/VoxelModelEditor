using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelTerrainPlugin {
public class BlockLibrary {
	List<BlockType> blockTypes = new List<BlockType>();

	public Image textureAtlas;

	int textureWidth = 16;

	public BlockType AddBlockType(BlockType blockType) {
		blockTypes.Add(blockType);
		ConstructTextureAtlas();
		return blockType;
	}

	public void ConstructTextureAtlas() {
		GD.Print("***");
		//Extract all textures from the BlockTypes. Sort textures into the same list if they are duplicate.
		Dictionary<string, List<BlockTexture>> textures = new Dictionary<string, List<BlockTexture>>();
		foreach(BlockType blockType in blockTypes) {
			List<BlockTexture> blockTextures = blockType.GetAllTextures();
			foreach(BlockTexture blockTexture in blockTextures) {
				string id = BitConverter.ToString(blockTexture.texture.GetData());
				if(!textures.ContainsKey(id)) textures.Add(id, new List<BlockTexture>());
				textures[id].Add(blockTexture);
			}
		}
		GD.Print("---");
		//Create the texture atlas image and color it purple
		int atlasWidth = CalculateAtlasSize(textures.Count);
		textureAtlas = Image.Create(atlasWidth*textureWidth, atlasWidth*textureWidth, true, Image.Format.Rgba8);

		for(int x = 0; x < atlasWidth*textureWidth; x++) {
			for(int y = 0; y < atlasWidth*textureWidth; y++) {
				textureAtlas.SetPixel(x, y, new Color(0.71f, 0.05f, 0.89f));
			}
		}

		for(int x = 0, i = 0; x < atlasWidth; x++) {
			for(int y = 0; y < atlasWidth; y++, i++) {
				if(i >= textures.Count) break;
				List<BlockTexture> textureList = textures.ElementAt(i).Value;
				foreach(BlockTexture blockTexture in textureList) ApplyTexture(new Vector2I(x*textureWidth, y*textureWidth), atlasWidth, atlasWidth*textureWidth, blockTexture);
			}
		}
	}

	private void ApplyTexture(Vector2I origin, int atlasWidth, float totalSize, BlockTexture blockTexture) {
		blockTexture.UVSize = Vector2.One/atlasWidth;

		for(int x = 0; x < textureWidth; x++) {
			for(int y = 0; y < textureWidth; y++) {
				Vector2I localPixel = new Vector2I(x,y);
				Vector2I globalPixel = origin + localPixel;
				Vector2 UVPosition = new Vector2(globalPixel.X, globalPixel.Y)/totalSize;
				textureAtlas.SetPixelv(globalPixel, blockTexture.texture.GetPixelv(localPixel)*blockTexture.owner.modulate);
			}
		}
	}

	private int CalculateAtlasSize(int textureCount, int width = 1) {
		int atlasSize = width*width;

		if(textureCount > atlasSize) return CalculateAtlasSize(textureCount, width+1);
		return width;
	}
}
}