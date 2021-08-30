#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
using System;
using System.Linq;
using System.Collections.Generic;
using Demo5;

Console.WriteLine(">>");
new[] { 1, 22, 3 }.Max();
new[] { "333", "22", "4444" }.Max(s => s.Length);
new[] { "333", "22", "4444" }.Max();


#if DEBUG
Exception Test1(List<Exception> list) {
    return list.Max();
}
Test1(new() { new() { }, new() { } });
#endif

// MaxBy 根据元素某个属性，返回列表里最大的这个元素
string maxStr = new[] { "333", "22", "4444" }.MaxBy(s => s.Length);
//maxStr == "4444";
Console.WriteLine(maxStr); {
#if DEBUG
    int? maxInt = new int?[] { 3, null, 2 }.MaxBy(x => x);
    //                                   ^^^^^
#endif
}


Demo5.EnumerableEx.Test1();


#if DEBUG
namespace Demo1 {
    public static class EnumerableEx {
        public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey) {
            T maxT = src.First();
            TKey maxKey = fnKey(maxT);
            foreach (var item in src.Skip(1)) {
                TKey itemKey = fnKey(item);
                if (maxKey < itemKey) {
                    maxT = item;
                    maxKey = itemKey;
                }
            }
            return maxT;
        }
    }
}
#endif

namespace Demo2 {
    public static class EnumerableEx {
        public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey)
        where TKey : IComparable<TKey> { // 此行修改
            T maxT = src.First();
            TKey maxKey = fnKey(maxT);
            foreach (var item in src.Skip(1)) {
                TKey itemKey = fnKey(item);
                if (maxKey.CompareTo(itemKey) < 0) { // 此行修改
                    maxT = item;
                    maxKey = itemKey;
                }
            }
            return maxT;
        }
    }
}


namespace Demo3 {
    struct SA {
        public int X;
        public static bool operator >(SA a, SA b) => a.X > b.X;
        public static bool operator <(SA a, SA b) => a.X < b.X;
    }
    static class CTest {
        public static bool Test1(SA? a1, SA? a2) {
            return a1 > a2;
        }
    }
    public static class EnumerableEx {
        public static T MaxBy<T, TKey>(this IEnumerable<T> src, Func<T, TKey> fnKey)
        where TKey : IComparable<TKey> { // 此行修改
            T maxT = src.First();
            TKey maxKey = fnKey(maxT);
            foreach (var item in src.Skip(1)) {
                TKey itemKey = fnKey(item);
                if (maxKey.CompareTo(itemKey) < 0) { // 此行修改
                    maxT = item;
                    maxKey = itemKey;
                }
            }
            return maxT;
        }
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
    }
}

namespace Demo4 {
    public static class EnumerableEx {
        static int CompareTo(this Nullable<int> self, Nullable<int> other) {
            if (self == null && other == null) { return 0; }
            if (self == null) { return -1; }
            if (other == null) { return 1; }
            return self.Value.CompareTo(other.Value);
        }
    }
}

namespace Demo5 {
    public static class EnumerableEx {
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

        public static void Test1() {
            {
                var max = new int[] { 1, 2, 33, 22 }.MaxBy(x => x);
                Console.WriteLine($"{33} == {max}");
            }
            {
                var max = new int?[] { 1, 2, null, 22 }.MaxBy(x => x);
                Console.WriteLine($"{22} == {max}");
            }
            {
                var max = new int?[] { null, null }.MaxBy(x => x);
                Console.WriteLine($"{null} == {max}");
            }
            Comparer
        }
    }
}