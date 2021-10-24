using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;
using TesteBanklyApi.Service;

namespace TesteBanklyApi
{
    public class JobQueue : IJobQueue
    {
        public string url = "https://acessoaccount.herokuapp.com/api/Account";


        private readonly ILogger<JobQueue> _logger;
        private readonly IContaService _contaService;
        public JobQueue(ILogger<JobQueue> logger, IContaService contaService)
        {
            _logger = logger;
            _contaService = contaService;
            
        }


        public async Task processaFilaAsync(Queue<QueueObject> processingQueue, int lastProcessed)
        {
            if (processingQueue.Count != 0)
            {
                for (int i = lastProcessed; i < processingQueue.Count; i++)
                {
                    if (!processingQueue.ToArray()[i].status.Equals("Error") || !processingQueue.ToArray()[i].status.Equals("Confirmed"))
                    {

                        using (var client = new HttpClient())
                        {
                            _logger.LogInformation("iniciando processamento do objeto da fila");
                            processingQueue.ToArray()[i].status = "Processing";

                            try
                            {
                                var uri = new Uri(url);
                                var json = JsonSerializer.Serialize(processingQueue.ToArray()[i].transferenciaDTO);
                                var response = await client.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
                                processingQueue.ToArray()[i].status = "Confirmed";
                                _contaService.setLastProcessed(i);
                                _logger.LogInformation("objeto processado com sucesso");
                            }
                            catch (Exception ex)
                            {
                                processingQueue.ToArray()[i].status = "Error";
                                processingQueue.ToArray()[i].message = "Houve um erro ao se comunicar com o endpoint";
                                _logger.LogInformation("objeto com erro");
                            }
                        }
                    }

                }
            }
        }
    }
}
