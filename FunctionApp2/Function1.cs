using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp2
{
    public static partial class Function1
    {
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public static partial class Function2
    {
        public static List<ProductInStock> products = new List<ProductInStock>
        {
            new ProductInStock
            {
                Nazwa = "Sweter",
                Kod = "123",
                Cena = 199.99m,
                Ilosc = 1,
                Stan = 12,
                Vat = 23
            },
            new ProductInStock
            {
                Nazwa = "Koszula",
                Kod = "501",
                Cena = 99.95m,
                Ilosc = 3,
                Stan = 102,
                Vat = 7
            },
            new ProductInStock
            {
                Nazwa = "Spodnie",
                Kod = "3112",
                Cena = 150.0m,
                Ilosc = 2,
                Stan = 42,
                Vat = 23
            },
        };

        [FunctionName("Function2")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var request = context.GetInput<OrderRequestModel>();
            // Replace "hello" with the name of your Durable Activity Function.
            var magazynResponse = await context.CallActivityAsync<MagazynResponse>("UslugaMagazyn", request);
            var platnoscResponse = await context.CallActivityAsync<PlatnoscResponse>("UslugaPlatnosc", new PaymentRequestModel {MagazynResponse = magazynResponse, OrderRequestModel = request });
            var vatResponse = await context.CallActivityAsync<VatResponse>("UslugaVat", new VatRequestModel {Id_klienta = request.Id_klienta, Total = platnoscResponse.Total, Zamowienie = request. });
            
            
            return "success";
        }

        [FunctionName("UslugaMagazyn")]
        public static MagazynResponse UslugaMagazyn([ActivityTrigger] OrderRequestModel request, ILogger log)
        {
            var result = new MagazynResponse
            {
                ProductDetails = new List<ProductDetails>()
            };
            foreach (var zamowienie in request.Zamowienie)
            {
                var productToDecrease = products.FirstOrDefault(_ => _.Kod == zamowienie.Kod);
                productToDecrease.Ilosc -= zamowienie.Ilosc;
                result.ProductDetails.Add(new ProductDetails
                {
                    Cena = productToDecrease.Cena,
                    Kod = productToDecrease.Kod,
                    Nazwa = productToDecrease.Nazwa,
                    Vat = productToDecrease.Vat
                });
            };
            return result;

        }

        [FunctionName("UslugaVat")]
        public static VatResponse UslugaVat([ActivityTrigger] VatRequestModel request, ILogger log)
        {
            return new VatResponse
            {
                Data_Zaksiegowania = DateTime.Now,
                Id_klienta = request.Id_klienta,
                Total = request.Total,
                Vat_id = Guid.NewGuid(),
                Zamowienie = request.Zamowienie
            };

        }

        [FunctionName("UslugaPlatnosc")]
        public static PlatnoscResponse UslugaPlatnosc([ActivityTrigger] PaymentRequestModel request, ILogger log)
        {
            var total = request.OrderRequestModel.Zamowienie.Sum(zam => (zam.Cena + (zam.Cena * request.MagazynResponse.ProductDetails.FirstOrDefault(_ => _.Kod == zam.Kod).Vat / 100)) * zam.Ilosc);
            return new PlatnoscResponse
            {
                Total = total
            };
        }

        [FunctionName("Function2_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var orderRequestModel = JsonConvert.DeserializeObject<OrderRequestModel>(await req.Content.ReadAsStringAsync());
            
            

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function2", orderRequestModel);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}