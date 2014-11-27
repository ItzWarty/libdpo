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
   public class NullSerializationIT : NMockitoInstance {
      [Fact]
      public void Run() {
         const string rootName = "root";
         const string rootLeftName = "root_left";
         const string rootRightName = "root_right";
         const string rootRightRightName = "root_right_right";

         var context = new PofContext();
         context.RegisterPortableObjectType(0, typeof(Node));
         var serializer = new PofSerializer(context);
         var rootRightRight = new Node(rootRightName, null, null);
         var rootRight = new Node(rootRightName, null, rootRightRight);
         var rootLeft = new Node(rootLeftName, null, null);
         var root = new Node("root", rootLeft, rootRight);
         using (var ms = new MemoryStream()) 
         using (var reader = new BinaryReader(ms, Encoding.UTF8, true))
         using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
            serializer.Serialize(writer, root);
            ms.Position = 0;
            var newRoot = serializer.Deserialize<Node>(reader);
            AssertEquals(root, newRoot);
         }
      }

      public class Node : IPortableObject, IEquatable<Node> {
         private string value;
         private Node left;
         private Node right;

         public Node() { }
         public Node(string value, Node left, Node right) { this.value = value; }

         public void Serialize(IPofWriter writer) {
            writer.WriteString(0, value);
            writer.WriteObject(1, left);
            writer.WriteObject(2, right);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadString(0);
            left = reader.ReadObject<Node>(1);
            right = reader.ReadObject<Node>(2);
         }

         public bool Equals(Node other) { return other != null && value == other.value && Equals(left, other.left) && Equals(right, other.right); }
      }
   }
}
