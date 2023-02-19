using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VoxelPlugin {
public partial class VoxelRenderer : Node3D
{
	enum STATE {
		DISABLED,
		IDLE,
		WAITING_FOR_UPDATE,
		UPDATING,
		WAIT_FOR_RENDER_PASS
	}

	private STATE activeState = STATE.DISABLED;

	Block[,,] recentGrid;
	Block[,,] activeGrid;

	Queue<MeshInstance> meshes = new Queue<MeshInstance>();
	List<MeshInstance> meshList = new List<MeshInstance>();

	int quadCount = 500;

	private VoxelMain.POOL_TYPE poolType = VoxelMain.POOL_TYPE.RENDERING;
	public VoxelWorld world;
	public Chunk chunk;

	static ConcurrentBag<long> times = new ConcurrentBag<long>();
	
	public override void _Ready() {
		for(int i = 0; i < 2; i++) {
			MeshInstance multiMeshInstance = new MeshInstance();
			multiMeshInstance.instance = RenderingServer.InstanceCreate();
        	RenderingServer.InstanceSetScenario(multiMeshInstance.instance, GetWorld3D().Scenario);

			multiMeshInstance.mesh = RenderingServer.MultimeshCreate();
			RenderingServer.InstanceSetBase(multiMeshInstance.instance, multiMeshInstance.mesh);
			RenderingServer.InstanceGeometrySetCastShadowsSetting(multiMeshInstance.instance, RenderingServer.ShadowCastingSetting.DoubleSided);
			RenderingServer.MultimeshSetMesh(multiMeshInstance.mesh, BlockLibrary.voxelMesh.GetRid());
			RenderingServer.MultimeshAllocateData(multiMeshInstance.mesh, quadCount, RenderingServer.MultimeshTransformFormat.Transform3D, true);
			RenderingServer.InstanceSetVisible(multiMeshInstance.instance, false);
			meshes.Enqueue(multiMeshInstance);
			meshList.Add(multiMeshInstance);
		}

		long total = 0;
		foreach(long time in times) {
			total += time;
		}

		if(times.Count > 0) GD.Print(total/times.Count);
	}

	public override void _Process(double delta) {
		switch(activeState) {
			case STATE.WAITING_FOR_UPDATE:
				if(world.RenderUpdatePass()) {
					activeState = STATE.UPDATING;
					activeGrid = (Block[,,]) recentGrid.Clone();
					VoxelMain.GetThreadPool(poolType, this).RequestFunctionCall(this, "UpdateMesh");
					poolType = VoxelMain.POOL_TYPE.RENDERING_CLOSE;
				}
			break;

			case STATE.WAIT_FOR_RENDER_PASS:
				if(world.RenderUpdatePass()) {
					activeState = changePending ? STATE.WAITING_FOR_UPDATE : STATE.IDLE;
					changePending = false;
					MeshInstance newMeshInstance = meshes.Dequeue();
					MeshInstance oldMeshInstance = meshes.Dequeue();

					RenderingServer.InstanceSetVisible(newMeshInstance.instance, true);
					RenderingServer.InstanceSetVisible(oldMeshInstance.instance, false);

					meshes.Enqueue(oldMeshInstance);
					meshes.Enqueue(newMeshInstance);
				}
			break;
		}
	}

	public void Activate() {
		activeState = STATE.IDLE;
		poolType = VoxelMain.POOL_TYPE.RENDERING;
	}

	public void Deactivate() {
		activeState = STATE.DISABLED;

		for(int i = 0; i < 2; i++) {
			MeshInstance meshInstance = meshes.Dequeue();
			RenderingServer.InstanceSetVisible(meshInstance.instance, false);
			meshes.Enqueue(meshInstance);
		}
	}

	bool changePending = false;
	public void RequestUpdate(Block[,,] grid, bool close = true) {
		if(!close) poolType = VoxelMain.POOL_TYPE.RENDERING;
		recentGrid = grid;
		
		if(activeState == STATE.IDLE) {
			activeState = STATE.WAITING_FOR_UPDATE;
		} else {
			changePending = true;
		}
	}

	
	private Dictionary<BlockTexture, Dictionary<SIDE, List<Vector3>>> faceList = new Dictionary<BlockTexture, Dictionary<SIDE, List<Vector3>>>();
	private List<Quad> quads = new List<Quad>();

	public void UpdateMesh() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		CollectFaces(activeGrid);
		CollectQuads();

		MeshInstance newMeshInstance = meshes.Dequeue();
		
		GenerateMesh(newMeshInstance);

		MeshInstance oldMeshInstance = meshes.Dequeue();

		meshes.Enqueue(newMeshInstance);
		meshes.Enqueue(oldMeshInstance);

		faceList.Clear();
		quads.Clear();

		activeState = STATE.WAIT_FOR_RENDER_PASS;

		watch.Stop();
		var elapsedMs = watch.ElapsedMilliseconds;
		times.Add(elapsedMs);
	}

	private void CollectFaces(Block[,,] grid) {
		Vector3I size = new Vector3I(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));

		for(int x = 0; x < size.X; x++) {
            for(int y = 0; y < size.Y; y++) {
                for(int z = 0; z < size.Z; z++) {
					Block block = grid[x,y,z];
					if(block.blockType == null || !block.blockType.rendered) continue;
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
		Vector3 direction = Block.SideToVector(side);

		if(!block.activeSides.HasFlag(side)) return;

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

					//Consume faces so they can't be used again
					for(int y = 0; y < quadLength; y++) for(int x = 0; x < quadWidth; x++) consumedFaces.Add(currentPosition + scanDirections[0]*y + scanDirections[1]*x);

					//Add a quad to the list
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
		if(quads.Count >= quadCount) {
			while(quads.Count >= quadCount) quadCount += 250;
		}

		if(RenderingServer.MultimeshGetInstanceCount(meshInstance.mesh) < quadCount) {
			RenderingServer.MultimeshAllocateData(meshInstance.mesh, quadCount, RenderingServer.MultimeshTransformFormat.Transform3D, true);
		}

		RenderingServer.MultimeshSetVisibleInstances(meshInstance.mesh, quads.Count);

		//RandomNumberGenerator rng = new RandomNumberGenerator();

		for(int i = 0; i < quads.Count; i ++) {
			Quad quad = quads[i];
			Transform3D transform = new Transform3D(basisTable[quad.quadIndex], Vector3.Zero);
			transform = transform.Scaled(quad.scale);
			transform.Origin += quad.position + quad.scale*0.5f;

			Vector2 UV2 = UVFlipTable[quad.quadIndex] ? quad.scale2D : new Vector2(quad.scale2D.Y, quad.scale2D.X);

			Color color = new Color(
				quad.blockTexture.UVPosition.X,
				quad.blockTexture.UVPosition.Y,
				1.0f/UV2.X,
				1.0f/UV2.Y
			);

			//Color color = new Color(rng.RandfRange(0f,1f),rng.RandfRange(0f,1f),rng.RandfRange(0f,1f));

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