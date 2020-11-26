# Xamarin.Binding.Helpers

Xamarin.Binding.Helpers is a NuGet package with MSBuild tasks and other useful tools to help make bindings easier!


## How to use this?

> If you want to jump in and use what's available right away, this section is for you.  Otherwise, continue reading below to get an idea of the background and context for why this exists.

Currently, only Android Studio projects are supported in using the Slim Binding pattern approach.  Eventually iOS will be supported in a similar fashion.

1. Install the NuGet package in your Binding Project, and any other projects which will reference your binding project directly.
2. Create a new project in Android Studio
3. Add a new module to your new project, choosing "Android Library" as the type.  Remember the name you choose (default is 'mylibrary')
4. Apply the Xamarin Binding Helpers Gradle plugin to your module's build.gradle file (not the project level one)
```
apply from: 'https://url-to-plugin.gradle'
```
5. Add a link to your Android Studio Project in your binding project:
```
<ItemGroup>
  <AndroidStudioProject Include="/path/to/project" Module="mylibrary" />
</ItemGroup>
```
6. Write the slim binding abstraction / wrapper API in your project.
7. Build your binding project!  You can even package it as a nuget (Note: multitargeted binding projects are NOT supported)


#  Challenges with Bindings

Bindings for Xamarin.Android and Xamarin.iOS can be hard.  This project aims to make some of these challenges a bit more easy:

### Finding the native SDK and its dependencies
One of the biggest challenges in creating a binding is getting the correct native SDK artifacts (binaries/libraries/maven/cocoapods/whatever) onto your system including their dependencies.  It's even more challenging when some of these dependencies already have nuget packages available with bindings.

Often there are tutorials or guides available for how to install and use these native SDK's with the native tools such as Android Studio or XCode, but it's not easy to know always how those steps translate into the Xamarin world.

### Creating the actual binding

While there is some great documentation on bindings for Xamarin.iOS and Xamarin.Android, the binding tools are not perfect, and when you encounter an SDK with a particularly large or complex set of API's, it often requires a great deal of manual intervention to get the bindings generated for these SDK's.  Additionally, you often only need a small fraction of the API surface for the SDK you are trying to bind.


## Slim Binding Approach

One of the patterns that's been helpful for accessing native SDK's in Xamarin apps is commonly referred to as the 'Slim Binding' pattern.

The elevator pitch is this: ***Xamarin binding tools work great with simple API surfaces***

The idea is to create a native library project in Android Studio and/or XCode and use Java/Kotlin and/or ObjC/Swift to create your own abstraction or 'wrapper' API to the native SDK's you are interested in calling from your Xamarin app.

This abstraction should be a relatively simple public API which exposes no types from the underlying SDK's you are avoiding binding, but instead only makes use of primitive types, or types that already have bindings in Xamarin.  For example, Xamarin.iOS already contains bindings for `UIView` so any time you need to return a view to your Xamarin app, you would (usually implicitly) cast the view type from the native SDK to its base `UIView` type in your abstraction.

Here's an example of a simple Java abstraction for some MapBox SDK API Calls:

```java
package codes.redth.mylibrary;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;

import androidx.annotation.NonNull;

import com.mapbox.mapboxsdk.Mapbox;
import com.mapbox.mapboxsdk.annotations.Marker;
import com.mapbox.mapboxsdk.annotations.MarkerOptions;
import com.mapbox.mapboxsdk.camera.CameraPosition;
import com.mapbox.mapboxsdk.geometry.LatLng;
import com.mapbox.mapboxsdk.maps.MapView;
import com.mapbox.mapboxsdk.maps.MapboxMap;
import com.mapbox.mapboxsdk.maps.MapboxMapOptions;
import com.mapbox.mapboxsdk.maps.OnMapReadyCallback;
import com.mapbox.mapboxsdk.maps.Style;

public class MapBoxSdk {
    private final Activity parentActivity;
    private final MapBoxSdkCallback callbackListener;

    private MapView mapView;
    private MapboxMap map;

    public MapBoxSdk(Activity activity, MapBoxSdkCallback listener)
    {
        parentActivity = activity;
        callbackListener = listener;
    }

    public void init(String mapboxAccessToken)
    {
        Mapbox.getInstance(parentActivity, mapboxAccessToken);
    }

    public void onCreate(Bundle savedInstanceState, Double lat, Double lng, Integer zoom)
    {
        MapboxMapOptions options = MapboxMapOptions.createFromAttributes(parentActivity, null)
                .camera(new CameraPosition.Builder()
                        .target(new LatLng(lat, lng))
                        .zoom(zoom)
                        .build());

        mapView = new MapView(parentActivity, options);
        mapView.onCreate(savedInstanceState);
        mapView.getMapAsync(new OnMapReadyCallback() {
            @Override
            public void onMapReady(@NonNull MapboxMap mapboxMap) {

                map = mapboxMap;
                mapboxMap.setStyle(Style.MAPBOX_STREETS, new Style.OnStyleLoaded() {
                    @Override
                    public void onStyleLoaded(@NonNull Style style) {
                        callbackListener.mapReady();
                    }
                });
                mapboxMap.setOnMarkerClickListener(new MapboxMap.OnMarkerClickListener() {
                    @Override
                    public boolean onMarkerClick(@NonNull Marker marker) {
                        callbackListener.markerClicked(marker.getTitle());
                        return true;
                    }
                });
            }
        });
    }

    public View getView()
    {
        return mapView;
    }

    public void addMarker(double lat, double lng, String title, String snippet)
    {
        map.addMarker(new MarkerOptions()
                .position(new LatLng(lat, lng))
                .title(title)
                .setSnippet(snippet));
    }

    public void onStart()
    {
        mapView.onStart();
    }

    public void onResume()
    {
        mapView.onResume();
    }

    public void onPause()
    {
        mapView.onPause();
    }

    public void onStop()
    {
        mapView.onStop();
    }

    public void onLowMemory()
    {
        mapView.onLowMemory();
    }

    public void onDestory()
    {
        mapView.onDestroy();
    }
}
```

Now, instead of generating a binding for any of the MapBox types, the Xamarin binding tools only need to generate it for the simple API you just created.  This means things like `Double`, or `String` or `View` or `Activity` - all of which are types already known.  The tools can easily generate a binding for this API without any manual intervention at all.

