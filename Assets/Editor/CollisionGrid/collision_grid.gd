extends Node3D

@export var boxSize = 20

var boxes : Dictionary = {}

var api : VoxelAPI

var block : Node

func _ready():
	api = get_tree().current_scene
	
	block = $Box
	remove_child(block)
	
	var half = boxSize/2
	
	for x in range(boxSize):
		for y in range(boxSize):
			for z in range(boxSize):
				var coord = Vector3(x,y,z)-Vector3.ONE*half
				var newBlock = block.duplicate()
				newBlock.position = coord
				add_child(newBlock)
				boxes[coord] = newBlock
	
	block.queue_free()

func _process(delta):
	var cameraPos = get_viewport().get_camera_3d().global_position.round()
	global_position = cameraPos
