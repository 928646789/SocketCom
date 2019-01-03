using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Net;

namespace SocketServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        Socket socket = null;
        Dictionary<string, Socket> SocketList = new Dictionary<string, Socket>();
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //创建一个新的Socket,这里我们使用最常用的基于TCP的Stream Socket（流式套接字）
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //将该socket绑定到主机上面的某个端口
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.bind.aspx
                socket.Bind(new IPEndPoint(IPAddress.Any, 4530));

                //启动监听，并且设置一个最大的队列长度
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.listen(v=VS.100).aspx
                socket.Listen(20);

                //开始接受客户端连接请求
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginaccept.aspx
                socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
                
                MessageBox.Show("服务器创建成功！");
                button1.Enabled = false;
                button1.Text = "接收中...";

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            
        }

        static byte[] buffer = new byte[1024];
        public void ClientAccepted(IAsyncResult ar)
        {

            var socket = ar.AsyncState as Socket;

            //这就是客户端的Socket实例，我们后续可以将其保存起来
            var client = socket.EndAccept(ar);
            string clientIP = client.RemoteEndPoint.ToString();
            listBox1.Items.Add(clientIP);
            SocketList.Add(clientIP, client);
            //给客户端发送一个欢迎消息
            client.Send(Encoding.Unicode.GetBytes("Server had accepted the request at " + DateTime.Now.ToString()));

            //接收客户端的消息(这个和在客户端实现的方式是一样的）
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
            
            //准备接受下一个客户端请求
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }



        public void ReceiveMessage(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;
            string remoteEndPoint = socket.RemoteEndPoint.ToString();
            try
            {            
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = socket.EndReceive(ar);
                //读取出来消息内容
                var message = Encoding.Unicode.GetString(buffer, 0, length);
                //显示消息
                textBox1.AppendText(remoteEndPoint+":"+message + "\n");

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                textBox1.AppendText(remoteEndPoint+ex.Message);
                listBox1.Items.Remove(remoteEndPoint);
                SocketList.Remove(remoteEndPoint);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SocketList.ContainsKey(listBox1.SelectedItem.ToString()))
            {
                Socket Socket = SocketList[listBox1.SelectedItem.ToString()];
                Socket.Send(Encoding.Unicode.GetBytes("Server:"+textBox2.Text));
            }
            else
                MessageBox.Show("error ip address");
            
        }
    }
}
