using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using System.Net;

namespace todo_api.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase
{

    Microsoft.Azure.Cosmos.CosmosClient? _client;
    string? databaseName = Environment.GetEnvironmentVariable("DatabaseName");
    string? containerName = Environment.GetEnvironmentVariable("ContainerName");
    DefaultAzureCredential credential = new DefaultAzureCredential();

    private Microsoft.Azure.Cosmos.Container getContainer()
    {
        if (_client is null)
        {
            _client = new Microsoft.Azure.Cosmos.CosmosClient(Environment.GetEnvironmentVariable("CosmosEndpoint"),
                    credential);
        }

        return _client.GetContainer(databaseName, containerName);
    }

    private readonly ILogger<TodoController> _logger;

    public TodoController(ILogger<TodoController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetTasks")]
    public IEnumerable<Todo> Get()
    {
        try
        {

            var query = getContainer().GetItemQueryIterator<Todo>(new QueryDefinition("SELECT * FROM c"));
            List<Todo> results = new List<Todo>();
            while (query.HasMoreResults)
            {
                var response = query.ReadNextAsync();

                results.AddRange(response.Result.ToList());
            }

            return results;
        }
        catch
        {
            _client = null;
        }

        return new List<Todo>();
    }

    [HttpPost(Name = "PutTasks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<Todo> CreateTask(Todo todo)
    {
        try
        {
            getContainer().CreateItemAsync<Todo>(todo, new PartitionKey(todo.id));
          
            return CreatedAtAction(
            nameof(GetTask),new { id = todo.id },todo);
        }
        catch
        {
            _client = null;
        }

        return BadRequest("error");
    }


    [HttpGet("{id}")]
    public ActionResult<Todo> GetTask(string id)
    {
        try
        {
            ItemResponse<Todo> response = getContainer().ReadItemAsync<Todo>(id, new PartitionKey(id)).Result;
            return response.Resource;
        }
        catch
        {
            _client = null;
        }
        return BadRequest("error");
    }
}
