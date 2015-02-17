using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.PortableObjects {
   [Flags]
   public enum SerializationFlags {
      None = 0,
      Default = None,
      Typeless = 1
   }
}
