dotnet new sln -n RabbitMqDemo
cd RabbitMqDemo

dotnet new webapi -n RabbitMqDemo.Producer
dotnet new worker -n RabbitMqDemo.Consumer

dotnet sln add RabbitMqDemo.Producer/RabbitMqDemo.Producer.csproj
dotnet sln add RabbitMqDemo.Consumer/RabbitMqDemo.Consumer.csproj
