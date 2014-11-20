using System.IO;

namespace Dargon.PortableObjects {
   public interface IPofSerializer {
      void Serialize<T>(Stream stream, T portableObject) where T : IPortableObject;
      void Serialize<T>(BinaryWriter writer, T portableObject) where T : IPortableObject;

      T Deserialize<T>(Stream stream)
         where T : IPortableObject;

      T Deserialize<T>(BinaryReader reader)
         where T : IPortableObject;
   }
}