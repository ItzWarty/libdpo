using System;
using System.IO;
using System.Text;
using ItzWarty.IO;

namespace Dargon.PortableObjects
{
   public class PofSerializer : IPofSerializer {
      private readonly IPofContext context;

      public PofSerializer(IPofContext context)
      {
         this.context = context;
      }

      public void Serialize<T>(Stream stream, T portableObject) where T : IPortableObject {
         using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
            Serialize(writer, portableObject);
         }
      }

      public void Serialize(Stream stream, object portableObject) {
         using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
            Serialize(writer, (object)portableObject);
         }
      }

      public void Serialize<T>(BinaryWriter writer, T portableObject) where T : IPortableObject { 
         Serialize(writer, (object)portableObject);
      }

      public void Serialize(BinaryWriter writer, object portableObject) {
         Serialize(writer, portableObject, SerializationFlags.Default);
      }

      public void Serialize(BinaryWriter writer, object portableObject, SerializationFlags serializationFlags) {
         var slotDestination = new SlotDestination();
         var pofWriter = new PofWriter(context, slotDestination);

         if (serializationFlags.HasFlag(SerializationFlags.Typeless)) {
            pofWriter.WriteObjectTypeless(0, portableObject);
         } else {
            pofWriter.WriteObject(0, portableObject);
         }

         var data = slotDestination[0];
         writer.Write((int)data.Length);
         writer.Write(data);
      }

      public void Serialize<T>(IBinaryWriter writer, T portableObject) where T : IPortableObject {
         Serialize(writer.__Writer, portableObject);
      }

      public void Serialize(IBinaryWriter writer, object portableObject) {
         Serialize(writer.__Writer, portableObject);
      }

      public void Serialize(IBinaryWriter writer, object portableObject, SerializationFlags serializationFlags) {
         Serialize(writer.__Writer, portableObject, serializationFlags);
      }

      public T Deserialize<T>(Stream stream) where T : IPortableObject { return (T)Deserialize(stream); }

      public object Deserialize(Stream stream) {
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            return Deserialize(reader);
      }

      public T Deserialize<T>(IBinaryReader reader) where T : IPortableObject { return Deserialize<T>(reader.__Reader); }

      public T Deserialize<T>(BinaryReader reader) where T : IPortableObject { return (T)Deserialize(reader); }

      public object Deserialize(IBinaryReader reader) { return Deserialize(reader.__Reader); }

      public object Deserialize(BinaryReader reader) { return Deserialize(reader, SerializationFlags.Default, null); }

      public object Deserialize(IBinaryReader reader, SerializationFlags serializationFlags, Type type) {
         return Deserialize(reader.__Reader, serializationFlags, type);
      }

      public object Deserialize(BinaryReader reader, SerializationFlags serializationFlags, Type type) {
         var dataLength = reader.ReadInt32();
         var data = reader.ReadBytes(dataLength);
         var pofReader = new PofReader(context, SlotSourceFactory.CreateWithSingleSlot(data));
         if (serializationFlags.HasFlag(SerializationFlags.Typeless)) {
            return pofReader.ReadObjectTypeless(0, type);
         } else {
            return pofReader.ReadObject(0);
         }
      }
   }
}