using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;



//需要移植到windows RTX上
//增加发送数据队列与线程
//需要增强现有功能
//需要增加marlin固件下载功能

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public SerialPort serial = null;
        public bool connected = false;

        public ListNode filelist;
        public bool fileisread = false;
  

        public serial_op Serial_op;

        public processor Processor;

        const int initsendcolumns = 12;


        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //开辟接收缓冲区
            int BytesToRead = serial.BytesToRead;
            byte[] ReDatas = new byte[BytesToRead];
            //从串口读取数据
            serial.Read(ReDatas, 0, ReDatas.Length);

            if(Processor.Getprinting_state())
            {
                Serial_op.store_char(ReDatas, BytesToRead);
            }
        }

        public bool Connect()
        {
            try
            {
                serial = new SerialPort();
                string port = "COM16";
                serial.PortName = port;
                serial.BaudRate = int.Parse("250000");
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
 
 
                serial.Open();

                serial.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);

                connected = true;
                
  
                
            }
            catch (IOException)
            {
                serial = null;
            }
            catch (System.UnauthorizedAccessException)
            {
                serial = null;
            }

            return true;
        }

        public  bool Disconnect()
        {
            if (serial == null) return true;
            connected = false;


            try
            {
                if (serial != null)
                {
                    serial.Close();
                    serial.Dispose();
                }
            }
            catch (Exception) { }
            serial = null;
            return true;
        }
        delegate void SetTextCallback(string text);

        public void SetText(string text)
        {
            //如果调用控件的线程和创建创建控件的线程不是同一个则为True
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.label1.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }

        public Form1()
        {
            InitializeComponent();
            filelist = new ListNode();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(connected)
            {
                Disconnect();
                if(!connected)
                {
                    this.button1.Text = "openport";
                    Processor.quitprocess();
                }
            }
            else
            {
                Connect();
                if(connected)
                {
                    this.button1.Text = "closeport";
                    Serial_op = new serial_op();
                    Processor = new processor(Serial_op, filelist, serial,this);
                    Processor.Setprinting_state(false);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                try
                {
                    string SendData = "G28 X";
                    serial.WriteLine(SendData);
                }
                catch (Exception)
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开", "错误提示");
                return;
            }


        }

    
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0219)
            {
                if (m.WParam.ToInt32() == 0x8004)
                {
                    if (connected)
                    {
                        Disconnect();
                        if (!connected)
                        {
                            this.button1.Text = "openport";
                        }
                    }
                    //MessageBox.Show("USB转串口拔出");
                }
                else if(m.WParam.ToInt32() == 0x8000)
                {
                    //MessageBox.Show("USB转串口插入");
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connected)
            {
                Disconnect();
                if (!connected)
                {
                    this.button1.Text = "openport";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            filelist.Clear();

            int counter = 0;
            string line;

            try
            {
                // Read the file and display it line by line.
                System.IO.StreamReader file =
                   new System.IO.StreamReader("tmp\\test.gcode");
                while ((line = file.ReadLine()) != null)
                {
                    filelist.Append(line);
                    counter++;
                }

                filelist.Append("U12");
                file.Close();


                fileisread = true;

                this.button3.Text = "file readed";
            }
            catch(IOException)
            {

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(connected&&fileisread)
            {
                Processor.Setprinting_state(true);
                filelist.MoveFrist();
                try
                {
                    string SendData;

                    for (int i = 0; i < initsendcolumns; i++)//
                    {
                        SendData = filelist.GetCurrentValue();
                        if (SendData != null)
                        {
                            filelist.MoveNext();
                            serial.WriteLine(SendData);
                        }
                    }
                }
                catch (Exception)
                {
                    Processor.Setprinting_state(false);
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }

            }
        }
    }
}
