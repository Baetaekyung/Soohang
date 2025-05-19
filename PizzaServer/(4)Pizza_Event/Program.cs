using _4_Pizza_Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_Pizza_Event
{
    internal class Program
    {
        static void Main()
        {
            // 루프 생성하고 서버 초기화 후 시작
            EventLoop eventLoop = new EventLoop();
            EventServer server = new EventServer(eventLoop);
            server.Start();
            eventLoop.RunForever();
        }
    }
}