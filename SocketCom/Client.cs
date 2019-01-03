using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
namespace SocketCom
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            textBox3.Text = "192.168.3.32";
        }
        Socket socket = null;
        static byte[] buffer = new byte[1024];
        private void button2_Click(object sender, EventArgs e)
        {
            Connect();
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = socket.EndReceive(ar);
                //读取出来消息内容
                var message = Encoding.Unicode.GetString(buffer, 0, length);
                //显示消息
                textBox1.AppendText(message+ "\n");

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.Message+ "\n");
                textBox1.AppendText("检测到服务器断开，3s后重连");
                Thread.Sleep(3000);
                Connect();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var outputBuffer = Encoding.Unicode.GetBytes(textBox2.Text);
            socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
        }

        public void Checkconnect()
        {
            try
            {
                if (socket.Poll(-1, SelectMode.SelectRead))
                {
                    textBox1.AppendText("检测到服务器断开，3s后重连");
                    Thread.Sleep(3000);
                    byte[] temp = new byte[1024];
                    int nRead = socket.Receive(temp);
                    if (nRead == 0)
                    {
                        Connect();
                    }
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.Message);
            }
        }

        public void Connect()
        {
            //创建一个Socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textBox3.Text.Trim());
            //连接到指定服务器的指定端口
            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connect.aspx
            //socket.Connect("localhost", 4530);
            socket.Connect(ip, 4530);
            textBox1.AppendText("connect to the server\n");
            //实现接受消息的方法

            //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginreceive.aspx
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
        }
    }
}
