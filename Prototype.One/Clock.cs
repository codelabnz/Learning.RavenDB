using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Prototype.One
{
    public static class Clock
    {
        static IClock _default = SystemClock.Instance;
        static IClock _current = null;

        static DateTimeZone _currentZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

        public static IClock Current
        {
            get { return _current ?? _default; }
            set { _current = value; }
        }

        public static LocalDate Today
        {
            get { return Current.Now.InZone(_currentZone).LocalDateTime.Date; }
        }
    }
}
