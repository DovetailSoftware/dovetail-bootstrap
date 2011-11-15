using System;
using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public static class ClarifyDataRowExtensions
    {
        public static int DatabaseIdentifier(this ClarifyDataRow row)
        {
            return Convert.ToInt32(row.UniqueID);
        }

        public static string AsString(this ClarifyDataRow row, string fieldName)
        {
            return row[fieldName].ToString();
        }

        public static int AsInt(this ClarifyDataRow row, string fieldName)
        {
            return Convert.ToInt32(row[fieldName]);
        }

        public static DateTime AsDateTime(this ClarifyDataRow row, string fieldName)
        {
            return Convert.ToDateTime(row[fieldName].ToString());
        }

        public static IEnumerable<ClarifyDataRow> DataRows(this ClarifyGeneric generic)
        {
            return generic.Rows.Cast<ClarifyDataRow>();
        }
    }
}