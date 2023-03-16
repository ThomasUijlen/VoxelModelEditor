extends Button

signal menuPressed(menuButton)

@export var menuPath : NodePath
@export var buttonEnabled : bool = false
var menu : Node

func _ready():
	menu = get_node(menuPath)
	connect("pressed", buttonPressed)

func buttonPressed():
	emit_signal("menuPressed", self)

func enable():
	menu.visible = true
	disabled = true

func disable():
	menu.visible = false
	disabled = false
