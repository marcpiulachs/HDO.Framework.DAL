using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuntoVenta.Entidades
{
        public class Base : CommonBase
        {
            public bool IsNew { get; set; }
        }

        public class CommonBase
        {
            // Let's setup standard null values
            public static DateTime DateTimeNullValue = DateTime.MinValue;
            public static DateTime DateTimeNowValue = DateTime.Now;
            public static Guid GuidNullValue = Guid.Empty;
            public static Guid GuidNewValue = Guid.NewGuid();
            public static int IntNullValue = int.MinValue;
            public static float FloatNullValue = float.MinValue;
            public static double DoubleNullValue = double.MinValue;
            public static decimal DecimalNullValue = decimal.MinValue;
            public static string StringNullValue = string.Empty;
            public static long LongNullValue = long.MinValue;
            public static bool BoolNullValue;
            public static TimeSpan TimeSpanNullValue = TimeSpan.Parse("00:00:00");
        }
}
