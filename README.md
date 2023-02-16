.Net 6 minimal & standard web api.  Docker container with Dapr to Service Bus

command to run:
dapr run --app-id "PubSub1" --app-port "5005" --dapr-grpc-port "50050" --dapr-http-port "5050" --components-path "./components" -- dotnet run --project ./pubsub1.csproj --urls="http://+:5005"

dapr run --app-id "PubSub2" --app-port "6006" --dapr-grpc-port "60060" --dapr-http-port "6060" --components-path "./components" -- dotnet run --project ./pubsub2.csproj --urls="http://+:6006"
