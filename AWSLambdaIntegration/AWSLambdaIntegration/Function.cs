using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaIntegration;

public class Function
{
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _serviceUrl;
    private const string tableName = "Registration";

    public Function()
    {
        _accessKey = Environment.GetEnvironmentVariable("AccessKey");
        _secretKey = Environment.GetEnvironmentVariable("SecretKey");
        _serviceUrl = Environment.GetEnvironmentVariable("ServiceURL");
    }
    public async Task FunctionHandler(Customer input, ILambdaContext context)
    {
        //Logging the application
        Console.WriteLine("Execution started for function -  {0} at {1}",
                        context.FunctionName, DateTime.Now);
        //Clinet of check
        var client = new AmazonDynamoDBClient(_accessKey, _secretKey, new AmazonDynamoDBConfig { ServiceURL = _serviceUrl });
        //Create table in dunamo db if not exists
        var tableCollection = await client.ListTablesAsync();
        if (!tableCollection.TableNames.Contains(tableName))
            await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                KeySchema = new List<KeySchemaElement> {
                      { new KeySchemaElement { AttributeName="Name",  KeyType= KeyType.HASH }},
                      new KeySchemaElement { AttributeName="EmailId",  KeyType= KeyType.RANGE }
                  },
                AttributeDefinitions = new List<AttributeDefinition> {
                      new AttributeDefinition { AttributeName="Name", AttributeType="S" },
                      new AttributeDefinition { AttributeName ="EmailId",AttributeType="S"}
               },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                },
            });

        //Insert into the logs of the 
        LambdaLogger.Log("Insert record in the table");

        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "Name", new AttributeValue { S = input.Name } },
                { "Email", new AttributeValue { S = input.Email } },
                { "Phone", new AttributeValue { S = input.Phone } }
            }
        };
        var response = client.PutItemAsync(request).Result;
       
    }
}
public class Customer
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
