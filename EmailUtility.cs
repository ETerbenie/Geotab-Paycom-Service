using PTC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GTPC.Service.Implementation.Implementation
{
    public class EmailUtility
    {
        public void SendEmailWithCsvAttached()
        {
            string fromEmail = AppSettings.GetStringValue("fromEmail");
            string toEmail = AppSettings.GetStringValue("toEmail");
            string subjectLine = AppSettings.GetStringValue("subjectLine");
            string mailBody = AppSettings.GetStringValue("mailBody");
            string smtpClient = AppSettings.GetStringValue("smtpClient");
            string networkUsername = AppSettings.GetStringValue("mailNetworkUsername");
            string netWorkPassword = AppSettings.GetStringValue("mailNetWorkPassword");

            Log.MethodStart();

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    SmtpClient mailServer = new SmtpClient(smtpClient);
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(toEmail);
                    mail.Subject = subjectLine;
                    mail.Body = mailBody;

                    System.Net.Mail.Attachment csv;
                    csv = new System.Net.Mail.Attachment(@"C:\PunchReport.csv");
                    mail.Attachments.Add(csv);

                    mailServer.Port = 587;
                    mailServer.Credentials = new System.Net.NetworkCredential(networkUsername, netWorkPassword);
                    mailServer.EnableSsl = true;

                    Log.Info($"Sending Report to {toEmail}");
                    mailServer.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }
    }
}
