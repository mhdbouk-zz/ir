# Information Retrieval Search Engine
### Pilot Search Engine for CSCI512 Information Retrieval

## Phase 1: Stoplist
Adjectives, adverbs, and connectives are less
useful because they work mainly as complements. These irrelevant terms (stopwords) are placed un a file called Stoplist (`"assets/StopLost.txt"`).

- Removal of the words found in a stoplist
- Removal of punctuations and symbols
- Generate text documents without stopwords `.stp`

## Phase 2: Stemming
- Removal of suffixes
- Generate text documents without suffixes `.sfx`
- Using porter algorithm