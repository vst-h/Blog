# C# - 静态构造函数与泛型字典
## 问题：有一个泛型 T，假设它可以通过字符串转换成 T，那么如何实现这个函数

``` csharp
T Parse<T>(string str); // 如何实现这个函数
void Test() {
    int valInt = Parse<int>("123");
    DateTime valDatetime = Parse<DateTime>("2021-7-30");
}
```
这个问题是2018年写项目代码的时候碰到的，先来看代码，当时也没有什么想到好的解决思路，我们尝试着去实现上面的 Parse<T> 函数：

```csharp
public static T Parse<T>(string str) {
    Type type = typeof(T);
    if (type == typeof(int)) {
        return int.Parse(str); //  无法将类型 “int” 隐式转换为 “T”	 
    }

    throw new InvalidOperationException("No parse method");
}
```
上面的代码出现了类型转换错误，但是这个是运行时正确的。虽然是运行时正确的，但是在类型系统上来说是不正确的。因为函数需要返回一个 `T` 而不是 `int`，这在编译期就编译不过了，那我们加一个强制转换行不行：`return (T)(int.Parse(str)); `，强制转换为 `T`。这当然也不行了，强制转换是可以从基类转换到继承类（运行时可能会出错），但是 `int` 到 `T` 根本没有这层类型关系。

当然还是可以解决这个类型转换问题，这时就引出了C#万恶之源：`object`。是的，我很讨厌这个东西，有时认为这个是类型系统上设计的错误，它就不应该存在。哪有什么「一切皆对象」。这个类型转换可以经过这么一个转换：`int` -> `object` -> `T`，这个是先向下转换，再向上转换，向上转换时不安全的，所以我们要在运行时保证安全。（有的语言直接把这种不安全转换禁掉了）

```csharp
public static T Parse<T>(string str) {
    Type type = typeof(T);
    if (type == typeof(int)) {
        return (T)(object)(int.Parse(str));	 
    }
    if (type == typeof(DateTime)) {
        return (T)(object)(DateTime.Parse(str));
    }

    throw new InvalidOperationException("No parse method");
}
```
就上面的代码而言，它需要写很多 if 做运行时判断，而且它还要装箱再拆箱，如果配合上后来 C# 提供的 switch 表达式，函数可以写得简洁点。这个函数还是存在问题，我并不满意：运行时做判断，装箱。装箱已经是大忌了。那时候因为装箱，虽然实现了函数，但我还是没有应用开来，甚至想放弃了泛型的转换，直接调用实际的类型 `int.Paser`，以避免装箱。卡住了很久一段时间，一直在寻求避免装箱的思路。

## 静态构造函数
### 静态构造函数
过了一段时间，偶然的看文章，看书，提到了一个重要的概念：类构造函数或静态构造函数。如果反编译去看代码，会发现它是 `.cctor`，实例的构造函数是 `.ctor`。

静态构造函数是负责类初始化的，比如静态字段、静态属性，静态构造函数必须是无参的私有的，不能手动调用。JIT 编译器在编译一个方法代码时，检查到未初始化的类型时，就会让 CLR 负责调用，且只调用**一次**，并且只有一个线程执行静态构造函数。只有一个线程会执行静态构造函数，这意味着什么？天然的单例模式啊，是的，只要写一个只读的静态属性就已经是单例模式了。

静态字段的（行内）初始化也是放在静态构造函数里面的，这在编译的时候就已经放进去了，放在前面。如果执行静态构造函数抛出未处理的异常，则将会抛出 `System.TypeInitializationException` 异常。关于静态构造函数的更多信息，可以去看书《CLR via C#》的第八章，8.3 的类型构造器。

```csharp
class CCtor {
    static readonly char ch = "AB"[3]; // throw System.TypeInitializationException
    static CCtor() {
        throw new Exception();
    }
}
```
### 泛型类的静态构造函数
那么，泛型类呢？只执行一次？那单例模式呢？
```csharp
class CA<T> {
    static readonly T Instance;
    static CA() {
        Instance = default(T);
        Console.WriteLine(typeof(T));
    }
}
```
看看这代码，很显然已经出答案了：每一个实际的泛型类型类都会去执行一遍静态构造函数，要不然就不能保证代码的正确了。

再看上面的代码，存在泛型的静态字段不好初始化。可能很多人会写出这种代码：

```csharp
class CB<T> {
    static readonly T Instance;
    static CB() {
        CB<int>.Instance = 1;
        CB<DateTime>.Instance = new DateTime(2021, 08, 08);
    }
}
```
这是存在问题的，前面提到过，静态构造函数是由 CLR 负责调用的，并且只有一个线程执行一次初始化。现在加上泛型，每一个实际的泛型类型都会调用它们的静态构造函数，所以这个静态构造函数会执行很多次，会产生很多实例。

