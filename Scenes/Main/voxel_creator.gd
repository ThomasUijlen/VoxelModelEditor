extends Node2D

var api : VoxelAPI

func _ready():
	api = get_tree().current_scene

func _on_voxel_type_list_type_selected(name):
	for texture in %Textures.get_children():
		if texture.has_method("prepare"):
			texture.prepare(name)

func _on_add_pressed():
	api.createBlockType(str(randi_range(0,10000)), null, Color.RED)
	%VoxelTypeList.refreshTypeList()
