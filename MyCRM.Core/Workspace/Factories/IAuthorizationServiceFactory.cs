using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCRM.Core.Factories {
    public interface IAuthorizationServiceFactory {
        IAuthorizationService GetService();
    }
}
