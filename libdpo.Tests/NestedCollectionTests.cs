using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using NMockito;
using Xunit;
using SCG = System.Collections.Generic;
using ICL = ItzWarty.Collections;

namespace Dargon.PortableObjects.Tests {
   public class NestedCollectionTests : NMockitoInstance {
      [Fact]
      public void NestedArraysTest() {
         var dict = new SCG.Dictionary<int, float[][]>();
         var random = new Random(0);
         for (var i = 0; i < 10; i++) {
            dict.Add(i, Util.Generate(i, x => Util.Generate(x, y => (float)random.NextDouble())));
         }

         var context = new PofContext();
         context.RegisterPortableObjectType(1, typeof(Wrapper));
         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, new Wrapper(dict));
            }
            ms.Position = 0;
            Console.WriteLine(ms.ToArray().ToHex());
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var wrapperCopy = (Wrapper)serializer.Deserialize(reader);
               var dictClone = (SCG.Dictionary<int, float[][]>)wrapperCopy.Value;
               AssertTrue(new ICL.HashSet<int>(dict.Keys).SetEquals(dictClone.Keys));
               random = new Random();
               for (var i = 0; i < 10; i++) {
                  var arr = dict[i];
                  for (var j = 0; j < arr.Length; j++) {
                     AssertTrue(new ICL.HashSet<float>(arr[j]).SetEquals(dictClone[i][j]));
                  }
               }
            }
         }
      }

      [Fact(Skip = "Not supported by dpo (cannot have dict be part of generic type - consider wrapper).")]
      public void NestedDictionariesTest() {
         var testObj = new Wrapper<SCG.Dictionary<int, SCG.Dictionary<int, bool[]>[]>[]>(Util.Generate(10, i => MakeSubDictionary(i)));

         var context = new PofContext().With(x => x.RegisterPortableObjectType(0, typeof(Wrapper<>)));
         var serializer = new PofSerializer(context);

         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, (object)testObj);
            }
            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               serializer.Deserialize(reader);
            }
         }
      }

      private SCG.Dictionary<int, SCG.Dictionary<int, bool[]>[]> MakeSubDictionary(int id) {
         var items = Util.Generate(id, subId => MakeSubSubDictionaries(id, subId));
         var dictionary = new SCG.Dictionary<int, SCG.Dictionary<int, bool[]>[]>();
         for (var i = 0; i < items.Length; i++) {
            dictionary.Add(i, items[i]);
         }
         return dictionary;
      }

      private SCG.Dictionary<int, bool[]>[] MakeSubSubDictionaries(int id, int subId) {
         return Util.Generate(id + subId, MakeSubSubSubDictionary);
      }

      private SCG.Dictionary<int, bool[]> MakeSubSubSubDictionary(int n) {
         var result = new SCG.Dictionary<int, bool[]>();
         for (var i = 0; i < n; i++) {
            bool[] data = Util.Generate(i, x => x % 2 == 0);
            result.Add(i, data);
         }
         return result;
      }

      public class Wrapper<T> : IPortableObject {
         private T value;

         public Wrapper() { }

         public Wrapper(T value) {
            this.value = value;
         }

         public T Value { get { return value; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteObject(0, value);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadObject<T>(0);
         }
      }
      public class Wrapper : IPortableObject {
         private object value;
         
         public Wrapper() { }

         public Wrapper(object value) {
            this.value = value;
         }

         public object Value { get { return value; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteObject(0, value);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadObject(0);
         }
      }
   }
}
