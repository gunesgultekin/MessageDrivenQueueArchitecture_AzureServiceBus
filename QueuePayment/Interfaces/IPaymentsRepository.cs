namespace QueuePayment.Interfaces
{
    public interface IPaymentsRepository
    {
        public Task<string> pay(int orderId);
    }
}
