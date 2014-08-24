namespace Dargon.PortableObjects
{
   public class SlotSource : ISlotSource
   {
      private readonly byte[][] slots;

      public SlotSource(byte[][] slots)
      {
         this.slots = slots;
      }

      public byte[] GetSlot(int slotId)
      {
         return this.slots[slotId];
      }

      public byte[] this[int i]
      {
         get { return GetSlot(i); }
      }
   }
}
