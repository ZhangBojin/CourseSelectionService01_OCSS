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

    public void SelectConfirmMq(int userId,int coursesId)
    {
        var data= JsonConvert.SerializeObject(new { userId, coursesId ,DateTime.Now});
        var body = Encoding.UTF8.GetBytes(data);
        _channel.BasicPublish(exchange: "",
            routingKey: "CourseSelection",
            basicProperties: null,
            body: body);
    }
}