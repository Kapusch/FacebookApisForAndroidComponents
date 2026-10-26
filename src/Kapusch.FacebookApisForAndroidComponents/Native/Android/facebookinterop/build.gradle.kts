plugins {
	id("com.android.library")
}

android {
	namespace = "com.kapusch.facebook.androidinterop"
	compileSdk = 34

	defaultConfig {
		minSdk = 21
		consumerProguardFiles("consumer-rules.pro")
	}

	buildTypes {
		release {
			isMinifyEnabled = false
		}
	}

	compileOptions {
		sourceCompatibility = JavaVersion.VERSION_17
		targetCompatibility = JavaVersion.VERSION_17
	}
}

dependencies {
	// Keep every Facebook module on the same version as DependencyLocks/Android/lockstate.txt.
	implementation("com.facebook.android:facebook-core:19.0.0")
	implementation("com.facebook.android:facebook-login:19.0.0")
	implementation("com.facebook.android:facebook-share:19.0.0")
}
