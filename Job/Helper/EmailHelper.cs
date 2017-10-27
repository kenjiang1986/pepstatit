using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Job.DTO;

namespace Job.Helper
{
    public static class EmailHelper
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="email"></param>
        public static void SendEmail(Email email)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(email.FromAddress);
            mailMessage.To.Add(email.ToAddress);
            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Content;
            //添加附件
            if (email.MailAttachmentList != null)
            {
                foreach (var attachment in email.MailAttachmentList)
                {
                    mailMessage.Attachments.Add(attachment);
                }
            }
            
            //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
            SmtpClient client = new SmtpClient(email.SmtpService, 25);
            //设置发送人的邮箱账号和密码
            client.Credentials = new NetworkCredential(email.EmailAccount, email.EmailPassword);
            //启用ssl,也就是安全发送
            //client.EnableSsl = true;
            //发送邮件
            client.Send(mailMessage);
        }
    }
}
