using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;

namespace TesteBanklyApi.Service
{
    public interface IJobQueue
    {
        public Task processaFilaAsync();
    }
}
