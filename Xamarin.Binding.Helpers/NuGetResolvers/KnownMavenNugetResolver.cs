using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Binding.Helpers.NugetResolvers
{
	public class KnownMavenNugetResolver : MavenNugetResolver
	{
		static Dictionary<string, string> map = new Dictionary<string, string>
		{
			{ "com.squareup.okio.okio", "Square.OkIO" },
			{ "com.squareup.okhttp3.okhttp", "Square.OkHttp3" },
			{ "com.squareup.okhttp3.okhttp-ws", "Square.OkHttp3.WS" },
			{ "com.squareup.okhttp3.okhttp-urlconnection", "Square.OkHttp.UrlConnection" },
			{ "com.squareup.okhttp.okhttp", "Square.OkHttp" },
			{ "com.squareup.okhttp.okhttp-ws", "Square.OkHttp.WS" },
			{ "com.squareup.okhttp.okhttp-urlconnection", "Square.OkHttp.UrlConnection" },
			{ "com.squareup.picasso.picasso", "Square.Picasso" },
			{ "com.squareup.retrofit2.retrofit", "Square.Retrofit2" },
			{ "com.squareup.retrofit2.converter-gson", "Square.Retrofit2.ConverterGson" },
			{ "com.squareup.retrofit2.adapter-rxjava", "Square.Retrofit2.AdapterRxJava2" },
			{ "com.squareup.moshi.moshi", "Square.Moshi" },
			{ "com.squareup.seismic", "Square.Seismic" },

			{ "com.jakewharton.picasso.picasso2-okhttp3-downloader", "JakeWharton.Picasso2OkHttp3Downloader" },
			{ "com.jakewharton.threetenabp.threetenabp", "Xamarin.Android.JakeWharton.ThreeTenAbp" },
			{ "com.jakewharton.timber.timber", "Xamarin.JakeWharton.Timber" },

			{ "com.github.bumptech.glide.glide", "Xamarin.Android.Glide" },
			{ "com.github.bumptech.glide.disklrucache", "Xamarin.Android.Glide.DiskLruCache" },
			{ "com.github.bumptech.glide.gifdecoder", "Xamarin.Android.Glide.GifDecoder" },
			{ "com.github.bumptech.glide.recyclerview-integration", "Xamarin.Android.Glide.RecyclerViewIntegration" },

			{ "org.reactivestreams.reactive-streams", "Xamarin.Android.ReactiveStreams" },

			{ "io.reactivex.rxjava2.rxandroid", "Xamarin.Android.ReactiveX.RxAndroid" },
			{ "io.reactivex.rxjava2.rxjava", "Xamarin.Android.ReactiveX.RxJava" },

			{ "com.android.volley.volley", "Xamarin.Android.Volley" },

			{ "com.facebook.android.facebook-android-sdk", "Xamarin.Facebook.Android" },
			{ "com.facebook.android.account-kit-sdk", "Xamarin.Facebook.AccountKit.Android" },
			{ "com.facebook.android.facebook-applinks", "Xamarin.Facebook.AppLinks.Android" },
			{ "com.facebook.android.audience-network-sdk", "Xamarin.Facebook.AudienceNetwork.Android" },
			{ "com.facebook.android.facebook-common", "Xamarin.Facebook.Common.Android" },
			{ "com.facebook.android.facebook-core", "Xamarin.Facebook.Core.Android" },
			{ "com.facebook.android.facebook-livestreaming", "Xamarin.Facebook.LiveStreaming.Android" },
			{ "com.facebook.android.facebook-login", "Xamarin.Facebook.Login.Android" },
			{ "com.facebook.android.facebook-loginkit", "Xamarin.Facebook.LoginKit.Android" },
			{ "com.facebook.android.facebook-marketing", "Xamarin.Facebook.Marketing.Android" },
			{ "com.facebook.android.notifications", "Xamarin.Facebook.Notifications.Android" },
			{ "com.facebook.android.facebook-places", "Xamarin.Facebook.Places.Android" },
			{ "com.facebook.android.facebook-share", "Xamarin.Facebook.Share.Android" },

			{ "com.google.android.datatransport.transport-api", "Xamarin.Google.Android.DataTransport.TransportApi" },
			{ "com.google.android.datatransport.transport-backend-cct", "Xamarin.Google.Android.DataTransport.TransportBackendCct" },
			{ "com.google.android.datatransport.transport-runtime", "Xamarin.Google.Android.DataTransport.TransportRuntime" },

			{ "com.google.android.libraries.places", "Xamarin.Google.Android.Places" },

			{ "com.google.android.play.core", "Xamarin.Google.Android.Play.Core" },

			{ "com.google.auto.value.auto-value-annotations", "Xamarin.Google.AutoValue.Annotations" },

			{ "com.google.dagger.dagger", "Xamarin.Google.Dagger" },

			{ "com.google.guava.guava", "Xamarin.Google.Guava" },

			{ "com.google.guava.failureaccess", "Xamarin.Google.Guava.FailureAccess" },

			{ "com.google.guava.listenablefuture", "Xamarin.Google.Guava.ListenableFuture" },

			{ "com.googlecode.libphonenumber.libphonenumber", "Xamarin.Google.LibPhoneNumber" },


			{ "com.google.zxing.core", "Xamarin.Google.ZXing.Core" },

			{ "io.grpc.grpc-android", "Xamarin.Grpc.Android" },
			{ "io.grpc.grpc-context", "Xamarin.Grpc.Context" },
			{ "io.grpc.grpc-core", "Xamarin.Grpc.Core" },
			{ "io.grpc.grpc-okhttp", "Xamarin.Grpc.OkHttp" },
			{ "io.grpc.grpc-protobuf-lite", "Xamarin.Grpc.Protobuf.Lite" },
			{ "io.grpc.grpc-stub", "Xamarin.Grpc.Stub" },

			{ "io.opencensus.opencensus-api", "Xamarin.Io.OpenCensus.OpenCensusApi" },
			{ "io.opencensus.opencensus-contrib-grpc-metrics", "Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics" },

			{ "io.perfmark.perfmark-api", "Xamarin.Io.PerfMark.PerfMarkApi" },

			{ "javax.inject.inject", "Xamarin.JavaX.Inject" },
			{ "org.jetbrains.annotations", "Xamarin.Jetbrains.Annotations" },

			{ "org.jetbrains.kotlin.kotlin-reflect", "Xamarin.Kotlin.Reflect" },
			{ "org.jetbrains.kotlin.kotlin-stdlib", "Xamarin.Kotlin.StdLib" },
			{ "org.jetbrains.kotlin.kotlin-stdlib-common", "Xamarin.Kotlin.StdLib.Common" },
			{ "org.jetbrains.kotlin.kotlin-stdlib-jdk7", "Xamarin.Kotlin.StdLib.Jdk7" },
			{ "org.jetbrains.kotlin.kotlin-stdlib-jdk8", "Xamarin.Kotlin.StdLib.Jdk8" },
			{ "org.jetbrains.kotlin.kotlin-stdlib-jre7", "Xamarin.Kotlin.StdLib.Jre7" },
			{ "org.jetbrains.kotlin.kotlin-stdlib-jre8", "Xamarin.Kotlin.StdLib.Jre8" },

			{ "org.tensorflow.tensorflow-lite", "Xamarin.TensorFlow.Lite" },
			{ "org.tensorflow.tensorflow-lite-gpu", "Xamarin.TensorFlow.Lite.Gpu" },
		};

		public override async Task<NuGetSuggestion> Resolve(string mavenGroupId, string mavenArtifactId, string mavenRequestedVersion, string mavenResolvedVersion)
		{
			var id = $"{mavenGroupId}.{mavenArtifactId}";

			var v = await NuGetUtil.FindBestVersion(id, mavenRequestedVersion, mavenResolvedVersion, true);

			if (v != null)
				return new NuGetSuggestion { Version = v.ToNormalizedString(), PackageId = id };

			return null;
		}
	}
}
