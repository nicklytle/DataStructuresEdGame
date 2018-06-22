# This is the source code for the converter
# The .exe was built using ocra: https://github.com/larsch/ocra

require 'tmx'

puts "Input (.tmx) file: #{ARGV[0]}"
puts "Output (.json) file: #{ARGV[1]}"

if ARGV[0] == nil or ARGV[1] == nil
	return
end

map_file = File.open(ARGV[0].to_str, "r")
out_file = File.new(ARGV[1].to_str, "w")

if map_file and out_file 	
	map = Tmx.load(map_file)
	mh = (map.height / 64).to_int
	# objects inside of the map
	Block = Struct.new(:x,:y)
	SizedBlock = Struct.new(:x,:y,:width,:height)
	LinkBlock = Struct.new(:x,:y,:connectTo)
	SingleLLPlatform = Struct.new(:x,:y,:name,:connectTo,:value)

	winCondition = map.properties["WinCondition"].to_str
	player = nil
	startLink = nil
	goalPortal = nil
	helicopterRobot = nil
	blocks = []
	linkBlocks = []
	objectiveBlocks = []
	singleLinkedListPlatforms = []

	for group in map.object_groups
		for obj in group.objects 
			if obj.type == "Player"
				player = Block.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int)
			elsif obj.type == "Ground" 
				blocks.push(SizedBlock.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int, (obj.width / 64).to_int, (obj.height / 64).to_int))
			elsif obj.type == "StartLinkBlock"
				startLink = LinkBlock.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int, obj.properties["ConnectTo"])
			elsif obj.type == "LinkBlock"
				linkBlocks.push(LinkBlock.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int, obj.properties["ConnectTo"]))
			elsif obj.type == "HelicopterRobot"
				helicopterRobot = Block.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int)
			elsif obj.type == "Goal"
				goalPortal = Block.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int)
			elsif obj.type == "SingleLLPlatform"
				singleLinkedListPlatforms.push(SingleLLPlatform.new((obj.x / 64).to_int + 1, mh - (obj.y / 64).to_int, obj.name, obj.properties["ConnectTo"], obj.properties["Value"]))
			elsif obj.type == "ObjectiveBlock"
				objectiveBlocks.push(Block.new((obj.x / 64).to_int, mh - (obj.y / 64).to_int))
			end
		end
	end

	# write to the output file
	out_file.syswrite("{\n")
	out_file.syswrite("\"winCondition\":\"#{winCondition}\",\n")
	out_file.syswrite("\"player\":{\"type\":\"PLAYER\",\"x\":#{player['x']},\"y\":#{player['y']}},\n")
	out_file.syswrite("\"startLink\":{\"type\":\"LINK_BLOCK\",\"x\":#{startLink['x']},\"y\":#{startLink['y']},\"objIDConnectingTo\":\"#{startLink['connectTo']}\"},\n")
	out_file.syswrite("\"goalPortal\":{\"type\":\"GOAL_PORTAL\",\"x\":#{goalPortal['x']},\"y\":#{goalPortal['y']}},\n")
	out_file.syswrite("\"helicopterRobot\":{\"type\":\"HELICOPTER_ROBOT\",\"x\":#{helicopterRobot['x']},\"y\":#{helicopterRobot['y']}},\n")
	out_file.syswrite("\"blocks\":[\n") # start blocks
	for b in blocks 
		out_file.syswrite("{\"type\":\"GROUND\",\"x\":#{b['x']},\"y\":#{b['y']},\"width\":#{b['width']},\"height\":#{b['height']}}")
		if b != blocks.last
			out_file.syswrite(",")
		end
		out_file.syswrite("\n")
	end
	out_file.syswrite("],\n") # end blocks
	out_file.syswrite("\"objectiveBlocks\":[\n") # start objectiveBlocks
	for ob in objectiveBlocks
		out_file.syswrite("{\"type\":\"OBJECTIVE_BLOCK\",\"x\":#{ob['x']},\"y\":#{ob['y']}}")
		if ob != objectiveBlocks.last
			out_file.syswrite(",")
		end
		out_file.syswrite("\n")
	end
	out_file.syswrite("],\n") # end objectiveBlocks
	out_file.syswrite("\"linkBlocks\":[\n") # start linkBlocks
	for lb in linkBlocks
		out_file.syswrite("{\"type\":\"LINK_BLOCK\",\"x\":#{lb['x']},\"y\":#{lb['y']},\"objIDConnectingTo\":\"#{lb['connectTo']}\"}")
		if lb != linkBlocks.last
			out_file.syswrite(",")
		end
		out_file.syswrite("\n")
	end
	out_file.syswrite("],\n") # end linkBlocks
	out_file.syswrite("\"singleLinkedListPlatforms\":[\n") # start singleLinkedListPlatforms
	for p in singleLinkedListPlatforms
		out_file.syswrite("{\"type\":\"SINGLE_LL_PLATFORM\",\"x\":#{p['x']},\"y\":#{p['y']},\"objId\":\"#{p['name']}\",")
		out_file.syswrite("\"value\":\"#{p['value']}\",\"childLinkBlockConnectId\":\"#{p['connectTo']}\"}")
		if p != singleLinkedListPlatforms.last
			out_file.syswrite(",")
		end
		out_file.syswrite("\n")
	end
	out_file.syswrite("]\n") # end singleLinkedListPlatforms
	out_file.syswrite("}")
	puts "Done."
else
	puts "Failed reading the input or output files"
end