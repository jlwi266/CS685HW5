import FinalScore
import TFIDFQuery

def CheckForStopWords(input):
    lines = ' '
    with open('stopwords.txt') as f:
        while lines != '':
            lines = f.readline()
            size = len(lines)
            for term in input:
                if term == lines[:size - 1]:
                    input.remove(term)


def UserInputLoop(rankings):
    while 1 == 1:
        val = input("Enter your query: ")
        queryInput = val.upper().split(' ')
        CheckForStopWords(queryInput)
        print("Searching for " + val)
        print("Loading...")
        finalScores = FinalScore.get_scores(rankings, TFIDFQuery.ProcessQuery(queryInput))
        counter = 1
        if(finalScores == {}):
            print("No matches were found for the query.")
        else:
            for score in finalScores:
                index = finalScores.index(score)
                print(str(counter) + ". " + str(finalScores[index]))
                counter += 1


