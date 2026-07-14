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
	implementation("com.facebook.android:facebook-login:18.1.3")
	implementation("com.facebook.android:facebook-share:18.1.3")
}
