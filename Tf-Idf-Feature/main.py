# Resources:
# https://www.geeksforgeeks.org/read-json-file-using-python/
# https://www.askpython.com/python/examples/tf-idf-model-from-scratch

import json

# Importing required module
import numpy as np
from nltk.tokenize import word_tokenize

import nltk
import ssl
#
# try:
#     _create_unverified_https_context = ssl._create_unverified_context
# except AttributeError:
#     pass
# else:
#     ssl._create_default_https_context = _create_unverified_https_context
#
# nltk.download()

# Python program to read
# json file

# Opening JSON file
f = open('test_index.json')

# returns JSON object as
# a dictionary
data = json.load(f)

# Iterating through the json
# list

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

print("test")
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
        word_occurance = word_count[word] + 1
    except:
        word_occurance = 1

    #     Deal with equal number of occurences and documents
    if total_documents + 1 == word_occurance:
        return 1
    else:
        return np.log(total_documents / word_occurance)

# Now you combine the TFIDF Functions
def tf_idf():
    tf_idf_vec = np.zeros((len(word_set),))
    for url in data:
        for term in data[url]:
            idf = inverse_doc_freq(term)

            if idf == 1:
                tf_idf_vec[index_dict[term]] = 1
            else:
                value = data[url].get(term) * idf
                tf_idf_vec[index_dict[term]] = value
    print(index_dict)
    print(tf_idf_vec)
    return tf_idf_vec


# term frequency
if __name__ == "__main__":
    vectors = []
    vectors.append(tf_idf())
    print(vectors[0])


# I left off with the ability to load the indexing now I am trying to create a word count for the document
# this will help me in my reverse indexing
# I am basically following the python tutorial but I am reformatting the input to work for our indexing


