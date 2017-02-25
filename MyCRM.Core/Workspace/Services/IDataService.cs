using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core.Workspace.Services {

    public interface IDataService {
        Guid GetID();

        MyCRM.Core.Transferable.TransferableResponse Save( MyCRM.Core.Transferable.TransferableSaveRequest request );

        MyCRM.Core.Transferable.TransferableResponse Load( MyCRM.Core.Transferable.TransferableLoadRequest request );

       
        Guid[] GetSupportedTypes();
    }
}
