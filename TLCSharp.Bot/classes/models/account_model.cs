using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Leonor.ORM;
using Leonor.Library;
using NHibernate;
using NHibernate.Criterion;

namespace TLCSharp.Bot.classes
{
    class account_model
    {

        public static List<tl_account_bot> get_listaccountbot(in ISession nhExtSession = null)
        {
            ISession nhSession = nhExtSession;
            if (nhExtSession == null) { nhSession = NHibernateSession.OpenSession(); }

            ICriteria nhCritria_tl_account_bot = utility.CreateQuery<tl_account_bot>(ref nhSession);
            nhCritria_tl_account_bot.Add(Expression.Lt(tl_account_bot._ProssimaEsecuzione, costanti.nowDate()));
            nhCritria_tl_account_bot.Add(Expression.Eq(tl_account_bot._AbilitazioneScheduling, 1));
            nhCritria_tl_account_bot.AddOrder(Order.Asc(tl_account_bot._Errori));
            List<tl_account_bot> list_tl_account_bot = nhCritria_tl_account_bot.List<tl_account_bot>().ToList();

            if (nhExtSession == null) { nhSession.Close(); }

            return list_tl_account_bot;
        }



    }
}
