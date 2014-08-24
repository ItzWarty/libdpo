namespace Dargon.PortableObjects
{
   public interface ISlotSource
   {
      byte[] GetSlot(int slotId);
      byte[] this[int i] { get; }
   }
}