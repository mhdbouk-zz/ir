# Information Retrieval Search Engine
### Pilot Search Engine for CSCI512 Information Retrieval using the MED test collection

## Phase 1: Stoplist
Adjectives, adverbs, and connectives are less
useful because they work mainly as complements. These irrelevant terms (stopwords) are placed under a file called Stoplist (`"assets/StopList.txt"`).

- Removal of the words found in a stoplist
- Removal of punctuations and symbols
- Generate text documents without stopwords `.stp`

## Phase 2: Stemming
The remaining terms are stemmed using Porter's algorithm, it brings down distinct words to their common grammatical root - Remove suffix. Stemming will reduces further the number of unique terms.
- Removal of suffixes
- Generate text documents without suffixes `.sfx`
- Using porter algorithm (https://github.com/nemec/porter2-stemmer)

## Phase 3: Weighting, Generating of Inverted File
Generate 2 CSV files that display all terms found in all documents (after phase 1 & 2) using boolean inverted file and TFIDF inverted file.

## Phase 4: Query processing and Reporting
User has the option to add a query or to run the MED queries collection into the system. System then will generate new document for the result with the COS values for each term in document, the recall and precesion for each document + new csv file will be generated contains the average precision per unique recall values.
