using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PCT.Http.Fanout
{
    /// <summary>
    /// This interface represents the deserialized response from the core service(s)
    /// </summary>
    public interface IServiceResponse
    {
        /// <summary>
        /// The source provider.
        /// </summary>
        string SourceProvider { get; set; }

        /// <summary>
        /// The status of the response
        /// </summary>
        ResponseStatus ResponseStatus { get; set; }
    }
}
