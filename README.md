# WPF_Azure_CosmosDB-.NET-6
WPF project - Test project using Azure Cosmos database (SQL API).

Создаем базу данных Azure Cosmos DB на портале:
Database name: dbserver-test-lia

Подключение:
Входим на портале в базу данных dbserver-test-lia, и слева ищем Настройки - Ключи - берем URI и Первичный ключ
Прописываем в файле App.config
 - EndpointUri value - https://dbserver-test-lia.documents.azure.com:443/
 - PrimaryKey  value - Первичный ключ