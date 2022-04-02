import UserQuery
import TFIDF

# term frequency
if __name__ == "__main__":
    # Generate TFIDF scores for given Index
    vectors = []
    TFIDFRankings = {}
    vectors.append(TFIDF.tf_idf())
    TFIDFRankings = TFIDF.formatOutput(vectors)

    UserQuery.UserInputLoop(TFIDFRankings)









