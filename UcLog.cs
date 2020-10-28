using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTUc
{
    /// <summary>
    /// 日志控件
    /// </summary>
    public partial class UcLog : UserControl
    {
        private bool IsStop = false;
        private UInt16 _logQueueSize;

        private ConcurrentQueue<LogModel> _logQueue;

        private Dictionary<string, int> _logIntDic = new Dictionary<string, int>();

        public UcLog()
        {
            InitializeComponent();
        }


        public void Init(UInt16 logQueueSize = 500, int index = 0)
        {
            if (logQueueSize == 0)
            {
                throw new Exception("日志队列不能设置为0");
            }
            IsStop = false;
            _logQueueSize = logQueueSize;
            _logQueue = new ConcurrentQueue<LogModel>();
            _logIntDic.Add("全部", 0);
            _logIntDic.Add("调试", 1);
            _logIntDic.Add("信息", 2);
            _logIntDic.Add("警告", 3);
            _logIntDic.Add("错误", 4);
            comboBoxEdit1.SelectedIndex = index;
        }

        public void UnInit()
        {
            IsStop = true;
        }
        private void UcLog_Load(object sender, EventArgs e)
        {

            Task.Factory.StartNew(GetMsg);
        }

        private void GetMsg()
        {
            while (!IsStop)
            {
                if (_logQueue != null && _logQueue.Count > 0)
                {
                    var item = new LogModel();
                    if (_logQueue.TryDequeue(out item))
                    {
                        UpdateUi(item);
                    }
                }
                Thread.Sleep(50);
            }
        }

        public void AddMsg(ELogLevel logLevel, string msg)
        {
            var filterStr = txtFiltter.Text;
            var filterStr2 = txtFiltter2.Text;
            var filterLevel = comboBoxEdit1.Text;//默认全部
            var levelStr = "全部";

            var color = SystemColors.WindowText;
            switch (logLevel)
            {
                case ELogLevel.DEBUG:
                    levelStr = "调试";
                    color = Color.Black;
                    break;
                case ELogLevel.INFO:
                    levelStr = "信息";
                    color = SystemColors.WindowText;
                    break;
                case ELogLevel.WARN:
                    levelStr = "警告";
                    color = Color.DarkGoldenrod;
                    break;
                case ELogLevel.ERROR:
                    levelStr = "错误";
                    color = Color.Red;
                    break;
                default:
                    break;
            }
            var s = String.Format("[{0}][{1}]:{2}", levelStr, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), msg);
            if (!checkEdit1.Checked)
            {
                if (_logIntDic[filterLevel] <= _logIntDic[levelStr])
                {
                    if (_logQueue.Count < _logQueueSize)
                    {
                        if (s.Contains(filterStr) && s.Contains(filterStr2))
                        {
                            _logQueue.Enqueue(new LogModel() { MColor = color, MMsg = s });
                            Thread.Sleep(1);
                        }

                    }
                    else
                    {

                    }

                }
            }


        }

        private void UpdateUi(LogModel item)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new MethodInvoker(delegate
                {
                    if (richTextBox1.Lines.Length > _logQueueSize)
                    {
                        richTextBox1.ResetText();
                    }
                    AppendTextColorful(richTextBox1, item.MMsg, item.MColor);
                }));
            }
            else
            {
                if (richTextBox1.Lines.Length > _logQueueSize)
                {
                    richTextBox1.ResetText();
                }
                AppendTextColorful(richTextBox1, item.MMsg, item.MColor);
            }

        }

        private void AppendTextColorful(RichTextBox rtBox, string text, Color color, bool addNewLine = true)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            rtBox.AppendText(text);
            rtBox.SelectionColor = rtBox.ForeColor;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.richTextBox1.ResetText();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length; //Set the current caret position at the end
            richTextBox1.ScrollToCaret(); //Now scroll it automatically
        }

    }

    public class LogModel
    {
        public Color MColor { get; set; }
        public string MMsg { get; set; }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum ELogLevel
    {
        /// <summary>
        /// 调试
        /// </summary>
        DEBUG,
        /// <summary>
        /// 信息
        /// </summary>
        INFO,
        /// <summary>
        /// 警告
        /// </summary>
        WARN,
        /// <summary>
        /// 错误
        /// </summary>
        ERROR
    }
}
