plugins {
    id("com.android.application")
}

android {
    namespace = "com.tervia.home"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.tervia.home"
        minSdk = 23
        targetSdk = 36
        versionCode = 1
        versionName = "0.2.0-alpha"
    }

    signingConfigs {
        create("release") {
            val storePath = System.getenv("TERVIA_HOME_KEYSTORE_PATH")
            if (!storePath.isNullOrBlank()) {
                storeFile = file(storePath)
                storePassword = System.getenv("TERVIA_HOME_KEYSTORE_PASSWORD")
                keyAlias = System.getenv("TERVIA_HOME_KEY_ALIAS")
                keyPassword = System.getenv("TERVIA_HOME_KEY_PASSWORD")
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            signingConfig = signingConfigs.getByName("release")
        }
    }
}
