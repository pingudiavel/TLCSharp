using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

using Leonor.ORM;
using Leonor.Library;

using NHibernate;
using NHibernate.Criterion;

namespace TLCSharp.Bot
{
    public abstract class Bot : IBot
    {

        protected string PhoneNumber = string.Empty;
        protected string UserName = string.Empty;
        protected int ApiId = 0;
        protected string ApiHash = string.Empty;

        protected string BotName = string.Empty;

        protected TelegramClientExtended client = null;
        protected TLInputPeerUser peer = null;

        protected IBalance botbalance_start = null;
        protected IBalance botbalance_current = null;

        protected tl_account tlaccount = null;
        protected tl_bot tlbot = null;
        protected tl_account_bot tlaccountbot = null;
        protected tl_bot_levels tlbotLevel = null;

        protected string Alert_ChatId = "10598112";
        protected string Alert_botToken = "bot610814292:AAGafL9wnGbNcqJgOLZAv3GpkovAuqHiZ2k";

        protected bool readloop_active = false;

        protected bool bbonus = false;
        protected bool bcollect = false;
        protected bool breinvest = false;

        public Bot(string _phoneNumber, string _username, string _ApiId, string _ApiHash)
        {
            _Bot(_phoneNumber: _phoneNumber, _username: _username, _ApiId: _ApiId, _ApiHash: _ApiHash);
        }

        public Bot(string _phoneNumber, string _username, string _ApiId, string _ApiHash, string _path)
        {
            _Bot(_phoneNumber: _phoneNumber, _username: _username, _ApiId: _ApiId, _ApiHash: _ApiHash, _path: _path);
        }

        protected void _Bot(string _phoneNumber, string _username, string _ApiId, string _ApiHash, string _path = "")
        {
            PhoneNumber = _phoneNumber.Replace("+", string.Empty);
            UserName = _username;
            ApiId = Convert.ToInt32(_ApiId);
            ApiHash = _ApiHash;

            client = new TLSharp.Core.TelegramClientExtended(apiId: ApiId,
                                                             apiHash: ApiHash,
                                                             store: null,
                                                             sessionUserId: UserName,
                                                             sessionPath: costanti_bot.SessionPath() + _path);

            Task task = null;
            task = Task.Factory.StartNew(async () => await client.ConnectAsync());
            task.Wait();
        }

        public async Task<bool> configure()
        {

            BotName = this.GetType().Name;

            ISession nhSession = NHibernateSession.OpenSession();

            ICriteria nhCriteria_tlaccount = utility.CreateQuery<tl_account>(ref nhSession);
            nhCriteria_tlaccount.Add(Expression.Eq(tl_account._NomeUtente, UserName));
            nhCriteria_tlaccount.SetMaxResults(1);
            tlaccount = nhCriteria_tlaccount.List<tl_account>().LastOrDefault();

            ICriteria nhCriteria_tlbot = utility.CreateQuery<tl_bot>(ref nhSession);
            nhCriteria_tlbot.Add(Expression.Eq(tl_bot._NomeUtente, BotName));
            nhCriteria_tlbot.SetMaxResults(1);
            tlbot = nhCriteria_tlbot.List<tl_bot>().LastOrDefault();

            ICriteria nhCriteria_tlaccountbot = utility.CreateQuery<tl_account_bot>(ref nhSession);
            nhCriteria_tlaccountbot.Add(Expression.Eq(tl_account_bot._Bot, tlbot.IdTabella));
            nhCriteria_tlaccountbot.Add(Expression.Eq(tl_account_bot._Account, tlaccount.IdTabella));
            nhCriteria_tlaccountbot.SetMaxResults(1);
            tlaccountbot = nhCriteria_tlaccountbot.List<tl_account_bot>().LastOrDefault();

            tlaccountbot.Errori += 1;

            nhSession.SaveOrUpdate(tlaccountbot);
            nhSession.Flush();

            tlbotLevel = nhSession.Load<tl_bot_levels>(tlaccountbot.Level);

            nhSession.Close();

            await Task.Delay(500);

            bool bContinueWhile = true;
            int countWhile = 0;

            while (bContinueWhile)
            {
                try
                {
                    await get_userdialog();
                    bContinueWhile = false;
                }
                catch (Exception ex)
                {
                    countWhile += 1;
                    if (countWhile == 20) { break; }

                    string Message = "msg_seqno too low (the server has already received a message with a lower msg_id but with either a higher or an equal and odd seqno)";
                    bContinueWhile = (ex.Message == Message);

                    if (!bContinueWhile) { throw ex; }
                }
            }            

            TeleSharp.TL.Contacts.TLFound found = await client.SearchUserAsync(BotName);

            long hash_bot = ((TeleSharp.TL.TLUser)found.Users[0]).AccessHash.Value;
            int id_bot = ((TeleSharp.TL.TLUser)found.Users[0]).Id;
            peer = new TeleSharp.TL.TLInputPeerUser() { UserId = id_bot, AccessHash = hash_bot };

            botbalance_start = await get_balance();
            botbalance_current = botbalance_start;

            return true;
        }

        public async Task<bool> macro_bonus()
        {
            return await get_bonus();
        }

