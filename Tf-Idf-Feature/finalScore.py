import math
import json
import operator

def get_scores(scores, query):
    docFinalScores = {}
    docValues = {}
    queryValue = 0.0

    f = open('../Outputs/pageRank.json')
    pageRank = json.load(f)

    #get document contributions to the equation
    for url in scores:
        termScores = 0.0
        normalizer = 0.0
        for term in query.keys():
            if term in scores[url].keys():
                termScores += scores[url][term]
                normalizer += math.pow(scores[url][term], 2)
            if termScores:
                normalizer = math.sqrt(normalizer)
                docValue = termScores/normalizer
                docValues[url] = docValue

    #get query contributions to the equation
    queryTermScores = 0.0
    queryNormalizer = 0.0
    for queryTerm in query.keys():
        queryTermScores += query[queryTerm]
        queryNormalizer += math.pow(query[queryTerm], 2)
    queryNormalizer = math.sqrt(queryNormalizer)
    queryValue = queryTermScores/queryNormalizer

    #compute final scores
    for nextUrl in docValues.keys():
        nextRelevanceScore = queryValue * docValues[nextUrl]
        finalScore = (nextRelevanceScore * 0.6) + (pageRank[nextUrl] * 100 * 0.4)
        docFinalScores[nextUrl] = finalScore

    sortedScores = sorted(docFinalScores, key=operator.itemgetter(1), reverse=True)

    return sortedScores
                

        

