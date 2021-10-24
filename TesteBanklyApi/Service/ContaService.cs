using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;

namespace TesteBanklyApi.Service
{
    public class ContaService : IContaService
    {
        private readonly ILogger<ContaService> _logger;
        private Queue<QueueObject> processingQueue { get; set; }
        private int lastProcessed { get; set; }
        public ContaService(ILogger<ContaService> logger)
        {
            processingQueue = new Queue<QueueObject>();
            lastProcessed = 0;
            _logger = logger;
        }

        string[] Status = new[]
           {
                "In Queue", "Processing", "Confirmed", "Error"
            };

        public TransacaoResponse adicionarFila(TransferenciaDTO dto)
        {
            var queueObject = new QueueObject
            {
                transferenciaDTO = dto,
                status = Status[0],
                Id = Guid.NewGuid()
            };
            if (queueObject.transferenciaDTO.accountOrigin == null || queueObject.transferenciaDTO.accountDestination == null || queueObject.transferenciaDTO.value == 0)
            {
                queueObject.status = Status[3];
                queueObject.message = "Conta destino, conta de origem ou valor invalidos";
            }
            processingQueue.Enqueue(queueObject);
            _logger.LogInformation("A transação de id " + queueObject.Id + " foi adicionada na fila");
            //A transação foi adicionada na fila e vai ser processada pelo job que foi adicionado no startup.cs usando hangfire.
            return new TransacaoResponse { transactionId = queueObject.Id.ToString() };


        }

        public void setLastProcessed(int processed)
        {
            this.lastProcessed = processed;
        }
        public int getLastProcessed()
        {
            return this.lastProcessed;
        }

        public void jobFila()
        {
            _logger.LogInformation("Disparando job que verifica a fila");

        }
        public ResponseDTO acharFila(string id)
        {
            _logger.LogInformation("Iniciando busca na fila");
            foreach (var item in processingQueue)
            {
                if (item.Id.ToString() == id)
                {
                    return new ResponseDTO
                    {
                        Status = item.status,
                        Message = item.message
                    };
                }


            }
            return null;
        }


        public void setQueue(Queue<QueueObject> queue)
        {
            this.processingQueue = queue;
        }

        public Queue<QueueObject> getQueue()
        {
            return this.processingQueue;
        }
    }
}
