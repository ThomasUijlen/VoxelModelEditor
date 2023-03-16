extends Node2D

var api : VoxelAPI

func _ready():
	api = get_tree().current_scene

func _on_voxel_type_list_type_selected(name):
	for texture in %Textures.get_children():
		if texture.has_method("prepare"):
			texture.prepare(name)

func _on_add_pressed():
	$NewVoxelPopup.visible = true

func _on_create_pressed():
	var name = %VoxelName.text
	if api.hasBlockType(name): return
	api.createBlockType(name, null, Color.WHITE)
	%VoxelTypeList.refreshTypeList()

func _on_cancel_pressed():
	$NewVoxelPopup.visible = false
