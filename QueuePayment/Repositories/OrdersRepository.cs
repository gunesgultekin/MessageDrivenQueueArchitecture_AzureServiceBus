

using QueueOrder.Interfaces;

namespace QueueOrder.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        public async Task<string> createOrder(int productId)
        {
            int stockCount = await checkStocks(productId);
            if (stockCount > 0)
            {
                int orderId = await saveOrder();
                return "success" +
                       "Order Created: " + orderId;
            }
            else
            {
                return "out of stock" +
                       "ProductId: " + productId;
            }
        }

        public async Task<int> checkStocks(int productId)
        {
            // Imitating a stock count check from database
            await Task.Delay(100);
            Random randStockCount = new Random();
            return randStockCount.Next(0,10000);
        }

        public async Task<int> saveOrder()
        {
            // Imitating order save to database
            await Task.Delay(200);
            Random randOrderId = new Random();
            return randOrderId.Next(5000, 100000);
        }
    }
}
