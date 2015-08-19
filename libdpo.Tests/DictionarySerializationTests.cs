using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class DictionarySerializationTests : NMockitoInstance {
      private readonly PofContext pofContext;
      private readonly PofSerializer pofSerializer;

      public DictionarySerializationTests() {
         pofContext = new PofContext().With(x => {
            x.RegisterPortableObjectType(1, typeof(SerializableClass));
         });
         pofSerializer = new PofSerializer(pofContext);
      }

      [Fact]
      public void NullSerializationTest() {
         IDictionary<int, object> dictionary = null;
         var serializable = new SerializableClass(dictionary);
         using (var ms = new MemoryStream()) {
            pofSerializer.Serialize(ms, serializable);
            ms.Position = 0;
            var copy = pofSerializer.Deserialize<SerializableClass>(ms);
            AssertNull(copy.Dictionary);
         }
      }

      [Fact]
      public void EmptySerializationTest() {
         IDictionary<int, object> dictionary = new Dictionary<int, object>();
         var serializable = new SerializableClass(dictionary);
         using (var ms = new MemoryStream()) {
            pofSerializer.Serialize(ms, serializable);
            ms.Position = 0;
            var copy = pofSerializer.Deserialize<SerializableClass>(ms);
            AssertNotNull(copy.Dictionary);
            AssertEquals(0, copy.Dictionary.Count);
         }
      }

      [Fact]
      public void ObjectValueSerializationTest() {
         IDictionary<int, object> dictionary = new Dictionary<int, object>();
         dictionary.Add(3, new object());
         var serializable = new SerializableClass(dictionary);
         using (var ms = new MemoryStream()) {
            pofSerializer.Serialize(ms, serializable);
            ms.Position = 0;
            var copy = pofSerializer.Deserialize<SerializableClass>(ms);
            AssertNotNull(copy.Dictionary);
            AssertEquals(1, copy.Dictionary.Count);
            AssertEquals(3, copy.Dictionary.Keys.First());
            AssertEquals(typeof(object), copy.Dictionary.Values.First().GetType());
         }
      }

      public class SerializableClass : IPortableObject {
         private IDictionary<int, object> dictionary;

         public SerializableClass() { }

         public SerializableClass(IDictionary<int, object> dictionary) {
            this.dictionary = dictionary;
         }

         public IDictionary<int, object> Dictionary => dictionary;

         public void Serialize(IPofWriter writer) {
            writer.WriteMap(0, dictionary);
         }

         public void Deserialize(IPofReader reader) {
            dictionary = reader.ReadMap<int, object>(0);
         }
      }
   }
}
