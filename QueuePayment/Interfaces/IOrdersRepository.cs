namespace QueueOrder.Interfaces
{
    public interface IOrdersRepository
    {
        public Task<string> createOrder(int productId);
        public Task<int> checkStocks(int productId);
        public Task<int> saveOrder();
    }
}
