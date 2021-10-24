using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;
using TesteBanklyApi.Service;

namespace TesteBanklyApi
{
    public class JobQueue : IJobQueue
    {
        public string url = "https://acessoaccount.herokuapp.com/api/Account";
        public Queue<QueueObject> processingQueue { get; set; }
        private int lastProcessed { get; set; }
        private readonly ILogger<JobQueue> _logger;
        private readonly IContaService _contaService;
        public JobQueue(ILogger<JobQueue> logger, IContaService contaService)
        {
            _logger = logger;
            _contaService = contaService;
            processingQueue = _contaService.getQueue();
            lastProcessed = _contaService.getLastProcessed();
        }

        public async Task processaFilaAsync()
        {
            //Este método tem como responsabilidade fazer a requisição post na api do heroku e alterar o status dos itens da fila
            //Demorei muito tempo pra fazer isso funcionar por que o HangFire sempre criava uma nova instancia da classe e resetava a fila, ao setar a fila no final do método
            // e receber no construtor eu contorno essa limitação. Sério, deu muito trabalho :(
            if (processingQueue.Count != 0)
            {
                for (int i = lastProcessed; i < processingQueue.Count; i++)
                {
                    if (!processingQueue.ToArray()[i].status.Equals("Error") && !processingQueue.ToArray()[i].status.Equals("Confirmed"))
                    {

                        using (var client = new HttpClient())
                        {
                            _logger.LogInformation("iniciando processamento do objeto da fila");
                            processingQueue.ToArray()[i].status = "Processing";

                            try
                            {
                                var uri = new Uri(url);
                                var jsonDebito = new TransferObject
                                {
                                    accountNumber = processingQueue.ToArray()[i].transferenciaDTO.accountOrigin,
                                    type = "Debit",
                                    value = processingQueue.ToArray()[i].transferenciaDTO.value
                                };
                                var jsonCredito = new TransferObject
                                {
                                    accountNumber = processingQueue.ToArray()[i].transferenciaDTO.accountDestination,
                                    type = "Credit",
                                    value = processingQueue.ToArray()[i].transferenciaDTO.value
                                };
                                var jsonDebitoSerialized = JsonSerializer.Serialize(jsonDebito);
                                var jsonCreditoSerialized = JsonSerializer.Serialize(jsonCredito);
                                var responseDebit = await client.PostAsync(uri, new StringContent(jsonDebitoSerialized, Encoding.UTF8, "application/json"));
                                var responseCredit = await client.PostAsync(uri, new StringContent(jsonCreditoSerialized, Encoding.UTF8, "application/json"));
                                processingQueue.ToArray()[i].status = "Confirmed";
                                _contaService.setQueue(processingQueue);
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
