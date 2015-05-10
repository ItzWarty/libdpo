using System.IO;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.PortableObjects {
   public interface SlotSourceFactory {
      ISlotSource CreateFromBinaryReader(BinaryReader reader);
      ISlotSource CreateFromBinaryReader(IBinaryReader reader);
   }

   internal interface SlotSourceFactoryInternal {
      ISlotSource CreateWithSingleSlot(byte[] data);
   }

   public class SlotSourceFactoryImpl : SlotSourceFactory {
      public ISlotSource CreateFromBinaryReader(IBinaryReader reader) {
         return CreateFromBinaryReader(reader.__Reader);
      }

      public ISlotSource CreateFromBinaryReader(BinaryReader reader) {
         int slotCount = reader.ReadInt32();
         int[] slotLengths = Util.Generate(slotCount, i => reader.ReadInt32());
         var slots = Util.Generate(slotCount, i => reader.ReadBytes(slotLengths[i]));
         return new SlotSource(slots);
      }
   }

   internal class SlotSourceFactoryInternalImpl : SlotSourceFactoryImpl, SlotSourceFactoryInternal {
      public ISlotSource CreateWithSingleSlot(byte[] data) {
         return new SlotSource(new[] { data });
      }
   }
}
