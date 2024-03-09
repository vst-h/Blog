#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
using Demo0;

Console.WriteLine("-------");

static void Test1(ICA<IB, IB> cb) {
    cb.FnB().FnA();
    cb.FnA().FnB();
}

namespace Reader {
    using System.Data;
    using System.Linq.Expressions;

    interface IContext<TSrc, TEntity> { }
    interface IContext<TSrc, TEntity, TData>: IContext<TSrc, TEntity> { }
    interface IReader<out TCtx>: IReader<TCtx, TCtx>  { }
    interface IReader<out TCtx, out _C> { }

    static class CTest {
        record EntityA(long Id, string Name);
        static void Test1(IReader<IContext<DataTable, EntityA>> reader) {
            var lmd5 = reader.Lambda(f => f.StartRow().ToDictionary(entity => entity.Id));
            var fn5 = lmd5.Compile(); // Func<DataTable, int, int, Dictionary<long, Entity>>
        }
        static void Test2(IReader<IContext<DataTable, EntityA, object>> reader) {
            // 带有 TData 的 IContext 可以有 ReturnData() 方法返回 TData
            var lmd6 = reader.Lambda(f => f.Start().ToDictionary(e => e.Id).ReturnData());
            var fn6 = lmd6.Compile(); // Func<DataTable, int, int, (Dictionary<long, Entity>, object)>
        }
    }

    static class ReaderEx {

        public static Expression<Func<Src, List<E>>> Lambda<C, Src, E>(
            this IReader<IContext<Src, E>, C> reader
        ) => throw new NotImplementedException();

        public static Expression<F> Lambda<C, Src, E, F>(
            this IReader<IContext<Src, E>, C> reader,
            Func<IBuildFunc<C, Func<Src, List<E>>>, IBuildFunc<C, F>> buildFn
        ) => throw new NotImplementedException();
    }

    interface IBuildFunc<out C, out F, out _C> { }
    interface IBuildFunc<out C, out F> : IBuildFunc<C, F, C> { }

    static class BuildFuncEx {

        public static IBuildFunc<C, Func<S, int, R>> StartRow<C, S, E, R>(
            this IBuildFunc<IContext<S, E>, Func<S, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<S, int, int, R>> Start<C, S, E, R>(
            this IBuildFunc<IContext<S, E>, Func<S, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, (R, Data)>> ReturnData
        <C, Src, E, Data, R>(
            this IBuildFunc<IContext<Src, E, Data>, Func<Src, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, (R, Data)>> ReturnData
        <C, Src, E, Data, R>(
            this IBuildFunc<IContext<Src, E, Data>, Func<Src, int, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, int, (R, Data)>> ReturnData
        <C, Src, E, Data, R>(
            this IBuildFunc<IContext<Src, E, Data>, Func<Src, int, int, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, List<E>>> ToList<C, Src, E, R>(
            this IBuildFunc<IContext<Src, E>, Func<Src, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, List<E>>> ToList<C, Src, E, R>(
            this IBuildFunc<IContext<Src, E>, Func<Src, int, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, int, List<E>>> ToList<C, Src, E, R>(
            this IBuildFunc<IContext<Src, E>, Func<Src, int, int, R>, C> b
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, Dictionary<Key, E>>> ToDictionary
        <C, Src, E, R, Key>(
            this IBuildFunc<IContext<Src, E>, Func<Src, R>, C> b,
            Expression<Func<E, Key>> key
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, Dictionary<Key, E>>> ToDictionary
        <C, Src, E, R, Key>(
            this IBuildFunc<IContext<Src, E>, Func<Src, int, R>, C> b,
            Expression<Func<E, Key>> key
        ) => throw new NotImplementedException();

        public static IBuildFunc<C, Func<Src, int, int, Dictionary<Key, E>>> ToDictionary
        <C, Src, E, R, Key>(
            this IBuildFunc<IContext<Src, E>, Func<Src, int, int, R>, C> b,
            Expression<Func<E, Key>> key
        ) => throw new NotImplementedException();
    }
}

#if false
namespace ReaderTry {
    using System.Data;
    interface IContext<TSrc, TEntity> { }
    interface IContext<TSrc, TEntity, TData>: IContext<TSrc, TEntity> { }
    interface IReader<TCtx> { }

    static class CTest {
        record EntityA(long Id, string Name);
        static void Test1(IReader<IContext<DataTable, EntityA>> reader) {
            var lmd5 = reader.Lambda(f => f.StartRow().ToDictionary(entity => entity.Id));
            var fn5 = lmd5.Compile(); // Func<DataTable, int, int, Dictionary<long, Entity>>
        }
        static void Test2(IReader<IContext<DataTable, EntityA, object>> reader) {
            // 带有 TData 的 IContext 可以有 ReturnData() 方法返回 TData
            var lmd6 = reader.Lambda(f => f.Start().ToDictionary(e => e.Id).ReturnData());
            var fn6 = lmd6.Compile(); // Func<DataTable, int, int, (Dictionary<long, Entity>, object)>
        }
    }
}
#endif

namespace Demo0 {
    static class CAEx {
        public static ICA<T, T> FnA<T>(this ICA<IA, T> self)
        => self as ICA<T, T> ?? throw new InvalidOperationException();

        public static ICA<T, T> FnB<T>(this ICA<IB, T> self)
        => self as ICA<T, T> ?? throw new InvalidOperationException();

    }

    interface ICA<out T, out _T> { }
    interface IA { }
    interface IB: IA { }
}

#if false
namespace Demo7 {
    static class CAEx {
        public static A FnA<A>(this ICA<IA, A> self) where A: ICA<IA, A>
        => self as A ?? throw new InvalidOperationException();
    }

    interface ICA<out T, out A> { }
    interface ICB<out T> : ICA<T, ICB<T>> {
        int B1 { get; set; }
    }
    interface IA { }
    interface IB: IA { }
}
#endif

#if false
namespace Demo6 {
    interface IA { }
    interface IB: IA { }
    static class CAEx {
        public static A FnA<A>(this CA<IA, A> self) where A: CA<IA, A>
        => self as A ?? throw new InvalidOperationException();
        public static void TestList1(List<string> strs, List<object> objs) {
            objs = strs;
        }
    }

    class CA<T, A> { }
    class CB<T> : CA<T, CB<T>> {
        public CB<T> FnB() => this;
    }
}
#endif

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