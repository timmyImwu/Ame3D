using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class ListNode   // 结点类
    {
        private ListNode Head;//头指针
        private ListNode Tail;//尾指针
        private ListNode Current;//当前指针
        private int ListCountValue;//链表数据的个数

        public ListNode()
        {
        }

        public ListNode(string s)
        {
            Value = String.Copy(s);
        }

        public ListNode Previous;      //前一个
        public ListNode Next;      //后一个
        public string Value;              //值





        public void Clist()
        {
            ListCountValue = 0;//初始化
            Head = null;
            Tail = null;
            Current = null;
        }


        public void Append(string s)//add in the end
        {
            ListNode NewNode = new ListNode(s);
            if (IsNull())//如果头指针为空
            {
                Head = NewNode;
                Tail = NewNode;
            }
            else
            {
                Tail.Next = NewNode;
                NewNode.Previous = Tail;
                Tail = NewNode;
            }
            Current = NewNode;
            ListCountValue += 1;//链表数据个数加一
        }

        public void Insert(string s)//add after current
        {
            ListNode NewNode = new ListNode(s);
            if (IsNull())
            {
                Append(s);//如果为空表，则添加
                return;
            }
            if (IsBof())
            {
                //为头部插入
                NewNode.Next = Head;
                Head.Previous = NewNode;
                Head = NewNode;
                Current = Head;
                ListCountValue += 1;
                return;
            }
            //中间插入
            NewNode.Next = Current;
            NewNode.Previous = Current.Previous;
            Current.Previous.Next = NewNode;
            Current.Previous = NewNode;
            Current = NewNode;
            ListCountValue += 1;
        }

        public void Delete()//delete current
        {
            if (!IsNull())//若为空链表
            {
                if (IsBof())//若删除头
                {
                    Head = Current.Next;
                    Current = Head;
                    ListCountValue -= 1;
                    return;
                }
                if (IsEof())//若删除尾
                {
                    Tail = Current.Previous;
                    Current = Tail;
                    ListCountValue -= 1;
                    return;
                }
                Current.Previous.Next = Current.Next;//若删除中间数据
                Current = Current.Previous;
                ListCountValue -= 1;
                return;
            }
        }

        public void MoveNext()
        {
            if (!IsEof()) Current = Current.Next;//向后移动一个数据
        }
        public void MovePrevious()
        {
            if (!IsBof()) Current = Current.Previous;//向前移动一个数据
        }
        public void MoveFrist()
        {
            Current = Head;//移动到第一个数据
        }
        public void MoveLast()
        {
            Current = Tail;//移动到最后一个数据
        }
        public bool IsNull()
        {
            if (ListCountValue == 0)//判断是否为空链表
                return true;
            return false;
        }
        public bool IsEof()
        {
            if (Current == Tail)//判断是否为到达尾
                return true;
            return false;
        }
        public bool IsBof()
        {
            if (Current == Head)//判断是否为到达头部
                return true;
            return false;
        }
        public string GetCurrentValue()
        {
            return Current.Value;//获取节点值
        }
        public int ListCount
        {
            get
            {
                return ListCountValue;//取得链表的数据个数
            }
        }
        //清空链表
        public void Clear()
        {
            MoveFrist();
            while (!IsNull())
            {
                Delete();//若不为空链表,从尾部删除
            }
        }

        

        





    }


}
