# Literal rule

Literals are simple strings.

For instance, the follow grammar:

```Python
rule main = "bob";
```

will match **one and only one** sample text:  `bob`.

Literal is of course a fundamental primitive and is typically combined with other primitives.

# Single & double quotes

Strings can be embedded in either single or double quotes.  Therefore the following rule:

```Python
rule main = "bob";
```

is equivalent to this one:

```Python
rule main = 'bob';
```

## Escape characters

Literal allow for escaping characters:

|Escape sequence|Represents|
|---|---|
|\n|New Line|
|\r|Carriage Return|
|\t|Horizontal tab|
|\v|Vertical tab|
|\\\\ |Backslash|
|\\'|Single quote|
|\\"|Double quote|
|\xhh (e.g. \xa2, \x9B)|ASCII character in heradecimal notation|

## Default Output

A litteral outputs the litteral it matches.  For instance, the following rule:

```Python
rule main = "bob";
```

will match `bob` and output `"bob"`.


---
[Go back to online documentation](../README.md)