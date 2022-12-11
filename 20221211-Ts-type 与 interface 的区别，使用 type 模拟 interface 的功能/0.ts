namespace prop {
  interface IData {
    a: number
    readonly b?: string
    fn1(): string
    fn2: () => string
  }
  type TData = {
    a: number
    readonly b?: string
    fn1(): string
    fn2: () => string
  }
}

namespace fnOverload {
  interface IFn {
    (): number | string
    (a: false): number
    (a: true): string
    readonly fnProp: boolean
  }
  type TFn =
    & (() => number | string)
    & ((a: false) => number)
    & ((a: true) => string)
    & { readonly fnProp: boolean }

  function test(tFn: TFn, iFn: IFn) {
    const t1 = tFn()
    //    ^?
    const t2 = tFn(false)
    //    ^?
    const t3 = tFn(true)
    //    ^?
    const t4 = tFn.fnProp
    //    ^?

    const i1 = iFn()
    //    ^?
    const i2 = iFn(false)
    //    ^?
    const i3 = iFn(true)
    //    ^?
    const i4 = iFn.fnProp
    //    ^?
  }
}

namespace inherit1 {
  interface IA {
    a: number
    fnA(): number
  }
  interface IB {
    fnB(): number
  }
  interface IC extends IA, IB {
    fnC(): number
  }

  type TA = {
    a: number
    fnA(): number
  }
  type TB = {
    fnB(): number
  }
  type TC = {
    fnC(): number
  } & TA & TB

  function test(ic: IC, tc: TC) {
    const res1 = ic.a + ic.fnB()
    const res2 = tc.a + tc.fnB()
  }
}

namespace inherit2 {
  interface IA {
    a: number
    b: string
    fnA(): number
    fnB(): string
  }

  interface IA1 extends IA {
    a: 1 | 2 // 类型安全的
    b: string | number // 不符合类型安全的
  }

  interface IA2 extends IA {
    fnA(): 1 | 2// 类型安全的
    fnB(): number | string// 不符合类型安全的
  }
}

namespace inherit3 {
  type TA = {
    a: number
    b: string
  }

  type TA1 = TA & {
    a: 1 | 2
    b: number // 不会报错
  }


  function test(ta1: TA1) {
    const res1 = ta1.a
    //    ^?
    const res2 = ta1.b
    //    ^?
  }
}

namespace inherit4 {
  interface IA {
    fn1(a: number): string
  }
  interface IA1 extends IA {
    fn1(a: boolean): boolean // 类型冲突
  }

  type TA1 = {
    fnA(a: 1 | 2): 10
    fnB(): number
  }

  type TA2 = {
    fnA(a: 3 | 4): 20
    fnB(): string
  }

  type TB1 = TA1 & TA2
  type TB2 = TA2 & TA1

  function test(tb1: TB1, tb2: TB2) {
    // 根据参数判断重载返回不同的类型
    const a1 = tb1.fnA(1)
    //    ^?
    const a2 = tb2.fnA(3)
    //    ^?

    // 无法判断是哪个重载，选择第一个的函数签名的返回类型
    const b1 = tb1.fnB()
    //    ^?
    const b2 = tb2.fnB()
    //    ^?
  }
}

namespace abstractClass {
  type TA = {
    a: number
    fn1(): number
  }
  type TA1 = Omit<TA, "fn1">

  const partialImpl1: TA1 = {
    a: 1
  }

  const impl1: TA = {
    ...partialImpl1
    // 实现剩下的部分成员
  }

  const impl2: TA = {
    ...partialImpl1,
    fn1: () => 1,
    // 当 TA 增加一个成员 fn2 时，TA1 不会报错，
    // 这里就会报错，达到了类型检查，约束 TA 的正确实现
  }

  function test1(ta: TA) {
    ta.a
    ta.fn1()
  }

}

namespace diff_alias {
  // 根据 obj1 的实际对象获取它的 key 类型
  type Obj1Key = keyof typeof obj1
  //   ^?
  const obj1 = { a: 1, b: 2, c: 3 }


  // 根据 tup1 的值获取字面量类型
  type MyKey1 = typeof tup1[number]
  //   ^?
  const tup1 = ["GET", "POST", "DELETE"] as const


  interface IA2 {
    fn1(a: number, b: string, c?: Date): number
  }
  type Fn1Args = Parameters<IA2["fn1"]>
  //   ^?
}

namespace diff_literal {

  type Num1 = 10 | 20 | 30

  type Action1 = "add" | "sub"

  type Value1 = string | false | undefined
}

namespace diff_union {
  interface Animal { animal: string }
  interface Dog extends Animal { dog: string }
  interface Cat extends Animal { cat: string }
  interface Duck extends Animal { duck: string }
  // 以上类型是第三方包提供的，我们已经不能修改

  /** 我的宠物 */
  type MyPet = Dog | Cat
  // 需要约束我的宠物只有 Dog 和 Cat，这个方法只处理这两种类型
  // interface 目前不能达到这种约束，只能约束为 Animal，
  // 或者在运行时抛出类型不支持
  function test(myPet: MyPet) {
    if ("dog" in myPet) {
      myPet.dog = "myDog"
      // ^?
    } else {
      myPet.cat = "myCat"
      // ^?
    }
  }
}
namespace diff_type_operation {
  // 根据 T 的不同类型，返回不同的类型
  type TA<T> = T extends true ? number : string
  type TA1 = TA<true> // number
  //   ^?
  type TA2 = TA<false> // string
  //   ^?

  // 把 TObj 所有属性转换成函数
  type TB<TObj> = {
    [K in keyof TObj]: () => TObj[K]
  }
  type TB1 = TB<{ a: number, b: string }>
  //   ^?

}

namespace index {
  interface IA {
    a: string
    b: number
    // c: Date // 报错，声明的属性必须满足索引类型的约束
    [key: string]: string | number
    // [num: number]: number
  }

  interface IB extends IA {
    d: Date // 报错
  }

  type TA = {
    a: string
    [key: string]: string
  } & {
    // 可以 & 其他属性类型
    b: number
    c: Date
  } & {
      // 这个也是允许的
      [Key in "fn1" | "fn2"]: () => number
    }


  function test(ta: TA) {
    const c = ta.c
    //    ^?
    const fn1 = ta["fn1"]
    //    ^?
    const other = ta["other"]
    //    ^?
  }
}

namespace index2 {
  // 本包声明的一个 interface
  export interface IA { [key: string]: string }
  export function fnA(ia: IA, key: keyof IA) {
    const value = ia[key]
    //    ^?
    value.toUpperCase() // 对 string 进行 toUpperCase
  }
  // 以上是本包声明到处的代码


  // 以下是第三方包想为 IA 添加声明合并，添加属性
  export interface IA {
    d: Date // 报错是对的，类型不安全
  }
}
namespace index3 {
  type StrRecord = { [k: string]: string }
  function fn1<T extends StrRecord>(obj: T) { }

  interface IA {
    a: string
    b: string
    // IA 的所有属性都是 string 类型，看起来满足了 StrRecord 的约束
    // [k: string]: string
  }

  function test(ia: IA) {
    fn1(ia) // 报错
    // Argument of type 'IA' is not assignable to parameter of type 'StrRecord'.
    // Index signature for type 'string' is missing in type 'IA'.
  }
}