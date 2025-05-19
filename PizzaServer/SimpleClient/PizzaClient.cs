using System.Net.Sockets;
using System.Text;

namespace _0_SimpleClient
{
    internal class PizzaClient
    {
        private const int BUFFER_SIZE = 1024;   // 데이터 송수신 최대 길이
        private const string SERVER_ADDRESS = "127.0.0.1"; //서버 IP 주소
        private const int SERVER_PORT = 12345; //소켓 포트

        static void Main()
        {
            try
            {   // 클라이언트 소켓 생성
                // IP 주소랑 Port 번호로 Tcp Client 생성
                using TcpClient client = new TcpClient(SERVER_ADDRESS, SERVER_PORT);
                // 네트워크로 받은 데이터 스트림
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Console.Write("How many pizzas do you want? ");
                    // 주문 받을 때 까지 Stop
                    string? order = Console.ReadLine();                     // 피자 주문 입력

                    // 주문이 Null이거나 없으면 클라이언트 종료
                    if (string.IsNullOrEmpty(order))
                        break;

                    // 주문을 byte로 바꿔서 네트워크에 전송
                    byte[] dataToSend = Encoding.UTF8.GetBytes(order);      // 바이트 변환
                    stream.Write(dataToSend, 0, dataToSend.Length);         // 데이터 전송

                    // 네트워크에서 받은 데이터 저장할 공간
                    byte[] buffer = new byte[BUFFER_SIZE];
                    // 네트워크에서 받은 데이터 읽기
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);    // 데이터 수신
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd();  // 바이트 변환

                    Console.WriteLine($"Server replied '{response}'");      // 주문 결과 출력
                }

                Console.WriteLine("Client closing");
            }
            // 연결 실패했을 때 띄우는 오류
            catch (SocketException ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }
    }
}
