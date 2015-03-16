using System.IO;

namespace Dargon.PortableObjects
{
   public interface ISlotDestination {
      void SetSlot(int slot, byte[] value);
      void SetSlot(int slot, byte[] value, int offset, int length);
      void SetSlot(int slot, MemoryStream ms);
      void WriteToStream(Stream stream);
      void WriteToWriter(BinaryWriter writer);
   }
}