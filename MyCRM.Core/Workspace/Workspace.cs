using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core {
    public class Workspace {
        private WorkspaceState state;
        Guid workspaceID;
        Dictionary<Guid, ITransferableSurrogate> cache;
        List<WorkspaceDataProvider> Providers;
        WorkspaceAuthorizationProvider AuthorizationProvider;

        private ActionBase currentAction;

        public void SetCurrentAction( ActionBase action ) {
            this.currentAction = action;
        }

        public Workspace( Guid id ) {
            this.state = WorkspaceState.Initializing;
            this.workspaceID = id;
            this.Providers = new List<Workspaces.WorkspaceDataProvider>();
            this.cache = new Dictionary<Guid, ITransferableSurrogate>();
        }



        public void AddAuthorizationProvider( IAuthorizationServiceFactory serviceFactory ) {
            WorkspaceAuthorizationProvider provider = new Workspaces.WorkspaceAuthorizationProvider(serviceFactory);
            this.AuthorizationProvider = provider;
        }

        public void AddDataSource( IDataServiceFactory serviceFactory ) {
            WorkspaceDataProvider provider = new WorkspaceDataProvider(serviceFactory);
            this.Providers.Add(provider);
        }

        public void Open() {
            if ( this.state == WorkspaceState.Open ) {
                throw new Exception("Workspace already open");
            }
            if ( this.Providers.Count == 0 ) {
                throw new Exception("Cannot open without Data Providers");
            }
            if ( this.AuthorizationProvider == null ) {
                throw new Exception("Cannot open without an Authorization Provider");
            }
            try {
                foreach ( var provider in Providers ) {
                    provider.Connect();
                }
                AuthorizationProvider.Connect();
                this.state = WorkspaceState.Open;
            } catch ( Exception e ) {
                foreach ( var provider in Providers ) {
                    if ( provider.Connected ) {
                        provider.Disconnect();
                    }
                }
                if ( AuthorizationProvider.Connected ) {
                    AuthorizationProvider.Disconnect();
                }
                throw new Exception("Failed To Open Workspace", e);
            }
        }

        public void Close() {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace already closed");
            }
            try {
                foreach ( var provider in Providers ) {
                    if ( provider.Connected ) {
                        provider.Disconnect();
                    }
                }
                if ( AuthorizationProvider.Connected ) {
                    AuthorizationProvider.Disconnect();
                }
                this.state = WorkspaceState.Closed;
            } catch ( Exception e ) {
                this.state = WorkspaceState.Closed;
                throw new Exception("Failed To Properly Close Workspace", e);
            }

        }

        private Guid GetTypeID( Type type ) {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            var typeAttribute = (EntityAttribute)type.GetCustomAttributes(false).Single(i => i is EntityAttribute);
            return typeAttribute.EntityTypeID;
        }

        public void AddToPendingCheckOuts( IEnumerable<ICheckOutable> guids ) {
            this.AuthorizationProvider.AddToPendingCheckOuts(guids);
        }

        public void ClearPendingCheckOuts() {
            this.AuthorizationProvider.ClearPendingCheckOuts();
        }

        public void RemoveFromPendingCheckOuts( IEnumerable<ICheckOutable> guids ) {
            this.AuthorizationProvider.AddToPendingCheckOuts(guids);
        }

        public bool CheckOutPending() {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            return (this.AuthorizationProvider.CheckOut(this.workspaceID));
        }

        public void CheckIn( params ICheckOutable[] list ) {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            this.AuthorizationProvider.CheckIn(this.workspaceID, list);
        }

        public T Create<T>()
            where T : ITransferableSurrogate, new() {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            Guid typeID = this.GetTypeID(typeof(T));
            var provider = this.Providers.Single(i => i.SupportedTypes.Contains(typeID));
            var entity = new T();
            ((ITransferableSurrogate)entity).Create(this, provider.ID);
            this.cache.Add(entity.ID, entity);
            return entity;
        }

        public List<T> LoadList<T>( ITransferableReferenceList referenceList )
            where T : ITransferableSurrogate, new() {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            foreach ( var dataService in Providers ) {
                if ( referenceList.Items.Any(i => dataService.SupportedTypes.Contains(i.TypeID)) ) {
                    foreach ( var result in dataService.Load(referenceList) ) {
                        T entity;
                        if ( this.cache.ContainsKey(result.ID) ) {
                            entity = (T)this.cache[result.ID];
                        } else {
                            entity = new T();
                            ((ITransferableSurrogate)entity).Receive(this, dataService.ID, result);
                            this.cache.Add(result.ID, entity);
                        }
                    }
                }
            }
            return referenceList.Items.Select(reference => (T)this.cache[reference.ID]).ToList();
        }

        public List<T> Load<T>()
            where T : ITransferableSurrogate, new() {
            if ( this.state != WorkspaceState.Open ) {
                throw new Exception("Workspace must be open.");
            }
            Guid typeID = this.GetTypeID(typeof(T));
            foreach ( var dataService in Providers ) {
                if ( dataService.SupportedTypes.Contains(typeID) ) {
                    foreach ( var result in dataService.Load(typeID) ) {
                        T entity;
                        if ( this.cache.ContainsKey(result.ID) ) {
                            entity = (T)this.cache[result.ID];
                        } else {
                            entity = new T();
                            ((ITransferableSurrogate)entity).Receive(this, dataService.ID, result);
                            this.cache.Add(result.ID, entity);
                        }
                    }
                }
            }
            return this.cache.Values.Where(i => i.TypeID == typeID).Cast<T>().ToList();
        }

        public void Save() {

        }

    }
}
