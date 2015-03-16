using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dargon.PortableObjects
{
   public class SlotDestination : ISlotDestination
   {
      private readonly Dictionary<int, SlotValue> slots = new Dictionary<int, SlotValue>();

      public void SetSlot(int slot, byte[] value)
      {
         SetSlot(slot, value, 0, value.Length);
      }

      public void SetSlot(int slot, byte[] value, int offset, int length) {
         if (slots.ContainsKey(slot))
            throw new InvalidOperationException("Attempted to set an already-set slot. Probably you done goofed.");

         slots.Add(slot, new SlotValue(value, offset, length));
      }

      public void SetSlot(int slot, MemoryStream ms) {
         SetSlot(slot, ms.GetBuffer(), 0, (int)ms.Length);
      }

      public byte[] this[int slot] {
         get {
            var value = slots[slot];
            if (value.offset == 0 && value.length == value.data.Length) {
               return value.data;
            } else {
               var buffer = new byte[value.length];
               Buffer.BlockCopy(value.data, value.offset, buffer, 0, value.length);
               return buffer;
            }
         }
         set { SetSlot(slot, value); }
      }

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
            writer.Write((Int32)slots[i].length);
         for (var i = 0; i < slotCount; i++) {
            var value = slots[i];
            writer.Write(value.data, value.offset, value.length);
         }
      }

      private struct SlotValue {
         public byte[] data;
         public int offset;
         public int length;

         public SlotValue(byte[] data, int offset, int length) {
            this.data = data;
            this.offset = offset;
            this.length = length;
         }
      }
   }
}