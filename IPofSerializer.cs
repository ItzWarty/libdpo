using System;
using System.IO;
using ItzWarty.IO;

namespace Dargon.PortableObjects {
   public interface IPofSerializer {
      void Serialize<T>(Stream stream, T portableObject);
      void Serialize(Stream stream, object portableObject);

      void Serialize<T>(BinaryWriter writer, T portableObject);
      void Serialize(BinaryWriter writer, object portableObject);
      void Serialize(BinaryWriter writer, object portableObject, SerializationFlags serializationFlags);

      void Serialize<T>(IBinaryWriter writer, T portableObject);
      void Serialize(IBinaryWriter writer, object portableObject);
      void Serialize(IBinaryWriter writer, object portableObject, SerializationFlags serializationFlags);

      T Deserialize<T>(Stream stream);
      object Deserialize(Stream stream);

      T Deserialize<T>(BinaryReader reader);
      object Deserialize(BinaryReader reader);
      object Deserialize(BinaryReader reader, SerializationFlags serializationFlags, Type type);

      T Deserialize<T>(IBinaryReader reader);
      object Deserialize(IBinaryReader reader);
      object Deserialize(IBinaryReader reader, SerializationFlags serializationFlags, Type type);
   }
}