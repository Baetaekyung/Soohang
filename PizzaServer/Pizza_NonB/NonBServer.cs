using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _3_Pizza_NonB
{
    internal class NonBServer
    {
        private const int BUFFER_SIZE = 1024;
        private const int PORT = 12345;
        private Socket serverSocket;
        // 클라이언트들을 List로 가지고 있음, Thread가 아닌
        private readonly List<Socket> clients = new();

        public NonBServer()
        {
            try
            {
                Console.WriteLine($"Starting up at: 127.0.0.1:{PORT}");

                // 소켓 정의
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 소켓 연결
                serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
                // 소켓 시작
                serverSocket.Listen(1000);      // 클라이언트 소켓 대기열 최대 1000 
                serverSocket.Blocking = false;  // 서버 소켓 : 블로킹 off!
            }
            // 오류 처리
            catch (SocketException)
            {
                serverSocket?.Close();
                Console.WriteLine("\nServer failed to start.");
            }
        }

        private void Accept()
        {
            try
            {
                // 소켓에 접속한 소켓을 받음
                Socket clientSocket = serverSocket.Accept();
                clientSocket.Blocking = false;      // 클라이언트소켓 : 블로킹 off!
                // 클라이언트 리스트에 연결된 클라이언트의 소켓을 저장함
                clients.Add(clientSocket);
                Console.WriteLine($"Connected to {clientSocket.RemoteEndPoint}");
            }
            // 오류 처리, ErrorCode가 WouldBlock일떄 
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        private void Serve(Socket client)
        {
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                // 소켓에서 데이터 받고
                int bytesRead = client.Receive(buffer);
                // 실패시 소켓 연결 해제
                if (bytesRead == 0)
                {
                    clients.Remove(client);
                    client.Close();
                    return;
                }

                // 역직렬화 하고
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                // 답변 달고
                if (int.TryParse(received, out int order))
                {
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    response = "Wrong number of pizzas, please try again\n";
                }

                Console.WriteLine($"Sending message to {client.RemoteEndPoint}");
                // 직렬화 해서 보내줌
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                client.Send(responseData);
            }
            // 오류 처리
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)        // 폴링 반복문
                {                   // 입출력 연산(accept(), send(), read() 이 성공할 때까지 연산 반복
                    Accept();       // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감

                    // 왜 새로운 List를 만드는 거지
                    // 접속한 모든 클라이언트에 Serve를 실행해줌
                    foreach (var client in new List<Socket>(clients))
                    {
                        Serve(client);  // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감
                    }

                    // Thread(1ms)쉬게 해서 무한으로 검사하는 것을 피해줌
                    Thread.Sleep(1); // avoid 100% CPU usage
                }
            }
            // 서버 종료시 serverSocket 종료
            finally
            {
                serverSocket.Close();
                Console.WriteLine("\nServer stopped.");
            }
        }

        static void Main()
        {
            // 서버 시작
            NonBServer server = new NonBServer();
            server.Start();
        }
    }
}