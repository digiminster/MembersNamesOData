using System.Collections.Generic;

namespace MNISOData.Services
{
    public interface ICsvGeneratorService
    {
        string PrepareCsv<T>(IEnumerable<T> objectlist, string[] ignoreProperties);
    }
}
