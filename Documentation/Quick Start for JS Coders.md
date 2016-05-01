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
x: 3
; And now x is 3.
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


In Minet a number of JavaScript keywords have been shortened.
- **function** is **fn**
- **return** is **ret**
- **delete** is **del**

Assignment is the only way to create functions.
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
Minet lexes input following the off-side rule.
Both spaces and tabs are supported; during lexing, tabs are treated as four spaces.
Tabs are recommended, but this is not enforced by the compiler.

Most statements and expressions must be on a single line.
Those contained in **()**, **{}** or **[]** can be spread across multiple lines, if formatted properly.
```
; The opening bracket ends the line.
; The values are indented over.
; The closing bracket is on the next line and dedented.
array: [
    1, 2, 3,
    4, 5, 6,
    7, 8, 9
]
```
Another goal of Minet is to avoid keyword repetition. To that end, both the **use** and **var** statements
can be continued over multiple lines through indentation.
```
use one, two
    three, four
    five

var x: 0
    y, z: 1, 2
```

### Classes
The only valid statements at the root of a document are **use**, JavaScript blocks, and class names.
Nested classes are created by dotting the names together.
```
MyProject.MyClass      ; Creates a class MyProject and a class MyClass within MyProject.
    Greet: fn()
        alert('hi')

MyProject              ; Adds a static property to class MyProject. The order in which these
	SomeProp: 3        ; are defined, or even which files these are defined in, doesn't matter.
```
Within a class, properties that start with a **.** are instance properties, and those that do not are static.
```
MyClass
    StaticCounter: 0
    .InstanceCounter: 0

    StaticGreet: fn()
        alert('hi')

    .GreetInstance: fn()
        alert('hi')
```
Classes can be defined in multiple places, and during code generation all are combined into a single class.
```
; This is the same:
MyClass
    .first, .second: 1, 2

; As this:
MyClass
    .first: 1

MyClass
    .second: 2
```
You are not limited to adding code to a single class at a time, multiple comma-delimited class names can be provided at once.
All of the classes will contain the shared code.
```
; Both MyClass and SecondClass will contain an x and y property, defaulted to zero.
MyClass, SecondClass
    .x, .y: 0, 0
```
In Minet, an instance of an object is constructed through **name{args}**, equivalent to the JavaScript **new name(args);**
```
obj: MyClass{1, 2}
```
The static **Main** method is special. If one is found in your project, a **window.onload** block
will be added to the end of the generated output:
```javascript
// Run Main
window.onload = function() {
    Project.Main();
};
```

### Variables
In Minet variables must be declared before use, and are available as appropriate at the current or any child scopes.
During code generation variables and their relationships are tracked, so that variables can be referenced
by just the last part of their name, and behind the scenes they will be expanded to their fully-qualified name.
```
MyClass
    Static: 1
    .Inst: 2

    .Func: fn()
        Inst: 3        ; This is recognized and replaced with this.Inst
        Static: 4      ; This is replaced with MyClass.Static
```
Static variables are also available to a class' subclasses.
```
MyClass
    Static: 1

MyClass.Sub
    .Func: fn()
        Static: 2      ; This is replaced with MyClass.Static
```
The **use** statement is used to provide Minet a list of extra allowed variable names.
This is primarily used to allow the use of existing JavaScript functions, such as **alert**.
```
use alert, console

Proj
    Print: fn()
        alert('hello')            ; Without the use statement, both of these statements
        console.log('hello')      ; would return errors for use of undefined variables.
```
If **this** is used within an anonymous function, it will automatically be replaced with **_this** so
that **this** in event handlers works properly.

### Assignment
; many-many: a, b: 3, 'hello'

; many-one: a, b: 3

; many-many: a, b +: 1, 2

; many-one: a, b +: 1

; all evaluated first, so can be used to swap

; unpack

; +:, -:, *:, /:, %:

### Chains
You can chain identifiers together through indentation. All indented parts will generate code as if they were
preceded by the prior identifier and a dot.
```
this
    x, y
        counter: 1
```
Will produce:
```javascript
this.x.counter = 1;
this.y.counter = 1;
```

### If
; normal, no arg, partial

### Loops
; for, loop
