using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public partial class VoxelRenderer : Node3D
{

	Material voxelMaterial;

	Block[,,] recentGrid;
	Block[,,] activeGrid;
	bool threadActive = false;

	Queue<MeshInstance> meshes = new Queue<MeshInstance>();
	
	public override void _Ready() {
		voxelMaterial = GD.Load<Material>("res://addons/VoxelTerrain/Parts/Blocks/Mesh/VoxelMaterial.tres");

		for(int i = 0; i < 2; i++) {
			MeshInstance multiMeshInstance = new MeshInstance();
			multiMeshInstance.instance = RenderingServer.InstanceCreate();
        	RenderingServer.InstanceSetScenario(multiMeshInstance.instance, GetWorld3D().Scenario);

			multiMeshInstance.mesh = RenderingServer.MeshCreate();
			RenderingServer.InstanceSetBase(multiMeshInstance.instance, multiMeshInstance.mesh);
			meshes.Enqueue(multiMeshInstance);
		}
	}

	float cooldown = 0.0f;
	bool processing = false;
	public override void _Process(double delta) {
		if(!processing || !active) return;

		cooldown -= Convert.ToSingle(delta);

		if(cooldown <= 0.0f) {
			if(threadActive) return;
			processing = false;
			threadActive = true;
			cancelThread = false;
			activeGrid = (Block[,,]) recentGrid.Clone();
			VoxelPluginMain.GetThreadPool(VoxelPluginMain.POOL_TYPE.RENDERING, this).RequestFunctionCall(this, "UpdateMesh");
		}
	}

	bool active = false;
	bool cancelThread = false;

	public void Activate() {
		active = true;
		cooldown = 1.0f;
	}

	public void Deactivate() {
		active = false;
		cancelThread = true;

		for(int i = 0; i < 2; i++) {
			MeshInstance meshInstance = meshes.Dequeue();
			RenderingServer.InstanceSetVisible(meshInstance.instance, false);
			meshes.Enqueue(meshInstance);
		}
	}

	public void RequestUpdate(Block[,,] grid) {
		recentGrid = grid;
		processing = true;
		if(cooldown < 0.0f) cooldown = 0.05f;
	}

	
	private Dictionary<BlockTexture, Dictionary<SIDE, List<Vector3>>> faceList = new Dictionary<BlockTexture, Dictionary<SIDE, List<Vector3>>>();
	private List<Quad> quads = new List<Quad>();
	int faceCount = 0;

	public void UpdateMesh() {
		faceCount = 0;
		CollectFaces(activeGrid);
		CollectQuads();

		MeshInstance newMeshInstance = meshes.Dequeue();
	
		GenerateMesh(newMeshInstance);

		RenderingServer.InstanceSetVisible(newMeshInstance.instance, true);

		MeshInstance oldMeshInstance = meshes.Dequeue();
		RenderingServer.MeshClear(oldMeshInstance.mesh);
		RenderingServer.InstanceSetVisible(oldMeshInstance.instance, false);

		meshes.Enqueue(oldMeshInstance);
		meshes.Enqueue(newMeshInstance);

		//GD.Print("faces collected "+GD.VarToStr(faceCount)+"    quads collected "+GD.VarToStr(quads.Count));

		faceList.Clear();
		quads.Clear();

		threadActive = false;
	}

	private void CollectFaces(Block[,,] grid) {
		Vector3I size = new Vector3I(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));

		for(int x = 0; x < size.X; x++) {
            for(int y = 0; y < size.Y; y++) {
                for(int z = 0; z < size.Z; z++) {
					Block block = grid[x,y,z];
					if(block.blockType == null) continue;
					CollectFace(block, SIDE.TOP);
					CollectFace(block, SIDE.BOTTOM);
					CollectFace(block, SIDE.LEFT);
					CollectFace(block, SIDE.RIGHT);
					CollectFace(block, SIDE.FRONT);
					CollectFace(block, SIDE.BACK);
				}
			}
		}
	}

	private void CollectFace(Block block, SIDE side) {
		BlockType blockType = block.blockType;
		if(blockType == null || !blockType.rendered) return;
		Vector3 direction = Block.SideToVector(side);

		BlockType neighbour = Chunk.GetBlockType(block.position + direction);
		if(neighbour == null || (!neighbour.transparent && neighbour.rendered)) return;

		BlockTexture blockTexture = blockType.GetTexture(side);
		if(!faceList.ContainsKey(blockTexture)) faceList.Add(blockTexture, new Dictionary<SIDE, List<Vector3>>());
		Dictionary<SIDE, List<Vector3>> positionList = faceList[blockTexture];
		if(!positionList.ContainsKey(side)) positionList.Add(side, new List<Vector3>());
		positionList[side].Add(block.position);
	}

	private void CollectQuads() {
		foreach(KeyValuePair<BlockTexture, Dictionary<SIDE, List<Vector3>>> blockTexturePair in faceList) {
			foreach(KeyValuePair<SIDE, List<Vector3>> sidePositionPair in blockTexturePair.Value) {
				List<Vector3> consumedFaces = new List<Vector3>();
				Vector3[] scanDirections = scanTable[sidePositionPair.Key];

				foreach(Vector3 position in sidePositionPair.Value) {
					faceCount += 1;
					if(consumedFaces.Contains(position)) continue;
					Vector3 currentPosition = position;

					//Calculate the width and length of the quad
					int quadLength = ScanDirection(currentPosition, 1, scanDirections[0], sidePositionPair.Value, consumedFaces);
					int quadWidth = -1;
					for(int i = 0; i < quadLength; i++) {
						int width = ScanDirection(currentPosition + scanDirections[0]*i, 1, scanDirections[1], sidePositionPair.Value, consumedFaces);
						if(quadWidth < 1 || width < quadWidth) quadWidth = width;
						if(quadWidth == 1) break;
					}

					//Consume faces so they can't be used in further iterations
					for(int y = 0; y < quadLength; y++) for(int x = 0; x < quadWidth; x++) consumedFaces.Add(currentPosition + scanDirections[0]*y + scanDirections[1]*x);

					Quad quad = new Quad();
					quad.blockTexture = blockTexturePair.Key;
					quad.position = currentPosition;
					quad.scale2D = new Vector2(quadLength, quadWidth);
					quad.scale = Vector3.One * (scanDirections[0]*quadLength+scanDirections[1]*quadWidth);
					if(quad.scale.X < 1) quad.scale.X = 1;
					if(quad.scale.Y < 1) quad.scale.Y = 1;
					if(quad.scale.Z < 1) quad.scale.Z = 1;
					quad.quadIndex = sidePositionPair.Key;
					quads.Add(quad);
				}
			}
		}
	}

	private int ScanDirection(Vector3 position, int x, Vector3 axis, List<Vector3> faces, List<Vector3> consumedFaces) {
		Vector3 scanPosition = position + axis*x;
		if(faces.Contains(scanPosition) && !consumedFaces.Contains(scanPosition)) return ScanDirection(position, x+1, axis, faces, consumedFaces);
		return x;
	}

	private void GenerateMesh(MeshInstance meshInstance) {
		if(quads.Count == 0) {
			RenderingServer.InstanceSetVisible(meshInstance.instance, false);
			return;
		}

		Godot.Collections.Array array = new Godot.Collections.Array();
		array.Resize((int) RenderingServer.ArrayType.Max);

		Vector3[] vertexList = new Vector3[quads.Count*6];
		Vector2[] uvList = new Vector2[vertexList.Length];
		Vector2[] uv2List = new Vector2[vertexList.Length];
		Vector3[] normalList = new Vector3[vertexList.Length];
		Color[] colors = new Color[vertexList.Length];

		// vertexList.Resize(quads.Count*6);
		// uvList.Resize(quads.Count*6);
		// normalList.Resize(quads.Count*6);

		int vertexCount = 0;
		foreach(Quad quad in quads) {
			Vector3[] vertices = quadTable[quad.quadIndex];
			Vector3 normal = normalTable[quad.quadIndex];

			Color color = new Color(
				quad.blockTexture.UVPosition.X,
				quad.blockTexture.UVPosition.Y,
				quad.blockTexture.UVSize.X
			);

			Vector2 UV2 = UVFlipTable[quad.quadIndex] ? new Vector2(quad.scale2D.Y, quad.scale2D.X) : quad.scale2D;

			for(int i = 0; i < 6; i++) {
				uvList[vertexCount] = UVTable[i]*UV2;
				uv2List[vertexCount] = UV2;
				vertexList[vertexCount] = (vertices[i]*quad.scale)+quad.position;
				normalList[vertexCount] = normal;
				colors[vertexCount] = color;
				vertexCount++;
			}
		}

		array[(int) RenderingServer.ArrayType.Vertex] = vertexList;
		array[(int) RenderingServer.ArrayType.TexUV] = uvList;
		array[(int) RenderingServer.ArrayType.TexUV2] = uv2List;
		array[(int) RenderingServer.ArrayType.Normal] = normalList;
		array[(int) RenderingServer.ArrayType.Color] = colors;

		// new ArrayMesh().AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
		
		RenderingServer.MeshAddSurfaceFromArrays(
			meshInstance.mesh,
			RenderingServer.PrimitiveType.Triangles,
			array
		);
		
		voxelMaterial.Set("shader_parameter/textureAtlas", BlockLibrary.texture);
		RenderingServer.MeshSurfaceSetMaterial(meshInstance.mesh, 0, voxelMaterial.GetRid());
	}

	private class MeshInstance {
		public Rid instance;
		public Rid mesh;
	}

	private class Quad {
		public BlockTexture blockTexture;
		public Vector3 position;
		public Vector2 scale2D = Vector2.One;
		public Vector3 scale = Vector3.One;
		public SIDE quadIndex = 0;
	}

	private static Dictionary<SIDE, Vector3[]> scanTable = new Dictionary<SIDE, Vector3[]> {
		{SIDE.FRONT, new Vector3[] {Vector3.Up, Vector3.Right}},
		{SIDE.BACK, new Vector3[] {Vector3.Right, Vector3.Up}},
		{SIDE.LEFT, new Vector3[] {Vector3.Back, Vector3.Up}},
		{SIDE.RIGHT, new Vector3[] {Vector3.Up, Vector3.Back}},
		{SIDE.TOP, new Vector3[] {Vector3.Back, Vector3.Right}},
		{SIDE.BOTTOM, new Vector3[] {Vector3.Right, Vector3.Back}}
	};

	private static Dictionary<SIDE, Vector3[]> quadTable = new Dictionary<SIDE, Vector3[]> {
		{SIDE.FRONT, new Vector3[] {new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,0,0), new Vector3(1,1,0)}},
		{SIDE.BACK, new Vector3[] {new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,0,1), new Vector3(0,1,1)}},
		{SIDE.LEFT, new Vector3[] {new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,0,1), new Vector3(0,1,0)}},
		{SIDE.RIGHT, new Vector3[] {new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,0,0), new Vector3(1,1,1)}},
		{SIDE.TOP, new Vector3[] {new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(1,1,0)}},
		{SIDE.BOTTOM, new Vector3[] {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(0,0,1)}}
	};

	private static Dictionary<SIDE, Vector3> normalTable = new Dictionary<SIDE, Vector3> {
		{SIDE.FRONT, Vector3.Forward},
		{SIDE.BACK, Vector3.Back},
		{SIDE.LEFT, Vector3.Left},
		{SIDE.RIGHT, Vector3.Right},
		{SIDE.TOP, Vector3.Up},
		{SIDE.BOTTOM, Vector3.Down}
	};

	private static Dictionary<SIDE, bool> UVFlipTable = new Dictionary<SIDE, bool> {
		{SIDE.FRONT, true},
		{SIDE.BACK, false},
		{SIDE.LEFT, false},
		{SIDE.RIGHT, true},
		{SIDE.TOP, false},
		{SIDE.BOTTOM, false}
	};

	private static Vector2[] UVTable = new Vector2[] {
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,0), new Vector2(1,1), new Vector2(1,0), new Vector2(0,1) 
	};
}
}