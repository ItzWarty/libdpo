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
   public class CollectionSerializationIT : NMockitoInstance {
      [Fact]
      public void PolymorphismTest() {
         var testObj1 = new TestClass(new object[] { 0xCDCDCDCD, "herp", new byte[] { 0xEE, 0xDD, 0xCC, 0xDD, 0xEE, 0xCC, 0xFF } });

         var context = new PofContext();
         context.RegisterPortableObjectType(0xFF, typeof(TestClass));

         var serializer = new PofSerializer(context);

         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, testObj1);
            }

            Console.WriteLine(ms.ToArray().ToHex());

            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var readObj1 = serializer.Deserialize<TestClass>(reader);
               AssertTrue(testObj1.Value.Take(2).SequenceEqual(readObj1.Value.Take(2)));
               AssertTrue(((IEnumerable<byte>)testObj1.Value[2]).SequenceEqual((IEnumerable<byte>)readObj1.Value[2]));
            }
         }
      }

      public class TestClass : IPortableObject {
         private object[] value;

         public TestClass() { }
         public TestClass(object[] value) {
            this.value = value;
         }

         public object[] Value { get { return value; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteS32(0, 0x12345678);
            writer.WriteCollection(1, value, true);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadArray<object>(1, true);
         }
      }
   }
}
