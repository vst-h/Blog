# C# - 为 Linq 的 Min 和 Max 做增强
## 现在 Linq 的 Min 和 Max 是怎么样的？

先来看一个简单的代码使用：`new[]{ 1, 22, 3}.Max()`，这个使用那个 Linq 函数呢？

``` csharp
public static int Max(this IEnumerable<int> source);
```
这个扩展方法是对特定 `IEnumerable<int>` 类型进行实现的。

--------
再来看一个例子：`new[]{ "333", "22", "4444" }.Max(s => s.Length)`，这个使用的是：

``` csharp
public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);
```
这个函数也是只能对 `selector` 的返回值为 `int` 类型实现的。

--------
有没有更通用的泛型版本？例如这个代码 `new[]{ "333", "22", "4444" }.Max()` 调用了哪个函数？

``` csharp
public static TSource Max<TSource>(this IEnumerable<TSource> source);
```
是有泛型的，是不是后来加上的？我特意检查一下， `net35` 就可用了，也就是从 Linq 出来一开始就有了。应该是对 `int` 这类**数值类型**可能做了特殊实现，进行**优化**吧。

## Max 的泛型版本是否有问题？

这个泛型版本是否有问题呢？看看下面代码：

``` csharp
Exception Test1(List<Exception> list) {
    return list.Max();
}
```
编译通过没有报错，编译期类型安全，运行的时候呢？报错了：

``` csharp
System.ArgumentException: 必须至少有一个对象实现 IComparable。
  + System.Collections.Comparer.Compare(object, object)
  + System.Linq.Enumerable.Max<TSource>(IEnumerable<TSource>)
```
这是个问题吗？谁会傻子般的拿两个 `Exception` 做比较？这么一说，好像也不是问题。  
那能解决吗？当然可以，后面会讲到。

## Linq 的 Max 够用了吗？

看看如下需求：

``` csharp
// MaxBy 根据元素某个属性，返回列表里最大的这个元素
string maxStr = new[] { "333", "22", "4444" }.MaxBy(s => s.Length);
maxStr == "4444";
```
**是的，本篇就是要实现这个需求：添加 `MaxBy` 和 `MinBy` 函数**。

如果读者感兴趣，可以停下来，去自己实现一下，对个人的泛型抽象会有很大的帮助。
我当时对泛型的理解还不够深入，断断续续的写下来，才实现了这个功能。实现的时候用到了泛型特化，当时并不知道这个概念。如果思维上有了，一看到这种需求就有了解决方案，我现在写起来就不到十分钟。

--------
先想想一下个函数的签名是怎样的？返回值是列表的元素，是一个泛型 `T`，第一个参数是 `IEnumerable<T>`。第二个参数是一个函数 `Func<>`，用于选择通过那个属性比较大小值（如：`s => s.Length`），函数参数是 `T`，返回某个属性，为了达到最佳可用性，返回的这个属性也是一个泛型，先定义为 `TKey`。那么函数签名大致就是这样了：

``` csharp
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey);
```
先做个粗略版的实现，不考虑参数（`src`、`fnKey`）为 `null`，不考虑 `src` 是一个空的集合（没有元素，没有最大值）。

## 粗略版实现

``` csharp
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey) {
    T maxT = src.First();
    TKey maxKey = fnKey(maxT);
    foreach(var item in src.Skip(1)) {
        TKey itemKey = fnKey(item);
        if (maxKey < itemKey) { // 注意这里
            maxT = item;
            maxKey = itemKey;
        }
    }
    return maxT;
}
```
代码很简单，一个循环，两个存储变量。问题是上面的代码编译不过，if 里面的大小值判断是拒绝的：`运算符“<”无法应用于“TKey”和“TKey”类型的操作数`。一定要实现泛型版本，不需要对 `int` 类型或者其他类型做特殊实现，不能在运行时做**类型判断**，前面提到的 Linq 已经存在方法 `TSource Max<TSource>(this IEnumerable<TSource> source)` 就是运行时检查，运行时报错。

再来思考一下我们的需求：根据元素的某个属性比较大小值，那么这个属性就必然是可比较的，也就是需要一个抽象，就是这个接口了：`System.IComparable<T>`。约束一下泛型 `Tkey`，进行实现：

``` csharp
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey)
where TKey: IComparable<TKey>{ // 此行修改
    T maxT = src.First();
    TKey maxKey = fnKey(maxT);
    foreach(var item in src.Skip(1)) {
        TKey itemKey = fnKey(item);
        if (maxKey.CompareTo(itemKey) < 0) { // 此行修改
            maxT = item;
            maxKey = itemKey;
        }
    }
    return maxT;
}
```
这个粗略的实现已经编译通过，并且可用的了，Linq 已经存在方法 `TSource Max<TSource>(this IEnumerable<TSource> source)` 应该也能这么做约束，让类型检查把错误在编译期暴露出来，但是现在也不能加上了，会破坏兼容性。

--------
这个粗略版的实现对类型支持已经完整了吗？
并没有，还是缺失了一部分类型的支持：

