dotnet clean
dotnet restore
dotnet publish -c Release -f net8.0 -r win-x64 -o bin\Release\net8.0\publish\win-x64\ --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
dotnet publish -c Release -f net8.0 -r linux-x64 -o bin\Release\net8.0\publish\linux-x64\ --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
dotnet publish -c Release -f net8.0 -r osx-x64 -o bin\Release\net8.0\publish\osx-x64\ --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true

