using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public partial class VoxelRenderer : Node3D
{
	Block[,,] recentGrid;
	Block[,,] activeGrid;
	bool threadActive = false;

	Queue<MeshInstance> meshes = new Queue<MeshInstance>();
	List<MeshInstance> meshList = new List<MeshInstance>();

	int quadCount = 500;

	private VoxelPluginMain.POOL_TYPE poolType = VoxelPluginMain.POOL_TYPE.RENDERING;
	
	public override void _Ready() {
		for(int i = 0; i < 2; i++) {
			MeshInstance multiMeshInstance = new MeshInstance();
			multiMeshInstance.instance = RenderingServer.InstanceCreate();
        	RenderingServer.InstanceSetScenario(multiMeshInstance.instance, GetWorld3D().Scenario);

			multiMeshInstance.mesh = RenderingServer.MultimeshCreate();
			RenderingServer.InstanceSetBase(multiMeshInstance.instance, multiMeshInstance.mesh);
			RenderingServer.MultimeshSetMesh(multiMeshInstance.mesh, BlockLibrary.voxelMesh.GetRid());
			RenderingServer.MultimeshAllocateData(multiMeshInstance.mesh, quadCount, RenderingServer.MultimeshTransformFormat.Transform3D, true);
			meshes.Enqueue(multiMeshInstance);
			meshList.Add(multiMeshInstance);
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
			VoxelPluginMain.GetThreadPool(poolType, this).RequestFunctionCall(this, "UpdateMesh");
			poolType = VoxelPluginMain.POOL_TYPE.RENDERING_CLOSE;
		}
	}

	bool active = false;
	bool cancelThread = false;

	public void Activate() {
		active = true;
		cooldown = 1.0f;
		poolType = VoxelPluginMain.POOL_TYPE.RENDERING;
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

	public void RequestUpdate(Block[,,] grid, bool close = true) {
		if(!close) poolType = VoxelPluginMain.POOL_TYPE.RENDERING;
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

		if(quads.Count >= quadCount) {
			while(quads.Count >= quadCount) quadCount += 250;
			foreach(MeshInstance mesh in meshList) {
				RenderingServer.MultimeshAllocateData(mesh.mesh, quadCount, RenderingServer.MultimeshTransformFormat.Transform3D, true);
			}
		}

		RenderingServer.MultimeshSetVisibleInstances(meshInstance.mesh, quads.Count);

		for(int i = 0; i < quads.Count; i ++) {
			Quad quad = quads[i];
			Transform3D transform = new Transform3D(basisTable[quad.quadIndex], Vector3.Zero);
			transform = transform.Scaled(quad.scale);
			transform.Origin += quad.position + quad.scale*0.5f;

			Vector2 UV2 = UVFlipTable[quad.quadIndex] ? quad.scale2D : new Vector2(quad.scale2D.Y, quad.scale2D.X);
			// Vector2 UV2 = quad.scale2D;

			Color color = new Color(
				quad.blockTexture.UVPosition.X,
				quad.blockTexture.UVPosition.Y,
				1.0f/UV2.X,
				1.0f/UV2.Y
			);

			RenderingServer.MultimeshInstanceSetTransform(meshInstance.mesh, i, transform);
			RenderingServer.MultimeshInstanceSetColor(meshInstance.mesh, i, color);
		}
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

	private static Dictionary<SIDE, Basis> basisTable = new Dictionary<SIDE, Basis> {
		{SIDE.FRONT, new Basis(new Vector3(1,0,0), -Mathf.Pi/2f)},
		{SIDE.BACK, new Basis(new Vector3(1,0,0), Mathf.Pi/2f)},
		{SIDE.LEFT, new Basis(new Vector3(0,0,1), Mathf.Pi/2f).Rotated(new Vector3(1,0,0), -Mathf.Pi/2f)},
		{SIDE.RIGHT,new Basis(new Vector3(0,0,1), -Mathf.Pi/2f).Rotated(new Vector3(1,0,0), Mathf.Pi/2f)},
		{SIDE.TOP, new Basis(new Vector3(1,0,0), 0)},
		{SIDE.BOTTOM, new Basis(new Vector3(1,0,0), Mathf.Pi)}
	};

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
		{SIDE.FRONT, false},
		{SIDE.BACK, true},
		{SIDE.LEFT, true},
		{SIDE.RIGHT, false},
		{SIDE.TOP, false},
		{SIDE.BOTTOM, true}
	};

	private static Vector2[] UVTable = new Vector2[] {
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,0), new Vector2(1,1), new Vector2(1,0), new Vector2(0,1) 
	};
}
}