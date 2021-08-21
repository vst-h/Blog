using System;
using System.Collections.Generic;

Console.WriteLine(">>");
Test1(null, null);
Test2(null, null);

static void Test1(CB<IA2<string>> bString, IA2<string> a2) {
    bString.A2Fn<IA2<string>, string>(a2);
    bString.A2Fn(a2);



}

static void Test2(CB<IA2Int> bInt, IA2Int a2) {
    bInt.A2Fn<IA2Int, int>(a2);
    bInt.A2Fn(a2);



}



#region Demo6

interface IA { }
interface IA2<T> : IA { }
interface IA2Int : IA2<int> { }

class CB<TIA> where TIA : IA { }

// 扩展方法的静态类是不允许是泛型类的，所以泛型参数都要写在扩展方法上面
static class CBExFn {
    public static void A2Fn<TIA2, T>(this CB<TIA2> _, TIA2 a2) where TIA2 : IA2<T> { }
    //                                                ^^^^ 这里，参数变为 TIA2
}
#endregion

static class Demo1 {
    interface IA { }
    interface IAA : IA { }
    interface IA2<T> : IA { }

    class CB<TIA> where TIA : IA { }
    // 需求：只要 CB<TIA> 的泛型参数 TIA 是 IA2<T> 的子类型，
    // 则提供一个方法的实现: void A2Fn(T t); // T 的类型是 IA2<T> 的泛型参数 T
    static void Test(CB<IA2<int>> bInt, CB<IA2<string>> bString) {
        // 以下类型都可以调用
        bInt.A2Fn(20210821);
        bString.A2Fn("20210821");
    }
}
static class Demo2 {
    interface IA { }
    interface IAA : IA { }
    interface IA2<T> : IA { }

    class CB<TIA> where TIA : IA { }
    // 需求：只要 CB<TIA> 的泛型参数 TIA 是 IA2<T> 的子类型，
    // 则提供一个方法的实现: void A2Fn(T t); // T 的类型是 IA2<T> 的泛型参数 T

    class CB2<T> : CB<IA2<T>> {
        public void A2Fn(T t) { }
    }
    static void Test(CB2<int> bInt, CB2<string> bString) {
        // 以下类型都可以调用
        bInt.A2Fn(20210821);
        bString.A2Fn("20210821");
    }
}

static class Demo3 {
    interface IA { }
    interface IAA : IA { }
    interface IA2<T> : IA { }
    interface IA2Int : IA2<int> { } // 注意这里

    class CB<TIA> where TIA : IA { }
    class CBString : CB<IA2<string>> { } // 注意这里
    class CB2<T> : CB<IA2<T>> {
        public void A2Fn(T t) { }
    }

    static void Test(CB2<int> bInt, CB2<string> bString) {
        bInt.A2Fn(20210821);
        bString.A2Fn("20210821");
    }
    // 注意这个 Test2 函数 
    static void Test2(CB<IA2Int> bInt, CB<IA2<string>> bString, CBString bString1) {
        bInt.A2Fn(20210821); // 没有找到方法
        bString.A2Fn("20210821"); // 没有找到方法
        bString1.A2Fn("20210821"); // 没有找到方法
    }
}

namespace Demo4 {
    interface IA { }
    interface IAA : IA { }
    interface IA2<T> : IA { }
    interface IA2Int : IA2<int> { }

    class CB<TIA> where TIA : IA { }
    class CBString : CB<IA2<string>> { }
    class CB2<T> : CB<IA2<T>> {
        public void A2Fn(T t) { }
    }
    static class CBExFn { // 注意这里
        public static void A2Fn<TIA2, T>(this CB<TIA2> self, T t) where TIA2 : IA2<T> { }
    }

    class CTest {
        public void Test(CB<IA2Int> bInt, CB<IA2<string>> bString, CBString bString1) {
            bInt.A2Fn(20210821);
            bString.A2Fn("20210821");
            bString1.A2Fn("20210821");
        }
    }
}

namespace Demo5 {
    interface IA { }
    interface IAA : IA { }
    interface IA2<T> : IA { }
    interface IA2Int : IA2<int> { }

    class CB<TIA> where TIA : IA { }
    class CBString : CB<IA2<string>> { }
    class CB2<T> : CB<IA2<T>> {
        public void A2Fn(T t) { }
    }
    static class CBExFn {
        public static void A2Fn<TIA2, T>(this CB<TIA2> self, T t) where TIA2 : IA2<T> { }
        public static void A2Fn(this CB<IA2<string>> self, string t) { }
    }

    class CTest {
        public void Test(CB<IA2Int> bInt, CB<IA2<string>> bString, CBString bString1) {
            bInt.A2Fn(20210821);
            bString.A2Fn("20210821");
            bString1.A2Fn("20210821");
        }
    }
}

static class Demo7 {
    class Point<T> { public T X; public T Y; }
    static void Test(Point<int> p1, Point<int> p2) {
        Point<int> p = p1 + p2;
    }
    static void Test(Point<float> p1, Point<float> p2) {
        Point<float> p = p1 + p2;
    }
}