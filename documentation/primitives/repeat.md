# Repeat rule

Repeat is a composite rule allowing the repetition of a given rule.

For instance, the follow grammar:

```Python
rule main = "a"*;
```

will match zero or many times the letter *a*.

## Cardinality

There are many ways to express the cardinality of a repeat rule:

|Format|Cardinality|Grammar example|
|---|---|---|
|*|Zero to infinity|"a"*|
|+|One to infinity|"a"+|
|?|Zero or one|"a"?|
|{*n*}|Exact *n* times|"a"{5}|
|{*min*, *max*}|At least *min*, at most *max* ; either *min* or *max* can be omitted ; omitting *min* means from zero to *max* while omitting *max* means at least *min* |"a"{2,5} ; "a" {,3} ; "a"{2,}|

## Default Output

Repeat rule outputs the array of outputs of the repeated sub rule.  For instance, the following rule:

```Python
rule main = "a"*;
```

The following texts would be match and generate the following outputs:

Text|Output
-|-
""|`[]`
"a"|`["a"]`
"aa"|`["a", "a"]`
"aaa"|`["a", "a", "a"]`

---
[Go back to online documentation](../README.md)