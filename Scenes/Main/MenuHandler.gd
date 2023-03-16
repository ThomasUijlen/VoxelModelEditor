extends Node2D

@export var menuButtonPaths : Array[NodePath]

var menuButtons : Array[Button] = []

func _ready():
	for path in menuButtonPaths:
		var button = get_node(path)
		menuButtons.append(button)
		button.connect("menuPressed", menuPressed)
		if button.buttonEnabled:
			button.enable()
		else:
			button.disable()

func menuPressed(menuButton):
	for button in menuButtons: button.disable()
	menuButton.enable()
