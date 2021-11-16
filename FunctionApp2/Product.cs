namespace FunctionApp2
{
    public class ProductInStock
    {
        public string Nazwa { get; set; }
        public string Kod { get; set; }
        public decimal Cena { get; set; }
        public int Ilosc { get; set; }
        public int Stan { get; set; }
        public decimal Vat { get; set; }
    }

    public class ProductDetails
    {
        public string Nazwa { get; set; }
        public string Kod { get; set; }
        public decimal Cena { get; set; }
        public decimal Vat { get; set; }
    }

}