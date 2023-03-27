extends Node2D

var api : VoxelAPI
var currentBlock : String = ""

func _ready():
	api = get_tree().current_scene

func _on_voxel_type_list_type_selected(name):
	currentBlock = name
	%ColorPicker.color = api.getModulate(name)
	
	%VoxelTypeList.call_deferred("refreshTypeList", false)
	for texture in %Textures.get_children():
		if texture.has_method("prepare"):
			texture.prepare(name)
	%QuickTexture.prepare(name)

func _on_color_picker_color_changed(color):
	if api.getModulate(name) != %ColorPicker.color:
		api.setModulate(currentBlock, %ColorPicker.color)
	_on_voxel_type_list_type_selected(currentBlock)

func _on_add_pressed():
	$NewVoxelPopup.visible = true

func _on_create_pressed():
	var name = %VoxelName.text
	if name.length() <= 0: return
	if api.hasBlockType(name): return
	api.createBlockType(name, null, Color.WHITE)
	_on_voxel_type_list_type_selected(name)
	$NewVoxelPopup.visible = false
	%ColorPicker.color = Color.WHITE

func _on_cancel_pressed():
	$NewVoxelPopup.visible = false

func _on_texture_changed():
	_on_voxel_type_list_type_selected(currentBlock)
