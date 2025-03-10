
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Diagnostics;


_ = Task.Run(() => StartClient(1111));
_ = Task.Run(() => StartClient(2222));

_ = Task.Run(StartServer);

Console.ReadLine();
return;

void StartServer() {
    for (var i = 5; true; i++) {
        var id = i * 10101;
        var server = new NamedPipeServerStream(
            "TestPipe1", PipeDirection.InOut, 32,
            PipeTransmissionMode.Message, PipeOptions.Asynchronous);

        Console.WriteLine($"服务端 {id}：等待客户端连接...");
        server.WaitForConnection();
        Task.Run(() => {
            try {

            var buffer = new byte[256];
            for (var i = 1; true; i++) {
                var len = server.Read(buffer);  // 阻塞直到读取到数据
                if (len == 0) { break; }

                var msg = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"服务端 {id}    收到: {msg}");

                Task.Delay(100).Wait();
                var resp = Encoding.UTF8.GetBytes($"({id})响应 OK {i}");
                server.WriteAsync(resp);
                server.Flush();
            }

            } catch(Exception e) {
                Console.WriteLine(e.ToString());
                throw;
            } finally {
                Console.WriteLine($"服务端 {id} 结束");
                server.Dispose();
            }
        });
    }
}


void StartClient(int id) {
    Task.Delay(id).Wait();
    using var client = new NamedPipeClientStream(".", "TestPipe1", PipeDirection.InOut);
    client.Connect();

    using var timer = new System.Timers.Timer(id);
    timer.Elapsed += (_, _) => {
        //Console.WriteLine($"客户端正在发送消息 {id}");
        var req = $"({id})请求 at {DateTime.Now:HH:mm:ss fff}";
        var data = Encoding.UTF8.GetBytes(req);
        client.Write(data);

        var buffer = new byte[256];
        var resp = client.Read(buffer);
        Console.WriteLine($"    客户端 {id} 收到: " + Encoding.UTF8.GetString(buffer));
    };

    timer.Start();
    Task.Delay(10_000).Wait();
}

// 管道直接关闭，里面还有数据的话，会抛异常： System.IO.IOException: Pipe is broken.