using System;
using System.Collections.Generic;

namespace FunctionApp2
{
    public class VatResponse
    {
        public string Id_klienta { get; set; }
        public List<OrderModel> Zamowienie { get; set; }
        public decimal Total { get; set; }
        public Guid Vat_id { get; set; }
        public DateTime Data_Zaksiegowania { get; set; }
    }
}