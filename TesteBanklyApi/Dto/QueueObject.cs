using System;

namespace TesteBanklyApi.Dto
{
    public class QueueObject
    {
        public Guid Id { get; set; }
        public TransferenciaDTO transferenciaDTO { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
}
