using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codentia.Common.Data
{
    interface IDbConnectionProvider
    {
        // TODO: will be inherited by various providers - might be anything from a file to mysql or sql server
        // TODO: connection name comes in, with the request for a connection object
        // TODO: Not sure if that object should be interface based too eg MySqlConnectionProvider returns MySqlConnection - makes sense

    }
}
