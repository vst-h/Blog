#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
using Demo0;

Console.WriteLine("-------");

static void Test1(CB<IB<DateTime>> b) {
    b.FnB().FnA();
    b.FnA().FnB();
}

namespace Demo0 {
    interface IA<T> { }
    interface IB<T>: IA<T> { }
    static class CAEx {
        public static A FnA<T, A, TEntity>(this CA<T, A> self)
        where A: CA<T, A>
        where T: IA<TEntity>
        => self as A ?? throw new InvalidOperationException();
    }

    class CA<T, A> { }
    class CB<T> : CA<T, CB<T>> {
        public CB<T> FnB() => this;
    }
}

#if false
namespace Demo5 {
    static class CAEx {
        public static A FnA<T, A>(this CA<T, A> self) where A: CA<T, A>
        => self as A ?? throw new InvalidOperationException();

        static void Test1(CB2 b) {
            b.FnB2().FnA();
            b.FnA().FnB2();
        }
    }

    class CA<T, A> { }
    class CB<T> : CA<T, CB<T>> { }
    class CB2: CB<int> {
        public CB2 FnB2() => this;
    }
}
#endif

#if false
namespace Demo4 {
    static class CAEx {
        public static A FnA<T, A>(this CA<T, A> self) where A: CA<T, A>
        => self as A ?? throw new InvalidOperationException();

        static void Test1(CB<int> b) {
            b.FnB().FnA();
            b.FnA().FnB();
        }
    }

    class CA<T, A> { }

    class CB<T> : CA<T, CB<T>> {
        public CB<T> FnB() => this;
    }
}
#endif

#if false
namespace Demo3 {
    class CA<T> { }

    class CB<T> : CA<T> {
        public CB<T> FnB() => this;
    }

    static class CAEx {
        public static TC FnA<TC, A>(this TC self) where TC : CA<A>
        => self;

        static void Test1(CB<int> b) {
            b.FnB().FnA();
            b.FnA().FnB();
        }
    }
}
#endif

#if false
namespace Demo2 {
    class CA { }

    class CB : CA {
        public CB FnB() => this;
    }

    static class CAEx {
        public static T FnA<T>(this T self) where T : CA
        => self;
    }
}
#endif

#if false
namespace Demo1 {
    class CA {
        public CA FnA() => this;
    }

    class CB : CA {
        public CB FnB() => this;
    }
}
#endif