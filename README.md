# Information Retrieval Search Engine
### Pilot Search Engine for CSCI512 Information Retrieval

## Phase 1: Stoplist
Adjectives, adverbs, and connectives are less
useful because they work mainly as complements. These irrelevant terms (stopwords) are placed under a file called Stoplist (`"assets/StopLost.txt"`).

- Removal of the words found in a stoplist
- Removal of punctuations and symbols
- Generate text documents without stopwords `.stp`

## Phase 2: Stemming
The remaining terms are stemmed using Porter's algorithm, it brings down distinct words to their common grammatical root - Remove suffix. Stemming will reduces further the number of unique terms.
- Removal of suffixes
- Generate text documents without suffixes `.sfx`
- Using porter algorithm (https://github.com/nemec/porter2-stemmer)

## Phase 3: Boolean Inverted File
Generating csv file that display all terms found in all documents (after phase 1 & 2) with 1 / 0 if the term exist in document or not
