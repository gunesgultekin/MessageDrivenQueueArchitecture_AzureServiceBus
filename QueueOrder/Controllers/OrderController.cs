using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using QueueOrder.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace QueueOrder.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class OrderController : ControllerBase
    {
        private IOrdersRepository _ordersRepository;

        public OrderController(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        ServiceBusClient client;

        ServiceBusSender OrderSender;
       
        [HttpGet("queue")]
        public async Task queue(int productId)
        {
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            client = new ServiceBusClient(
                "Endpoint=AZURE_SERVICE_BUS_ENDPOINT",
                clientOptions);

            OrderSender = client.CreateSender("ordersQueue");

            // SEND ORDER TO ORDER QUEUE

            using ServiceBusMessageBatch OrderBatch = await OrderSender.CreateMessageBatchAsync();

            // SEND PRODUCT ID REQUEST PARAMETER TO THE ORDERS QUEUE
            OrderBatch.TryAddMessage(
                new ServiceBusMessage(
                    productId.ToString()
                    )
                ); 

            await OrderSender.SendMessagesAsync(OrderBatch);

        }
    }
}
