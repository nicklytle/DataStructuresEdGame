Made by Kevin for rapid level creation

Making levels with this:

1. Make an image of any size.
2. Make a .JSON config file. It contains
	one element "blocks_def" which is
	an array of objects where each object
	defines one block type. The objects
	have these attributes:
	- "type", a String type for that color
	- "color_*", The Red, Green, Blue values
		for what pixel color in the input
		image represents that block type
3. Run the program with three arguments: The config
	file, the input image, and the name of the
	output file (in that order). 
