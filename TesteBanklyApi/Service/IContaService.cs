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
        public void setQueue(Queue<QueueObject> queue);
        public Queue<QueueObject> getQueue();
        public int getLastProcessed();
    }
}
