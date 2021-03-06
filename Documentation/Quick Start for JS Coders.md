# Minet
## Quick Start for JavaScript Coders
This guide is meant for coders familiar with JavaScript. If you don't see something mentioned here
it is likely because its usage is the same as in JavaScript.

### Table of Contents
1. [Basics](#basics)
2. [Indentation](#indentation)
3. [Classes](#classes)
4. [Variables](#variables)
5. [Getters and Setters](#getters-and-setters)
6. [Enumerations](#enumerations)
7. [Assignment](#assignment)
8. [Chains](#chains)
9. [If](#if)
10. [Comparisons](#comparisons)
11. [Loops](#loops)

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
In Minet a number of JavaScript keywords can be (optionally) shortened. You can use either the original
JavaScript keywords or the shortened Minet keywords interchangeably.
- **function** is **fn**
- **return** is **ret**
- **delete** is **del**
- **finally** is **fin**
- **continue** is **cont**
- **instanceof** is **instof**
- **undefined** is **undef**
- **Infinity** is **inf**

Assignment is the only way to create functions.
```
mult: fn(x, y)
    ret x * y
```
For consistency with the rest of Minet's assignment operators, object creation follows the pattern
of **{property names : values}**
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
; The values are indented.
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

MyProject              ; Adds a static variable to class MyProject. The order in which these
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
A constructor is defined by creating an instance function with the same name as the class.
```
MyClass
    .xVal

    .MyClass: fn(x)
        xVal: x
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
; Both MyClass and SecondClass will contain an x and y variable, defaulted to zero.
MyClass, SecondClass
    .x, .y: 0, 0
```
In Minet, an instance of an object is constructed through **name{args}**, equivalent to the JavaScript **new name(args);**
```
obj: MyClass{1, 2}
```
The static **Main** function is special. If one is found in your project, a **window.onload** block
will be added to the end of the generated output:
```javascript
// Run Main
window.onload = function() {
    Project.Main();
};
```
The second special static function is **Init**, which is automatically called at the end of all class creation, but before
the **Main** function is run.

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
The **use** statement is used to provide Minet a list of extra allowed variable names.
This is primarily used to allow the use of existing JavaScript functions, such as **alert**.
```
use alert, console

Proj
    Print: fn()
        alert('hello')            ; Without the use statement, both of these statements
        console.log('hello')      ; would return errors for use of undefined variables.
```
The **use** statement can also be used to define extra allowed variable expansions.
This allows you to define shorthand variable or function names that are expanded behind the scenes.
The format is **name for expansion**.
```
use document
    getById for document.getElementById

Proj
    Main: fn()
        var myElem: getById("someElementId")
```
Please note that expansion and variable checking is done recursively, so without the first line of
**use document** any uses of **getById** will fail with an error that **document** is not defined.

### Getters and Setters
Getters and setters can be defined on class variables, both static and instance, and will be created
behind the scenes with **Object.defineProperty**.

**Get** and **set** can be declared in either order, and only one is required to create a getter and/or
setter for a property. **Get** is used alone on an indented line, **set** is followed by the name of
the variable passed into the **set**.
```
MyClass
    _Val: 1
    Val
        get
            ret _Val
        set v
            _Val: v
```

### Enumerations
Minet allows for easy enumeration creation, creating multiple static variables behind the scenes.
```
Project.MyEnum
    enum
        First, Second
        Third, Fourth, Fifth
```
Enumeration values start at **0** by default. You can specify a starting value after the **enum** keyword if
necessary.
```
SomeEnum
    enum 100
        Hundred, HundredAndOne
```
You can also specify a step using the **by** keyword. Both the starting value and step are optional, and can
be used together or independent of each other.
```
SomeEnum
    enum 100 by 10
        Hundred, HundredTen
        HundredTwenty
```

### Assignment
Minet allows many-to-many assignment in the form **variables : values**. All values are evaluated left-to-right
into temporary variables, and then all are assigned.
```
a, b: 1, 2    ; Assigns 1 to a and 2 to b.
x, y: y, x    ; Since all values are evaluated before assignment, this successfully swaps x and y.
```
Many-to-one assignment is also accepted. Please note that the code generated evaluates the right-hand side once
per variable. If only a single evaluation is necessary, the result should first be assigned to a temporary
variable.
```
a, b, c: calculate()    ; calculate() is called once for each of a, b and c.

var _t: calculate()     ; If you only want calculate() called once, store it in a temporary variable first.
a, b, c: _t
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
var __t = array;
first = __t[0];
second = __t[1];
third = __t[2];
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
Minet also has its own version of the JavaScript conditional operator **?:** which is spelled out as
an **if then else** expression.
```
val: if x < 3 then "small" else "large"
```
Generates:
```javascript
val = (x < 3 ? "small" : "large");
```

### Comparisons
Minet also supports multiple comparisons on a single line like Python. For example, these two are equivalent:
```
if x < y and y < z and z < 10
    alert('in order and all < 10')

if x < y < z < 10
    alert('in order and all < 10')
```
The **<, <=, >, >=** operators all have higher precedence than **=** and **!=**, so the following code:
```
if x < y = y < z
```
Will generate:
```javascript
if ((x < y) === (y < z))
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
Minet's other looping construct is the **while** loop, which takes a condition. Minet does not
have a direct analog to the **do/while** loop, but it can be easily emulated with a **while true**
and **break** statements.

The **break** statement works the same as in JavaScript, and is used to break a loop.
```
while true
    x: doSomeLogic()
    if x > 7
        break
```
Both **for** and **while** can be preceded by an identifier that serves as the loop's label. When
using **break**, you can optionally include the label of the loop you want to break out of if it
is not the innermost containing loop.
```
myLoop while x < 7
    inner while true
        x: doSomething()
        if x < 0
            break myLoop
```