""" 
    __author__ =  Anam Navied
    __email__ = anavied@ncsu.edu
    This makes and maps a POP for level 5 to the input file
    This script reads through the csv file outputted by selectUser.java (also provided)
    to identify what each line of code maps to which subgoal and prints that value to the column

    how to run:
    python level5popscript.py INPUTFILE.csv OUTPUTFILE.csv
"""
import re
import collections
import sys
import csv

#current subgoal that the player is at
currentStep = 0
#flag for the third subgoal, first step
firstCondStep3Met = False
#flag for the third subgoal, second step
secondCondStep3Met = False
#flag for the third subgoal, third step
thirdCondStep3Met = False
#flag for the fourth subgoal, first step
firstCondStep4Met = False
#flag for the fourth subgoal, second step
secondCondStep4Met = False
#flag for the fourth subgoal, third step
thirdCondStep4Met = False
#flag for the fifth subgoal, first step
firstCondStep5Met = False
#flag for the fifth subgoal, second step
secondCondStep5Met = False
#flag for the fifth subgoal, third step
thirdCondStep5Met = False
step2AlreadyMet = False
deathStatus = ""

#opens the file containing all the player/level data and retrieves the string containing the action message
def getSubgoalColumn():
    global currentStep, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, firstCondStep4Met, secondCondStep4Met, thirdCondStep4Met, firstCondStep5Met, secondCondStep5Met, thirdCondStep5Met, step2AlreadyMet
    if len(sys.argv) != 3:
        sys.exit()
    filepath = sys.argv[1]
    csvinput = open(filepath)
    r = csv.reader(csvinput)
    outputFile = open(sys.argv[2], "w+")
    all = []

    currentRow = next(r)
    #open file, and escape each column's value of , and " and \
    while currentRow:
        goalValue = identifySubGoal(currentRow[3])
        ctr = 0
        for item in currentRow:
            toEscapeJSON = str(currentRow[ctr])
            toEscapeJSON = toEscapeJSON.replace("\"", "\"\"")
            escapedJSON = "\"" + toEscapeJSON + "\""
            currentRow[ctr] = escapedJSON
            ctr += 1
        currentRow.append(str(goalValue))
        currentRow.append(deathStatus)
        strToReport = ','.join(currentRow) + "\n"
        outputFile.write(strToReport)
        currentRow = next(r)

def identifySubGoal(lineToEval):

    global currentStep, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, step2AlreadyMet, deathStatus, firstCondStep4Met, secondCondStep4Met, thirdCondStep4Met, firstCondStep5Met, secondCondStep5Met, thirdCondStep5Met
    deathStatus = ""
    #cond A: match #0 RESET version
    #if(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Level was reset', lineToEval)):
    if(re.search(r'Level was reset', lineToEval)):
        currentStep = 0
        firstCondStep2Met = False
        secondCondStep2Met = False
        thirdCondStep2Met = False
        firstCondStep3Met = False
        secondCondStep3Met = False
        thirdCondStep3Met = False
        firstCondStep4Met = False
        secondCondStep4Met = False
        thirdCondStep4Met = False
        firstCondStep5Met = False
        secondCondStep5Met = False
        thirdCondStep5Met = False
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
        firstCondStep4Met = False
        secondCondStep4Met = False
        thirdCondStep4Met = False
        firstCondStep5Met = False
        secondCondStep5Met = False
        thirdCondStep5Met = False
        step2AlreadyMet = False
        deathStatus = "D"
        return currentStep
    #cond C: match #1
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Platform is added from link headLink at (9.341855, -10.82084)', lineToEval)):
    elif(re.search(r'Platform is added from link headLink at \((\W*)(\d*)(\W)(\d+)(\W)(\W)(\W*)(\d*)(\W)(\d+)\)', lineToEval)):
        currentStep = 1
        return currentStep

    #cond D: match #2
    #elif(re.search(r'Player landed onto platform: sllp0', lineToEval)):
    elif(re.search(r'Player landed onto platform: sllp0', lineToEval)):
        currentStep = 2
        return currentStep
    #cond E: match #3, sub step #1
    #elif (re.search(r'The player has selected the link: sllp0Link', lineToEval)):
    elif (re.search(r'The player has selected the link: sllp0Link', lineToEval)):
        firstCondStep3Met = True
        return currentStep

    #cond F: match #1, sub step #2
    #elif (re.search(r'The user has hovered over link block l0', lineToEval)):
    elif (re.search(r'The user has hovered over link block l0', lineToEval)):
        if(firstCondStep3Met == True):
            secondCondStep3Met = True
        return currentStep #return anyways

    #cond E: match 1, sub step #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Connection made: sllp0Link was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: sllp0Link was clicked and dragged to null', lineToEval)):
        if(secondCondStep3Met == True):
            thirdCondStep3Met = True
            currentStep = 3
        return currentStep #return anyways

    ##########################STEP TWO STARTS
    #cond F: match 2, sub step #1
    #elif(re.search(r'The player has selected the link: helicopterRobotLink', lineToEval)):
    elif(re.search(r'The player has selected the link: helicopterRobotLink', lineToEval)):
        firstCondStep4Met = True
        return currentStep
    #cond G: match #2, sub step #2
    #elif (re.search(r'The user has hovered over link block sllp2Link', lineToEval) and not currentStep==3):
    elif (re.search(r'The user has hovered over link block sllp2Link', lineToEval)):
        if(firstCondStep4Met == True):
            secondCondStep4Met = True
        return currentStep #return anyways
    #cond H match 2, sub step #3
    #elif(re.search(r'Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval)):
        if(secondCondStep4Met == True):
            thirdCondStep4Met = True
            currentStep = 4
        return currentStep

    elif(re.search(r'Platform is added from link sllp3Link at \((\W*)(\d*)(\W)(\d+)(\W)(\W)(\W*)(\d*)(\W)(\d+)\)', lineToEval)):
        currentStep = 5
        return currentStep
    elif(re.search(r'Player landed onto platform: sllp1', lineToEval)):
        currentStep = 6
        return currentStep
    elif(re.search(r'Level 5 won at time', lineToEval)):
        currentStep = 7
        return currentStep
    #cond K: other line
    else:
        return currentStep



def main():
    getSubgoalColumn()
main()