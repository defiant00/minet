# Minet Language Overview
### General
Minet is a web scripting language with a focus on readability and conciseness without resorting to a a large number of operators and special characters. Minet is compiled to Javascript.

The core tenets guiding the syntax are as follows:
* There should be a single, clear way to do a task.
* An expression should read as a sentence.
* Keywords are preferred over operators if they are of similar length.
* Reducing typing time is important.
* Things that can be inferred by the compiler should be, unless it hampers readability.
* Special characters to help the compiler are a waste of typing time.
* Any place where you have to repeat a leading keyword should also allow an indented block.
* When switching a single statement to an indented block, you shouldn't have to rewrite the command itself.

### Indentation
Minet lexes input following the off-side rule. Any increase in indentation generates an *IDENT* token, and a decrease generates a *DEDENT* as long as the new indentation lines up with a previous indentation level.

Both spaces and tabs are supported; during lexing, tabs are treated as four spaces. Tabs are recommended, but this is not enforced by the compiler.

### Overview
```
<; Welcome to Minet, this is a multi-line comment.
   Comments can be nested, so telling you that <; and ;> begin and end a multi-line
   comment does not break parsing. ;>

; This is a single-line comment.
; The only valid top-level statement in Minet is a class name.
; Class names can be chained together, resulting in classes containing other classes.
; In this example, MyClass is a child class of MyProject, which is also a class.
MyProject.MyClass
    Counter: 0                         ; Instance variables and functions start with a "." and static
                                        ; variables and functions do not. This is a static counter.

    .InstCounter: 0                    ; This is an instance variable.

    Main: fn()                          ; A project should contain a single static Main function
        alert("Hello from Minet!")      ; which will be used to run the project.
```

### Syntax Guide
```javascript
Minet                                           Javascript

v: myArr[3]                                     v = myArr[3];

fn(a, b)                                        function(a, b);

a: b                                            a = b;

a, b: 3, 'hello'                                var __t0 = 3;
                                                var __t1 = 'hello';
                                                a = __t0;
                                                b = __t1;

a, b: b, a                                      var __t0 = b;
                                                var __t1 = a;
                                                a = __t0;
                                                b = __t1;

arr: [1, 2, 3]                                  arr = [1, 2, 3];

break                                           break;
break myLoop                                    break myLoop;

MyClass                                         var MyClass = (function () {
                                                    function MyClass() {
                                                    }
                                                    return MyClass;
                                                })();

MyClass{}                                       new MyClass()
MyClass{3}                                      new MyClass(3)
MyClass{                                        new MyClass(1, 2, 3, 4)
    1, 2,
    3, 4
}

for i in 0 to 10 by 2                           for (var i = 0; i < 10; i += 2) {
myLoop for i in 0 to 10                         myLoop: for (var i = 0; i < 10; i++) {

for i in myItems                                for (var __i0 = 0; __i0 < myItems.length; __i0++) {
                                                    var i = myItems[__i0];

myObj.doThing(1, 2, 3)                          myObj.doThing(1, 2, 3);

MyFunc: fn(a, b)                                MyFunc = function(a, b) {
    alert('hi')                                     alert('hi');
                                                }

if x < 3                                        if (x < 3) {

if x < 3                                        if (x < 3) {
    alert('< 3')                                    alert('< 3');
else                                            } else {
    alert('>= 3')                                   alert('>= 3');
                                                }

if                                              if (x < 3) {
    x < 3                                        alert("< 3");
        alert("< 3")                            } else if (x > 7) {
    x > 7                                           alert("> 7");
        alert("> 7")                            } else {
    else                                            alert(">= 3");
        alert(">= 3")                           }

if x                                            if (x === 2 || x === 3) {
    = 2, 3                                          alert("2 or 3");
        alert("2 or 3")                         } else if (x === 4) {
    = 4                                             alert("4");
        alert("4")                              }

loop                                            while(true) {
myLoop loop                                     myLoop: while(true) {

ret 3                                           return 3;

var x, y: 1, 2                                  var x = 1, y = 2;
    a, b: "hello", "world"                      var a = "hello", b = "world"

x: 3                                            x = 3;
x +: 3                                          x += 3;

x, y -: 1, 2                                    var __t0 = x - 1;
                                                var __t1 = y - 2;
                                                x = __t0;
                                                y = __t1;

x, y -: 1                                       var __t = 1;
                                                x -= t;
                                                y -= t;

x: true or false                                x = true || false;
y: x and true                                   y = x && true;
```