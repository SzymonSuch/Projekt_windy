using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winda2._0
{
    public class RideEntry
    {
        public DateTime Timestamp { get; set; }
        public int From { get; set; }   
        public int To { get; set; }

        public RideEntry(int from, int to) // Delegowanie do innego konstruktora
            : this(from, to, DateTime.Now)
        {
        }

        public RideEntry(int from, int to, DateTime timestamp) // Dodatkowy konstruktor do obsługi zapisu/ wczytywania z pliku
        {
            Timestamp = timestamp;
            From = from;
            To = to;
        }

        public override string ToString()
        {
            // Format: yyyy-mm-dd hh:mm:ss — z 3 na 5
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} — z {From} na {To}";
        }

    }
}