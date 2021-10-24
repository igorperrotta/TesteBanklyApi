using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;

namespace TesteBanklyApi.Service
{
    public interface IContaService
    {
        public TransacaoResponse adicionarFila(TransferenciaDTO dto);
        public ResponseDTO acharFila(string id);
        public void setLastProcessed(int processed);
        public  Task processaFilaAsync(Queue<QueueObject> queue,int processed);
    }
}
