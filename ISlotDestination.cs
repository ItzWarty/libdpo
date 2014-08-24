using System.IO;

namespace Dargon.PortableObjects
{
   public interface ISlotDestination
   {
      void SetSlot(int slot, byte[] value);
      void WriteToStream(Stream stream);
      void WriteToWriter(BinaryWriter writer);
   }
}