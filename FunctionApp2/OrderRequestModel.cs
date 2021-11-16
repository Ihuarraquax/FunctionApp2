using System.Collections.Generic;

namespace FunctionApp2
{
        public class OrderRequestModel
        {
            public string Id_klienta { get; set; }
            public List<OrderModel> Zamowienie { get; set; }
        }
}