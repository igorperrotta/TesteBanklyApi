using System;

namespace TesteBanklyApi.Dto
{
    public class TransferenciaDTO
    {
        public string accountOrigin { get; set; }
        public string accountDestination { get; set; }
        public float value { get; set; }
    }
}
