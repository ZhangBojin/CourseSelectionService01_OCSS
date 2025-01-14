using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CourseSelectionService01_OCSS.Infrastructure.RabbitMq;

public class RabbitMqProducer
{
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RabbitMqProducer(IConfiguration configuration)
    {
        var factory = new ConnectionFactory()
        {
            HostName = configuration.GetSection("RabbitMq")["HostName"]!,
            Port = Convert.ToInt32(configuration.GetSection("RabbitMq")["Port"]),
            UserName = configuration.GetSection("RabbitMq")["Username"]!,
            Password = configuration.GetSection("RabbitMq")["Password"]!
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "Logs",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueDeclare(queue: "CourseSelection",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    ~RabbitMqProducer()
    {
        _channel.Close();
        _connection.Close();
    }

    public void Log()
    {
        var log = new
        {
          
        };
        var jsonMessage = JsonConvert.SerializeObject(log);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: "",
            routingKey: "Logs",
            basicProperties: null,
            body: body);
    }

    public void SelectConfirmMq(int id)
    {
        var body = Encoding.UTF8.GetBytes(id.ToString());
        _channel.BasicPublish(exchange: "",
            routingKey: "Logs",
            basicProperties: null,
            body: body);
    }
}