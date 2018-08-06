""" 
    __author__ =  Anam Navied
    __email__ = anavied@ncsu.edu
    This makes and maps a POP for level 6 to the input file
    This script reads through the csv file outputted by selectUser.java (also provided)
    to identify what each line of code maps to which subgoal and prints that value to the column

    how to run:
    python level6popscript.py INPUTFILE.csv OUTPUTFILE.csv
"""
import re
import collections
import sys
import csv
#current subgoal the player is at
currentStep = 0
# the following are flags for the condition X of subgoal Y where XconditionStepYMet is var name
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
#how the player died
deathStatus = ""

#opens the file containing all the player/level data and retrieves the string containing the action message
def getSubgoalColumn():
    global firstCondStep2Met, currentStep, secondCondStep2Met, thirdCondStep2Met, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, firstCondStep4Met, secondCondStep4Met, thirdCondStep4Met, firstCondStep5Met, secondCondStep5Met, thirdCondStep5Met, step2AlreadyMet
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

    global firstCondStep2Met, currentStep, secondCondStep2Met, thirdCondStep2Met, firstCondStep3Met, secondCondStep3Met, thirdCondStep3Met, step2AlreadyMet, deathStatus, firstCondStep4Met, secondCondStep4Met, thirdCondStep4Met, firstCondStep5Met, secondCondStep5Met, thirdCondStep5Met
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
    ########### STEP ONE ###############
    #cond C: match #1, sub step #1
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The player has selected the link: helicopterRobotLink', lineToEval)):
    elif (re.search(r'The player has selected the link: helicopterRobotLink', lineToEval) and currentStep==0):
        firstCondStep2Met = True
        return currentStep

    #cond D: match #1, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block l0', lineToEval)):
    elif (re.search(r'The user has hovered over link block sllp0Link', lineToEval) and currentStep==0):
        if(firstCondStep2Met == True):
            secondCondStep2Met = True
        return currentStep #return anyways

    #cond E: match 1, sub step #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval) and (currentStep==0)):
        if(secondCondStep2Met == True):
            thirdCondStep2Met = True
            currentStep = 1
            step2AlreadyMet = True
        return currentStep #return anyways
    ##########################STEP TWO STARTS
    #cond F: match 2, sub step #1
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The player has selected the link: l0', lineToEval)):
    elif(re.search(r'The player has selected the link: l0', lineToEval) and currentStep==1):
        firstCondStep3Met = True
        return currentStep
    #cond G: match #2, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block sllp1Link', lineToEval)):
    #elif (re.search(r'The user has hovered over link block sllp1Link', lineToEval) and not currentStep==3):
    elif (re.search(r'The user has hovered over link block sllp1Link', lineToEval) and currentStep==1):
        if(firstCondStep3Met == True):
            secondCondStep3Met = True
        return currentStep #return anyways
    #cond H match 2, sub step #3
    #elif(re.search(r'Connection made: l0 was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: l0 was clicked and dragged to null', lineToEval) and currentStep==1):
        if(secondCondStep3Met == True):
            thirdCondStep3Met = True
            currentStep = 2
        return currentStep
    #############STEP THREE STARTS
    #cond I: match #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Platform is added from link sllp1Link at (18.17908, -13.52166)', lineToEval)):
    elif(re.search(r'Platform is added from link sllp1Link at \((\W*)(\d*)(\W)(\d+)(\W)(\W)(\W*)(\d*)(\W)(\d+)\)', lineToEval)):
        currentStep = 3
        return currentStep
    #STEP FOUR STARTS
    ###################################
    elif (re.search(r'The player has selected the link: helicopterRobotLink', lineToEval) and currentStep==3):
        firstCondStep4Met = True
        return currentStep

    #cond E: match #1, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block l0', lineToEval)):
    elif (re.search(r'The user has hovered over link block sllp1Link', lineToEval) and currentStep==3):
        if(firstCondStep4Met == True):
            secondCondStep4Met = True
        return currentStep #return anyways

    #cond F: match 1, sub step #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval) and currentStep==3):
        print(currentStep)
        if(secondCondStep4Met == True):
            thirdCondStep4Met = True
            currentStep = 4
        return currentStep #return anyways
    #####################################
    
    
    
    #####################################
    # STEP FIVE NOW
    elif (re.search(r'The player has selected the link: sllp3Link', lineToEval) and currentStep==4):
        firstCondStep5Met = True
        return currentStep

    #cond E: match #1, sub step #2
    #elif (re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)The user has hovered over link block l0', lineToEval)):
    elif (re.search(r'The user has hovered over link block l0', lineToEval) and currentStep==4):
        if(firstCondStep5Met == True):
            secondCondStep5Met = True
        return currentStep #return anyways

    #cond F: match 1, sub step #3
    #elif(re.search(r'(\d+)(\W+)(\d+)(\W+)(\w+)(\W+)Connection made: helicopterRobotLink was clicked and dragged to null', lineToEval)):
    elif(re.search(r'Connection made: sllp3Link was clicked and dragged to null', lineToEval) and currentStep==4):
        if(secondCondStep5Met == True):
            thirdCondStep5Met = True
            currentStep = 5
        return currentStep #return anyways
    #################################
    #STEP 6 NOW
    elif(re.search(r'Player landed onto platform: sllp3', lineToEval) and currentStep==5):
        currentStep = 6
        return currentStep
    #################################
    #STEP 7 NOW
    elif(re.search(r'Player landed onto platform: sllp2', lineToEval)):
        currentStep = 7
        return currentStep
    #################################
    #STEP 8 NOW
    elif(re.search(r'Level 6 won at time', lineToEval)):
        currentStep = 8
        return currentStep
    #################################
    #cond K: other line
    else:
        return currentStep


def main():
    getSubgoalColumn()
main()