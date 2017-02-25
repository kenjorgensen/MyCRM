using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core {
    public class WorkspaceDataProvider {
        public bool Connected { get; private set; }
        IDataServiceFactory factory;
        IDataService service;
        public Guid[] SupportedTypes { get; private set; }
        public Guid ID { get; private set; }

        public WorkspaceDataProvider( IDataServiceFactory providerFactory ) {
            this.factory = providerFactory;
            this.SupportedTypes = new Guid[0];
        }

        public void Connect() {
            if ( this.service == null ) {
                this.Connected = true;
                this.service = this.factory.GetService();
                this.ID = this.service.GetID();
                this.SupportedTypes = this.service.GetSupportedTypes();
            }
        }

        public void Disconnect() {
            if ( this.service != null ) {
                this.Connected = false;
                this.service = null;
                this.SupportedTypes = new Guid[0];
                this.ID = Guid.Empty;
            }
        }

        public IEnumerable<ITransferableEntity> Load( Guid typeID ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            TransferableLoadRequest loadRequest = new Transferable.TransferableLoadRequest(typeID);
            TransferableResponse response = service.Load(loadRequest);
            if ( response.IsSuccessful ) {
                foreach ( var transferable in response.Entities ) {
                    yield return transferable;
                }
            }
        }

        public IEnumerable<ITransferableEntity> Load( ITransferableReferenceList referenceList ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            TransferableLoadRequest loadRequest = new Transferable.TransferableLoadRequest(referenceList.Items);
            TransferableResponse response = service.Load(loadRequest);
            if ( response.IsSuccessful ) {
                foreach ( var transferable in response.Entities ) {
                    yield return transferable;
                }
            }
        }

    }
}
