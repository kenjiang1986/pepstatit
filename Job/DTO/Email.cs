using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;


namespace Job.DTO
{
    public class Email
    {
        public string EmailAccount { get; set; }

        public string EmailPassword { get; set; }

        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }

        public string SmtpService { get; set; }

        public IList<System.Net.Mail.Attachment> MailAttachmentList { get; set; }
    }
}