        public async Task<bool> macro_collect()
        {
            bcollect = true;
            if (tlaccountbot.AbilitazioneColleziona == 1)
            {
                bcollect = await collect();
            }
            return bcollect;
        }

        public async Task<bool> macro_reinvest()
        {
            breinvest = true;
            if (tlaccountbot.AbilitazioneReinvest == 1)
            {
                breinvest = await reinvest();
            }
            return breinvest;
        }

        public abstract Task<IBalance> get_balance();
        public abstract Task<bool> get_bonus();
        public abstract Task<bool> collect();
        public abstract Task<bool> reinvest();

        public async Task<TLMessagesSlice> get_lastmessages(int count = 1, int offset = 0)
        {
            await Task.Delay(250);
            var Chat_Messages = await client.GetHistoryAsync(peer, offset, 0, count);
            return Chat_Messages as TLMessagesSlice;
        }

        public async Task<bool> get_userdialog()
        {
            await Task.Delay(500);
            await client.GetUserDialogsAsync();
            return true;
        }

        public async Task<string> get_lasttextmessage(int index = 1)
        {
            var listmessage = await get_lastmessages(index);
            var currentmessage = listmessage.Messages[index - 1] as TLMessage;
            return currentmessage.Message;
        }

        public async Task<List<InlineButton>> get_lastinlinebuttons(int count = 1, int offset = 0)
        {
            await Task.Delay(250);
            var Chat_Messages = await client.GetHistoryAsync(peer, offset, 0, count);

            TLMessagesSlice ChatBot_Messages = Chat_Messages as TLMessagesSlice;

            List<InlineButton> list_inlinebuttons = new List<InlineButton>();
            InlineButton tmp_inlinebuttons = null;

            foreach (TLMessage message in ChatBot_Messages.Messages)
            {
                tmp_inlinebuttons = new InlineButton();

                tmp_inlinebuttons.message_id = message.Id;
                tmp_inlinebuttons.text = message.Message;

                TLReplyInlineMarkup List_InlineButtons = message.ReplyMarkup as TLReplyInlineMarkup;

                if (List_InlineButtons != null)
                {
                    foreach (TLKeyboardButtonRow row in List_InlineButtons.Rows)
                    {
                        if (row != null)
                        {
                            foreach (TLKeyboardButtonCallback button in row.Buttons)
                            {
                                tmp_inlinebuttons.list_choises.Add(new InlineButton.Choises(_text: button.Text, _data: button.Data));
                            }
                        }
                    }
                }

                list_inlinebuttons.Add(tmp_inlinebuttons);
            }

            return list_inlinebuttons;
        }

        public async Task<bool> send_message(string message)
        {
            await Task.Delay(1000);
            await client.SendMessageAsync(peer, message);
            await Task.Delay(1000);
            return true;
        }

        public bool save_all()
        {

            if (!bbonus) return false;
            if (!bcollect) return false;
            if (!breinvest) return false;

            DateTime nowDate = costanti.nowDate();

            ISession nhSession = NHibernateSession.OpenSession();

            ITransaction nhTransaction = nhSession.BeginTransaction();

            try
            {

                tlbot = nhSession.Load<tl_bot>(tlbot.IdTabella);
                tlaccountbot = nhSession.Load<tl_account_bot>(tlaccountbot.IdTabella);

                tlaccountbot.UltimaEsecuzione = nowDate;
                tlaccountbot.ProssimaEsecuzione = nowDate.AddMinutes(tlbot.TimingRipetizione);
                tlaccountbot.Errori = 0;

                nhSession.SaveOrUpdate(tlaccountbot);

                tl_account_bot_balance_log tlaccountbotbalance_log = utility.CreateRecord<tl_account_bot_balance_log>();

                tlaccountbotbalance_log.AccountBot = tlaccountbot.IdTabella;
                tlaccountbotbalance_log.DataOra = nowDate;

                tlaccountbotbalance_log.GameFounds = ((Balance)botbalance_current).Game_Founds;
                tlaccountbotbalance_log.GameBonus = ((Balance)botbalance_current).Game_Bonus;

                tlaccountbotbalance_log.ProductionFounds = ((Balance)botbalance_current).Production_Founds;
                tlaccountbotbalance_log.ProductionBonus = ((Balance)botbalance_current).Production_Bonus;

                tlaccountbotbalance_log.WithdrawFounds = ((Balance)botbalance_current).Withdraw_Founds;
                tlaccountbotbalance_log.WithdrawBonus = ((Balance)botbalance_current).Withdraw_Bonus;

                nhSession.SaveOrUpdate(tlaccountbotbalance_log);

                nhTransaction.Commit();

            }
            catch(Exception ex)
            {
                nhTransaction.Rollback();

                string message = string.Empty;

                message += "Errore Bot: " + BotName + Environment.NewLine;
                message += "Metodo: " + "save_all" + Environment.NewLine;
                message += "Username: " + UserName + Environment.NewLine;
                message += "Exception: " + ex.Message;

                send_error(message);
            }

            nhSession.Close();

            return true;
        }

        public void send_error(string message)
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


        public async Task start_readloop()
        {
            readloop_active = true;
            while (readloop_active)
            {
                await Task.Delay(4000);
                await get_lasttextmessage();
            }
        }

        public void stop_readloop()
        {
            readloop_active = false;
        }

    }
}
