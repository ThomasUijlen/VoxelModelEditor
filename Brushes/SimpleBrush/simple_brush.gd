extends VoxelTool

var hoverDistance : int = 15
var castDistance : int = 15
var radius : int = 1
var blockType : String = "Stone"

var ray : RayCast3D
var cube : Node3D

var cubePool : Array = []
var activeCubes : Array = []

func _ready():
	super._ready()
	ray = $RayCast3D
	cube = $Cube
	remove_child(cube)
	setRadius(radius)

func setRadius(radius : int):
	if radius < 1: radius = 1
	if radius > 15: radius = 15
	self.radius = radius
	
	var half = floor(radius/2.0)
	
	for cube in activeCubes:
		$Cubes.remove_child(cube)
		cubePool.append(cube)
	activeCubes.clear()
	
	for x in range(radius):
		for y in range(radius):
			for z in range(radius):
				var coord = Vector3(x,y,z)
				if radius % 2 == 0: coord += Vector3.ONE*0.5
				if (Vector3.ZERO+Vector3.ONE*half).distance_to(coord) <= radius/2.0:
					var newCube = null
					if cubePool.size() == 0:
						newCube = cube.duplicate()
					else:
						newCube = cubePool.pop_back()
					newCube.position = coord-Vector3.ONE*half
					$Cubes.add_child(newCube)
					activeCubes.append(newCube)

func _physics_process(delta):
	var camera : Camera3D = get_viewport().get_camera_3d()
	ray.global_transform.basis = camera.global_transform.basis
	ray.global_transform.origin = camera.global_position
	
	ray.target_position.z = -castDistance
	
	var colPos : Vector3
	if ray.is_colliding():
		colPos = ray.get_collision_point()
		colPos += ray.get_collision_normal()*0.55
	else:
		colPos = ray.to_global(Vector3(0,0,-hoverDistance-radius/2))
	
	$Cubes.global_position = colPos.floor()
	if Input.is_action_just_pressed("MouseLeft"):
		for cube in activeCubes:
			api.setBlock(cube.global_position, blockType)

func _input(event):
	var radiusIncrease = 0
	if event.is_action_pressed("ScrollUp"):
		radiusIncrease += 1
	if event.is_action_pressed("ScrollDown"):
		radiusIncrease -= 1
	if radiusIncrease == 0: return
	
	setRadius(radius + radiusIncrease)
