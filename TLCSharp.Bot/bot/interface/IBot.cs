using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TLCSharp.Bot
{
    public interface IBot
    {
        Task<bool> configure();

        Task<bool> macro_bonus();
        Task<bool> macro_collect();
        Task<bool> macro_reinvest();

        Task<IBalance> get_balance();
        Task<bool> get_bonus();
        Task<bool> collect();
        Task<bool> reinvest();

        Task<TLMessagesSlice> get_lastmessages(int count = 1, int offset = 0);
        Task<string> get_lasttextmessage(int index = 1);
        Task<List<InlineButton>> get_lastinlinebuttons(int count = 1, int offset = 0);
        Task<bool> send_message(string message);

        void send_error(string message);

        bool save_all();

        Task start_readloop();
        void stop_readloop();
    }
}