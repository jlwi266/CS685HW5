# Resources:
# https://www.geeksforgeeks.org/read-json-file-using-python/
# https://www.askpython.com/python/examples/tf-idf-model-from-scratch

import json
import os
import copy

# Importing required module
import numpy as np

# Opening JSON file
f = open('../Outputs/index.json')

# returns JSON object as
# a dictionary
data = json.load(f)

# Closing file
f.close()

total_documents = len(data)
word_count = {}

print('Building term matrix...')

for doc in data:
    for docTerm in data[doc]:
        if not docTerm in word_count:
            word_count[docTerm] = 1
        else:
            word_count[docTerm] += 1

print('Term matrix ready!')

def GetTFIDF():
    result = copy.deepcopy(data)

    for doc in result:
        for docTerm in result[doc]:
            idf = np.log10(total_documents/word_count[docTerm])
            result[doc][docTerm] = data[doc][docTerm] * idf

    return result

def ProcessQuery(query):

    result = {}

    for queryTerm in query: 
        if not queryTerm in word_count:
            word_count[queryTerm] = 1
        else:
            word_count[queryTerm] += 1

    for term in query:
        idf = np.log10(total_documents/word_count[term])
        result[term] = query.count(term) * idf

    return result