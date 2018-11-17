using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using TeleSharp.TL;

using Leonor.ORM;
using Leonor.Library;

using NHibernate;
using NHibernate.Criterion;

namespace TLCSharp.Bot
{
    class BitcoinMines_Bot : Bot
    {

        private const int max_errors = 3;

        private enum balance_index : int
        {
            purchase = 0,
            withdraw = 1,
        };

        public BitcoinMines_Bot(string _phoneNumber, string _username, string _ApiId, string _ApiHash) : base(_phoneNumber, _username, _ApiId, _ApiHash) { }

        public BitcoinMines_Bot(string _phoneNumber, string _username, string _ApiId, string _ApiHash, string _path) : base(_phoneNumber, _username, _ApiId, _ApiHash, _path) { }

        public override async Task<bool> collect()
        {
            decimal Production_Founds = 0;

            int count_error = 0;

            while (count_error != max_errors)
            {
                try
                {
                    await send_message(BitcoinMining_Bot_Keyboard.main.Working);

                    List<InlineButton> listInlineButton = await get_lastinlinebuttons(1, 0);

                    if (listInlineButton.Count > 0)
                    {
                        InlineButton InlineButton = listInlineButton.First();

                        await client.SendBotButton(peer, InlineButton.list_choises.First().data, InlineButton.message_id);

                        botbalance_current = await get_balance();

                        Production_Founds = ((BitcoinMining_Bot_Balance)botbalance_current).Production_Founds - ((BitcoinMining_Bot_Balance)botbalance_start).Production_Founds;
                    }
                }
                catch
                {
                    Production_Founds = 0;
                }

                if (Production_Founds > 0)
                { break; }
                else
                { count_error += 1; }
            }

            if (count_error == max_errors)
            {
                string message = string.Empty;

                message += "Errore Bot: " + BotName + Environment.NewLine;
                message += "Metodo: " + "collect" + Environment.NewLine;
                message += "Username: " + UserName + Environment.NewLine;

                send_error(message);
            }

            return (Production_Founds > 0);
        }

        public override async Task<IBalance> get_balance()
        {
            BitcoinMining_Bot_Balance balance = new BitcoinMining_Bot_Balance();

            await send_message(BitcoinMining_Bot_Keyboard.main.Menu);

            string text_message = string.Empty;
            bool btrovato = false;

            for (int i = 1; i <= 5; i++)
            {
                text_message = await get_lasttextmessage(i);
                if (text_message.Contains("ACCOUNT")) { btrovato = true; break; }
            }

            if (btrovato)
            {
                List<decimal> list_balance = utility.get_ListDecimal(text_message);

                balance.Game_Founds = list_balance[(int)balance_index.purchase];
                balance.Production_Founds = list_balance[(int)balance_index.withdraw];
                balance.Withdraw_Founds = balance.Production_Founds;
            }

            return balance;
        }

        public override async Task<bool> get_bonus()
        {
            decimal bonus = 0;

            int count_error = 0;

            while (count_error != max_errors)
            {
                try
                {
                    await send_message(BitcoinMining_Bot_Keyboard.main.Bonus);

                    List<InlineButton> listInlineButton = await get_lastinlinebuttons(1, 0);

                    if (listInlineButton.Count > 0)
                    {
                        InlineButton InlineButton = listInlineButton.First();

                        await client.SendBotButton(peer, InlineButton.list_choises.First().data, InlineButton.message_id);

                        botbalance_current = await get_balance();

                        bonus = ((BitcoinMining_Bot_Balance)botbalance_current).Game_Founds - ((BitcoinMining_Bot_Balance)botbalance_start).Game_Founds;

                    }
                }
                catch
                {
                    bonus = 0;
                    string lastmessage = await get_lasttextmessage();
                    if (lastmessage.ToLower().Contains("you have already received"))
                    {
                        bonus = 1;
                        break;
                    }
                }

                if (bonus > 0)
                {
                    ((BitcoinMining_Bot_Balance)botbalance_current).Game_Founds = bonus;
                    break;
                }
                else
                { count_error += 1; }
            }

            if (count_error == max_errors)
            {
                string message = string.Empty;

                message += "Errore Bot: " + BotName + Environment.NewLine;
                message += "Metodo: " + "get_bonus" + Environment.NewLine;
                message += "Username: " + UserName + Environment.NewLine;

                send_error(message);
            }

            bbonus = (bonus > 0);

            return bbonus;
        }

        public override async Task<bool> reinvest()
        {
            bool bresult = false;

            int count_error = 0;

            ISession nhSession = NHibernateSession.OpenSession();

            tlbotLevel = nhSession.Load<tl_bot_levels>(tlbotLevel.IdTabella);
            tlbot = nhSession.Load<tl_bot>(tlbot.IdTabella);

            while (count_error != max_errors)
            {
                try
                {
                    IBalance botbalance = await get_balance();

                    decimal countBuy = Math.Floor(((BitcoinMining_Bot_Balance)botbalance).Game_Founds / tlbotLevel.GamePrice);

                    bresult = (countBuy == 0);

                    if (countBuy > 0)
                    {
                        bresult = false;

                        await send_message(BitcoinMines_Bot_Keyboard.main.Market);

                        List<InlineButton> listInlineButton = await get_lastinlinebuttons(1, 0);

                        if (listInlineButton.Count > 0)
                        {
                            InlineButton InlineButton = listInlineButton.First();
#pragma warning disable CS4014
                            client.SendBotButton(peer, InlineButton.list_choises.First().data, InlineButton.message_id);
#pragma warning restore CS4014
                        }

                        await Task.Delay(10000);
                        client.Dispose();
                        _Bot(_phoneNumber: PhoneNumber, _username: UserName, _ApiId: ApiId.ToString(), _ApiHash: ApiHash);
                        await Task.Delay(2000);

                        listInlineButton = await get_lastinlinebuttons(tlbot.Levels, 0);

                        if (listInlineButton.Count > 0)
                        {
                            for (int i = 0; i < tlbot.Levels; i++)
                            {
                                InlineButton InlineButton = listInlineButton[i];
                                if (InlineButton.text.ToLower().Contains(tlbotLevel.Description.ToLower()))
                                {
                                    for (int j = 1; j <= countBuy; j++)
                                    {
                                        await client.SendBotButton(peer, InlineButton.list_choises.First().data, InlineButton.message_id);
                                        bresult = true;
                                    }
                                }
                            }
                        }

                    }

                }
                catch
                {
                    bresult = false;
                }

                if (bresult)
                {
                    break;
                }
                else
                { count_error += 1; }
            }

            nhSession.Close();

            if (count_error == max_errors)
            {
                string message = string.Empty;

                message += "Errore Bot: " + BotName + Environment.NewLine;
                message += "Metodo: " + "reinvest" + Environment.NewLine;
                message += "Username: " + UserName + Environment.NewLine;

                send_error(message);
            }

            breinvest = bresult;

            return breinvest;
        }

    }
}
