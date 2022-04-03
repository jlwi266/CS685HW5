import UserQuery
import TFIDF
import FinalScore

# term frequency
if __name__ == "__main__":
    # Generate TFIDF scores for given Index
    rankings = {}
    print("Building TFIDF scores...")
    rankings = TFIDF.GetTFIDF()
    print("Search is ready!")
    
    while 1 == 1:
        query = UserQuery.GetUserInput()
        print("Loading...")
        finalScores = FinalScore.get_scores(rankings, TFIDF.ProcessQuery(query))
        counter = 1
        if(finalScores == {}):
            print("No matches were found for the query.")
        else:
            outputResults = True
            counter = 0
            while outputResults and counter < len(finalScores):
                print(str(counter) + ". " + str(finalScores[counter]))
                counter += 1
                if (counter % 10 == 0):
                    continueOutput = input("Get more results? (y/n): ")
                    if (continueOutput == "n"):
                        outputResults = False








