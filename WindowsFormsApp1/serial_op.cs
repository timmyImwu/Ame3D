using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public struct ring_buffer
    {
        public byte []buffer;
        public Int32 head;
        public Int32 tail;
    };
    public class serial_op
    {
        public ring_buffer rx_buffer;

        public serial_op()
        {
            rx_buffer.buffer = new byte[1024];
            flush();
        }

        public void store_char(byte c)
        {
            int i = ((rx_buffer.head + 1) % 1024);

            // if we should be storing the received character into the location
            // just before the tail (meaning that the head would advance to the
            // current location of the tail), we're about to overflow the buffer
            // and so we don't write the character or advance the head.
            if (i != rx_buffer.tail)
            {
                rx_buffer.buffer[rx_buffer.head] = c;
                rx_buffer.head = i;
            }
        }

        public void store_char(byte[] thearray, int num)
        {
            for (int i = 0; i < num; i++)
            {
                rx_buffer.buffer[rx_buffer.head] = thearray[i];
                rx_buffer.head = (rx_buffer.head + 1) % 1024;
            }
        }

        public int empty()
        {
            return (1024 - 1 - rx_buffer.head + rx_buffer.tail) % 1024;
        }

        public int peek()
        {
            if (rx_buffer.head == rx_buffer.tail)
            {
                return -1;
            }
            else
            {
                return rx_buffer.buffer[rx_buffer.tail];
            }
        }

        public int available()
        {
            return (1024 + rx_buffer.head - rx_buffer.tail) % 1024;
        }

        public int read()
        {
            int ret;
            // if the head isn't ahead of the tail, we don't have any characters
            if (rx_buffer.head == rx_buffer.tail)
            {
                ret = -1;
            }
            else
            {
                byte c = rx_buffer.buffer[rx_buffer.tail];
                rx_buffer.tail = (rx_buffer.tail + 1) % 1024;
                ret = c;
            }


            return ret;
        }

        public void flush()
        {
            rx_buffer.head = rx_buffer.tail = 0;
        }
    }
}
