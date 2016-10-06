using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DylanBriedis.iMessageBridge
{
    internal static class JSON
    {
        public static string FormatDictionaryResponse<T>(Dictionary<int, T> dictionary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"status\":\"ok\",\"data\":[");
            foreach (var d in dictionary)
                sb.Append(FormatJSONObject(d.Value, false) + ", ");
            if (sb.ToString().EndsWith(", "))
                sb.Remove(sb.Length - 2, 2);
            sb.AppendLine(" ] }");
            return sb.ToString();
        }

        public static string Error(Exception ex)
        {
            return "{\"status\":\"exception\",\"exception\":" + FormatJSONObject(ex, false) + "}";
        }

        public static string FormatString(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        public static string FormatJSONObject(object obj, bool objIdOnly)
        {
            if (!objIdOnly)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                foreach (var f in obj.GetType().GetProperties())
                {
                    string name = f.Name[0].ToString().ToLower() + f.Name.Substring(1);
                    sb.Append(FormatJSONValue(name, f.GetValue(obj)));
                }
                if (sb.ToString().EndsWith(","))
                    sb.Remove(sb.Length - 1, 1);
                sb.Append("}");
                return sb.ToString();
            }
            else
                return (obj as IIdObject).Id.ToString();
        }

        public static string FormatJSONValue(string name, object value)
        {
            if (value != null)
                switch (value.GetType().Name)
                {
                    case "Int32":
                        return "\"" + name + "\":" + value + ",";
                    case "Boolean":
                        return "\"" + name + "\":" + ((bool)value ? "true" : "false") + ",";
                    case "Recipient":
                    case "Conversation":
                    case "Message":
                        return "\"" + name + "\":" + FormatJSONObject(value, true) + ",";
                    case "List`1":
                        StringBuilder sb = new StringBuilder();
                        sb.Append("[ ");
                        IList list = (IList)value;
                        foreach (object obj in list)
                            sb.Append(((IIdObject)obj).Id + ", ");
                        if (sb.ToString().EndsWith(", "))
                            sb.Remove(sb.Length - 2, 2);
                        sb.Append(" ]");
                        return "\"" + name + "\":" + sb.ToString() + ",";
                    default:
                        return "\"" + name + "\":\"" + FormatString(value.ToString()) + "\",";
                }
            else
                return "\"" + name + "\":null,";
        }
    }
}
