using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace WMailService
{
    public static class MailHelper
    {
        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        public static string Smtp { get; set; }
        /// <summary>
        /// SMTP服务器端口号
        /// </summary>
        public static int SmtpPort { get; set; }
        /// <summary>
        /// IMAP服务器地址
        /// </summary>
        public static string Imap { get; set; }
        /// <summary>
        /// IMAP服务器地址端口号
        /// </summary>
        public static int ImapPort { get; set; }
        /// <summary>
        /// 系统发邮件地址
        /// </summary>
        public static string MailAddress { get; set; }
        /// <summary>
        /// 邮件密码
        /// </summary>
        public static string MailPassWord { get; set; }
        /// <summary>
        /// 设置邮件服务器
        /// </summary>
        /// <param name="Smtp">SMTP服务器地址</param>
        /// <param name="SmtpPort">SMTP服务器端口号</param>
        public static void SetServer(string smtp, int smtpport)
        {
            Smtp = smtp;
            SmtpPort = smtpport;
        }

        /// <summary>
        /// 设置邮件服务器
        /// </summary>
        /// <param name="Smtp">SMTP服务器地址</param>
        /// <param name="SmtpPort">SMTP服务器端口号</param>
        /// <param name="Imap">IMAP服务器地址</param>
        /// <param name="ImapPort">IMAP服务器地址端口号</param>
        public static void SetServer(string smtp,int smtpport,string imap,int imapport)
        {
            Smtp = smtp;
            SmtpPort = smtpport;
            Imap = imap;
            ImapPort = imapport;
        }
        /// <summary>
        /// 设置发件人地址
        /// </summary>
        /// <param name="mailaddress">发件人地址</param>
        public static void SetMailAddress(string mailaddress,string password)
        {
            MailAddress = mailaddress;
            MailPassWord = password;
        }
        /// <summary>
        /// 设置邮件服务器地址，默认从客户服务器配置初始化，若客户未配置则通过服务器配置文件初始化
        /// </summary>
        public static void InitMailServer()
        {
            Smtp = ConfigurationManager.AppSettings["smtp"];
            SmtpPort= int.Parse(ConfigurationManager.AppSettings["smtpport"]);
            Imap = ConfigurationManager.AppSettings["imap"];
            ImapPort = int.Parse(ConfigurationManager.AppSettings["imapport"]);
            MailAddress = ConfigurationManager.AppSettings["mailaddress"];
            MailPassWord = ConfigurationManager.AppSettings["mailpassword"];
        }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="args">邮件信息：JSON数组。如：[{"to":"收件人地址列表","cc":"抄送人地址列表","bcc":"密送人地址列表","subject":"邮件标题","body":"邮件内容","file":"附件地址"},……]</param>
        public static dynamic SendMail( dynamic args,bool sl=false,int timeout=0)
        {
            SmtpClient client = new SmtpClient();
            client.Host = Smtp;
            try
            {
                client.Port = SmtpPort;
                if (SmtpPort == 465)
                    sl = true;
            }catch(Exception ex)
            {
                client.Port = 25;
            }
            client.EnableSsl = sl;
            //client.Timeout = 18000;//timeout;

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            //验证发件人凭证
            client.Credentials = new System.Net.NetworkCredential(MailAddress, MailPassWord);
            MailMessage mm;
            foreach (dynamic item in args)
            {
                mm = new MailMessage();
                mm.From = new MailAddress(MailAddress);
                string[] to = null ;
                string[] cc=null;
                string[] bcc=null;
                string subject="";
                string body="";
                string[] SUpFile=null;
                try
                {
                    Type type = item.GetType(); 
                    if(type.Name=="JObject")
                    {
                        subject = item["subject"];
                        body = item["body"];
                        //设置发件人地址
                        to = item["to"].ToString().Split(',');
                        //设置抄送人地址
                        cc = item["cc"].ToString().Split(',');
                        //设置密送人地址
                        bcc = item["bcc"].ToString().Split(',');
                        //设置待发送文件
                        SUpFile = item["file"].ToString().Split(',');
                    }
                    else
                    {
                        subject = item.subject;
                        body = item.body;
                        //设置发件人地址
                        to = item.to.ToString().Split(',');  
                        //设置抄送人地址
                        cc = item.cc.ToString().Split(',');
                        //设置密送人地址
                        bcc = item.bcc.ToString().Split(',');
                        SUpFile = item.file.ToString().Split(',');
                    }
                    //设置收件人地址
                    foreach (string itemto in to)
                    {
                        if(!string.IsNullOrEmpty(itemto))
                            mm.To.Add(itemto);
                    }
                    //设置抄送人
                    foreach (string itemcc in cc)
                    {
                        if (!string.IsNullOrEmpty(itemcc))
                            mm.CC.Add(itemcc);
                    }
                    //设置密送人
                    foreach (string itembcc in bcc)
                    {
                        if (!string.IsNullOrEmpty(itembcc))
                            mm.Bcc.Add(itembcc);
                    }
                    foreach(string file in SUpFile)
                    {
                        if (string.IsNullOrEmpty(file))
                            continue;
                        //将文件进行转换成Attachments
                        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                        // Add time stamp information for the file.
                        ContentDisposition disposition = data.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(file);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

                        mm.Attachments.Add(data);
                        System.Net.Mime.ContentType ctype = new System.Net.Mime.ContentType();
                    }
                    mm.Subject = subject;
                    mm.Body = body;
                    mm.BodyEncoding = UTF8Encoding.UTF8;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    client.Send(mm);
                }catch(Exception ex)
                {
                    return new { status = false, message = ex.Message };
                }
            }
            return new { status = true, message = "" };
        }
    }
}