using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core.Network;

namespace TLSharp.Core
{
    public class TelegramClientExtended : TelegramClient
    {
        public TelegramClientExtended(int apiId,
                                      string apiHash,
                                      ISessionStore store = null,
                                      string sessionUserId = "session",
                                      string sessionPath = null,
                                      TcpClientConnectionHandler handler = null) : base(apiId, apiHash, store, sessionUserId, sessionPath, handler) { }

        public async Task<bool> SendBotButton(TLAbsInputPeer peer, byte[] Data, int messageId)
        {
            if (!IsUserAuthorized())
                throw new InvalidOperationException("Authorize user first!");

            TLRequestGetBotCallbackAnswer TLRequestGetBotCallbackAnswer = new TLRequestGetBotCallbackAnswer();
            TLRequestGetBotCallbackAnswer.Peer = peer;
            TLRequestGetBotCallbackAnswer.Data = Data;
            TLRequestGetBotCallbackAnswer.MsgId = messageId;

            var task_send = await SendTLBotCallbackAnswer(TLRequestGetBotCallbackAnswer);

            return true;
        }


        private async Task<bool> SendTLBotCallbackAnswer(TLRequestGetBotCallbackAnswer TLRequestGetBotCallbackAnswer)
        {
            await SendRequestAsync<TLBotCallbackAnswer>(TLRequestGetBotCallbackAnswer);
            return true;
        }

    }
}
