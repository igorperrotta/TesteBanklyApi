using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TesteBanklyApi.Dto;
using TesteBanklyApi.Service;

namespace TesteBanklyApi.Controllers
{
    [ApiController]
    [Route("api/conta")]
    public class ContaController : ControllerBase
    {
        private readonly ILogger<ContaController> _logger;
        private readonly IContaService _contaService;

        public ContaController(ILogger<ContaController> logger, IContaService contaService)
        {
            _logger = logger;
            _contaService = contaService;
        }
        //Endrpoint que faz a transação entre contas
        [HttpPost]
        [Route("/transacao")]
        public ActionResult<TransacaoResponse> transacaoConta([FromBody] TransferenciaDTO dto)
        {
            _logger.LogInformation("Adicionando na fila");
            var retorno = _contaService.adicionarFila(dto);
            return Accepted(retorno);
        }
        //endpoint que retorna o status de intens na fila
        [HttpGet]
        [Route("/fila")]
        public ActionResult<ResponseDTO> transacaoConta(string transaciontId)
        {
            if (transaciontId == null)
            {
                return BadRequest();
            }
            _logger.LogInformation("Buscando item na fila com id: " + transaciontId);
            var retorno = _contaService.acharFila(transaciontId);
            if (retorno == null)
            {
                return NotFound();
            }
            return Accepted(retorno);
        }


    }
}
