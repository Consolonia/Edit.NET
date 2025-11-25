dotnet tool uninstall -g EditDotNet
dotnet pack -c Release 
dotnet tool install -g EditDotNet --source bin/Release
