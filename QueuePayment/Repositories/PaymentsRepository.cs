using QueuePayment.Interfaces;

namespace QueuePayment.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        public async Task<string> pay(int orderId)
        {
            // İmitate payment logic
            await Task.Delay(300);
            return "Payment Succesfull" +
                   "OrderId: " + orderId;

        }
    }
}
