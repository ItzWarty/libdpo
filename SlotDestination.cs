using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dargon.PortableObjects
{
   public class SlotDestination : ISlotDestination
   {
      private readonly Dictionary<int, byte[]> slots = new Dictionary<int, byte[]>();

      public void SetSlot(int slot, byte[] value)
      {
         if (slots.ContainsKey(slot))
            throw new InvalidOperationException("Attempted to set an already-set slot. Probably you done goofed.");

         slots.Add(slot, value);
      }

      public byte[] this[int slot] { get { return slots[slot]; } set { SetSlot(slot, value); } }

      public void WriteToStream(Stream stream)
      {
         using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
            WriteToWriter(writer);
         }
      }

      public void WriteToWriter(BinaryWriter writer)
      {
         var slotCount = slots.Count;
         writer.Write((Int32)slotCount);
         for (var i = 0; i < slotCount; i++)
            writer.Write((Int32)slots[i].Length);
         for (var i = 0; i < slotCount; i++)
            writer.Write((byte[])slots[i]);
      }
   }
}