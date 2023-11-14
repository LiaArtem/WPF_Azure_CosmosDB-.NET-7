# WPF_Azure_CosmosDB-.NET-8
WPF project - Test project using Azure Cosmos database (SQL API).

Створюємо базу даних Azure Cosmos DB на порталі:
Database name: dbserver-test-lia

Підключення:
Входимо на порталі в базу даних dbserver-test-lia, і зліва шукаємо Налаштування - Ключі - беремо URI та Первинний ключ
Прописуємо у файлі App.config
 - EndpointUri value - https://dbserver-test-lia.documents.azure.com:443/
 - PrimaryKey value - Первинний ключ