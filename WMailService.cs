using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WMailService
{
    public partial class WMailService : Form
    {
        int readtime = 3000;
        int sendtime = 60000;
        string readapi = "";
        string sendapi = "";
        string completeapi = "";
        int dailyreport = 0;
        string dailyreportapi = "";
        bool readsending = false;
        bool sendstatus = true;
        string path = Application.StartupPath + @"\Log\";
        string LogFile="";
        List<MailList> maillist = new List<MailList>();
        TxtFileControl file = new TxtFileControl();
        bool ssl = true;
        static string today = "2018-04-19";
        string MsgId = "";

        private delegate void Serial_Data_Calback();
        public WMailService()
        {
            InitializeComponent();
        }

        private void WMailService_Load(object sender, EventArgs e)
        {
            readtime = ConfigurationManager.AppSettings["readtime"]==""?3000:int.Parse(ConfigurationManager.AppSettings["readtime"])*1000;
            sendtime = ConfigurationManager.AppSettings["sendtime"] == "" ? 60000 : int.Parse(ConfigurationManager.AppSettings["readtime"]) * 60000;
            readsending = ConfigurationManager.AppSettings["checksending"] == "" ? false : bool.Parse(ConfigurationManager.AppSettings["checksending"]);
            readapi = ConfigurationManager.AppSettings["getmailapi"];
            sendapi= ConfigurationManager.AppSettings["sendingmailapi"]; 
            completeapi= ConfigurationManager.AppSettings["sendedmailapi"];
            dailyreport= int.Parse(ConfigurationManager.AppSettings["dailyreport"]);
            dailyreportapi = ConfigurationManager.AppSettings["dailyreportapi"];
            ssl = bool.Parse(ConfigurationManager.AppSettings["ssl"]);
            MsgId = ConfigurationManager.AppSettings["MsgId"];
            MailHelper.InitMailServer();
            tbFrom.Text = MailHelper.MailAddress;
            tbTo.Focus();
            timer1.Enabled = true;
            timer1.Interval = readtime;
            timer2.Enabled = true;
            timer2.Interval = sendtime;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void btSendMail_Click(object sender, EventArgs e)
        {
            string mailfrom = tbFrom.Text;
            string mailto = tbTo.Text;
            string mailcc = tbcc.Text;
            string mailbcc = tbbcc.Text;
            string mailbody = mailBody.Text;
            string mailsubject = tbSubject.Text;
            if(!string.IsNullOrEmpty(mailto) && !string.IsNullOrEmpty(mailfrom)&&!string.IsNullOrEmpty(mailbody)&&!string.IsNullOrEmpty(mailsubject))
            {
                List<dynamic> list = new List<dynamic>();
                list.Add(new { to=mailto,cc=mailcc,bcc=mailbcc, subject = mailsubject, body = mailbody,file=""});
                dynamic sendlog = MailHelper.SendMail(list, ssl);
                if (sendlog.status)
                {
                    if (sendMailList.Items.Count > 20)
                    {
                        sendMailList.Items.RemoveAt(sendMailList.Items.Count-1);
                        sendMailList.Items.Insert(0, "from:" + mailfrom + "; to:" + mailto + "; cc:" + mailcc + "; bcc:" + mailbcc + "; subject:" + mailsubject + "; body:" + mailbody);
                    }
                    else
                    {
                        sendMailList.Items.Insert(0, "from:" + mailfrom + "; to:" + mailto + "; cc:" + mailcc + "; bcc:" + mailbcc + "; subject:" + mailsubject + "; body:" + mailbody);
                    }
                    tbTo.Text = "";
                    tbcc.Text = "";
                    tbbcc.Text = "";
                    mailBody.Text = "";
                    tbSubject.Text = "";
                    tbTo.Focus();
                }
                else
                {
                    file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：邮件发送失败:" + sendlog.message);
                }
            }
        }

        public void sendmail()
        {
            sendstatus = false;
            try
            {
                ThreadPool.QueueUserWorkItem(h =>
                {
                    Control.CheckForIllegalCrossThreadCalls = false;

                    Serial_Data_Calback sdc = new Serial_Data_Calback(like);
                    this.Invoke(sdc);
                    while (maillist.Count > 0)
                    {
                        MailList mail = maillist[0];
                        dynamic data = mail.Data;
                        string sendmessage = "from:" + MailHelper.MailAddress + "; to:" + data.MailTo + "; cc:" + data.CC + "; bcc:" + data.BCC + "; subject:" + data.Title + "; body:" + data.Body;
                        List<dynamic> list = new List<dynamic>();
                        list.Add(new { to = data.MailTo, cc = data.CC, bcc = data.BCC, subject = data.Title, body = data.Body, file = data.Source });
                        dynamic sendlog = MailHelper.SendMail(list, ssl);
                        if (sendlog.status)
                        {
                            var where = new { RowId = data.RowId };
                            HttpHelper.PostWebApi(completeapi, JsonConvert.SerializeObject(where), 18000);
                            if (sendMailList.Items.Count > 20)
                            {
                                sendMailList.Items.RemoveAt(sendMailList.Items.Count - 1);
                                sendMailList.Items.Insert(0, sendmessage);
                            }
                            else
                            {
                                sendMailList.Items.Insert(0, sendmessage);
                            }
                            file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + sendmessage);
                            maillist.Remove(mail);
                        }
                        else
                        {
                            file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：邮件发送失败：RowId" + mail.RowId + ":" + data.MailTo);
                            file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：邮件发送失败:" + sendlog.message);
                            if (sendMailList.Items.Count > 20)
                            {
                                sendMailList.Items.RemoveAt(sendMailList.Items.Count - 1);
                                sendMailList.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：邮件发送失败：RowId" + mail.RowId + ":" + data.MailTo);
                            }
                            else
                            {
                                sendMailList.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：邮件发送失败：RowId" + mail.RowId + ":" + data.MailTo);
                            }
                            maillist.Remove(mail);
                        }
                        Thread.Sleep(5000);
                    }
                    sendstatus = true;
                });
            }catch(Exception ex)
            {
                file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：数据错误：" + ex.Message);
                if (sendMailList.Items.Count > 20)
                {
                    sendMailList.Items.RemoveAt(sendMailList.Items.Count - 1);
                    sendMailList.Items.Insert(0, ex.Message);
                }
                else
                {
                    sendMailList.Items.Insert(0, ex.Message);
                }
                sendstatus = true;
            }
        }
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(DateTime.Now.Hour== dailyreport && today != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                dynamic result = HttpHelper.GetWebApi(dailyreportapi);
                today = DateTime.Now.ToString("yyyy-MM-dd");
            }
            try
            {
                LogFile = path + DateTime.Now.ToString("yyyyMMddHH") + ".log";
                if (!File.Exists(LogFile))
                {
                    file.newFile(LogFile);
                }
                string status = "?Status=-1";
                if (readsending)
                    status = "?Status=10";// &id="+MsgId;
                dynamic result = HttpHelper.GetWebApi(readapi + status);
                if (result != null)
                {
                    string rowid = result.RowId;
                    MailList mail = new MailList();
                    mail.RowId = rowid;mail.Data = result;
                    maillist.Insert(maillist.Count,mail);
                    var where = new { RowId = rowid };
                    HttpHelper.PostWebApi(sendapi, JsonConvert.SerializeObject(where), 18000);
                }
                if (sendstatus)
                    sendmail();
            }
            catch (Exception ex)
            {
                file.addLine(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：数据错误：" + ex.Message);
                sendMailList.Items.Insert(0, ex.Message);
            }
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sendstatus)
            {
                sendmail();
            }
        }
        public void like()
        {
            ;
        }
    }

    public class MailList
    {
        public string RowId { get; set; }
        public dynamic Data { get; set; }
    }
}
