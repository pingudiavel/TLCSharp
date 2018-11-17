using Leonor.Library;
using Leonor.ORM;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TLCSharp.Bot
{
    public class jobs
    {
        static tl_account tmp_tlaccount = null;
        static tl_bot tmp_tlbot = null;
        static IBot tmp_bot = null;
        static IBot tmp_bot_read = null;
        static Type type = null;

        public static async Task<bool> start()
        {
            List<tl_account_bot> list_tl_account_bot = classes.account_model.get_listaccountbot();

            List<tl_account_bot> list_tl_account_bot_errors = list_tl_account_bot.Where(x => x.Errori > 0).ToList().OrderBy(x => x.Account).ToList();

            ISession nhSession = NHibernateSession.OpenSession();

            string Alertmessage = "!!!Errori Bot!!!" + Environment.NewLine;

            foreach (tl_account_bot tmp_tlaccountbot in list_tl_account_bot_errors)
            {
                tmp_tlaccount = nhSession.Load<tl_account>(tmp_tlaccountbot.Account);
                tmp_tlbot = nhSession.Load<tl_bot>(tmp_tlaccountbot.Bot);

                Alertmessage += Environment.NewLine;
                Alertmessage += "Username: " + tmp_tlaccount.NomeUtente + Environment.NewLine;
                Alertmessage += "Bot: " + tmp_tlbot.NomeUtente + Environment.NewLine;
                Alertmessage += "Numero Errori: " + tmp_tlaccountbot.Errori.ToString() + Environment.NewLine;
            }

            if (list_tl_account_bot_errors.Count > 1) { botutility.send_error(Alertmessage); }

            foreach (tl_account_bot tmp_tlaccountbot in list_tl_account_bot)
            {

                clearItems();

                try
                {

                    tmp_tlaccount = nhSession.Load<tl_account>(tmp_tlaccountbot.Account);
                    tmp_tlbot = nhSession.Load<tl_bot>(tmp_tlaccountbot.Bot);

                    type = botutility.get_TypefromNamespace(tmp_tlbot.NomeUtente);

                    List<Task> listTask = new List<Task>();
                    listTask.Add(work());

                    int startCountTask = listTask.Count;

                    while (listTask.Count == startCountTask)
                    {
                        Task firstFinishedTask = await Task.WhenAny(listTask);
                        listTask.Remove(firstFinishedTask);
                        await firstFinishedTask;
                    }

                }
                catch (Exception ex)
                {
                    string message = string.Empty;

                    message += "Errore Bot: " + type.Name + Environment.NewLine;
                    message += "Metodo: " + "Generico" + Environment.NewLine;
                    message += "Username: " + tmp_tlaccount.NomeUtente + Environment.NewLine;
                    message += "Eccezione: " + ex.Message + Environment.NewLine;

                    tmp_bot.send_error(message);
                }
                finally
                {
                    if (tmp_bot_read != null) { tmp_bot_read.stop_readloop(); }
                    await Task.Delay(10000);
                }

            }

            nhSession.Close();

            return true;
        }

        public static async Task<bool> work()
        {

            tmp_bot = ((IBot)Activator.CreateInstance(type, new[] { tmp_tlaccount.NumeroTelefono, tmp_tlaccount.NomeUtente, tmp_tlaccount.ApiId.ToString(), tmp_tlaccount.ApiHash }));

            await tmp_bot.configure();

            await tmp_bot.macro_bonus();
            await tmp_bot.macro_collect();
            await tmp_bot.macro_reinvest();
            tmp_bot.save_all();

            return true;
        }

        public static void clearItems()
        {

            tmp_tlaccount = null;
            tmp_tlbot = null;
            tmp_bot = null;
            tmp_bot_read = null;
            type = null;
        }

    }
}