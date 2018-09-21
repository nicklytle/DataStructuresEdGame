import io, json, sys

if __name__ == "__main__":
	jsonObject = {}
	jsonObject["player"] = {}
	jsonObject["player"]["logId"] = "player"
	jsonObject["player"]["type"] = "PLAYER"
	jsonObject["player"]["x"] = 0;
	jsonObject["player"]["y"] = 0;
	jsonObject["helicopterRobot"] = {}
	jsonObject["helicopterRobot"]["logId"] = "helicopterRobot"
	jsonObject["helicopterRobot"]["type"] = "HELICOPTER_ROBOT"
	jsonObject["helicopterRobot"]["x"] = 63
	jsonObject["helicopterRobot"]["y"] = 63
	jsonObject["goalPortal"] = {}
	jsonObject["goalPortal"]["logId"] = "goal"
	jsonObject["goalPortal"]["type"] = "GOAL_PORTAL"
	jsonObject["goalPortal"]["x"] = 63
	jsonObject["goalPortal"]["y"] = 63
	jsonObject["blocks"] = []

	if(len(sys.argv)<2):
		print "ERROR: Not enough arguments"
		exit()
	#print 'Argument List:', str(sys.argv)
	fh = open(sys.argv[1], "r")
	fh.readline()
	fh.readline()
	
	s = fh.readline()
	if(s=="UNKNOWN\n"):
		print "ERROR: MALFORMED INPUT"
		exit()
	s = fh.readline()
	if(s=="UNSATISFIABLE\n"):
		print "ERROR: UNSATISFIABLE"
		exit()
	s = fh.readline()
	split = s.split()
	print split
	blockLogID = 0;
	for string in split:
		if(string.startswith("winCondition")):
			winCondition = string[13:-1]
			print winCondition
			jsonObject["winCondition"] = winCondition.capitalize()
		if(string.startswith("start")):
			x = string[6:7]
			#print x
			y = string[8:9]
			#print y
			jsonObject["player"]["x"] = int(x);
			jsonObject["player"]["y"] = int(y);
		if(string.startswith("goal")):
			x = string[5:6]
			#print x
			y = string[7:8]
			#print y
			jsonObject["goalPortal"]["x"] = int(x);
			jsonObject["goalPortal"]["y"] = int(y);
		if(string.startswith("helicopter")):
			x = string[11:12]
			#print x
			y = string[13:14]
			#print y
			jsonObject["helicopterRobot"]["x"] = int(x);
			jsonObject["helicopterRobot"]["y"] = int(y);
		if(string.startswith("block")):
			block = {}
			x = string[6:7]
			#print x
			y = string[8:9]
			#print y
			w = string[10:11]
			#print w
			h = string[12:13]
			#print h
			block["type"] = "GROUND"
			block["x"] = int(x)
			block["y"] = int(y)
			block["width"] = int(w)
			block["height"] = int(h)
			block["logId"] = "g" + str(blockLogID)
			blockLogID+=1
			jsonObject["blocks"].append(block)

			
			
	
	with open('data.json', 'w') as outfile:
		json.dump(jsonObject, outfile)



	fh.close()