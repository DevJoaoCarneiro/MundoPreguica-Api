using Application.Interfaces;
using Application.Notifications;
using System.Threading.Channels;

namespace Infrastructure.ExternalServices
{
    public class OrderCreatedEmailQueue : IOrderCreatedEmailQueue
    {
        private readonly Channel<OrderCreatedEmailNotification> _queue;

        public OrderCreatedEmailQueue()
        {
            var options = new BoundedChannelOptions(200)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            _queue = Channel.CreateBounded<OrderCreatedEmailNotification>(options);
        }

        public async Task QueueAsync(OrderCreatedEmailNotification notification, CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(notification, cancellationToken);
        }

        public async Task<OrderCreatedEmailNotification> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
