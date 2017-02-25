using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core.Entity.Transferable {
    public class TransferableEntity {
        public Guid ID { get; set; }
        public Guid TypeID { get; set; }
        public Guid DatabaseID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, TransferableSimpleProperty> Properties { get; set; }
        public Dictionary<string, TransferableReferenceProperty> References { get; set; }
    }
}
