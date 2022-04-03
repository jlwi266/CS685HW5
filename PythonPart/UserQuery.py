from argparse import _CountAction

def CheckForStopWords(input):
    lines = ' '
    with open('stopwords.txt') as f:
        while lines != '':
            lines = f.readline()
            size = len(lines)
            for term in input:
                if term == lines[:size - 1]:
                    input.remove(term)


def GetUserInput():
        val = input("Enter your query: ")
        print("Searching for " + val)
        queryInput = val.upper().split(' ')
        CheckForStopWords(queryInput)
        return queryInput


