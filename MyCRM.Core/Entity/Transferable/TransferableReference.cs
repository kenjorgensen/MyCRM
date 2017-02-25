using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core.Entity.Transferable {
    public class TransferableReference {
        public Guid ID { get; set; }
        public Guid TypeID { get; set; }
        public Guid DatabaseID { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}
