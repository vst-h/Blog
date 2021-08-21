use std::ops::Add;

#[derive(Debug)]
struct Point<T> { x: T, y: T }

// 为 Point<T> 进行特化实现
impl<T: Add<Output = T>> Add for Point<T> {
    //   ^^^^^^ 这里已经约束了 T 必须实现 Add 运算符
    type Output = Point<T>;

    fn add(self, other: Point<T>) -> Point<T> {
        return Point {
            x: self.x + other.x,
            y: self.y + other.y,
        };
    }
}

fn main() {
    let p1: Point<i32> = Point{ x: 1, y: 2 };
    let p2: Point<i32> = Point{ x: 10, y: 20 };
    println!("{:?}", p1 + p2); // Point { x: 11, y: 22 }
    let p1: Point<f64> = Point{ x: 0.1, y: 0.2 };
    let p2: Point<f64> = Point{ x: 10.0, y: 20.0 };
    println!("{:?}", p1 + p2); // Point { x: 10.1, y: 20.2 }
}
