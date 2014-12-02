using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class CollectionSerializationIT : NMockitoInstance {
      [Fact]
      public void Run() {
         var testObj1 = new TestClass(new object[] { 1, "herp" });

         var context = new PofContext();
         context.RegisterPortableObjectType(1, typeof(TestClass));

         var serializer = new PofSerializer(context);

         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, testObj1);
            }
            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var readObj1 = serializer.Deserialize<TestClass>(reader);
               AssertTrue(testObj1.Value.SequenceEqual(readObj1.Value));
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
            writer.WriteCollection(0, value, true);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadArray<object>(0, true);
         }
      }
   }
}
