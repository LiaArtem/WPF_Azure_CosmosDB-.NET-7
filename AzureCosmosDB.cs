using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace WPF_Azure_CosmosDB
{
    class Azure_CosmosDB
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string? EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string? PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;        

        // The name of the database and container we will create
        private readonly string databaseId = "ToDoList";
        private readonly string containerId = "Items";

        public List<UserData> UserDataList;
        public Boolean IsControl;

        /// Create the database if it does not exist
        private async Task CreateDatabaseAsync()
        {
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        /// Create the container if it does not exist. 
        private async Task CreateContainerAsync()
        {
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
        }

        public async Task SelectItemsAsync(string sqlQueryText)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Azure_CosmosDB" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.QueryItemsAsync(sqlQueryText);            
        }

        public async Task InsertItemsAsync(string sqlQueryText, UserData ud)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Azure_CosmosDB" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.AddItemsToContainerAsync(ud);
            await this.QueryItemsAsync(sqlQueryText);
        }

        public async Task ReplaceItemsAsync(string sqlQueryText, UserData ud)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Azure_CosmosDB" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.ReplaceItemAsync(ud);
            await this.QueryItemsAsync(sqlQueryText);
        }

        public async Task DeleteItemsAsync(string sqlQueryText, UserData ud)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Azure_CosmosDB" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.DeleteItemAsync(ud);
            await this.QueryItemsAsync(sqlQueryText);
        }

        /// Run a query
        private async Task QueryItemsAsync(string sqlQueryText)
        {            
            QueryDefinition queryDefinition = new(sqlQueryText);
            FeedIterator<UserData> queryResultSetIterator = this.container.GetItemQueryIterator<UserData>(queryDefinition);
            List<UserData> uds = [];            

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserData> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (UserData ud in currentResultSet)
                {
                    uds.Add(ud);
                }                
            }
            this.UserDataList = uds;
        }

        /// Add items to the container        
        public async Task AddItemsToContainerAsync(UserData ud)
        {
            try
            {
                // Read the item to see if it exists.            
                _ = await this.container.ReadItemAsync<UserData>(ud.Id, new PartitionKey(ud.PartitionKey));                
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container
                _ = await this.container.CreateItemAsync<UserData>(ud, new PartitionKey(ud.PartitionKey));
            }            
        }

        /// Replace an item in the container
        private async Task ReplaceItemAsync(UserData ud)
        {
            ItemResponse<UserData> rezultResponse = await this.container.ReadItemAsync<UserData>(ud.Id, new PartitionKey(ud.PartitionKey));
            var itemBody = rezultResponse.Resource;

            // защита от неконтролируемого обновления
            this.IsControl = false;
            if (itemBody != null)
            {
                if (ud.Version != itemBody.Version)
                {
                    this.IsControl = true;
                    return;
                }
            }

            itemBody!.TextValue = ud.TextValue;
            itemBody.IntValue = ud.IntValue;
            itemBody.DoubleValue = ud.DoubleValue;
            itemBody.BoolValue = ud.BoolValue;
            itemBody.DateValue = ud.DateValue;
            itemBody.Version++;
            
            // replace the item with the updated content
            _ = await this.container.ReplaceItemAsync<UserData>(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));           
        }

        /// Delete an item in the container
        private async Task DeleteItemAsync(UserData ud)
        {
            // Delete an item. Note we must provide the partition key value and id of the item to delete
            _ = await this.container.DeleteItemAsync<UserData>(ud.Id, new PartitionKey(ud.PartitionKey));            
        }

    }
}
