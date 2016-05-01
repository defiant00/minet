# Minet
## Quick Start for JavaScript Coders
This guide is meant for coders familiar with JavaScript. If you don't see something mentioned here
it is likely because its usage is the same as in JavaScript.

### Table of Contents
1. [Basics](#basics)
2. [Indentation](#indentation)
3. [Classes](#classes)
4. [Variables](#variables)
5. [Assignment](#assignment)
6. [Chains](#chains)
7. [If](#if)
8. [Loops](#loops)

### Basics
Semicolons do not end lines; they do, however, start a single-line comment.
Assignment in Minet is done with **:** instead of **=**
```
x: 3      ; And now x is 3.
```
Multi-line comments start with **<;** and end with **;>**
Multi-line comments can be nested, so the below code is all a comment:
```
<;
  In Minet <; and ;> are used to start and end
  a multi-line comment.
;>
```
The **=** operator is equivalent to JavaScript's **===** operator.
There is currently no equivalent to JavaScript's **==** operator.

The words **and** and **or** are used instead of **&&** and **||** respectively.

In Minet both **function** and **return** have been shortened to **fn** and **ret** respectively.
Also, assignment is the only way to create functions.
```
	mult: fn(x, y)
		ret x * y
```
For consistency with the rest of Minet's assignment operators, object creation follows the pattern
of {property names : values}
```
	var obj: {firstVal, secondVal: 1, 2}
```
Since Minet compiles to JavaScript, you may find times where you want to be able to directly inject
JavaScript. You can do so using the **<js** and **js>** start and end tags.
```
<js
alert('Hello from JavaScript!');
js>
```

### Indentation
; spaces or tabs
; can be used for multiple var or use
; how to multi-line {} () []

### Classes
; nested, partial, multiple names for shared code, constructors
; . is inst, not is static
; Main

### Variables
; names are checked and expanded
; use use to add extra allowed (js functions, libraries, etc)

### Assignment
; many-many: a, b: 3, 'hello'
; many-one: a, b: 3
; many-many: a, b +: 1, 2
; many-one: a, b +: 1
; all evaluated first, so can be used to swap
; unpack
; +:, -:, *:, /:, %:

### Chains

## If
; normal, no arg, partial

### Loops
; for, loop
