extends Node3D

var hoverDistance : int = 5
var castDistance : int = 15
var blockType : String = "Stone"

var ray : RayCast3D
var cube : Node3D

var api : VoxelAPI

func _ready():
	ray = $RayCast3D
	cube = $Cube
	remove_child(cube)
	api = get_tree().current_scene

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
		colPos = ray.to_global(Vector3(0,0,-hoverDistance))
	
	cube.global_position = colPos.floor()
	if Input.is_action_just_pressed("MouseLeft"):
		api.setBlock(colPos, blockType)
