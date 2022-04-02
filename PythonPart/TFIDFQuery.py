import numpy as np
import json

def ProcessQuery(query):
    
    f = open('test_index.json')
    data = json.load(f)
    docCount = len(data)+1
    termCounts = {}
    result = {}

    for doc in data:
        for docTerm in data[doc]:
            if not docTerm in termCounts:
                termCounts[docTerm] = 1
            else:
                termCounts[docTerm] += 1

    for queryTerm in query: 
        if not queryTerm in termCounts:
                termCounts[queryTerm] = 1
        else:
            termCounts[queryTerm] += 1


    for term in query:
        idf = np.log10(docCount/termCounts[term])
        result[term] = query.count(term) * idf

    return result