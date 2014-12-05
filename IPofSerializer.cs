using System.IO;
using ItzWarty.IO;

namespace Dargon.PortableObjects {
   public interface IPofSerializer {
      void Serialize<T>(Stream stream, T portableObject) where T : IPortableObject;
      void Serialize(Stream stream, object portableObject);

      void Serialize<T>(BinaryWriter writer, T portableObject) where T : IPortableObject;
      void Serialize(BinaryWriter writer, object portableObject);

      void Serialize<T>(IBinaryWriter writer, T portableObject) where T : IPortableObject;
      void Serialize(IBinaryWriter writer, object portableObject);

      T Deserialize<T>(Stream stream) where T : IPortableObject;
      object Deserialize(Stream stream);

      T Deserialize<T>(BinaryReader reader) where T : IPortableObject;
      object Deserialize(BinaryReader reader);

      T Deserialize<T>(IBinaryReader reader) where T : IPortableObject;
      object Deserialize(IBinaryReader reader);
   }
}