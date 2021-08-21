using System;
using System.Collections.Generic;

Console.WriteLine(">>");
new Demo3.CCtor();

namespace Demo1 {
#if DEBUG
    class Demo {
        public static T Parse<T>(string str) {
            Type type = typeof(T);
            if (type == typeof(int)) {
                return int.Parse(str); // error:  无法将类型 “int” 隐式转换为 “T”	 
            }

            throw new InvalidOperationException("No parse method");
        }
    }
#endif
}
namespace Demo2 {

    class Demo {
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
    }
}
namespace Demo3 {

    class CCtor {
        static char ch = "AB"[3]; // System.TypeInitializationException
        static CCtor() {
            throw new Exception();
        }
    }

    class CA<T> {
        static readonly T Instance;
        static CA() {
            Instance = default(T);
            Console.WriteLine(typeof(T));
        }
    }

    class CB<T> {
        static readonly T Instance;
        static CB() {
            CB<int>.Instance = 1;
            CB<DateTime>.Instance = new DateTime(2021, 08, 08);
        }
    }

    class CC<T> {
        internal static T Instance;
    }
    static class CC {
        static CC() {
            CC<int>.Instance = 1;
            CC<DateTime>.Instance = new DateTime(2021, 08, 08);
        }
    }

#if DEBUG
    class CD<T> {
        static readonly T Instance;

        static class CD { // error CS0542: 成员名不能与它们的封闭类型相同

        }
    }
#endif

    class CE<T> {
        static T _instance;
        public static T Instance => _instance;

        static class CEInit {
            static CEInit() {
                CE<int>._instance = 1;
                CE<DateTime>._instance = new DateTime(2021, 08, 08);
                Console.WriteLine(typeof(T));
            }
        }
    }

    class CF<T> {
        public static string Name = typeof(T).Name;
    }
    static class CFTest {
        public static void Test1<T>() {
            Console.WriteLine(CF<T>.Name);
        }
    }

    class CG<TArg, T> {
        public static Func<TArg, T> GetInstance;
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

}