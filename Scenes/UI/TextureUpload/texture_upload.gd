extends Control

@export var side : VoxelAPI.SIDE

var api : VoxelAPI

var typeName : String

func prepare(name):
	typeName = name
	$HBoxContainer/Container/Texture.texture = ImageTexture.create_from_image(api.getTexture(name, side))
	$HBoxContainer/Container/Texture.modulate = api.getModulate(name)
	$HBoxContainer/Name.text = "Side: "+str(api.sideToStr(side))

func _ready():
	api = get_tree().current_scene

func _on_file_dialog_file_selected(path):
	var image : Image = Image.load_from_file(path)
	if image == null: return
	var textureSize = api.getTextureSize()
	image.resize(textureSize, textureSize)
	api.setTexture(typeName, side, image)
	$HBoxContainer/Container/Texture.texture = ImageTexture.create_from_image(image)


func _on_button_pressed():
	$HBoxContainer/Container/FileDialog.popup_centered()

func _on_clear_pressed():
	api.setTexture(typeName, side, null)
	$HBoxContainer/Container/Texture.texture = ImageTexture.create_from_image(api.getTexture(typeName, side))
