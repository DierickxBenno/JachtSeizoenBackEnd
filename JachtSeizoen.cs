namespace MCT.Function;

public class JachtSeizoen
{
    //azure
    static string ConnectionString = Environment.GetEnvironmentVariable("CosmosDb");

    static CosmosClientOptions options = new CosmosClientOptions()
    {
        ConnectionMode = ConnectionMode.Gateway
    };

    static CosmosClient client = new CosmosClient(ConnectionString, options);
    static Container container = client.GetContainer(General.COSMOS_DB_JACHTSEIZOEN, General.COSMOS_CONTAINER_GAMES);

    // mqtt
    static IMqttClient mqttClient = null;

    [FunctionName("GetGames")]
    public static async Task<IActionResult> GetGames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games")] HttpRequest req,
        ILogger log)
    {
        try
        {
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

    [FunctionName("GetGamefromDuration")]
    public static async Task<IActionResult> GetGameFromDuration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "games/code/{gameDuration}")] HttpRequest req,
        string gameDuration,
        ILogger log)
    {
        try
        {
            string sql = $"SELECT * FROM c WHERE c.durationGame = {gameDuration} ORDER BY c.gespeeldeTijd";
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
    // timer function run on start up
    [FunctionName("CheckConnection")]
    public static void CheckConnection([TimerTrigger("*/30 * * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
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

    public static async Task PublishMessageAsync(string message, string topicEndpoint)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"hetJachtSeizoen/{topicEndpoint}")
            .WithPayload(message)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
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
                    if (topic == "hetJachtSeizoen/gameResults")
                    {
                        Console.WriteLine("Game results received");
                        HandleGameResultsAsync(payload);
                    }
                    return Task.CompletedTask;
                };
    }

    public static async Task HandleGameResultsAsync(string payload)
    {
        GameResults gameResults = JsonConvert.DeserializeObject<GameResults>(payload);
        //send to database (datenow, winner, GameId)
        string spelcode = gameResults.GameId;
        try
        {
            string sql = $"SELECT * FROM c WHERE c.spelcode = '{spelcode}'";
            var iterator = container.GetItemQueryIterator<Game>(sql);
            List<Game> results = new List<Game>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            foreach (Game game in results)
            {
                game.EndTime = DateTime.Now;
                game.Winner = gameResults.Winner;
                game.GameInProgress = false;
                Console.WriteLine($"Game: \n\t" + game.StartTime + $"\n\t" + game.EndTime);
                await container.ReplaceItemAsync(game, game.Id);
            }
            Console.WriteLine("Game updated BITCH");

        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR BITCH");
            Console.WriteLine(ex.Message);
        }

    }

    //spel start publish

    [FunctionName("PublishMqtt")]
    public static async Task<IActionResult> StartSpelMqtt(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "mqtt/{topic}")] HttpRequest req, string topic,
        ILogger log)
    {
        try
        {
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            await PublishMessageAsync(json, topic);
            return new OkObjectResult("published");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}



