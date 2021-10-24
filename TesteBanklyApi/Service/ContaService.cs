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
        Queue<QueueObject> processingQueue { get; set; }
        public string url = "https://acessoaccount.herokuapp.com/api/Account";
        JobQueue job;
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

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
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
            RecurringJob.AddOrUpdate(() => processaFilaAsync(processingQueue, lastProcessed),Cron.Minutely);
            return new TransacaoResponse { transactionId = queueObject.Id.ToString() };


        }

        public void setLastProcessed(int processed)
        {
            this.lastProcessed = processed;
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
        [DisableConcurrentExecution(10 * 60)]
        public async Task processaFilaAsync(Queue<QueueObject> queue, int processed)
        {

            if (queue.Count != 0)
            {
                for (int i = processed; i < queue.Count; i++)
                {
                    if (queue.ToArray()[i].status != "Error" || queue.ToArray()[i].status != "Confirmed")
                    {

                        using (var client = new HttpClient())
                        {
                            _logger.LogInformation("iniciando processamento do objeto da fila");
                            queue.ToArray()[i].status = "Processing";

                            try
                            {
                                var uri = new Uri(url);
                                var jsonDebito = new TransferObject
                                {
                                    accountNumber = queue.ToArray()[i].transferenciaDTO.accountOrigin,
                                    type = "Debit",
                                    value = queue.ToArray()[i].transferenciaDTO.value
                                };
                                var jsonCredito = new TransferObject
                                {
                                    accountNumber = queue.ToArray()[i].transferenciaDTO.accountDestination,
                                    type = "Credit",
                                    value = queue.ToArray()[i].transferenciaDTO.value
                                };
                                var jsonDebitoSerialized = JsonSerializer.Serialize(jsonDebito);
                                var jsonCreditoSerialized = JsonSerializer.Serialize(jsonCredito);
                                var responseDebit = await client.PostAsync(uri, new StringContent(jsonDebitoSerialized, Encoding.UTF8, "application/json"));
                                var responseCredit = await client.PostAsync(uri, new StringContent(jsonCreditoSerialized, Encoding.UTF8, "application/json"));
                                processingQueue.ToArray()[i].status = "Confirmed";
                                setLastProcessed(i);
                                _logger.LogInformation("objeto processado com sucesso");
                            }
                            catch (Exception ex)
                            {
                                queue.ToArray()[i].status = "Error";
                                queue.ToArray()[i].message = "Houve um erro ao se comunicar com o endpoint";
                                _logger.LogInformation("objeto com erro");
                            }
                        }
                    }

                }
            }
        }
    }
}
