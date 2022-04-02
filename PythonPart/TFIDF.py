# Resources:
# https://www.geeksforgeeks.org/read-json-file-using-python/
# https://www.askpython.com/python/examples/tf-idf-model-from-scratch

import json
import os

# Importing required module
import numpy as np

# Opening JSON file
f = open('test_index.json')

# returns JSON object as
# a dictionary
data = json.load(f)

# Closing file
f.close()

# Preprocessing the index data
sentences = []
word_set = []

for url in data:
    for term in data[url]:
        if term not in word_set:
            word_set.append(term)

# Set of vocab
word_set = set(word_set)
# Total documents in our corpus
total_documents = len(data)

# Create an index for each word in our vocab
index_dict = {} #Dictionary for storing index for each word
i = 0
for word in word_set:
    index_dict[word] = i
    i += 1

# Create a count dictionary
def count_dict():
    word_count = {}
    for word in word_set:
        word_count[word] = 0
        for url in data:
            for term in data[url]:
                if term == word:
                    word_count[term] += 1
    return word_count

word_count = count_dict()

# Define a function to calculate Term Frequency
# This is already defined in the indexed data structure

# Define a function to caluculate IDF
def inverse_doc_freq(word):
    try:
        word_occurance = word_count[word]
    except:
        word_occurance = 1

    #     Deal with equal number of occurences and documents
    # TODO: Deal with more word occurances than documents in a set
    # if total_documents + 1 == word_occurance:
    #     return 1
    # else:
    return np.log10(total_documents / word_occurance)

# Now you combine the TFIDF Functions
def tf_idf():
    tf_idf_vec = np.zeros((len(word_set),))
    for url in data:
        for term in data[url]:
            idf = inverse_doc_freq(term)

            if idf == 1:
                tf_idf_vec[index_dict[term]] = 1
            else:
                value = data[url][term] * idf
                tf_idf_vec[index_dict[term]] = value
    return tf_idf_vec

def formatOutput(vectors):
    TFIDFOutput = {}
    # This puts all the data outputted in a digestable dictionary relationship
    #  {URL1:{Term1:TFIDF, Term2: TFIDF, Term3:TFIDF},URL2:{Term1:TFIDF, Term2: TFIDF}}
    for url in data:
        tmpTermScore = {}
        for indx, word in enumerate(index_dict):
            for term in data[url]:
                if term == word:
                    tmpTermScore[word] = str(vectors[0][indx])
            if tmpTermScore:
                TFIDFOutput[url] = tmpTermScore
    return TFIDFOutput