``` csharp
new int?[] { 3, null, 2 }.MaxBy((int? x) => x)
//                        ^^^^^
```
在 `MaxBy` 处报错：`可以为 null 的类型“int?”不满足“System.IComparable<int?>”的约束。可以为 null 的类型不能满足任何接口约束`。想一下，如果要支持 `Nullable<T>` 类型，应该怎么处理。

## `Nullable<T>` 的比较性

注意上面的报错：`可以为 null 的类型不能满足任何接口约束`，看一下 `Nullable<T>` 的声明，确实没有实现任何接口：
``` csharp
public struct Nullable<T> where T : struct
```
对于**数值类型**，`Nullable<T>` 却可以进行一些运算符操作，如 `+`、`-`、`*`、`/`，还有 `>`、`<`、`==` 比较。对于自定义结构体如何支持 `Nullable<T>` 的比较运算？

``` csharp
struct SA {
    public int X;
    public static bool operator >(SA a, SA b) => a.X > b.X;
    public static bool operator <(SA a, SA b) => a.X < b.X;
}
public static bool Test1(SA? a1, SA? a2) {
    return a1 > a2;
}
```
只要实现对应的运算符即可，这应该是编译器的魔法了吧，因为 `Nullable<T>` 并不是对所有 `T` 类型都能支持运算符操作的。这种情况应该使用**泛型特化**进行实现，现在靠的是编译器魔法，不是类型系统，C# 的语言一致性 -3 分。

现在知道了实现了对应的比较运算符，`Nullable<T>` 就可以进行 `>`、`<` 比较。对于 `MaxBy` 我们实现的时候是不能直接进行 `>`、`<` 比较的，因为我们无法对 `TKey` 进行这种抽象约束，也就没法写出泛型约束。我们能用的就是 `IComparable<T>`，这个接口和 `>`、`<` 运算符是两个东西，可以分开实现。

一个抽象有两个实现？还可以分开？实现了 `IComparable<T>` 不意味着可以使用 `>`、`<` 运算符；实现了 `>`、`<` 运算符不意味着它有**可比较性**的抽象，C# 语言的一致性 -3 分。

## 粗略的实现 `Nullable<T>` 版

``` csharp
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey?> fnKey)
where TKey : struct, IComparable<TKey> {                     // ^^^^^
    T maxT = src.First();
    Nullable<TKey> maxKey = fnKey(maxT);
    foreach (var item in src.Skip(1)) {
        Nullable<TKey> itemKey = fnKey(item);
        if (itemKey == null) { continue; }
        if (maxKey == null || maxKey.Value.CompareTo(itemKey.Value) < 0) {
            maxT = item;
            maxKey = itemKey;
        }
    }
    return maxT;
}
```
现在我们的 `MaxBy` 有了两个版本的实现。那么 `MinBy` 呢，也要写两个版本，还有其他的方法呢？如：（`Sort`，`OrderBy`），每次实现都要写两个版本。Linq 自带的实现使用的是 `System.Collections.Generic.Comparer<T>` 进行比较，它不能对 `TKey` 进行泛型约束 `IComparable<TKey>`，便在运行时抛出了异常。

如果有了泛型特化，就可以为 `Nullable<T>` 实现 `IComparable<Nullable<T>>`，前提是 `T` 是满足 `IComparable<T>` 的。

假设代码实现：
``` csharp
implement IComparable<Nullable<T>> for Nullable<T> where T :IComparable<T> {
    int CompareTo(Nullable<T> other) {
        if (this == null && other == null) { return 0; }
        if (this == null) { return -1; }
        if (other == null) { return 1; }
        return this.Value.CompareTo(other.Value);
    }
}
```
可惜现在 C# 还没能实现。

## 简单的 MaxBy 实现：

``` csharp
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey)
where TKey : IComparable<TKey> {
    if (src == null) { throw new ArgumentNullException(nameof(src)); }

    using IEnumerator<T> iter = src.GetEnumerator();
    if (false == iter.MoveNext()) {
        throw new InvalidOperationException("Sequence does not contain any elements");
    }

    T maxT = iter.Current;
    TKey maxKey = fnKey(maxT);
    while (iter.MoveNext()) {
        T item = iter.Current;
        TKey itemKey = fnKey(item);
        if (maxKey.CompareTo(itemKey) < 0) {
            maxT = item;
            maxKey = itemKey;
        }
    }

    return maxT;
}
public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey?> fnKey)
where TKey : struct, IComparable<TKey> {
    if (src == null) { throw new ArgumentNullException(nameof(src)); }

    using IEnumerator<T> iter = src.GetEnumerator();
    if (false == iter.MoveNext()) {
        throw new InvalidOperationException("Sequence does not contain any elements");
    }

    T maxT = iter.Current;
    TKey? maxKey = fnKey(maxT);
    while (iter.MoveNext()) {
        T item = iter.Current;
        TKey? itemKey = fnKey(item);
        if (itemKey == null) { continue; }
        if (maxKey == null || maxKey.Value.CompareTo(itemKey.Value) < 0) {
            maxT = item;
            maxKey = itemKey;
        }
    }

    return maxT;
}
```
