using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Leonor.ORM;
using NHibernate;
using NHibernate.Criterion;


namespace Leonor.Library
{
    public static class botutility
    {

        static string Alert_ChatId = "10598112";
        static string Alert_botToken = "bot610814292:AAGafL9wnGbNcqJgOLZAv3GpkovAuqHiZ2k";

        public static Type get_TypefromNamespace(string value)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => x.Name == value).LastOrDefault();
        }

        public static void send_error(string message)
        {

            try
            {
                string sURL = "https://api.telegram.org/" + Alert_botToken + "/sendMessage?chat_id=" + Alert_ChatId + "&text=";
                sURL += System.Web.HttpUtility.UrlEncode(message);
                System.Net.WebClient webclient = new System.Net.WebClient();
                webclient.UploadString(sURL, "BootTelegram");
            }
            catch
            {

            }

        }

    }
}