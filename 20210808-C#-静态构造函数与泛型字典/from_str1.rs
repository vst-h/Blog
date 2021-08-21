
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