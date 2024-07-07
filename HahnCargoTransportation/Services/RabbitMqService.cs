using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using HahnCargoTransportation.Services.Interfaces;
using HahnCargoTransportation.Models;
using System.Collections.Concurrent;
using System.Globalization;

namespace HahnCargoTransportation.Services
{
    public class RabbitMqService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly ConcurrentQueue<Order> _availableOrders = new();

        public RabbitMqService(IServiceProvider serviceProvider, ILogger<RabbitMqService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "HahnCargoSim_NewOrders",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMqService initialized and queue declared.");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                        var order = JsonConvert.DeserializeObject<Order>(message);
                        _availableOrders.Enqueue(order);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                }
            };

            _channel.BasicConsume(queue: "HahnCargoSim_NewOrders", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessOrdersAsync(stoppingToken);
                await ManageTransportersAsync(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessOrdersAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var transporterService = scope.ServiceProvider.GetRequiredService<ITransporterService>();
                var gridService = scope.ServiceProvider.GetRequiredService<IGridService>();

                while (!_availableOrders.IsEmpty && !stoppingToken.IsCancellationRequested)
                {
                    if (_availableOrders.TryDequeue(out var order))
                    {
                        try
                        {
                            var transporters = await transporterService.GetTransportersAsync();
                            var gridData = await gridService.GetGridDataAsync();
                            var connections = gridData.Connections;
                            var edges = gridData.Edges;
                            var acceptedOrders = await orderService.GetAllAcceptedOrdersAsync();

                            var suitableTransporter = FindSuitableTransporter(order, transporters, connections, edges, acceptedOrders);

                            if (suitableTransporter != null)
                            {
                                await AcceptOrderAndInitiateDelivery(order, suitableTransporter, orderService, transporterService);
                                _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
                            }
                            else
                            {
                                _logger.LogInformation("No suitable transporter found for order ID: {OrderId}", order.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing order ID: {OrderId}", order.Id);
                        }
                    }
                }
            }
        }

        private CargoTransporter FindSuitableTransporter(Order order, List<CargoTransporter> transporters, List<Connection> connections, List<Edge> edges, List<Order> acceptedOrders)
        {
            // Calculate the total remaining capacity of all transporters
            double totalAcceptedLoad = acceptedOrders.Sum(o => o.Load);
            double totalTransporterCapacity = transporters.Sum(t => t.Capacity);
            double totalRemainingCapacity = totalTransporterCapacity - totalAcceptedLoad;

            // Check if total remaining capacity is sufficient for the new order
            if (totalRemainingCapacity < order.Load)
            {
                _logger.LogInformation("Insufficient capacity for order ID: {OrderId}", order.Id);
                return null;
            }

            // Iterate through transporters to find a suitable one
            foreach (var transporter in transporters)
            {
                // Check if transporter has enough capacity for the order
                if (transporter.Capacity >= order.Load && CanDeliverOnTime(order, transporter, connections, edges))
                {
                    return transporter;
                }
            }

            return null;
        }

        private List<int> FindShortestPath(int startNodeId, int endNodeId, List<Connection> connections, List<Edge> edges)
        {
            var edgeLookup = edges.ToDictionary(e => e.Id);
            var connectionLookup = new Dictionary<int, List<Connection>>();

            foreach (var connection in connections)
            {
                if (!connectionLookup.ContainsKey(connection.FirstNodeId))
                    connectionLookup[connection.FirstNodeId] = new List<Connection>();
                connectionLookup[connection.FirstNodeId].Add(connection);

                if (!connectionLookup.ContainsKey(connection.SecondNodeId))
                    connectionLookup[connection.SecondNodeId] = new List<Connection>();
                connectionLookup[connection.SecondNodeId].Add(connection);
            }

            var distances = new Dictionary<int, double>();
            var previousNodes = new Dictionary<int, int>();
            var priorityQueue = new SortedSet<(double Distance, int NodeId)>(Comparer<(double, int)>.Create((x, y) => x.Item1 != y.Item1 ? x.Item1.CompareTo(y.Item1) : x.Item2.CompareTo(y.Item2)));

            foreach (var connection in connections)
            {
                distances[connection.FirstNodeId] = double.MaxValue;
                distances[connection.SecondNodeId] = double.MaxValue;
            }

            distances[startNodeId] = 0;
            priorityQueue.Add((0, startNodeId));

            while (priorityQueue.Any())
            {
                var (currentDistance, currentNodeId) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentNodeId == endNodeId)
                    break;

                if (!connectionLookup.ContainsKey(currentNodeId))
                    continue;

                foreach (var connection in connectionLookup[currentNodeId])
                {
                    int neighborNodeId = connection.FirstNodeId == currentNodeId ? connection.SecondNodeId : connection.FirstNodeId;
                    double edgeCost = edgeLookup[connection.EdgeId].Cost;
                    double newDistance = currentDistance + edgeCost;

                    if (newDistance < distances[neighborNodeId])
                    {
                        priorityQueue.Remove((distances[neighborNodeId], neighborNodeId));
                        distances[neighborNodeId] = newDistance;
                        previousNodes[neighborNodeId] = currentNodeId;
                        priorityQueue.Add((newDistance, neighborNodeId));
                    }
                }
            }

            var path = new List<int>();
            int current = endNodeId;

            while (previousNodes.ContainsKey(current))
            {
                path.Add(current);
                current = previousNodes[current];
            }

            path.Add(startNodeId);
            path.Reverse();

            return path;
        }

        private bool CanDeliverOnTime(Order order, CargoTransporter transporter, List<Connection> connections, List<Edge> edges)
        {
            var shortestPathToOrder = FindShortestPath(transporter.PositionNodeId, order.OriginNodeId, connections, edges);
            var shortestPathToTarget = FindShortestPath(order.OriginNodeId, order.TargetNodeId, connections, edges);
            if (shortestPathToOrder == null || shortestPathToTarget == null)
            {
                return false;
            }

            double totalCost = 0;
            TimeSpan totalTime = TimeSpan.Zero;

            for (int i = 0; i < shortestPathToOrder.Count - 1; i++)
            {
                int currentNodeId = shortestPathToOrder[i];
                int nextNodeId = shortestPathToOrder[i + 1];

                var connection = connections.FirstOrDefault(c =>
                    (c.FirstNodeId == currentNodeId && c.SecondNodeId == nextNodeId) ||
                    (c.FirstNodeId == nextNodeId && c.SecondNodeId == currentNodeId));

                if (connection == null)
                {
                    return false;
                }

                var edge = edges.FirstOrDefault(e => e.Id == connection.EdgeId);
                if (edge == null)
                {
                    return false;
                }

                totalCost += edge.Cost;
                totalTime += edge.Time;
            }

            for (int i = 0; i < shortestPathToTarget.Count - 1; i++)
            {
                int currentNodeId = shortestPathToTarget[i];
                int nextNodeId = shortestPathToTarget[i + 1];

                var connection = connections.FirstOrDefault(c =>
                    (c.FirstNodeId == currentNodeId && c.SecondNodeId == nextNodeId) ||
                    (c.FirstNodeId == nextNodeId && c.SecondNodeId == currentNodeId));

                if (connection == null)
                {
                    return false;
                }

                var edge = edges.FirstOrDefault(e => e.Id == connection.EdgeId);
                if (edge == null)
                {
                    return false;
                }

                totalCost += edge.Cost;
                totalTime += edge.Time;
            }

            var estimatedDeliveryTime = DateTime.UtcNow.Add(totalTime);

            const string dateFormat = "MM/dd/yyyy HH:mm:ss";

            if (DateTime.TryParseExact(order.expirationDateUtc, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expirationDate))
            {
                // Use the parsed date directly if it's valid
                _logger.LogInformation("Order ID: {OrderId}, TotalCost: {TotalCost}, OrderValue: {OrderValue}, EstimatedDeliveryTime: {EstimatedDeliveryTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}, ExpirationDate: {ExpirationDate:yyyy-MM-ddTHH:mm:ss.fffffffZ}",
                    order.Id, totalCost, order.Value, estimatedDeliveryTime, expirationDate);

                return totalCost <= order.Value && estimatedDeliveryTime <= expirationDate;
            }
            else
            {
                _logger.LogError("Invalid ExpirationDate format for order ID: {OrderId}", order.Id);
                return false;
            }
        }

        private async Task AcceptOrderAndInitiateDelivery(Order order, CargoTransporter transporter, IOrderService orderService, ITransporterService transporterService)
        {
            await orderService.AcceptOrderAsync(order.Id);

            // Get the shortest path from origin node to target node
            var gridService = _serviceProvider.GetRequiredService<IGridService>();
            var gridData = await gridService.GetGridDataAsync();
            var connections = gridData.Connections;
            var edges = gridData.Edges;

            var shortestPathToOrder = FindShortestPath(transporter.PositionNodeId, order.TargetNodeId, connections, edges);
            var shortestPathToTarget = FindShortestPath(order.OriginNodeId, order.TargetNodeId, connections, edges);
            // Move the transporter node by node along the shortest path
            for (int i = 0; i < shortestPathToOrder.Count - 1; i++)
            {
                int currentNodeId = shortestPathToOrder[i];
                int nextNodeId = shortestPathToOrder[i + 1];

                await transporterService.MoveTransporterAsync(transporter.Id, nextNodeId);
            }
            for (int i = 0; i < shortestPathToTarget.Count - 1; i++)
            {
                int currentNodeId = shortestPathToTarget[i];
                int nextNodeId = shortestPathToTarget[i + 1];

                await transporterService.MoveTransporterAsync(transporter.Id, nextNodeId);
            }
        }

        private async Task ManageTransportersAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var transporterService = scope.ServiceProvider.GetRequiredService<ITransporterService>();
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var currentTransporters = await transporterService.GetTransportersAsync();
                    var coinBalance = await userService.GetCoinAmountAsync();

                    if (coinBalance >= 1500)
                    {
                        await Task.Run(async () =>
                        {
                            await transporterService.BuyTransporterAsync(0);
                        }, stoppingToken);

                        _logger.LogInformation("New transporter purchased.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ManageTransportersAsync");
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
