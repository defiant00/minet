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

Regular expressions are started with **//** instead of a single **/**, but are otherwise declared
and used the same as in JavaScript. This is to prevent ambiguity with the **/** operator.
```
var r: //hello/i
```
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
Minet allows many-to-many assignment in the form **variables : values**. All values are evaluated left-to-right
into temporary variables, and then all are assigned.
```
a, b: 1, 2    ; Assigns 1 to a and 2 to b.
x, y: y, x    ; Since all values are evaluated before assignment, this successfully swaps x and y.
```
Many-to-one assignment is also accepted, with the value calculated once and then assigned to all variables.
```
a, b, c: calculate()    ; calculate() is called once and then the value is assigned to a, b and c.
```
Both many-to-many and many-to-one operations are also supported through the shorthand
operators **+:, -:, *:, /:** and **%:**
These operators correspond to the JavaScript operators **+=, -=, *=, /=** and **%=**

The unpack operator **::** takes one or more variables and a single array, and unpacks the items of the array
into the provided variables.
```
first, second, third :: array
```
Generates:
```javascript
var _t = array;
first = _t[0];
second = _t[1];
third = _t[2];
```
The unpack operator along with array creation using **[]** is an easy way in Minet to return multiple values.
```
MyClass
    GetPersonInfo: fn()
        ret ["first", "last", 30]

firstName, lastName, age :: MyClass.GetPersonInfo()
```

### Chains
You can chain identifiers together through indentation. All indented parts will generate code as if they were
preceded by the prior identifier and a dot.
```
this
    x, y
        counter: 1
    calculate()
    print()
```
Will produce:
```javascript
this.x.counter = 1;
this.y.counter = 1;
this.calculate();
this.print();
```

### If
If statements have multiple formats, and serve as Minet's equivalent to both the JavaScript **if**
and **switch** statements.

A normal **if** has a condition and an optional **else**. There is no **else if** as that is covered by the next form.
```
if x > 3
    alert('> 3')
else
    alert('not > 3')
```
A no-condition if is an **if** on its own with the conditions (or **else**) indented on their own lines.
This will automatically generate an **else if** for the appropriate conditions beyond the first.
```
if
    x < 3
        alert('< 3')
    x > 7
        alert('> 7')
    else
        alert('something else')
```
A partial if is an **if** containing an expression, but the comparison operator and value(s) are on their own
indented lines. This is Minet's equivalent to JavaScript's **switch** statement, but with the added flexibility
of allowing any comparision operator and multiple potential comma-separated values.

The expression in a partial if is evaluated once and stored in a temporary variable.
```
if x
    = 1, 2
        alert('1 or 2')
    > 5
        alert('> 5')
    else
        alert('something else')
```

### Loops
For loops in Minet have two main forms. The first is numeric, in which you supply the starting and ending
values and, optionally, the step. The format is:
- **for** i **in** start **to** end
- **for** i **in** start **to** end **by** step

Loops are always inclusive to the smaller number and exclusive to the larger, regardless of whether
it is an increasing or decreasing loop.

A loop will default to a step of 1 or -1 if none is specified. The scenarios in which Minet is able
to determine that -1 should be the step are when:
- Both start and end are literal numbers, and start > end.
- End is 0.
```
for i in 0 to 10
    alert('hi 1')
for i in 10 to 0
    alert('hi 2')
for i in 0 to 10 by 2
    alert('hi 3')
```
Produces:
```javascript
for (var i = 0; i < 10; i++) {
    alert('hi 1');
}
for (var i = (10) - 1; i >= 0; i--) {
    alert('hi 2');
}
for (var i = 0; i < 10; i += 2) {
    alert('hi 3');
}
```
The other for loop form in Minet is **for** i **in** object with an optional **by** step.
This uses **object.length** to determine the iterations, creating a temporary counter which is
used to then assign **object[counter]** to **i**.
```
for i in myItems
    alert('hi ' + i)
```
Produces:
```javascript
for (var _i0 = 0; _i0 < myItems.length; _i0++) {
    var i = myItems[_i0];
    alert('hi ' + i);
}
```
Minet also has a more general looping construct, named **loop**, that is a substitute for
JavaScript's **while** and **do/while** loops. There are no arguments, it is merely specified
by the keyword **loop** with the statements to be looped indented starting on the next line.

The **break** statement works the same as in JavaScript, and is used to break a **loop**.
```
loop
    x: doSomeLogic()
    if x > 7
        break
```
Both **for** and **loop** can be preceded by an identifier that serves as the loop's label. When
using **break**, you can optionally include the label of the loop you want to break out of if it
is not the innermost containing loop.
```
myLoop loop
    inner loop
        x: doSomething()
        if x < 0
            break myLoop
```
