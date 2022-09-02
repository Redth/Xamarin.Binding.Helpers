New-Item -ItemType Directory -Path .\logs\ -Force
New-Item -ItemType Directory -Path .\output\ -Force

& dotnet build -t:Rebuild .\Xamarin.Binding.Helpers\Xamarin.Binding.Helpers.csproj -bl:logs/xbh.binlog

Remove-Item .\Samples\packages\ -Force -Recurse

# Run restore/rebuilds
& dotnet build -t:Rebuild .\Samples\SampleAndroidBinding\SampleAndroidBinding.csproj -bl:logs/android.binlog
& dotnet build -t:Pack .\Samples\SampleAndroidBinding\SampleAndroidBinding.csproj -bl:logs/android-pack.binlog
& dotnet build -t:Rebuild .\Samples\SampleAndroidApp\SampleAndroidApp.csproj -bl:logs/android-app.binlog