对于要初始化含有泛型的属性时，所以最好不要在泛型类型的静态构造函数执行初始化。注意这里是针对泛型属性或泛型字段。

常用的做法：

```csharp
class CC<T> {
    internal static T Instance;
}
static class CC {
    static CC() {
        CC<int>.Instance = 1;
        CC<DateTime>.Instance = new DateTime(2021, 08, 08);
    }
}
```
这个代码和和上一个代码也存在了差异，`Instance` 不再是只读的了，它必须由外部的一个类帮它初始化，并且只初始化一次。这又有些不完美了，意味着我必须保证其他人不要随便给 `Instance` 赋值。

用 `protected` 进行约束？不行！尝试一下声明：

```csharp
class CCInit<T>: CC<T>
```
这个类必须带一个泛型参数，去掉这个泛型参数又必须实例化这个泛型参数：

```csharp
class CCInt: CC<int>
```
这样更加不好让 CLR 触发调用初始化了。那嵌套类呢？

### 泛型类的嵌套类
使用嵌套类，报错了：

```csharp
class CD<T> {
    static readonly T Instance; 

    static class CD { // error CS0542: 成员名不能与它们的封闭类型相同

    }
}
```
修改嵌套类的名称：

```csharp
class CE<T> {
    static T _instance;
    public static T Instance => _instance;

    static class CEInit {
        static CEInit() {
            CE<int>._instance = 1;
            CE<DateTime>._instance = new DateTime(2021, 08, 08);
            Console.WriteLine(typeof(T)); // @a
        }
    }
}
```
徒劳，写出来的时候也已经察觉到了，这个也有一样的问题：不好让 CLR 触发调用嵌套类的静态构造函数，而且因此我发现了一个问题，或者特性，嵌套类可以访问外部类的泛型参数的，看 `@a` 的那行代码。**每一个泛型类的嵌套类都是不同的类型**，由泛型的类型而不同，`typeof(CE<int>.CEInit) == typeof(CE<long>.CEInit)`，这行代码的结果是 `false`。

关于泛型类的问题就探讨到此。我也没能解决访问权限的问题。

## 泛型字典？静态分发？
每一个实际的泛型类型都是一个独立的类型，都会去执行一遍类的静态构造函数。有趣的东西就有了：

```csharp
class CF<T> {
    public static string Name = typeof(T).Name;
}
static class CFTest {
    public static void Test1<T>() {
        Console.WriteLine(CF<T>.Name);
    }
}
```
当我们外部调用函数：`Test1<int>()`，内部引用了 `CF<int>.Name`，这个是 JIT 编译时决定了，意味着我不用在运行时写 `if (typeof(T) == xxx)` 进行类型判断。它就像一个字典，输入泛型，输出实际存储的东西，像个字典，其实如果一个人熟悉泛型类的话，就会知道这是很常见这种泛型静态字段。日常写代码的时候，我之前也是一直在用，但是没有察觉出有什么特别之处，直到要求写 `Test1<T>` 时才察觉出来：编译期产生了不同的代码调用。来个更特别的：

```csharp
class CG<TArg, T> {
    internal static Func<TArg, T> GetInstance;
}

class CG {
    static CG() {
        CG<int, List<int>>.GetInstance = (int n) => new List<int>(n);
        CG<int, HashSet<int>>.GetInstance = (int n) => new HashSet<int>(n);
    }
    public static T GetInstance<T>(int n) {
        return CG<int, T>.GetInstance(n);
    }
}
```
当我们调用 `CG.GetInstance<List<int>>(10)` 就可以得到一个 `List<int>`。可能会不屑的说：「这有什么用？」，直接 `new List<int>(10)` 不就完事了吗？是的，问题是我这个是泛型，泛型的抽象才是我要解决的问题。

这个写法或者模式，当时给我的感觉是：我触摸到了很强的抽象，没有面向对象，只需要函数和泛型，便可实现了这个 `GetInstance<T>`。我当时不知道这个模式叫什么，它并不是什么设计模式，它也很简单，泛型静态字段 + 函数。当时自己给这个写法或模式叫做**泛型注册**，因为它就是类型进行注册然后就可以使用了，没有注册的类型进行调用会引起空对象引用异常，所以使用者要保证类型注册了。

几年后我也在网上看到一些文章也有提到，把这个写法或模式叫做**泛型字典**。也是在几年后我接触到了另一门语言 `Rust`，看 Rust 的一些文章时提到了**静态分发**和**动态分发**。

> 在编译期决定要调用的方法时，它被称为静态分发或早期绑定
> 
> 直到运行时才能确定调用的方法，被称为动态分发

我们经常使用的面向对象的多态就是动态分发，它是运行时决定的。

## 实现 `T Parse<T>(string)` 函数
回到开头提到的那个问题：

