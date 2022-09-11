
namespace example1 {
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
}

namespace example2 {
  function fn1<T extends number | string>(a: T = 100): T {
    // 函数参数不能使用默认值
    return a
  }
  function fn2<T extends number | string = number>(a: T = 100): T {
    // 泛型参数加上默认值还是会报错
    return a
  }
}

namespace example3 {
  function fn1<T extends number | string>(a: T): T {
    if (typeof a == "number" && a < 0) {
      return a + 100
      // 此处的 a 已经确定是 number 类型了，为什么不能把它当作 T 返回呢，
      // 新手可能直接用 as 断言做为 T 返回了，反正都是 number 类型，运行时也是 number 类型，不会有问题的
    }
    return a
  }
}
