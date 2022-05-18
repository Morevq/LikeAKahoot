using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LikeAKahoot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int countClick = 0;
        // Класс-обработчик клиента
        class Client
        {
            // Отправка страницы с ошибкой
            private void SendHtmlJson(TcpClient Client, int Code)
            {
                // Получаем строку вида "200 OK"
                // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
                string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();

                //создаем json, что-то типо "{"status":"notfound","word":""}"
                string JsonStr = "{" + "countClick:" + countClick + "}";

                // Код простой HTML-странички
                string Html = "<html><head></head><body>" + JsonStr + " </body></html>";
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
                // Приведем строку к виду массива байт
                byte[] Buffer = Encoding.ASCII.GetBytes(Str);
                // Отправим его клиенту
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                // Закроем соединение
                Client.Close();
            }

            // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
            public Client(TcpClient Client)
            {
                SendHtmlJson(Client, 200);
            }
        }

        class Server
        {
            TcpListener Listener; // Объект, принимающий TCP-клиентов

            // Запуск сервера
            public Server(int Port)
            {
                // Создаем "слушателя" для указанного порта
                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start(); // Запускаем его

                // В бесконечном цикле
                while (true)
                {
                    // Принимаем нового клиента
                    TcpClient Client = Listener.AcceptTcpClient();
                    // Создаем поток
                    Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                    // И запускаем этот поток, передавая ему принятого клиента
                    Thread.Start(Client);
                }
            }

            static void ClientThread(Object StateInfo)
            {
                new Client((TcpClient)StateInfo);
            }

            // Остановка сервера
            ~Server()
            {
                // Если "слушатель" был создан
                if (Listener != null)
                {
                    // Остановим его
                    Listener.Stop();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            // Создадим новый сервер на порту 80
            new Server(80);
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            countClick++;
            tb.Text = countClick.ToString();
        }
    }
}
