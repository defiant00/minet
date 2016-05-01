### Minet to JS Guide
```javascript
Minet                                           JavaScript

v: myArr[3]                                     v = myArr[3];

fn(a, b)                                        function(a, b) {
    alert('hi')                                     alert('hi');
                                                }

a: b                                            a = b;

a, b: 3, 'hello'                                var _t0 = 3;
                                                var _t1 = 'hello';
                                                a = _t0;
                                                b = _t1;

a, b: b, a                                      var _t0 = b;
                                                var _t1 = a;
                                                a = _t0;
                                                b = _t1;

arr: [1, 2, 3]                                  arr = [1, 2, 3];

first, second :: getValues()                    var _t = getValues();
                                                first = _t[0];
                                                second = _t[1];

myObj.subObject                                 myObj.subObject.first = 1;
    first: 1                                    myObj.subObject.second = 2;
    second: 2
    

break                                           break;
break myLoop                                    break myLoop;

MyClass                                         var MyClass = (function () {
                                                    function MyClass() {
                                                    }
                                                    return MyClass;
                                                })();

MyClass{}                                       new MyClass();
MyClass{3}                                      new MyClass(3);
MyClass{                                        new MyClass(1, 2, 3, 4);
    1, 2,
    3, 4
}

{}                                              {}
{first, last: 'first', 'last'}                  {first: 'first', last: 'last'}

for i in 0 to 10                                for (var i = 0; i < 10; i++)
for i in 10 to 0                                for (var i = (10) - 1; i >= 0; i--)
for i in 0 to 10 by 2                           for (var i = 0; i < 10; i += 2) {
myLoop for i in 0 to 10                         myLoop: for (var i = 0; i < 10; i++) {

for i in myItems                                for (var _i0 = 0; _i0 < myItems.length; _i0++) {
                                                    var i = myItems[_i0];

myObj.doThing(1, 2, 3)                          myObj.doThing(1, 2, 3);

MyFunc: fn(a, b)                                MyFunc = function(a, b) {
    alert('hi')                                     alert('hi');
                                                };

if x < 3                                        if (x < 3) {

if x < 3                                        if (x < 3) {
    alert('< 3')                                    alert('< 3');
else                                            } else {
    alert('>= 3')                                   alert('>= 3');
                                                }

if                                              if (x < 3) {
    x < 3                                           alert("< 3");
        alert("< 3")                            } else if (x > 7) {
    x > 7                                           alert("> 7");
        alert("> 7")                            } else {
    else                                            alert(">= 3");
        alert(">= 3")                           }

if x                                            if (x === 2 || x === 3) {
    = 2, 3                                          alert("2 or 3");
        alert("2 or 3")                         } else if (x === 4) {
    = 4                                             alert("4");
        alert("4")                              } else if (x > 8) {
    > 8                                             alert("> 8");
        alert("> 8")                            } else {
    else                                            alert("something else");
        alert("something else")                 }

loop                                            while(true) {
myLoop loop                                     myLoop: while(true) {

ret 3                                           return 3;

var x, y: 1, 2                                  var x = 1, y = 2;
    a, b: "hello", "world"                      var a = "hello", b = "world"

x: 3                                            x = 3;
x +: 3                                          x += 3;

x, y -: 1, 2                                    var _t0 = x - 1;
                                                var _t1 = y - 2;
                                                x = _t0;
                                                y = _t1;

x, y -: 1                                       var _t = 1;
                                                x -= t;
                                                y -= t;

x: true or false                                x = true || false;
y: x and true                                   y = x && true;
```
