using System.IO;
using ItzWarty;

namespace Dargon.PortableObjects
{
   internal static class SlotSourceFactory
   {
      public static ISlotSource CreateFromBinaryReader(BinaryReader reader)
      {
         int slotCount = reader.ReadInt32();
         int[] slotLengths = Util.Generate(slotCount, i => reader.ReadInt32());
         var slots = Util.Generate(slotCount, i => reader.ReadBytes(slotLengths[i]));
         return new SlotSource(slots);
      }

      public static ISlotSource CreateWithSingleSlot(byte[] data) { return new SlotSource(new[] { data }); }
   }
}
