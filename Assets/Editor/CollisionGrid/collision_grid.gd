extends Node3D

@export var boxSize : Vector3

var api : VoxelAPI

var block : Node

func _ready():
	api = get_tree().current_scene
	
	block = $Box
	remove_child(block)
	
	var half = boxSize/2
	
	for x in range(boxSize.x):
		for y in range(boxSize.y):
			for z in range(boxSize.z):
				var coord = Vector3(x-half.x,y-half.y,z-half.z)
				var newBlock = block.duplicate()
				newBlock.position = coord
				add_child(newBlock)
	
	block.queue_free()

func _process(delta):
	var cameraPos = get_viewport().get_camera_3d().global_position.floor()
	global_position = cameraPos
