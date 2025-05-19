using System.Net.Sockets;
using System.Net;
using System.Text;

namespace _2_Pizza_Thread
{
    internal class Handler
    {
        private const int BUFFER_SIZE = 1024;
        private readonly TcpClient client;

        public Handler(TcpClient client)    // 각 스레드마다 클라이언트 소켓을 부여받는다.
        {
            this.client = client;
        }

        public void Run()   // 스레드마다 서버의 역할 수행
        {
            IPEndPoint? clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Connected to {clientEndPoint}");

            // 클라이언트한테 Stream으로 입력 데이터 받음
            NetworkStream stream = client.GetStream();
            // buffer에 캐싱
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                while (true)
                {
                    // 버퍼에 데이터 읽어 놓기
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                    // 읽기 실패하면 스레드 종료
                    if (bytesRead == 0) break;

                    // 받은 데이터 스트링으로 역직렬화
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string response; // 답변

                    // 숫자면 감사 메시지 작성
                    if (int.TryParse(received, out int order))
                    {
                        response = $"Thank you for ordering {order} pizzas!\n";
                    }
                    // 아니면 오류 메시지 작성
                    else
                    {
                        response = "Wrong number of pizzas, please try again\n";
                    }

                    Console.WriteLine($"Sending message to {clientEndPoint}");
                    // 답변 직렬화후 stream에 답변 데이터 작성
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            // 스레드 종료시 클라이언트도 닫아줌
            finally
            {
                Console.WriteLine($"Connection with {clientEndPoint} has been closed");
                client.Close();
            }
        }
    }

    internal class ThreadServer
    {
        private const int PORT = 12345;
        private readonly TcpListener server;

        public ThreadServer()   // 서버 소켓 세팅
        {
            try
            {
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);
                Console.WriteLine($"Starting up at: {localAddress}");
                server = new TcpListener(localAddress);
                server.Start();
            }
            catch (SocketException)
            {
                server?.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)
                {
                    //클라이언트 연결 받음
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Client connection request from {client.Client.RemoteEndPoint}");

                    // 그리고 스레드로 각 클라에 해줄 작업을 Handler를 통해 해줌
                    // 그래서 Accept를 계속 해줄 수 있음
                    Handler handler = new Handler(client);
                    Thread thread = new Thread(new ThreadStart(handler.Run));
                    thread.Start();
                } // 클라이언트 요청이 들어올 때마다 새로운 스레드를 생성한다.
            }
            // 서버 종료
            finally
            {
                server.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }
        static void Main()
        {
            // 서버 시작
            ThreadServer server = new ThreadServer();
            server.Start();
        }
    }
}