``` csharp
T Parse<T>(string str); // 如何实现这个函数，不能装箱，不需要运行时判断类型
void Test() {
    int valInt = Parse<int>("123");
    DateTime valDatetime = Parse<DateTime>("2021-7-30");
}
```
简略的写一下：

``` csharp
public delegate bool FnTry<in T, TValue>(T obj, out TValue val);

static class StrParse<T> {
    public static Func<string, T> Parse;
    public static FnTry<string, T> TryParse;
}

public static class StrParse {
    static StrParse() {
        StrParse<int>.Parse = int.Parse;
        StrParse<int>.TryParse = int.TryParse;

        StrParse<DateTime>.Parse = DateTime.Parse;
        StrParse<DateTime>.TryParse = DateTime.TryParse;
    }

    public static T Parse<T>(string str) {
        return StrParse<T>.Parse(str);
    }

    public static bool TryParse<T>(string str, out T result) {
        return StrParse<T>.TryParse(str, out result);
    }
}
```
我已经实现了这个 `StrParse` 的库，更多细节实现可以看：
[https://github.com/vst-h/StringParse](https://github.com/vst-h/StringParse "StringParse")

## C# 这个模式的缺陷

C# 的这个模式是存在问题的：没有注册的类型会引起空对象引用异常。这个是不安全的，对于编写实现的人来说是个大问题，你不懂你的队友会怎么搞，第三方使用者能正确的使用吗？而且这个泛型静态字段读写权限是 `public` 或 `internal` 的。都不完美，C# 限制了人，限制了很多思想。

回到开始的问题：
> 有一个泛型 T，假设它可以通过字符串转换成 T，那么如何实现这个函数

这个假设就离谱，对于一个没有实现 `T Paser(string)` 的类型来说，它就不能注册，也不应该进行调用。例如：`StrParse.Parse<List<int>>("xxxxx")`。这个假设缺少了一种抽象：「一个类型是可以通过 `string` 解析出来的」，我要对所有有这个抽象特征的类型实现 `T Parse(string)`，以保证编译期的类型安全。再假设，我有一个类型 `T` 它实现了加法运算符，那么我就可以对它的实例进行加法运算：

``` csharp
void Add<T>(T left, T right) where T: operator +(T, T) {
    T val = left + right;
}
```

把这个模式叫泛型注册或者泛型字典，都是不对的，其他语言（Rust、C++）直接提供了有这个特性，而且写起来更安全，叫**泛型特化**。

看看 Rust 的实现
[Rust Playground](https://play.rust-lang.org/?version=stable&mode=debug&edition=2018&gist=779716fcd9fa3769594bb191ea126416)
：
``` rust
// 写个简化版
trait FromStr1 { // 相当于 interface
    fn from_str(s: &str) -> Self; // 函数声明
}

#[derive(Debug)]
struct SA1 { // 结构体 SA1
    val: i32
}

impl FromStr1 for SA1 { // 为 SA1 实现 FromStr1
    fn from_str(s: &str) -> Self {
        return SA1{ val: s.parse::<i32>().unwrap() };
    }
}

#[derive(Debug)]
struct SA2 { // 结构体 SA2，没有实现  FromStr1
    val: i64
}

fn parse<T>(s: &str) -> T
where T: FromStr1 { // 泛型特化
   return T::from_str(s)
}


fn main() {
    let sa1 = parse::<SA1>("111");
    // let sa2 = parse::<SA2>("111"); // 编译期就报错了，因为它没有实现 FromStr1
    println!("{:?}", &sa1);
}
```
静态分发，编译期类型安全。没有什么需要编写运行时的类型注册。一切都很自然。

我可以看到 Rust 的 `paser<T>(&str) -> T` 的实现，`T::from_str(s)` 像是调用静态方法一样，C# 的 `int.Paser(string)` 也是 `int` 的静态方法，`int` 的加法运算符也是静态方法，这些个抽象，Rust 都可以通过 `trait` 进行抽象，而且 `trait` 的成员是可以有常量的。

如果不是 Rust，我根本不会想到 C# 的 `interface` 缺少了静态函数的抽象，缺少静态成员。`interface` 是面向对象提出的概念，从来不应该假设 `interface` 应该添加静态成员，`interface` 必须是依附于一个实例，依附于一个对象，怎么可以是静态的。甚至可能在讨论的时候被面向对象的拥趸者认为是**异教徒**。

还好 C# 已经有一个提案被接受了，就允许给 `interface` 添加静态成员，可以看 issue: [Static abstract members in interfaces](https://github.com/dotnet/csharplang/issues/4436)。如果这个特性实现了出来，那么「泛型注册」、「泛型字典」这个模式也就不存在了，会被泛型特化更好的实现，我的 `StringParse` 也毫无用武之地了，希望这个特性尽快到来。




