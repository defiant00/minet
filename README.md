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
; The only valid top-level statements in Minet are class names, use statements and javascript blocks

; This tells the compiler that alert is a valid identifier so it doesn't complain about an undeclared variable.
use alert

<js
// This is a Javascript block, any code entered here will be output as-is to the output file.
// Javascript blocks can be used at most any part of a Minet program.
alert("Hello from Javascript");
js>

; Class names can be chained together, resulting in classes containing other classes.
; In this example, MyClass is a child class of MyProject, which is also a class.
MyProject.MyClass
    Counter: 0                          ; Instance variables and functions start with a "." and static
                                        ; variables and functions do not. This is a static counter.

    .InstCounter: 0                     ; This is an instance variable.

    Main: fn()                          ; A project should contain a single static Main function
        alert("Hello from Minet!")      ; which will be used to run the project.
```

