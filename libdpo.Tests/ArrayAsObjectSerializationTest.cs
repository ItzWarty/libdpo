using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class ArrayAsObjectSerializationTest : NMockitoInstance {
      [Fact]
      public void Run() {
         var context = new PofContext();
         context.RegisterPortableObjectType(1, typeof(TestClass));

         var serializer = new PofSerializer(context);
         var testObj1 = new TestClass(EnumerateValues());
         var testObj2 = new TestClass(new object[] { null, null });
         var testObj3 = new TestClass(new List<object> { 2, "string", null });

         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, testObj1);
               serializer.Serialize(writer, testObj2);
               serializer.Serialize(writer, testObj3);
            }
            ms.Position = 0;
            using (var reader = new BinaryReader(ms, Encoding.UTF8, true)) {
               var readObj1 = serializer.Deserialize<TestClass>(reader);
               var a = ((IEnumerable<object>)testObj1.Value).ToArray();
               var b = ((IEnumerable<object>)readObj1.Value).ToArray();
               AssertEquals(5, a.Length);
               AssertEquals(5, b.Length);
               for (var i = 0; i < b.Length; i++) {
                  var ai = a[i];
                  var bi = b[i];
                  if (ai is IEnumerable && !(ai is string)) {
                     AssertTrue(((IEnumerable<object>)ai).SequenceEqual((IEnumerable<object>)bi));
                  } else {
                     AssertEquals(ai, bi);
                  }
               }

               var readObj2 = serializer.Deserialize<TestClass>(reader);
               AssertTrue(((IEnumerable<object>)testObj2.Value).SequenceEqual((IEnumerable<object>)readObj2.Value));
               
               var readObj3 = serializer.Deserialize<TestClass>(reader);
               AssertTrue(((IEnumerable<object>)testObj3.Value).SequenceEqual((IEnumerable<object>)readObj3.Value));
            }
         }
      }

      public IEnumerable<object> EnumerateValues() {
         yield return null;
         yield return null;
         yield return 1;
         yield return "pineapple";
         yield return new object[] { 1, "banana" };
      } 

      public class TestClass : IPortableObject {
         private object value;

         public TestClass() {}

         public TestClass(object value) {
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
