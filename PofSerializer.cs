using System.IO;
using System.Text;

namespace Dargon.PortableObjects
{
   public class PofSerializer : IPofSerializer {
      private readonly IPofContext context;

      public PofSerializer(IPofContext context)
      {
         this.context = context;
      }

      public void Serialize<T>(Stream stream, T portableObject) where T : IPortableObject
      {
         using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
            Serialize(writer, portableObject);
         }
      }

      public void Serialize<T>(BinaryWriter writer, T portableObject) where T : IPortableObject 
      {
         var slotDestination = new SlotDestination();
         var pofWriter = new PofWriter(context, slotDestination);
         pofWriter.WriteObject(0, portableObject);

         var data = slotDestination[0];
         writer.Write((int)data.Length);
         writer.Write(data);
      }

      public T Deserialize<T>(Stream stream)
         where T : IPortableObject
      {
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            return Deserialize<T>(reader);
      }

      public T Deserialize<T>(BinaryReader reader)
         where T : IPortableObject
      {
         var dataLength = reader.ReadInt32();
         var data = reader.ReadBytes(dataLength);
         var pofReader = new PofReader(context, SlotSourceFactory.CreateWithSingleSlot(data));
         return pofReader.ReadObject<T>(0);
      }
   }
}