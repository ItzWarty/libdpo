namespace Dargon.PortableObjects
{
   public interface ISlotSource
   {
      int Count { get; }
      byte[] GetSlot(int slotId);
      byte[] this[int i] { get; }
   }
}