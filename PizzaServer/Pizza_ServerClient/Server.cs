using System.Net.Sockets;
using System.Net;
using System.Text;

internal class Server
{
    // 버퍼 크기
    private const int BUFFER_SIZE = 1024;
    // IP주소, 포트번호
    private static readonly IPEndPoint ADDRESS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
    private TcpListener serverSocket; // Tcp 통신 소켓

    public Server()     // 서버 소켓 생성
    {
        try
        {
            Console.WriteLine($"Starting up at: {ADDRESS}");
            // 주소로 새로운 Listener생성
            serverSocket = new TcpListener(ADDRESS);
            // Listen 시작
            serverSocket.Start();
        }
        // Socket 통신 오류 발생
        // 소켓 있으면 Stop
        catch (SocketException)
        {
            Console.WriteLine("\nServer failed to start.");
            serverSocket?.Stop();
        }
    }

    public TcpClient Accept()   // 클라이언트 서버 접속 대기 및 클라이언트 소켓 반환
    {
        // 소켓 열어서 클라이언트 접속 대기, 접속 할 때까지
        TcpClient client = serverSocket.AcceptTcpClient();
        IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        Console.WriteLine($"Connected to {clientEndPoint}");
        return client;
    }

    public void Serve(TcpClient client)     // 클라가 보내는 데이터 수신 및 서버의 응답 송신
    {
        // 네트워크 통신 데이터 스트림
        NetworkStream stream = client.GetStream();
        // 캐싱할 버퍼
        byte[] buffer = new byte[BUFFER_SIZE];

        try
        {
            // 이게 While문이라 계속 실행되면서 서버의 Accept를 Block한다.
            while (true)
            {
                // 네트워크 상의 데이터 읽음
                int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                // 읽기 실패시 서버 종료
                if (bytesRead == 0) break;

                // 바이트에서 string으로 파싱, 역 직렬화
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                if (int.TryParse(receivedData, out int order))
                {
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    response = "Wrong number of pizzas, please try again\n";
                }

                Console.WriteLine($"Sending message to {client.Client.RemoteEndPoint}");
                // 직렬화
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                // 바이트로 변경 후 네트워크에 쓰기
                stream.Write(responseData, 0, responseData.Length);
            }
        }
        // Serve 종료 시 client Close
        finally
        {
            Console.WriteLine($"Connection with {client.Client.RemoteEndPoint} has been closed");
            client.Close();
        }
    }

    public void Start() // 클라 연결 대기를 위한 loop문
    {
        Console.WriteLine("Server listening for incoming connections");

        try
        {
            while (true)
            {
                // 클라 Accept로 받아서 받은 client한테 Serve
                // 클라 접속 할 때까지 계속 기다림
                TcpClient client = Accept();
                // 1개의 클라이언트한테 밖에 Serve를 못함
                Serve(client);
            }
        }
        // 서버 종료시 Socket Stop
        finally
        {
            serverSocket.Stop();
            Console.WriteLine("\nServer stopped.");
        }
    }

    static void Main(string[] args) // Main문 서버 시작
    {
        Server server = new Server();
        server.Start();
    }
}
