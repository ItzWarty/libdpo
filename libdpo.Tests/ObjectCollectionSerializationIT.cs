using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class ObjectCollectionSerializationIT : NMockitoInstance {
      [Fact]
      public void Run() {
         var context = new PofContext();
         context.RegisterPortableObjectType(1, typeof(Box));

         var serializer = new PofSerializer(context);
         var obj = new object[] { null, 1, 2, 3 };
         var box = new Box(obj);
         using (var ms = new MemoryStream())
         using (var reader = new BinaryReader(ms, Encoding.UTF8, true))
         using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            serializer.Serialize(writer, box);
            serializer.Serialize(writer, null);
            ms.Position = 0;
            var newBox = serializer.Deserialize<Box>(reader);
            var nullBox = serializer.Deserialize<Box>(reader);
            AssertEquals(box, newBox);
            AssertNull(nullBox);
         }
      }

      public class Box : IPortableObject, IEquatable<Box> {
         private object[] value;

         public Box() { }

         public Box(object[] value) {
            this.value = value;
         }

         public object[] Value { get { return value; } }

         public void Serialize(IPofWriter writer) { writer.WriteCollection(0, value, true); }
         public void Deserialize(IPofReader reader) { value = reader.ReadArray<object>(0, true); }

         public bool Equals(Box other) { return other != null && this.value.SequenceEqual(other.value); }
      }
   }
}
