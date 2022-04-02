
def CheckForStopWords(input):
    lines = ' '
    with open('stopwords.txt') as f:
        while lines != '':
            lines = f.readline()
            size = len(lines)
            for term in input:
                if term == lines[:size - 1]:
                    input.remove(term)


def UserInputLoop():
    while 1 == 1:
        val = input("Enter your query: ")
        queryInput = val.split(' ')
        CheckForStopWords(queryInput)
        print("Searching for " + val)
        print("Loading...")
