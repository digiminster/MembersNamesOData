using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MNISOData.Services
{
    public class CsvGeneratorService : ICsvGeneratorService
    {
        private const string Separator = ",";
        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static readonly char[] CharactersThatMustBeQuoted = { ',', '"', '\n' };

        public string PrepareCsv<T>(IEnumerable<T> list, string[] includeProperties)
        {
            var csv = new StringBuilder();
            var properties = typeof(T).GetProperties();

            //Prepare Header-Row
            csv.AppendLine(PrepareHeaderRow(properties, includeProperties));

            //Prepare Data-Rows
            foreach (var item in list)
            {
                csv.AppendLine(PrepareDataRow(item, properties, includeProperties));
            }

            return csv.ToString();
        }

        private static string PrepareHeaderRow(PropertyInfo[] properties, string[] includeProperties)
        {
            var headings = new List<string>();

            foreach (var prop in includeProperties)
            {
                var propInfo = properties.SingleOrDefault(p => p.Name == prop);
                if (propInfo != null) headings.Add(prop);
            }

            return String.Join(Separator, headings);
        }

        private static string PrepareDataRow(object item, PropertyInfo[] properties, string[] includeProperties)
        {
            var row = new StringBuilder();

            foreach (var prop in includeProperties)
            {
                if (row.Length > 0) row.Append(Separator);

                var propInfo = properties.SingleOrDefault(p => p.Name == prop);
                if (propInfo != null)
                {
                    var x = propInfo.GetValue(item, null);

                    if (x != null)
                    {
                        row.Append(Escape(x.ToString()));
                    }
                }
            }

            return row.ToString();
        }

        /* 
         * The following code is taken from http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file/769713#769713 
         */
        public static string Escape(string s)
        {
            if (s.Contains(Quote))
                s = s.Replace(Quote, EscapedQuote);

            if (s.IndexOfAny(CharactersThatMustBeQuoted) > -1)
                s = Quote + s + Quote;

            return s;
        }

        public static string Unescape(string s)
        {
            if (s.StartsWith(Quote) && s.EndsWith(Quote))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(EscapedQuote))
                    s = s.Replace(EscapedQuote, Quote);
            }
            return s;
        }

    }
}
