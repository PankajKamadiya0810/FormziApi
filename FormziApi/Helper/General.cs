using FormziApi.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FormziApi.Helper
{
    public class General
    {
        public static string Encrypt(string password)
        {
            string strmsg = string.Empty;
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            strmsg = Convert.ToBase64String(encode);
            return strmsg;
        }

        public static string UniqueFileName()
        {
            return string.Format(@"{0}", Guid.NewGuid());
        }

        public static string CreatePassword()
        {
            int length = 6;
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string str = "";
            Random rnd = new Random();
            while (0 < length--)
                str += valid[rnd.Next(valid.Length)];
            return str;
        }

        public static string Decrypt(string encryptpwd)
        {
            string decryptpwd = string.Empty;
            UTF8Encoding encodepwd = new UTF8Encoding();
            Decoder Decode = encodepwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptpwd);
            int charCount = Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            decryptpwd = new String(decoded_char);
            return decryptpwd;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        //Formzi specific
        //Settings according to formzi (Ex. To email address, from email address)
        public static void SendEmail(string ToEmailAddress, string MailBody, string Subject)
        {
            try
            {
                string FromAddress = Convert.ToString(ConfigurationManager.AppSettings["RecipientFromEmail"]);

                int mintSmtpAuthenticate = 0;
                String mstrSendUserName = "";
                String mstrSendPassword = "";
                String mstrUseSSL = "";
                String mstrSMTPPort = "";
                String mstrSMTPServer = "";

                string strmintSmtpAuthenticate = Convert.ToString(ConfigurationManager.AppSettings["SmtpAuthenticate"]);
                mintSmtpAuthenticate = Convert.ToInt32(strmintSmtpAuthenticate);

                string strmstrSendUserName = Convert.ToString(ConfigurationManager.AppSettings["SenderUserName"]);
                mstrSendUserName = strmstrSendUserName;

                string strmstrSendPassword = Convert.ToString(ConfigurationManager.AppSettings["SenderPassword"]);
                mstrSendPassword = strmstrSendPassword;

                string strmstrUseSSL = Convert.ToString(ConfigurationManager.AppSettings["UseSSL"]);
                mstrUseSSL = strmstrUseSSL;

                string strmstrSMTPPort = Convert.ToString(ConfigurationManager.AppSettings["SMTPPort"]);
                mstrSMTPPort = strmstrSMTPPort;

                string strmstrSMTPServer = Convert.ToString(ConfigurationManager.AppSettings["SmtpServer"]);
                mstrSMTPServer = strmstrSMTPServer;

                System.Net.Mail.MailMessage lobjMail = new System.Net.Mail.MailMessage();
                SmtpClient sc = new System.Net.Mail.SmtpClient();
                System.Net.NetworkCredential auth = new System.Net.NetworkCredential(mstrSendUserName, mstrSendPassword);

                lobjMail.Body = MailBody;
                lobjMail.Subject = Subject;
                lobjMail.From = new MailAddress(FromAddress, "");
                //Added By Hiren
                string[] ToMuliId = ToEmailAddress.Split(',');
                foreach (string ToEMailId in ToMuliId)
                {
                    lobjMail.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id
                }
                //End

                // lobjMail.To.Add(new MailAddress(ToEmailAddress));

                if (!string.IsNullOrEmpty(strmstrSMTPServer))
                {
                    sc.Host = mstrSMTPServer;
                }
                sc.Port = Convert.ToInt32(mstrSMTPPort);
                lobjMail.IsBodyHtml = true;
                sc.UseDefaultCredentials = false;
                sc.Credentials = auth;
                sc.EnableSsl = ConfigurationManager.AppSettings["UseSSL"] == "0" ? false : true;
                sc.Send(lobjMail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Added by jay on 12-feb-2016
        //I-Witness specific
        public static void InnfySendInquiryEmail(string ToEmailAddress, string MailBody, string Subject)
        {
            try
            {
                string FromAddress = Convert.ToString(ConfigurationManager.AppSettings["InnfyRecipientFromEmail"]);

                int mintSmtpAuthenticate = 0;
                String mstrSendUserName = "";
                String mstrSendPassword = "";
                String mstrUseSSL = "";
                String mstrSMTPPort = "";
                String mstrSMTPServer = "";

                string strmintSmtpAuthenticate = Convert.ToString(ConfigurationManager.AppSettings["SmtpAuthenticate"]);
                mintSmtpAuthenticate = Convert.ToInt32(strmintSmtpAuthenticate);

                string strmstrSendUserName = Convert.ToString(ConfigurationManager.AppSettings["InnfySenderUserName"]);
                mstrSendUserName = strmstrSendUserName;

                string strmstrSendPassword = Convert.ToString(ConfigurationManager.AppSettings["InnfySenderPassword"]);
                mstrSendPassword = strmstrSendPassword;

                string strmstrUseSSL = Convert.ToString(ConfigurationManager.AppSettings["UseSSL"]);
                mstrUseSSL = strmstrUseSSL;

                string strmstrSMTPPort = Convert.ToString(ConfigurationManager.AppSettings["SMTPPort"]);
                mstrSMTPPort = strmstrSMTPPort;

                string strmstrSMTPServer = Convert.ToString(ConfigurationManager.AppSettings["SmtpServer"]);
                mstrSMTPServer = strmstrSMTPServer;

                System.Net.Mail.MailMessage lobjMail = new System.Net.Mail.MailMessage();
                SmtpClient sc = new System.Net.Mail.SmtpClient();
                System.Net.NetworkCredential auth = new System.Net.NetworkCredential(mstrSendUserName, mstrSendPassword);

                lobjMail.Body = MailBody;
                lobjMail.Subject = Subject;
                lobjMail.From = new MailAddress(FromAddress, "");
                lobjMail.To.Add(new MailAddress(ToEmailAddress));

                if (!string.IsNullOrEmpty(strmstrSMTPServer))
                {
                    sc.Host = mstrSMTPServer;
                }
                sc.Port = Convert.ToInt32(mstrSMTPPort);
                lobjMail.IsBodyHtml = true;
                sc.UseDefaultCredentials = false;
                sc.Credentials = auth;
                sc.EnableSsl = ConfigurationManager.AppSettings["UseSSL"] == "0" ? false : true;
                sc.Send(lobjMail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //added by jay
        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        #region Access Code Generator

        public static string GenerateRandomNumeric(int length)
        {
            var chars = "0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        public static string GenerateAccessCode(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        #endregion

        public static int GetAge(DateTime reference, DateTime birthday)
        {
            int age = reference.Year - birthday.Year;
            if (reference < birthday.AddYears(age)) age--;

            return age;
        }

        public static int CalculateAge(DateTime BirthDate)
        {
            int YearsPassed = DateTime.Now.Year - BirthDate.Year;
            // Are we before the birth date this year? If so subtract one year from the mix
            if (DateTime.Now.Month < BirthDate.Month || (DateTime.Now.Month == BirthDate.Month && DateTime.Now.Day < BirthDate.Day))
            {
                YearsPassed--;
            }
            return YearsPassed;
        }

        public static DateTime ConvertToDateTime(string strDateTime)
        {
            DateTime dtFinaldate; string sDateTime;
            try
            {
                if (strDateTime.Contains('/'))
                {
                    string[] sDate = strDateTime.Split('/');
                    sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];

                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
                else if (strDateTime.Contains('-'))
                {
                    string[] sDate = strDateTime.Split('-');
                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
                else
                {
                    string[] sDate = strDateTime.Split('/');
                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
                return dtFinaldate;
            }
            catch (Exception)
            {
                if (strDateTime.Contains('/'))
                {
                    string[] sDate = strDateTime.Split('/');
                    sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];

                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
                else if (strDateTime.Contains('-'))
                {
                    string[] sDate = strDateTime.Split('/');
                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
                else
                {
                    string[] sDate = strDateTime.Split('/');
                    sDateTime = sDate[2] + '/' + sDate[1] + '/' + sDate[0];
                    dtFinaldate = Convert.ToDateTime(sDateTime);
                }
            }
            return dtFinaldate;

        }

        public void A1()
        {
            System.Diagnostics.Debug.WriteLine("A1------------------------------------------");
        }

        public static string GenerateQueryAreaChart(FilterRule query)
        {
            try
            {
                if (query == null) return string.Empty;

                string queryStr = string.Empty;

                if (query.Rules.Count == 0) return queryStr;

                foreach (var model in query.Rules)
                {
                    //If string is NOT null then add operator
                    if (!string.IsNullOrEmpty(queryStr))
                    {
                        //queryStr += " " + query.Condition + " ";
                    }

                    if (model.Operator == "equal")
                    {
                        queryStr += "Pvt.[" + model.Id + "] = '" + model.Value + "'";
                    }
                    else if (model.Operator == "not_equal")
                    {
                        queryStr += "Pvt.[" + model.Id + "] != '" + model.Value + "'";
                    }
                    else if (model.Operator == "in")
                    {
                        queryStr += "Pvt.[" + model.Id + "] IN (" + model.Value + ")";
                    }
                    else if (model.Operator == "not_in")
                    {
                        queryStr += "Pvt.[" + model.Id + "] NOT IN (" + model.Value + ")";
                    }
                    else if (model.Operator == "contains")
                    {
                        queryStr += "Pvt.[" + model.Id + "] Like '%" + model.Value + "%'";
                    }
                    else if (model.Operator == "not_contains")
                    {
                        queryStr += "Pvt.[" + model.Id + "] NOT Like '%" + model.Value + "%'";
                    }
                    else if (model.Operator == "begins_with")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  LIKE '" + model.Value + "%'";
                    }
                    else if (model.Operator == "not_begins_with")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  NOT LIKE '" + model.Value + "%'";
                    }
                    else if (model.Operator == "ends_with")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  LIKE '%" + model.Value + "'";
                    }
                    else if (model.Operator == "not_ends_with")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  NOT LIKE '%" + model.Value + "'";
                    }
                    else if (model.Operator == "is_empty")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  = ''";
                    }
                    else if (model.Operator == "is_not_empty")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  != ''";
                    }
                    else if (model.Operator == "is_null")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  IS NULL";
                    }
                    else if (model.Operator == "is_not_null")
                    {
                        queryStr += "Pvt.[" + model.Id + "]  IS NOT NULL";
                    }
                }

                return " AND (" + queryStr + ")";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string QuestionList(Query query)
        {
            try
            {
                List<string> questionIds = new List<string>();

                string queryStr = string.Empty;

                foreach (var model in query.condition_objects)
                {
                    //If string is NOT null then add operator
                    if (!string.IsNullOrEmpty(queryStr))
                    {
                        queryStr += " " + query.@operator + " ";
                    }

                    if (model.Rule == "equal")
                    {
                        queryStr += "Pvt.[" + model.Question_id + "] = ''" + model.Answer + "''";

                        if (!questionIds.Any(i => i == model.Question_id))
                        {
                            questionIds.Add(model.Question_id);
                        }
                    }
                    else if (model.Rule == "not equal")
                    {
                        queryStr += "Pvt.[" + model.Question_id + "] != ''" + model.Answer + "''";

                        if (!questionIds.Any(i => i == model.Question_id))
                        {
                            questionIds.Add(model.Question_id);
                        }
                    }
                    else if (model.Rule == "not equal")
                    {
                        queryStr += "Pvt.[" + model.Question_id + "] != ''" + model.Answer + "''";
                        if (!questionIds.Any(i => i == model.Question_id))
                        {
                            questionIds.Add(model.Question_id);
                        }
                    }
                    else if (model.Rule == "is empty or null")
                    {
                        queryStr += "Pvt.[" + model.Question_id + "] = ''" + " OR " + model.Question_id + "null" + model.Answer;
                        if (!questionIds.Any(i => i == model.Question_id))
                        {
                            questionIds.Add(model.Question_id);
                        }
                    }
                    else if (model.Rule == "contain")
                    {
                        queryStr += "Pvt.[" + model.Question_id + "] Like ''%" + model.Answer + "%''";
                        if (!questionIds.Any(i => i == model.Question_id))
                        {
                            questionIds.Add(model.Question_id);
                        }
                    }
                }

                return "AND (" + queryStr + ")";
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetMimeType(string extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            if (extension.StartsWith("."))
                extension = extension.Substring(1);


            switch (extension.ToLower())
            {
                #region Big freaking list of mime types

                case "art": return Constants.ImageFolder;
                case "bmp": return Constants.ImageFolder;
                case "cmx": return Constants.ImageFolder;
                case "cod": return Constants.ImageFolder;
                case "gif": return Constants.ImageFolder;
                case "dib": return Constants.ImageFolder;
                case "ico": return Constants.ImageFolder;
                case "ief": return Constants.ImageFolder;
                case "jfif": return Constants.ImageFolder;
                case "jpe": return Constants.ImageFolder;
                case "jpeg": return Constants.ImageFolder;
                case "jpg": return Constants.ImageFolder;
                case "pic": return Constants.ImageFolder;
                case "mac": return Constants.ImageFolder;
                case "pbm": return Constants.ImageFolder;
                case "pct": return Constants.ImageFolder;
                case "pgm": return Constants.ImageFolder;
                case "pict": return Constants.ImageFolder;
                case "png": return Constants.ImageFolder;
                case "pnm": return Constants.ImageFolder;
                case "pnz": return Constants.ImageFolder;
                case "pnt": return Constants.ImageFolder;
                case "pntg": return Constants.ImageFolder;
                case "ppm": return Constants.ImageFolder;
                case "rgb": return Constants.ImageFolder;
                case "qti": return Constants.ImageFolder;
                case "qtif": return Constants.ImageFolder;
                case "rf": return Constants.ImageFolder;
                case "wbmp": return Constants.ImageFolder;
                case "wdp": return Constants.ImageFolder;
                case "3g2": return Constants.VideoFolder;
                case "3gp": return Constants.VideoFolder;
                case "3gp2": return Constants.VideoFolder;
                case "3gpp": return Constants.VideoFolder;
                case "avi": return Constants.VideoFolder;
                case "flv": return Constants.VideoFolder;
                case "m4v": return Constants.VideoFolder;
                case "mod": return Constants.VideoFolder;
                case "movie": return Constants.VideoFolder;
                case "mp2": return Constants.VideoFolder;
                case "mp2v": return Constants.VideoFolder;
                case "mp4": return Constants.VideoFolder;
                case "mp4v": return Constants.VideoFolder;
                case "mpa": return Constants.VideoFolder;
                case "mpe": return Constants.VideoFolder;
                case "mpeg": return Constants.VideoFolder;
                case "mpg": return Constants.VideoFolder;
                case "mpv2": return Constants.VideoFolder;
                case "mqv": return Constants.VideoFolder;
                case "asf": return Constants.VideoFolder;
                case "asr": return Constants.VideoFolder;
                case "asx": return Constants.VideoFolder;
                case "dif": return Constants.VideoFolder;
                case "dv": return Constants.VideoFolder;
                case "IVF": return Constants.VideoFolder;
                case "lsf": return Constants.VideoFolder;
                case "lsx": return Constants.VideoFolder;
                case "m1v": return Constants.VideoFolder;
                case "m2t": return Constants.VideoFolder;
                case "m2ts": return Constants.VideoFolder;
                case "m2v": return Constants.VideoFolder;
                case "aa": return Constants.AudioFolder;
                case "aac": return Constants.AudioFolder;
                case "aax": return Constants.AudioFolder;
                case "ac3": return Constants.AudioFolder;
                case "adt": return Constants.AudioFolder;
                case "adts": return Constants.AudioFolder;
                case "aif": return Constants.AudioFolder;
                case "aifc": return Constants.AudioFolder;
                case "aiff": return Constants.AudioFolder;
                case "au": return Constants.AudioFolder;
                case "caf": return Constants.AudioFolder;
                case "cdda": return Constants.AudioFolder;
                case "gsm": return Constants.AudioFolder;
                case "m3u": return Constants.AudioFolder;
                case "m3u8": return Constants.AudioFolder;
                case "m4a": return Constants.AudioFolder;
                case "m4b": return Constants.AudioFolder;
                case "m4p": return Constants.AudioFolder;
                case "m4r": return Constants.AudioFolder;
                case "mid": return Constants.AudioFolder;
                case "midi": return Constants.AudioFolder;
                case "mp3": return Constants.AudioFolder;
                case "pls": return Constants.AudioFolder;
                case "ra": return Constants.AudioFolder;
                case "ram": return Constants.AudioFolder;
                case "rmi": return Constants.AudioFolder;
                case "rpm": return Constants.AudioFolder;
                case "sd2": return Constants.AudioFolder;
                case "smd": return Constants.AudioFolder;
                case "smx": return Constants.AudioFolder;
                case "smz": return Constants.AudioFolder;
                case "snd": return Constants.AudioFolder;
                case "wav": return Constants.AudioFolder;
                case "wave": return Constants.AudioFolder;
                case "wax": return Constants.AudioFolder;
                case "wma": return Constants.AudioFolder;

                #endregion

                default: return Constants.ImageFolder;
            }
        }
    }
}