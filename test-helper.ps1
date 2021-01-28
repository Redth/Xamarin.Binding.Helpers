param (
    [switch]$android = $false,
    [switch]$ios = $false
)


New-Item -ItemType Directory -Path .\logs\ -Force
New-Item -ItemType Directory -Path .\output\ -Force

# Build the main lib/package
& msbuild /r /t:Rebuild .\Xamarin.Binding.Helpers\Xamarin.Binding.Helpers.csproj /bl:logs\xbh.binlog

# Clear out nuget caches
Remove-Item .\Samples\packages\xamarin.binding.helpers -Force -Recurse

if ($IsWindows)
{
    Remove-Item C:\Users\jondi\.nuget\packages\Xamarin.Binding.Helpers -Force -Recurse
}

if ($IsMacOS)
{
    Remove-Item \Users\redth\.nuget\packages\Xamarin.Bindings.Helpers -Force -Recurse
}


# Run builds for each platform specified
if ($ios)
{
    & msbuild /r /t:Rebuild .\Samples\SampleiOSApp\SampleiOSApp.csproj /bl:logs\ios-app.binlog
}

if ($android)
{
    & msbuild /r /t:Rebuild .\Samples\SampleAndroidBinding\SampleAndroidBinding.csproj /bl:logs\android.binlog
    & msbuild /t:Pack .\Samples\SampleAndroidBinding\SampleAndroidBinding.csproj /bl:logs\android-pack.binlog
    & msbuild /r /t:Rebuild .\Samples\SampleAndroidApp\SampleAndroidApp.csproj /bl:logs\android-app.binlog
}
