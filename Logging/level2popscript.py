""" 
    __author__ =  Anam Navied
    __email__ = anavied@ncsu.edu
    This makes and maps a POP for level 2 to the input file
    This script reads through the csv file outputted by selectUser.java (also provided)
    to identify what each line of code maps to which subgoal and prints that value to the column

    how to run:
    python level2popscript.py INPUTFILE.csv OUTPUTFILE.csv
"""
import re, collections, sys, csv

#current subgoal that the player is at
currentStep = 0
#flag for the second subgoal, first step
firstCondStep2Met = False
#flag for second subgoal, second step
secondCondStep2Met = False
#flag for second subgoal, third step
thirdCondStep2Met = False
#flag for third subgoal, first step
firstCondStep3Met = False
#flag for third subgoal, second step
secondCondStep3Met = False
#flag for third subgoal, third step
thirdCondStep3Met = False
#flag for whether step 2 has met been met when step 1 is reached as it is possible to skip step 2 and directly go to step 1 
step2AlreadyMet = False
#string that is either "R" for reset or "D" for death
deathStatus = ""

#opens the file containing all the player/level data and retrieves the string containing the action message
def getSubgoalColumn():
    global firstCondStep2Met, currentStep, secondCondStep2Met, thirdCondStep2Met, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, step2AlreadyMet
    if len(sys.argv) != 3:
        sys.exit()
    filepath = sys.argv[1]
    csvinput = open(filepath)
    r = csv.reader(csvinput)
    outputFile = open(sys.argv[2], "w+")
    all = []
    currentRow = next(r)
    while currentRow:
        subgoalValue = identifySubGoal(currentRow[3])        
        ctr = 0
        for item in currentRow:
            #escape backslash \ within the JSON string
            toEscapeJSON = str(currentRow[ctr])
            toEscapeJSON = toEscapeJSON.replace("\"", "\"\"")
            #then add double quotes to escape any commas within your JSON string, so it doesn't get split in the CSV
            escapedJSON = "\"" + toEscapeJSON + "\""
            #then write this new string into the appropriate column
            currentRow[ctr] = escapedJSON
            ctr += 1
        #appends to the newly made column for the Subgoal (R or D to show how you died)
        currentRow.append(str(subgoalValue))
        #appends to the newly made column for the Death Status (R or D to show how you died)
        currentRow.append(deathStatus)
        strToWrite = ','.join(currentRow) + "\n"
        outputFile.write(strToWrite)
        currentRow = next(r)

# identifies subgoal based on the action message string passed in
# lineToEval is passed in from getSubgoalColumn()
# commented out code before each subgoal is how you'd use regex if you were looking at the whole line at a time, not just the action message column
def identifySubGoal(lineToEval):
    global firstCondStep2Met, currentStep, secondCondStep2Met, thirdCondStep2Met, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, step2AlreadyMet, deathStatus
    deathStatus = ""
    #cond A: match #0 RESET version
    #use this if you have each line from the csv file instead of just the actionMessage column field
    #if(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Level was reset', lineToEval)):
    if(re.search(r'Level was reset', lineToEval)):
        currentStep = 0
        firstCondStep2Met = False
        secondCondStep2Met = False
        thirdCondStep2Met = False
        firstCondStep3Met = False
        secondCondStep3Met = False
        thirdCondStep3Met = False
        step2AlreadyMet = False
        deathStatus = "R"
        return currentStep
    #cond B: match #0, player DIED version
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Player fell off and died, level was reset', lineToEval)):
    elif(re.search(r'Player fell off and died, level was reset', lineToEval)):
        currentStep = 0
        firstCondStep2Met = False
        secondCondStep2Met = False
        thirdCondStep2Met = False
        firstCondStep3Met = False
        secondCondStep3Met = False
        thirdCondStep3Met = False
        step2AlreadyMet = False
        deathStatus = "D"
        return currentStep
    #cond C: match #1
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Player landed onto ground: g8', lineToEval)):
    elif (re.search(r'Player landed onto ground: g8', lineToEval)):
        if (step2AlreadyMet == False):
            currentStep = 1
            firstCondStep2Met = True
        return currentStep
    #cond D: match #2, sub step #1
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The player has selected the link: headLink', lineToEval)):
    elif (re.search(r'The player has selected the link: headLink', lineToEval)):
        #if (firstCondStep2Met == True):
        #    secondCondStep2Met = True
        firstCondStep2Met = True
        return currentStep
    #cond E: match #2, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block l0', lineToEval)):
    elif (re.search(r'The user has hovered over link block l0', lineToEval)):
        if(firstCondStep2Met == True):
            secondCondStep2Met = True
        #print("Cond E met/done ", currentStep)
        return currentStep #return anyways
    #cond F: match 2, sub step #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Connection made: headLink was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: headLink was clicked and dragged to null', lineToEval)):
        if(secondCondStep2Met == True):
            thirdCondStep2Met = True
            currentStep = 2
            step2AlreadyMet = True
        return currentStep #return anyways
    #cond G: match 3, sub step #1
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The player has selected the link: sllp1Link', lineToEval)):
    elif(re.search(r'The player has selected the link: sllp1Link', lineToEval)):
        firstCondStep3Met = True
        return currentStep
    #cond H: match #3, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block l1', lineToEval)):
    elif (re.search(r'The user has hovered over link block l1', lineToEval)):
        if(firstCondStep3Met == True):
            secondCondStep3Met = True
        return currentStep #return anyways
    #cond I match 3, sub step #3
    #elif(re.search(r'Connection made: sllp1Link was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: sllp1Link was clicked and dragged to null', lineToEval)):
        if(secondCondStep3Met == True):
            thirdCondStep3Met = True
            currentStep = 3
        return currentStep
    #cond J: match #4
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Player landed onto ground: g4', lineToEval)):
    elif(re.search(r'Level 2 won at time', lineToEval)):
        currentStep = 4
        return currentStep
    #cond K: other line
    else:
        return currentStep


def main():  
    getSubgoalColumn()
main()