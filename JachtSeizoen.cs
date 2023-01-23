namespace MCT.Function;

public class JachtSeizoen
{
    [FunctionName("GetGames")]
    public static async Task<IActionResult> GetGames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games")] HttpRequest req,
        ILogger log)
    {
        try
        {
            var ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

            CosmosClientOptions options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway
            };

            CosmosClient client = new CosmosClient(ConnectionString, options);
            var container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);

            string sql = "SELECT * FROM c";
            var iterator = container.GetItemQueryIterator<Game>(sql);
            var results = new List<Game>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }

    }

    [FunctionName("AddGame")]
    public static async Task<IActionResult> AddGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "games")] HttpRequest req,
        ILogger log)
    {
        try
        {
            // body uitlezen en omzetten naar Person object
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var task = JsonConvert.DeserializeObject<Game>(json);

            // connectie maken met CosmosDb
            var ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

            CosmosClientOptions options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway
            };

            CosmosClient client = new CosmosClient(ConnectionString, options);
            var container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);
            task.Id = Guid.NewGuid().ToString();
            await container.CreateItemAsync(task, new PartitionKey(task.Id));

            return new OkObjectResult(task);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }

    }

    [FunctionName("UpdateGame")]
    public static async Task<IActionResult> UpdateGame(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "games")] HttpRequest req,
        ILogger log)
    {
        try
        {
            // body uitlezen en omzetten naar Person object
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var game = JsonConvert.DeserializeObject<Game>(json);

            // connectie maken met CosmosDb
            var ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

            CosmosClientOptions options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway
            };

            CosmosClient client = new CosmosClient(ConnectionString, options);

            var container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);
            await container.ReplaceItemAsync(game, game.Id);

            return new OkObjectResult(game);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }

    }


    [FunctionName("GetGamefromGroep")]
    public static async Task<IActionResult> GetGameFromgroep(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games/{groep}")] HttpRequest req,
        string groep,
        ILogger log)
    {
        try
        {
            var ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

            CosmosClientOptions options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway
            };

            CosmosClient client = new CosmosClient(ConnectionString, options);
            var container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);

            string sql = $"SELECT * FROM c WHERE c.groep = '{groep}'";
            var iterator = container.GetItemQueryIterator<Game>(sql);
            var results = new List<Game>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }

    }

    [FunctionName("GetGamefromSpelcode")]
    public static async Task<IActionResult> GetGameFromSpelcode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games/code/{spelcode}")] HttpRequest req,
        string spelcode,
        ILogger log)
    {
        try
        {
            var ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

            CosmosClientOptions options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway
            };

            CosmosClient client = new CosmosClient(ConnectionString, options);
            var container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);

            string sql = $"SELECT * FROM c WHERE c.spelcode = '{spelcode}'";
            var iterator = container.GetItemQueryIterator<Game>(sql);
            var results = new List<Game>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            log.LogError(ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }

    }


    //mqtt
    static IMqttClient mqttClient = null;

    [FunctionName("CheckConnection")]
    public static void CheckConnection([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
    {
        BrokerInfo broker = new BrokerInfo() { BrokerAddress = "13.81.105.139", BrokerPort = 1883 };

        if (mqttClient == null) Connect(broker);
        else if (!mqttClient.IsConnected) Connect(broker);
    }

    public static async void Connect(BrokerInfo client)
    {

        var factory = new MqttFactory();
        mqttClient = factory.CreateMqttClient();
        MqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId(client.ClientId)
            .WithTcpServer(client.BrokerAddress, client.BrokerPort)
            .WithCleanSession()
            .Build();

        await mqttClient.ConnectAsync(options);
        while (!mqttClient.IsConnected)
        {
            await Task.Delay(1000);
            Console.WriteLine("Connecting to MQTT Broker ...");
        }

        Subscription subscription = new Subscription();
        List<string> topics = subscription.Topics;
        foreach (string topic in topics)
        {
            SubscribeTopicAsync(mqttClient, topic);
        }
        HandleMessages();
    }

    public static async Task PublishMessageAsync(IMqttClient mqttClient, string message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic("hetJachtSeizoen/gameResults")
            .WithPayload(message)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            .WithRetainFlag()
            .Build();

        if (mqttClient.IsConnected)
        {
            await mqttClient.PublishAsync(mqttMessage);
            Console.WriteLine("Message Published");
        }
        else
        {
            Console.WriteLine("Client not connected");
        }
    }

    public static async Task SubscribeTopicAsync(IMqttClient mqttClient, string topic)
    {
        var test = mqttClient.SubscribeAsync(topic);
        Console.WriteLine("Subscribed to topic: " + topic);
        Task.WaitAll(test);
    }

    public static async Task HandleMessages()
    {
        mqttClient.ApplicationMessageReceivedAsync += (e) =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var topic = e.ApplicationMessage.Topic;
                    if (topic == "hetJachtSeizoen/GameResults")
                    {
                        Console.WriteLine("Game results received");
                        HandleGameResults(payload);
                    }
                    else if (topic == "hetJachtSeizoen/GameInfo")
                    {
                        Console.WriteLine("Game info received");
                        HandleGameInfo(payload);
                    }
                    return Task.CompletedTask;
                };
    }

    public static void HandleGameResults(string payload)
    {
        GameResults gameResults = JsonConvert.DeserializeObject<GameResults>(payload);
        Console.WriteLine($"Game results: \n\t" + gameResults.GameId + $"\n\t" + gameResults.Winner);
        //send to database (datenow, winner, GameId)
    }

    public static void HandleGameInfo(string payload)
    {
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(payload);
        Console.WriteLine("Game info: " + gameInfo);
        //send to database if game is over
    }
}



