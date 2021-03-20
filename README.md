Provides extension methods to split strings on Word boundaries, according to the [Unicode Standard Annex #29 rules](http://www.unicode.org/reports/tr29/). Grapheme Cluster support should come soon.

# Features

## Word iteration

```csharp
var input = "The quick (“brown”) fox can’t jump 32.3 feet, right?";
var result = new List<string>();
foreach (var word in input.EnumerateWords())
    result.Add(word.ToString());
// This code iterates over words in the specified string and produces:
// "The", "quick", "brown", "fox", "can’t", "jump", "32.3", "feet", "right"
```

## Word boundary iteration

```csharp
var input = "The quick (“brown”) fox can’t jump 32.3 feet, right?";
var result = new List<string>();
foreach (var word in input.EnumerateWordBoundaries())
    result.Add(word.ToString());
// This code iterates over words in the specified string and produces:
// "The", " ", "quick", " ", "(", "“", "brown", "”", ")", " ", "fox", " ",
// "can’t", " ", "jump", " ", "32.3", " ", "feet", ",", " ", "right", "?"

```