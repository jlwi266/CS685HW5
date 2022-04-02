import math
import json
import operator

def get_scores(scores, query):
    if not (scores == {}) and not (query == {}):
        docFinalScores = {}
        docValues = {}
        queryValue = 0.0

        f = open('../Outputs/pageRank.json')
        pageRank = json.load(f)

        #get document contributions to the equation
        for url in scores:
            termScores = 0.0
            normalizer = 0.0
            for term in query:
                if term in scores[url] and not (float(scores[url][term]) == 0.0):
                    termScores = termScores + float(scores[url][term])
                    normalizer += math.pow(float(scores[url][term]), 2)
            if not (termScores == 0.0):
                normalizer = math.sqrt(normalizer)
                docValue = termScores/normalizer
                docValues[url] = docValue

                #get query contributions to the equation
                queryTermScores = 0.0
                queryNormalizer = 0.0
                for queryTerm in query:
                    queryTermScores += float(query[queryTerm])
                    queryNormalizer += math.pow(float(query[queryTerm]), 2)
                queryNormalizer = math.sqrt(queryNormalizer)
                queryValue = queryTermScores/queryNormalizer

                nextRelevanceScore = queryValue * docValues[url]
                finalScore = (nextRelevanceScore * 0.6) + (pageRank[url] * 100 * 0.4)
                docFinalScores[url] = finalScore

            else:
                docFinalScores[url] = pageRank[url]

        sortedScores = sorted(docFinalScores.items(), key=lambda x:x[1], reverse=True)
    else:
        sortedScores = {}

    return sortedScores