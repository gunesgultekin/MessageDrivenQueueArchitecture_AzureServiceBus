using Azure.Core;
using Azure.Messaging.ServiceBus;
using QueuePayment.Interfaces;
using QueuePayment.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using QueueOrder.Interfaces;

namespace QueuePayment.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private IPaymentsRepository _paymentsRepository;
        private IOrdersRepository _ordersRepository;

        public PaymentController(IPaymentsRepository paymentsRepository, IOrdersRepository ordersRepository)
        {
            _paymentsRepository = paymentsRepository;
            _ordersRepository = ordersRepository;
        }

        // HANDLE RECEIVED QUEUE MESSAGE
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            // CHECK STOCKS USING PRODUCT ID VALUE RECEIVED FROM ORDERS QUEUE
            int stocks = await _ordersRepository.checkStocks(int.Parse(body));
            if (stocks == 0)
            {
                Debug.WriteLine("* Out of stock PRODUCT_ID= "+body);
            }
                Debug.WriteLine($"* Stock check completed {stocks} for PRODUCT_ID= "+body);

            // SAVE ORDER GET ORDER ID
            int orderId = await _ordersRepository.saveOrder();

            // ADD ORDER ID TO PAYMENTS QUEUE
            ServiceBusClient client;

            ServiceBusSender PaymentSender;

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            client = new ServiceBusClient("Endpoint=AZURE_SERVICE_BUS_ENDPOINT",clientOptions);
            PaymentSender = client.CreateSender("paymentsQueue");

            using ServiceBusMessageBatch PaymentBatch = await PaymentSender.CreateMessageBatchAsync();

            // ADD ORDER ID TO PAYMENTS QUEUE
            PaymentBatch.TryAddMessage(
                new ServiceBusMessage(
                    orderId.ToString()
                    )
                );
            // SEND TO QUEUE
            await PaymentSender.SendMessagesAsync(PaymentBatch);

            // STOP PROCESSING DELETE FROM ORDERS QUEUE
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        [HttpGet("QueuePayment")]
        public async Task<string> QueuePayment()
        {
            ServiceBusClient client;

            ServiceBusProcessor processor;

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
            };
            client = new ServiceBusClient("Endpoint=AZURE_SERVICE_BUS_ENDPOINT", clientOptions);

            processor = client.CreateProcessor("ordersQueue", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete

            }           
            );

            processor.ProcessMessageAsync += MessageHandler;
            
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync(CancellationToken.None);

            // WAIT WHILE START PROCESSING CALLS MESSAGE HANDLER  
            await Task.Run(() =>
            {
                Thread.Sleep(2 *1000);
            });


            await processor.StopProcessingAsync();


            return "success";

        }
    }
}
