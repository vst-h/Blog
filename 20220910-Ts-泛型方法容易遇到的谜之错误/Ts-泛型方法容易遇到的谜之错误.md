# TypeScript - 泛型方法容易遇到的谜之错误
几个例子解释这个谜之错误的发生：
` xx is assignable to the constraint of type 'T', but 'T' could be instantiated with a different subtype of constraint '...'.(2322)`

---

## 样例1 - 函数内部使用一个具体的实现
``` ts
interface A1 {
  getData(code: number): string
}

class C1 implements A1 {
  getData(code: number): string {
    return "C1 Data " + code
  }
}

interface Data<T extends A1> {
  code: number
  name: string
  opt: T
}

function fn1<T extends A1>(name: string, opt?: T): Data<T> {
  if (opt === undefined) {
    return { code: 1, name, opt: new C1() }
    // return { code: 1, name, opt: new C1() as T }
  }

  return { code: 2, name, opt }
}
```
可以在 [TS Playground](https://www.typescriptlang.org/play?jsx=0#code/FASwdgLgpgTgZgQwMZQAQEECMqDexWoDmUEAIghAgBRID2AJlAFypgCuAtgEawCULAZwgxwhYAF9gwJABsEAgagDC2EBwAOMqByiRFWXPiIlylGg2atOPGP1RCRYQoYIEYJNjDCoARCtSmCL6oANSodIxGkpKgkLCIKAEUCAA8ACqoUAAe0GD0+pgAfC7hFizs3LBGYAg6gsKiRrTqECxpElJwbGBIECC03nBgmOmZObr5GEVUNXX2DU4ANKjNEAD8bXaB6cV4BCBwqFSrqAC856jdjHDgUPS8JW4eXriljCyYy7NQy6vlUAB3ZSYKgPSSuVAAekhqHcEE83hwb0sn1YtR+Kxa-yBKlBqHkqAy4NQMSe8JeSIilgATF90b8WiSJEA) 上查看错误，`fn1` 函数内 new 一个 满足 `T extends A1` 的 `C1` 对象， 想法是 `fn1` 函数内提供一个默认实现作为默认值给外部使用，或者通过参数 `opt?: T` 提供实现。
Ts 拒绝了这种行为，如果通过 as 断言，强行把 `C1` 断言为 `T` 可以绕过报错，但实际上还是会有错误的。

---

错误示例 [TS Playground](https://www.typescriptlang.org/play?jsx=0#code/FASwdgLgpgTgZgQwMZQAQEECMqDexWoDmUEAIghAgBRID2AJlAFypgCuAtgEawCULAZwgxwhYAF9gwJABsEAgagDC2EBwAOMqByiRFWXPiIlylGg2atOPGP1RCRYQoYIEYJNjDCoARCtSmCL6oANSodIxGkpKgkLCIKAEUCAA8ACqoUAAe0GD0+pgAfC7hFizs3LBGYAg6gsKiRrTqECxpElJwbGBIECC03nBgmOmZObr5GEVUNXX2DU4ANKjNEAD8bXaB6cV4BCBwqFSrqAC856jdjHDgUPS8JQQA9E+o7hCe3jiljCyYy7MoMtVuUoAB3ZSYKgPSSuN4eLy4H6Wf6sWpAlYtUEQlTQ1DyVAZWGoGJuBFfZEsABMAPRwJaJI60jkCmUVNQak02l0EAKJWIZGS5l+Vkqtnqjmce1c70+viU7MCPlCyKiUgIAoActZYMLLBUbHYHKJHvCPoifFgAAxW5VhCJQNUxLo9PoDVDQITHFobNkPaV0MBCVD0ZJnVBDKE+MFgnz0iC8ADcqBeqEAK-GAPbUkpQUgrioBvH0A0epGVOuAB6ayMocoADpVjWtTqYFRqwgaw6kyXXoBlfUxEDOFyuUBuYDuqEAb6aAGH-AFBygCN0wCb8YAZCMAx3KAQA9AGA6gGyjQBfioAgfUAHHodT0QPGp+eAKjlAL+KgAdTQAL8Yvx4BGHUAWPLbwCYqcAgA) ：
``` ts
function fn1<T extends A1>(name: string, opt?: T): Data<T> {
  if (opt === undefined) {
    // return { code: 1, name, opt: new C1() }
    return { code: 1, name, opt: new C1() as T }
  }

  return { code: 2, name, opt }
}

class C2 implements A1 {
  getData(code: number): string {
    return "C2 Data" + code
  }

  getNumber(code: number): string {
    return "A100" + code
  }
}

function test(opt?: C2) {
  const data = fn1("ww", opt); // 返回 Data<C2> 类型
  //    ^?
  data.opt.getNumber(data.code);
  // 当 opt === undefined 时，上面这行代码将抛出异常
}
test() // 这个函数运行时就会出错
```
错误的返回类型，导致一些错误的行为看起来是安全的，实际上应该报错的，问题就在 `fn1` 函数的实现。它不应该用 as
断言绕过编译错误。


## 样例2 - 泛型的函数参数不能使用默认值
``` ts
function fn1<T extends number | string>(a: T = 100): T {
  // 函数参数不能使用默认值
  return a
}
function fn2<T extends number | string = number>(a: T = 100): T {
  // 泛型参数加上默认值还是会报错
  return a
}
```
[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/FAMwrgdgxgLglgewgAhBAjAHgCrIKYAeMeEAJgM7IRgC2ARngE7IA+y5MjcEA5gHwAKAIYAuZLgC8ydAAYZASjG4A3sGTIA9BuSBfxUAOpoCHlPYFg5QL8Bgf3lAFK6ANvMAl0YB4FNckZ4YYRiiHAAvqEixEFDQAJhx8IhIKKloGZjYOLl5kKWp6JkFRcWTpOUUs1XUtZEBtm0Bo9SNAAqVAKDk7e0AN+MB6M0AseUBSo0BMVKcXNw9kL28gA) ，两个函数的参数 `(a: T = 100)` 都不能使用默认值。如果认真的看待，其实这个行为和 `样例1` 是同一个行为，都是为泛型对象赋值一个具体的实现。

---

那么来看看，这个例子的错误使用 [Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/FAMwrgdgxgLglgewgAhBAjAHgCrIKYAeMeEAJgM7IRgC2ARngE7IA+y5MjcEA5gHwAKAIYAuZLgC8ydAAYZyIZWwBKMbgDewZMgD0O5IF-FQA6mgIeUjgWDlAvwGB-eUAUroA28wCXRgHgUtyRnhhhGKIcAC+oJCwiChoAEw4+EQkFFS0DMxsHFy8yFLU9EyCouLp0nIKSqp5mtp6yIDbNoDR6maABUqAUHJOzoD6coD0ZuaAgoqAHdGAIW5uHl4+CgHAQdDwSMjEHABiGAIpYincPAA08TRimYnKyGXIUEgc7njk6Plo6IucexWAK-GAe2rsnKvIgN4+1YAw-4DG1oB2Hm4KtpkAA9AD8biOEBOHnI4UuC0yd30T02iQ+33+gP0wPBkOOMFO5AAzAjrsjkKiVmk2DsmF92v9el9PoBI7UAForMwBXym1PuZAPiagD6fAHlHHaPH46GE2EAFjJmGp-AEFKprzSny+T16gDztQCiaZ82mj6YBN+M+gEYdQCYqYAwuS+5g5VuxwNBEIGZxlADoYAgAKoAB19TAAwoo8MrdPo9QajYwvoAF+MAMhGAN9NAFjygFKjC2jcYhKYzGDzcI3RgAeUY3rIeBA3DwpDByzVPD2ByhMLOFykEQV9cEKueioxmseOv11UNdMYjtxLu0zelZ3h7YgBZSJbLpArVdIPZeqR4-a1kZH0Yn4oh-iAA) ：
``` ts
function fn1<T extends number | string>(a: T = 100 as T): T {
  // 函数参数不能使用默认值
  return a
}
function fn2<T extends number | string = number>(a: T = 100 as T): T {
  // 泛型参数加上默认值也是不允许的
  return a
}

function testFn1(str: string, num: number) {
  const res1 = fn1(str) // 返回 string 类型，正确
  //    ^?
  const res2 = fn1(num) // 返回 number 类型，正确
  //    ^?
  const res3 = fn1() // 返回 string | number，也是正确的，类型安全的，只是类型不够精确
  //    ^?

  const res4 = fn1<string>() // 返回 string 类型，返回的实际类型是 number，这类型就错了，不安全了
  //    ^?

  res4.toUpperCase() // 实际类型是 number，运行时会报错
}

function testFn2(strOrUndefined?: string) {
  const res1 = fn2<string>() // 返回 string 类型，返回的实际类型是 number
  //    ^?

  const res2 = fn2(strOrUndefined) // 返回 string 类型，返回的实际类型是 number
  //    ^?
}
```

## 样例3 - 泛型方法内部返回一个具体的类型

``` ts
function fn1<T extends number | string>(a: T): T {
  if (typeof a == "number" && a < 0) {
    return a + 100
    // 此处的 a 已经确定是 number 类型了，为什么不能把它当作 T 返回呢，
    // 新手可能直接用 as 断言做为 T 返回了，反正都是 number 类型，运行时也是 number 类型，不会有问题的
  }
  return a
}
```
[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/FAMwrgdgxgLglgewgAhBAjAHgCrIKYAeMeEAJgM7IRgC2ARngE7IA+y5MjcEA5gHwAKAIYAuZNgCUY3AG9gyZHBDIBMAJ4AHPAmVDkAXn3IARNXpNjyAGRXkezMnQAGCcjkKFjPDDCMUegGpHJyd5DwB6cORAE2tAEE1AELc7ZEAn3UB5v0A7D0As7UB6MypaBmZAbx9AaPVAMLlAGH-ALjlAAblACTlAWDlAX4DAKKNAYO1AZX1AHXlxZEAV+MA9tUAjFXKwhUjkQAbTQGkjQHvlRsAXt0BS40AKVztKQFrTQAAowC0FSp6BisBZ5UBja0BfhNyzAuQS8sAF+MAZCMA300B9OQv8pmvi8vrALHlASHNAHepgAyM+JhAC+YS8Pj8dmAYKAA) ，`a` 在 if 分支里面确实会是 `number` 类型，运行时也是。为什么还要报错，什么情况下还会出错，存在类型不安全吗。

---

看一下错误的使用，[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/FAMwrgdgxgLglgewgAhBAjAHgCrIKYAeMeEAJgM7IRgC2ARngE7IA+y5MjcEA5gHwAKAIYAuZNgCUY3AG9gyZHBDIBMAJ4AHPAmVDkAXn3IARNXpNjyAGRXkezMnQAGCcjkKFjPDDCMUegGpHJyc7SlwAegjkQHvlQFO5QC59QDYlQFrTQAAowC45cWRAbx9AaPVAGH-ATfjAejN8wFg5QEjtQAtFQBC3eWQAXwavHz87YGbQSFhEFGIOdGExdFZkACZXd2QoJA5kL0ojNCGhCQaoj2QAPQB+YAalFUWDI3QphoVNgDpb5EAtBUAAOUBsuUBouUBRgwbG-AAbcjxFMoBCdDBMLlsbncnm9Pgpmo0gA) ：
``` ts
function fn1<T extends number | string>(a: T): T {
  if (typeof a == "number" && a < 10) {
    return a + 100 as T // 可以强制断言为 T 类型，这是类型不安全的
  }
  return a
}

function test1(a: 1 | 2) {
  const res = fn1(a)
  //    ^?

  if (res == 1) {
    // ... 做一些事情
  } else if (res == 2) {
    // ... 做一些事情
  }
}
```
`res` 的类型根据 `a` 推导为 `1 | 2`，`res` 的实际值可能是 `101 | 102`，调用者在使用 `fn1` 方法时完全相信了该方法的返回类型，做了一些分支处理。但是上面的代码会不符合预期的，因为它没有预想的走进任何分支，这就是类型不安全的原因。`number` 是有其他的子类型的，`1 | 2 | 3 | 4 | 5 | 6` 是 `number` 的子类型，`1 | 2` 也是 `number` 的子类型。代码 `a + 100` 得到得是 `number` 类型，如果此时 `T` 实际传入的泛型是 `1 | 2`，那么 `a + 100` 的 `number` 类型是不可以赋值给 `1 | 2` 的。

## 样例 1 和样例 2 的解决方法
样例 1 和样例 2 都可以通过函数重载来达到目的，根据不同的参数返回不同的类型。

样例1，[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/JYOwLgpgTgZghgYwgAgIIEZkG8BQzkDmEYAInGHABQID2AJhAFzIgCuAtgEbQCUzAzmCigCOAL44cCADZx+-ZAGFMwdgAdpEdhHAKM2PIWJkK1ekxYduUPskHCQBA-nxRirKCGQAiZchNwPsgA1Mi0DIYSEjigkLCIKAEAPAAqyBAAHpAgdHroAHzOYebMbFzQhiBw2gJCIoY0amDMKeKSMKwgCGDANF4wIOiUVTV2dY62ycr5OB1dPX3IA+ip6Vk6uWgFw9UW9iIANMiNzcgpk+RwqTNz3b39g6uZ2ZsY+Tuj+45HJwD8LRcKNcisAYMhKCdkABeGHIToMGCgCB0HhFVzuTzYYoMZjoI4jCA-JqlCAAdyUQ1RcjOyAk+Gi6LAHi8WGxFgATPjdkSwLTxEA)
``` ts
interface A1 {
  getData(code: number): string
}

class C1 implements A1 {
  getData(code: number): string {
    return "C1 Data " + code
  }
}

interface Data<T extends A1> {
  code: number
  name: string
  opt: T
}

function fn1(name: string): Data<C1>
function fn1<T extends A1>(name: string, opt: T): Data<T>
function fn1<T extends A1>(name: string, opt?: T): Data<T> {
  if (opt === undefined) {
    return { code: 1, name, opt: new C1() as T }
  }

  return { code: 2, name, opt }
}
```

样例2，[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/GYVwdgxgLglg9mABMMBGAFASgFyLCAWwCMBTAJwChRJYFk0AeAFURIA8oSwATAZz0KkyiAD6JeUMjDABzAHzoAhriY5ETKuGjwkKVM1YcufAcXKjxk6fKUrEAXkSoADM8SL+quwG8KiRGQkUCBkSIoUAL5AA)
```ts
function fn1(): number
function fn1<T extends number | string>(a: T): T
function fn1<T extends number | string>(a: T = 100 as T): T {
  return a
}
```

---

样例 3 确实没找到什么解决办法。这三个例子都想在函数内部用实例化（具体）的类型，来赋值给不确定的泛型值，这是一种类型不安全的行为，并且参杂了运行时的类型判断。由于泛型 T 可能还有其他的子类型，就算运行时判定了 T 已经没有子类型了，也不能把这个具体的实例做为 T。

[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/GYVwdgxgLglg9mABMMBGAPAFUQUwB5Q5gAmAzomCALYBGOATogD6KlT0xgDmAfABQBDAFyJMAShHYA3gChEiGMER8oATwAOOOEoGIAvHsQAiSrQZGxiWfPmLlug4lSXrN+fRxQQ9JALk2AX1wAG1IcBSVBfUMAJhd-Nw8vH0QYhMQA-0z3T29fGUyZUEhYBGQwGKxcAiIyJ35hUQlRK38kvKcCmSA) ：
```ts
function fn1<T extends number | string>(a: T): T {
  if (typeof a == "number") {
    if (a == 1) {
      return a
    } else if (a == 2) {
      return 2
    }
  }
  return a
}

function fn2<T extends 1>(a: T): T {
  return 1
}
```

`fn2` 的 `T extends 1` 泛型 `T` 实际上有两个子类型 `1 | never`。`never` 是底层类型 (bottom type)，是所有类型的子类。

[Ts Playground](https://www.typescriptlang.org/play?jsx=0#code/FAMwrgdgxgLglgewgAhBATAHgCrIKYAeMeEAJgM7ICMAfABQCGAXMtgJQu4DewyyATnhhh+KKsgaVswAL7BQkWIhRoAcngBuefnQ7IIm7ch58YAC34IA7sgBE2-rdnzw0eEmTFyMXcd7IoJG9kAmQAXlQMOjVDHTY2ZAB6RORATfjAejNAbx9AaPVASO1AC0VAELdAGH-QwDsPQDztdP1Y4vTAWDlAe+VABiVAPvjC-2S+PgA9AH5ZIA) ：
```ts
function fn2<T extends 1>(a: T): T {
  return 1 as T
}

function fnNever(): never {
  throw "err"
}

function test() {
  const x = fn2(fnNever()) // 这是类型安全的，x 确实是 never，是不可到达的
  //    ^?
}
```
