using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    public class ElvenarBase<T>
    {
        public T[] Add(T[] array, T data)
        {
            var list = array.ToList();
            list.Add(data);
            return list.ToArray();
        }

        public void Write(TextWriter writer)
        {
            var serializer = new XmlSerializer(GetType());
            serializer.Serialize(writer, this);
        }
    }

    public class DynamicExtension
    {
        public static dynamic GetKeyValue(dynamic settings, string key)
        {
            try
            {
                return settings[key];
            }
            catch
            {
                return null;
            }
        }
    }
}
