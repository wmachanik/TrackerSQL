using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace TrackerDotNet.Classes.Sql
{
    internal static class DbMapper
    {
        private static readonly Dictionary<Type, PropertyInfo[]> _propsCache = new Dictionary<Type, PropertyInfo[]>();

        public static T Map<T>(IDataRecord r) where T : new()
        {
            var t = typeof(T);
            if (!_propsCache.TryGetValue(t, out var props))
            {
                props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _propsCache[t] = props;
            }

            var inst = new T();
            for (int i = 0; i < r.FieldCount; i++)
            {
                var name = r.GetName(i);
                var pi = props.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (pi == null || !pi.CanWrite) continue;
                var val = r.GetValue(i);
                if (val == DBNull.Value) continue;
                try
                {
                    var targetType = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                    if (targetType.IsEnum)
                    {
                        pi.SetValue(inst, Enum.Parse(targetType, val.ToString()));
                    }
                    else if (targetType == typeof(Guid))
                    {
                        if (val is Guid g) pi.SetValue(inst, g);
                        else if (Guid.TryParse(val.ToString(), out Guid gg)) pi.SetValue(inst, gg);
                    }
                    else
                    {
                        pi.SetValue(inst, Convert.ChangeType(val, targetType));
                    }
                }
                catch
                {
                    // ignore conversion errors for now
                }
            }

            return inst;
        }
    }
}
