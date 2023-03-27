using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelPlugin {
public static class BlockLibrary {
	public static Mesh voxelMesh = GD.Load<Mesh>("res://addons/VoxelTerrain/Parts/Blocks/Mesh/VoxelFace.tres");
	public static Dictionary<string, BlockType> blockTypes = new Dictionary<string, BlockType>();
	public static List<VoxelRenderer> renderers = new List<VoxelRenderer>();

	public static Image textureAtlas;
	public static ImageTexture texture;

	public static int textureWidth = 16;
	public static float atlasScale = 0.0f;

	static BlockLibrary() {
		AddBlockType("Air", new BlockType(new Color(1,1,1,0), textureWidth, true));
		AddBlockType("Default", new BlockType(new Color(1,1,1), textureWidth));
		GetBlockType("Air").rendered = false;
	}

	static public BlockType AddBlockType(string name, BlockType blockType) {
		blockType.name = name;
		blockTypes.Add(name, blockType);
		ConstructTextureAtlas();
		return blockType;
	}

	static public BlockType GetBlockType(string name) {
		if(blockTypes.ContainsKey(name)) return blockTypes[name];
		return null;
	}

	static public void SetTexture(string name, SIDE side, Image texture) {
		if(!blockTypes.ContainsKey(name)) return;
		BlockType type = blockTypes[name];
		BlockTexture blockTexture = null;
		
		if(texture != null) {
			blockTexture = new BlockTexture(type, texture);
		} else {
			blockTexture = new BlockTexture(type, textureWidth);
		}
		type.textureTable[side] = blockTexture;
		type.textures[side] = blockTexture;
		ConstructTextureAtlas();
	}

	static public void SetModulate(String name, Color modulate) {
		if(!blockTypes.ContainsKey(name)) return;
		BlockType type = blockTypes[name];
		type.modulate = modulate;
		ConstructTextureAtlas();
	}

	static public Godot.Collections.Dictionary GetTexturesAsDictionary() {
		Godot.Collections.Dictionary textures = new Godot.Collections.Dictionary();

		foreach(KeyValuePair<string, BlockType> keyValuePair in blockTypes) {
			textures[keyValuePair.Key] = keyValuePair.Value.GetTexturesAsDictionary();
		}

		return textures;
	}

	static public void SetTexturesFromDictionary(Godot.Collections.Dictionary settings) {
		blockTypes.Clear();

		foreach(KeyValuePair<Variant, Variant> keyValuePair in settings) {
			string name = (string) keyValuePair.Key;
			BlockType blockType = new BlockType(new Color(1,1,1,0), textureWidth, false);
			blockType.SetTexturesFromDictionary((Godot.Collections.Dictionary) keyValuePair.Value);
			AddBlockType(name, blockType);
		}
	}

	static private void ConstructTextureAtlas() {
		//Extract all textures from the BlockTypes. Sort textures into the same list if they are duplicate.
		Dictionary<string, List<BlockTexture>> textures = new Dictionary<string, List<BlockTexture>>();
		foreach(BlockType blockType in blockTypes.Values) {
			List<BlockTexture> blockTextures = blockType.GetAllTextures();
			foreach(BlockTexture blockTexture in blockTextures) {
				string id = blockTexture.GetData();
				if(!textures.ContainsKey(id)) textures.Add(id, new List<BlockTexture>());
				textures[id].Add(blockTexture);
			}
		}
		
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

		textureAtlas.GenerateMipmaps();
		texture = ImageTexture.CreateFromImage(textureAtlas);
		Material voxelMaterial = voxelMesh.SurfaceGetMaterial(0);
		voxelMaterial.Set("shader_parameter/textureAtlas", BlockLibrary.texture);
		voxelMaterial.Set("shader_parameter/atlasScale", BlockLibrary.atlasScale);
		UpdateRenderers();
	}

	static private void ApplyTexture(Vector2I origin, int atlasWidth, float totalSize, BlockTexture blockTexture) {
		blockTexture.UVSize = Vector2.One/atlasWidth;
		blockTexture.UVPosition = new Vector2(origin.X, origin.Y)/totalSize;
		atlasScale = blockTexture.UVSize.X;

		for(int x = 0; x < textureWidth; x++) {
			for(int y = 0; y < textureWidth; y++) {
				Vector2I localPixel = new Vector2I(x,y);
				Vector2I globalPixel = origin + localPixel;
				Vector2 UVPosition = new Vector2(globalPixel.X, globalPixel.Y)/totalSize;
				textureAtlas.SetPixelv(globalPixel, blockTexture.texture.GetPixelv(localPixel)*blockTexture.owner.modulate);
			}
		}
	}

	static private int CalculateAtlasSize(int textureCount, int width = 1) {
		int atlasSize = width*width;

		if(textureCount > atlasSize) return CalculateAtlasSize(textureCount, width+1);
		return width;
	}

	static private void UpdateRenderers() {
		foreach(VoxelRenderer renderer in renderers) renderer.RequestUpdate();
	}
}
}