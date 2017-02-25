using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core {
    public class WorkspaceAuthorizationProvider {
        public bool Connected { get; private set; }
        private IAuthorizationServiceFactory factory;
        private IAuthorizationService service;
        private HashSet<ICheckOutable> pendingCheckOutList;
        private HashSet<ICheckOutable> checkoutList;

        public WorkspaceAuthorizationProvider( IAuthorizationServiceFactory authorizationService ) {
            this.Connected = false;
            this.factory = authorizationService;
            this.pendingCheckOutList = new HashSet<ICheckOutable>();
            this.checkoutList = new HashSet<ICheckOutable>();
        }

        public void Connect() {
            if ( this.service == null ) {
                this.Connected = true;
                this.service = this.factory.GetService();
            }
        }

        public void Disconnect() {
            if ( this.service != null ) {
                this.Connected = false;
                this.service = null;
            }
        }

        public void AddToPendingCheckOuts( IEnumerable<ICheckOutable> guids ) {
            foreach ( var item in guids ) {
                if ( !this.checkoutList.Contains(item) && !this.pendingCheckOutList.Contains(item) ) {
                    this.pendingCheckOutList.Add(item);
                    item.Mark();
                }
            }
        }

        public void ClearPendingCheckOuts() {
            foreach ( var item in pendingCheckOutList ) {
                item.Unmark();
            }
        }

        public void RemoveFromPendingCheckOuts( IEnumerable<ICheckOutable> guids ) {
            foreach ( var item in guids ) {
                if ( this.pendingCheckOutList.Contains(item) ) {
                    this.pendingCheckOutList.Remove(item);
                    item.Unmark();
                }
            }
        }

        public bool CheckIn( Guid workspaceID, IEnumerable<ICheckOutable> items ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            if ( service.CheckIn(workspaceID, pendingCheckOutList.Select(i => i.ID).ToArray()) ) {
                foreach ( var item in items ) {
                    item.Unmark();
                }
                this.checkoutList = new HashSet<ICheckOutable>(this.checkoutList.Except(items));
                return true;
            }
            return false;
        }

        public bool CheckOut( Guid workspaceID ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            if ( service.CheckOut(workspaceID, this.pendingCheckOutList.Select(i => i.ID).ToArray()) ) {
                foreach ( var item in this.pendingCheckOutList ) {
                    item.Mark();
                    this.checkoutList.Add(item);
                }
                return true;
            }
            return false;
        }

        public void RegisterWorkspace( Guid workspaceID ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            service.RegisterWorkspace(workspaceID);
            this.checkoutList.Clear();
            this.pendingCheckOutList.Clear();
        }

        public void UnregisterWorkspace( Guid workspaceID ) {
            if ( !Connected ) {
                throw new Exception("Not Connected");
            }
            service.UnregisterWorkspace(workspaceID);
            this.checkoutList.Clear();
            this.pendingCheckOutList.Clear();
        }
    }
}
