using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.IO.Ports;
using System.IO;

namespace WindowsFormsApp1
{
  
    public class processor
    {
        Thread thread;
		bool quitthread ;
		bool isquit;

		serial_op Serial_op;

		ListNode filelist;

		SerialPort serial;

		Form1 mainForm;

		const int MAX_CMD_SIZE = 96;
		const int BUFSIZE = 8;

		int serial_count; // = 0;

		char []serial_line_buffer;
		bool serial_comment_mode;

		byte commands_in_queue, // Count of commands in the queue
		cmd_queue_index_r , // Ring buffer read (out) position
		cmd_queue_index_w; // Ring buffer write (in) position

		char [][]command_queue=new char[BUFSIZE][];

		bool isprinting;

		public processor(serial_op the_serial_op, ListNode the_filelist, SerialPort the_serial,Form1 the_mainForm)
        {
			for(int i=0;i<BUFSIZE;i++)
				command_queue[i] = new char[MAX_CMD_SIZE];

			serial_count = 0 ; // = 0;

			serial_line_buffer = new char[MAX_CMD_SIZE];
			serial_comment_mode = false;

			commands_in_queue = 0;// Count of commands in the queue
			cmd_queue_index_r = 0; // Ring buffer read (out) position
			cmd_queue_index_w = 0; // Ring buffer write (in) position

			Serial_op = the_serial_op;


			filelist= the_filelist;

			serial=the_serial;

			mainForm = the_mainForm;

			isprinting = false;

			quitthread = false;
			isquit = false;

			thread = new Thread(new ThreadStart(mainprocess));

			//只是 建议操作系统把当前线程当成 低级别
			//thread.Priority = ThreadPriority.Lowest;

			//给开发人员用，来识别不同系统
			thread.Name = "mainprocess";

			//后台线程：如果所有的前台线程都退出了，那么后台线程自动关闭
			thread.IsBackground = true;

			//并没有执行，告诉操作系统
			thread.Start();

			//关闭线程
			//thread.Abort();
		}

		public void quitprocess()
        {
			quitthread = true;
			while(!isquit) Thread.Sleep(1000);

			quitthread = false;
			isquit = false;

		}

		public void Setprinting_state(bool state)
        {
			isprinting = state;

		}


		public bool Getprinting_state()
		{
			return isprinting ;

		}


		public bool _enqueuecommand(char []cmd)
		{
			if (cmd[0] == ';' || commands_in_queue >= BUFSIZE) return false;
			string s = new string(cmd);
			command_queue[cmd_queue_index_w] = s.ToCharArray();
			if (++cmd_queue_index_w >= BUFSIZE) cmd_queue_index_w = 0;
			commands_in_queue++;
			return true;
		}

		public void get_serial_commands()
		{
			// If the command buffer is empty for too long,
			// send "wait" to indicate Marlin is still waiting.
			/**
			* Loop while serial characters are incoming and the queue is not full
			*/
			int c;
			while (commands_in_queue < BUFSIZE && (c = Serial_op.read()) >= 0)
			{
				char serial_char = (char)c;

				/**
				* If the character ends the line
				*/
				if (serial_char == '\n' || serial_char == '\r')
				{
					serial_comment_mode = false;                      // end of line == end of comment
																	  // Skip empty lines and comments
					if (serial_count==0) { continue; }

					serial_line_buffer[serial_count] = '0';             // Terminate string
					serial_count = 0;                                 // Reset buffer

					// Add the command to the queue
					_enqueuecommand(serial_line_buffer);
				}
				else if (serial_count >= MAX_CMD_SIZE - 1)
				{
					// Keep fetching, but ignore normal characters beyond the max length
					// The command will be injected when EOL is reached
				}
				else if (serial_char == '\\') // Handle escapes
				{
					if ((c = Serial_op.read()) >= 0 && !serial_comment_mode) // if we have one more character, copy it over
						serial_line_buffer[serial_count++] = (char)c;
					// otherwise do nothing
				}
				else  // it's not a newline, carriage return or escape char
				{
					if (serial_char == ';') serial_comment_mode = true;
					if (!serial_comment_mode) { serial_line_buffer[serial_count++] = serial_char; }
				}

			} // queue has space, serial has data
		}


		void process_next_command()
		{
		    string current_command = new string(command_queue[cmd_queue_index_r]);
			bool isContain1 = current_command.Contains("ok");
			bool isContain2 = current_command.Contains("ok RE U12 S1");

			if ((isContain1) && isprinting)
			{
				if (isContain2)
				{
					isprinting = false;
					mainForm.SetText("print finish");
					return;
				}
				string SendData;
				SendData = filelist.GetCurrentValue();
				if (SendData != null)
				{
					filelist.MoveNext();
					serial.WriteLine(SendData);
					mainForm.SetText(SendData);
				}
			}
		}


		void mainprocess()
        {
			while (!quitthread)//main process
			{
				get_serial_commands();

				if (commands_in_queue>0)
				{
					process_next_command();
					if (commands_in_queue>0)
					{
						--commands_in_queue;
						if (++cmd_queue_index_r >= BUFSIZE) cmd_queue_index_r = 0;
					}
				}
			}
			isquit = true;
		}


	}
}
