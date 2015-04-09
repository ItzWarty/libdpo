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

      /// <summary>
      /// The POF Type has been passed as a parameter Deserialize(...).
      /// </summary>
      Typeless = 1,

      /// <summary>
      /// The given Stream/Reader represents the data of a POF Frame, 
      /// excluding its length.
      /// </summary>
      Lengthless = 2
   }
}
