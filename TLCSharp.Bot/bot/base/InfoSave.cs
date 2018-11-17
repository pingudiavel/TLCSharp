using System;

using Leonor.Library;

namespace TLCSharp.Bot
{
    public class InfoSave : IInfoSave
    {

        public DateTime UltimaEsecuzione = costanti.defaultDate();
        public DateTime ProssimaEsecuzione = costanti.defaultDate();

        public decimal Moneta_Bonus = -1;
        public decimal Moneta_Prodotta = -1;
        public decimal Moneta_Disponibile = -1;
    }
}