# TLCSharp

Unofficial Telegram (http://telegram.org) client library implemented in C# on .NET Core.

This project is based on [TLSharp](https://github.com/sochix/TLSharp) by [sochix](https://github.com/sochix).

-------------------------------

# What's new
- .NET Core 2.1
- Multiuser
- Custom Session Path
- SendBotButton (TLRequestGetBotCallbackAnswer)

-------------------------------

# Starter Guide

See official wiki at [TLSharp Starter Guide](https://github.com/sochix/TLSharp/#starter-guide)

-------------------------------

# SendBotButton Sample

- Get the bot inline buttons (client is TelegramClientExtended and peer is TeleSharp.TL.TLInputPeerUser)

```csharp
public async Task<List<InlineButton>> get_lastinlinebuttons(int count = 1, int offset = 0) {

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
```

- Send the First InlineButton found

```csharp
List<InlineButton> listInlineButton = await get_lastinlinebuttons(1, 0);

if (listInlineButton.Count > 0) {

	InlineButton InlineButton = listInlineButton.First();

	await client.SendBotButton(peer, InlineButton.list_choises.First().data, InlineButton.message_id);
}
```

-------------------------------

# Donate

If this project help you reduce time to develop, you can give me a cup of coffee :)

BTC: 18S52vQkMSGitySfrbA1QesnM2A4SYXkcw

[![Donate](https://img.shields.io/badge/Donate-Bitcoin-orange.svg)](https://blockchain.info/address/18S52vQkMSGitySfrbA1QesnM2A4SYXkcw)

XRP: rKyHLa3NmWkHtCcfv9prUZvBLeopNpK3vb

[![Donate](https://img.shields.io/badge/Donate-Ripple-orange.svg)](https://bithomp.com/explorer/rKyHLa3NmWkHtCcfv9prUZvBLeopNpK3vb)

ETH: 0x43569f781d526bafE7578DADa24053DF39D479A2

[![Donate](https://img.shields.io/badge/Donate-Ethereum-orange.svg)](https://ethplorer.io/address/0x43569f781d526bafE7578DADa24053DF39D479A2)

LTC: LM9SvwwxuzXABWpVwLXdJM4iCxRuRFQa7L

[![Donate](https://img.shields.io/badge/Donate-Litecoin-orange.svg)](https://live.blockcypher.com/ltc/address/LM9SvwwxuzXABWpVwLXdJM4iCxRuRFQa7L/)

-------------------------------
