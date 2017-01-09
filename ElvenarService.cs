using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Elvenar
{
    public class ElvenarService
    {
        public bool IsModified { get; private set; }

        public ElvenarEnv Load(string filename)
        {
            return Load(new StreamReader(filename));
        }

        public ElvenarEnv Load(TextReader reader)
        {
            IsModified = false;
            var serializer = new XmlSerializer(typeof(ElvenarEnv));
            var elvenar = serializer.Deserialize(reader) as ElvenarEnv;
            return elvenar;
        }

        public void Save(IEnumerable<Symbol> symbols, IEnumerable<Macro> macros, string filename)
        {
            Save(symbols, macros, new StreamWriter(filename));
        }

        public void Save(IEnumerable<Symbol> symbols, IEnumerable<Macro> macros, TextWriter writer)
        {
            var elvenar = new ElvenarEnv { Symbols = symbols.ToArray(), Macros = macros.ToArray() };
            var serializer = new XmlSerializer(elvenar.GetType());
            serializer.Serialize(writer, elvenar);
            IsModified = false;
        }
    }
}
